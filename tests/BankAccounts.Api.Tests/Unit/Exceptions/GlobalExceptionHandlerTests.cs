using BankAccounts.Api.Exceptions;
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

    [TestCase(typeof(InvalidOperationException))]
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

    private sealed class RecordingProblemDetailsService : IProblemDetailsService
    {
        public ProblemDetailsContext? Written { get; private set; }

        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            Written = context;
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            Written = context;
            return ValueTask.FromResult(true);
        }
    }
}
