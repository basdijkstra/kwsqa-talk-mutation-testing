using BankAccounts.Api.Exceptions;
using BankAccounts.Api.Models;
using Microsoft.AspNetCore.Http;

namespace BankAccounts.Api.Tests.Unit.Exceptions;

[TestFixture]
public class GlobalExceptionHandlerTests
{
    private RecordingProblemDetailsService _problemDetailsService = null!;
    private GlobalExceptionHandler _handler = null!;
    private DefaultHttpContext _httpContext = null!;

    [SetUp]
    public void CreateHandler()
    {
        _problemDetailsService = new RecordingProblemDetailsService();
        _handler = new GlobalExceptionHandler(_problemDetailsService);
        _httpContext = new DefaultHttpContext();
    }

    private static IEnumerable<TestCaseData> DomainExceptions()
    {
        yield return new TestCaseData(new AccountNotFoundException(1), 404, "Account not found")
            .SetName("AccountNotFound maps to 404");
        yield return new TestCaseData(new InvalidAmountException(0m), 400, "Invalid amount")
            .SetName("InvalidAmount maps to 400");
        yield return new TestCaseData(new InsufficientFundsException(1, 0m, 1m), 422, "Insufficient funds")
            .SetName("InsufficientFunds maps to 422");
        yield return new TestCaseData(new InterestNotAllowedException(1, AccountType.Checking), 422, "Interest not allowed")
            .SetName("InterestNotAllowed maps to 422");
    }

    [TestCaseSource(nameof(DomainExceptions))]
    public async Task DomainException_IsWrittenAsProblemDetails(Exception exception, int expectedStatus, string expectedTitle)
    {
        var handled = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        var written = _problemDetailsService.Written!;
        Assert.Multiple(() =>
        {
            Assert.That(handled, Is.True);
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(expectedStatus));
            Assert.That(written.HttpContext, Is.SameAs(_httpContext));
            Assert.That(written.Exception, Is.SameAs(exception));
            Assert.That(written.ProblemDetails.Status, Is.EqualTo(expectedStatus));
            Assert.That(written.ProblemDetails.Title, Is.EqualTo(expectedTitle));
            Assert.That(written.ProblemDetails.Detail, Is.EqualTo(exception.Message));
        });
    }

    [TestCase(typeof(InvalidOperationException))]
    [TestCase(typeof(OverflowException))]
    [TestCase(typeof(ArgumentOutOfRangeException))]
    public async Task OtherException_IsNotHandled(Type exceptionType)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType)!;

        var handled = await _handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(handled, Is.False);
            Assert.That(_problemDetailsService.Written, Is.Null);
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        });
    }

    [Test]
    public async Task DomainException_ProblemDetailsServiceFails_ReportsNotHandled()
    {
        _problemDetailsService.Succeeds = false;

        var handled = await _handler.TryHandleAsync(_httpContext, new AccountNotFoundException(1), CancellationToken.None);

        Assert.That(handled, Is.False);
    }

    private sealed class RecordingProblemDetailsService : IProblemDetailsService
    {
        public bool Succeeds { get; set; } = true;

        public ProblemDetailsContext? Written { get; private set; }

        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            Written = context;
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            Written = context;
            return ValueTask.FromResult(Succeeds);
        }
    }
}
