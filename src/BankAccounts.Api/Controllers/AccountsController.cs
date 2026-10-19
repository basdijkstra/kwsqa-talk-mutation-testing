using BankAccounts.Api.Models;
using BankAccounts.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankAccounts.Api.Controllers;

[ApiController]
[Route("accounts")]
[Produces("application/json")]
public class AccountsController(IAccountService accountService) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<AccountResponse> GetAccount(int id) =>
        AccountResponse.From(accountService.GetAccount(id));

    [HttpPost]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<AccountResponse> CreateAccount(CreateAccountRequest request)
    {
        var account = accountService.CreateAccount(request.Type!.Value);
        return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, AccountResponse.From(account));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public IActionResult DeleteAccount(int id)
    {
        accountService.DeleteAccount(id);
        return NoContent();
    }

    [HttpPost("{id:int}/deposits")]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<AccountResponse> Deposit(int id, TransactionRequest request) =>
        AccountResponse.From(accountService.Deposit(id, request.Amount!.Value));

    [HttpPost("{id:int}/withdrawals")]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public ActionResult<AccountResponse> Withdraw(int id, TransactionRequest request) =>
        AccountResponse.From(accountService.Withdraw(id, request.Amount!.Value));

    [HttpPost("{id:int}/interest")]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public ActionResult<AccountResponse> AddInterest(int id) =>
        AccountResponse.From(accountService.AddInterest(id));
}
