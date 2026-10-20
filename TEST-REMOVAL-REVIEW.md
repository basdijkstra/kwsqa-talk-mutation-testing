# Same coverage, less protection: reviewing the removal of 169 tests

## Context

The `BankAccounts.Api` test suite had 182 tests: NUnit unit tests for the service, repository and
exception handler, and RestAssured.Net integration tests against the API running in memory.

Using the per-test data in the Stryker.NET report (`coveredBy` and `killedBy` for each mutant) plus
per-test line coverage from coverlet, we computed the smallest set of tests that keeps everything
the full suite had:

- every mutant that was killed is still killed (44)
- every mutant that was covered is still covered
- every line and branch that was covered is still covered (129 lines, 28 branch outcomes)

That set has **13 tests**. Stryker's data alone said 12, but coverlet found one line that only one
extra test reached: a fallback `switch` arm that has no mutant on it, so Stryker could not see it.

The other **169 tests were deleted**. On paper, nothing was lost.

This review looks at the deleted tests the way a senior automation engineer would. Which of them
added value that code coverage and mutation coverage do not measure?

## Experiment: does the trimmed suite still catch bugs?

Three realistic bugs were introduced into the API, one at a time, and the 13 remaining tests were
run against each:

| Bug introduced | Change in the code | Kept 13 tests | Deleted tests that would have caught it |
|---|---|---|---|
| Savings accounts can overdraw up to $2500, like checking accounts | `AccountType.Savings => 0m` → `AccountType.Savings => -CheckingOverdraftLimit` | **all pass** | `Savings_BeyondBalance_*` (unit and integration), `FailedWithdrawal_DoesNotAffectLaterWithdrawals` |
| Interest is rounded to 4 decimals instead of 2 | `Math.Round(account.Balance * InterestRate, 2)` → `..., 4)` | **all pass** | `AddInterest_RoundsInterestToNearestCent`, `AddInterest_HalfCent_RoundsToEven`, integration `SavingsAccount_Returns200...(0.26, 0.27)` |
| Request validation is removed | `[Required]` removed from `CreateAccountRequest` and `TransactionRequest` | **all pass** | `TypeIsNull_Returns400`, `InvalidBody_*("{}")`, `InvalidBody_*("{\"amount\": null}")` |

The first bug breaks one of the core business rules of the API: you cannot overdraw a savings
account. After the cleanup, **no test checks that rule anymore.**

"Same code coverage and same mutation score" turned out not to mean "same protection".

## Why mutation coverage did not see this

A mutation score can only be as good as the mutants that were generated. Three blind spots
showed up here:

1. **Number values are not mutated.** As far as I know, Stryker.NET has no mutator that changes
   numeric values, so `Savings => 0m` and the `2` in `Math.Round(x, 2)` were never mutated. The
   savings rule only *looked* covered because it shares the `newBalance < lowestAllowedBalance`
   comparison with the checking rule, and the checking tests killed the mutants on that line.
2. **Behaviour that lives in attributes and configuration is not mutated.** Validation attributes
   (`[Required]`), route constraints (`{id:int}`), `[Produces]` and the JSON options in
   `Program.cs` define a large part of an ASP.NET API's behaviour. None of them produce mutants.
3. **Mutants that never ran do not count.** 18 of the 66 mutants were `Ignored` or caused a
   `CompileError`, and they included every mutant in the exception message code. That is why
   `ExceptionMessageTests` showed zero coverage. They were not worthless: Stryker had nothing to
   measure them against.

## Deleted tests worth keeping, and why

### 1. Business rules the score cannot see

This is the strongest case.

- `Withdraw_FromSavings_BeyondBalance_ThrowsInsufficientFunds` (unit) and
  `Savings_BeyondBalance_Returns422ProblemAndKeepsBalance` (integration) are the only tests that
  check the savings overdraft rule.
- `Withdraw_FromSavings_WithinBalance_Succeeds(100, 100)` checks the boundary where a savings
  balance lands exactly on zero.
- `AddInterest_RoundsInterestToNearestCent` and `AddInterest_HalfCent_RoundsToEven`: rounding is a
  rule about money. Banker's rounding, where an exact half cent goes to the even cent, surprises
  people, so it deserves a test that names it.

### 2. API contract defined by attributes and configuration

- `TypeIsNull_Returns400`, plus `InvalidBody_*` for `{}` and `{"amount": null}`. Without
  `[Required]`, the controller reaches `request.Type!.Value` on a null value, and the client gets a
  500 Internal Server Error instead of a 400 Bad Request.
- One `IdThatIsNotAnInteger_Returns404`, for example `"abc"` on `GET /accounts/{id}`. It checks
  the `{id:int}` route constraint. One endpoint is enough.
- `LocationHeader_PointsToTheNewAccount`. A `201 Created` should come with a `Location` header
  that works, and no remaining test checks it.
- `BodyThatIsNotJson_Returns415` checks the response to a body that is not JSON.
- `AmountAsNumericString_IsAccepted` documents a surprising ASP.NET default: `{"amount": "10.50"}`
  is accepted as a number. If someone makes number parsing stricter, this test shows that the API
  contract changed, not just an implementation detail.

### 3. Formatting bugs that depend on the machine

- `ExceptionMessageTests` run under `[SetCulture("nl-NL")]`, where the decimal separator is a
  comma. While building this API we actually hit this bug: error messages said `102,01` instead of
  `102.01` on a Dutch Windows machine.
- The kept integration tests compare messages using the culture of whatever machine runs them. On
  a CI server with English settings, a return of this bug would go unnoticed. The deleted tests
  force the Dutch format, so they catch it on any machine.

### 4. Protection for future code, not only today's code

Mutation testing judges the code as it is today. These tests protect against changes that are
likely to happen later:

- `Add_FromManyThreads_GivesUniqueIds` is the only check that account IDs stay unique under
  parallel requests. Stryker cannot replace `Interlocked.Increment(ref _nextId)` with `++_nextId`,
  but a developer can.
- `OnlyTheTargetAccountChanges`, `OnlyTheRequestedAccountIsDeleted` and
  `ReturnsTheRequestedAccountWhenThereAreSeveral` cannot fail against today's in-memory
  dictionary. Once the repository is backed by a real database, a missing `WHERE` clause is a
  classic bug, and these tests catch exactly that.
- A few `InMemoryAccountRepositoryTests`, such as sequential ID assignment and
  `Add_IdsAreNotReusedAfterDelete`, describe what *any* repository must do. They become the test
  suite for a second implementation.

### 5. Finding the cause quickly when something breaks

- The two journey tests that were kept each cover 32 to 36 mutants. When one of them fails, you
  have to debug to find out which of about 15 rules broke.
- A focused test such as `Checking_WithinOverdraftLimit_Returns200AndStoresNewBalance(0, 2500)`
  tells you what broke from its name alone.
- One focused test per rule turns "something is broken" into "this rule is broken".

### 6. Checking that failed operations do not change data

- The `KeepsBalance` assertions and the `Repository.UpdateCount == 0` checks verify that a
  rejected deposit or withdrawal leaves the stored balance unchanged.
- This matters because `InMemoryAccountRepository` hands out the stored object itself, not a copy.
  If someone moves `account.Balance = newBalance` above the overdraft check, a failed withdrawal
  would still change the balance.
- After the cleanup, one integration test still checks this for checking accounts. Nothing checks
  it for savings accounts or for deposits.

In total, about 30 of the 169 deleted tests fall into one of these groups.

## Deletions I agree with

The remaining ~140 deleted tests were mostly repetition:

- extra parameter cases that run the same code path, such as `Deposit(1)` vs
  `Deposit(999999.99)`, or `UnknownId(-1)`, `UnknownId(0)` and `UnknownId(999)`
- eight different invalid JSON bodies that all end up in the same deserializer error
- the same "id is not an integer" check repeated on every endpoint
- `AccountResponseTests`, which tests a one-line copy method
- unit tests that repeat an integration test one-to-one, like the four exception handler mapping
  tests

## Takeaways

- **Mutation testing is a good way to find tests that check nothing**, and a poor way to decide
  which tests to delete. Use it to find what to look at, then decide test by test.
- **A mutation score is only as good as the mutators behind it.** Know what your tool does not
  mutate: numbers, attributes, configuration, framework behaviour.
- **Coverage, of any kind, measures today's code.** Some of the most valuable tests protect against
  code that has not been written yet.
- **Inject a few realistic bugs before you trust a trimmed suite.** Three small changes took a few
  minutes to try and showed what the numbers did not.
