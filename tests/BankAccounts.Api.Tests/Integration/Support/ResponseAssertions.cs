using RestAssured.Response;

namespace BankAccounts.Api.Tests.Integration.Support;

/// <summary>Account as it appears on the wire. Type is a string to check the exact JSON value.</summary>
public record AccountJson(int Id, string Type, decimal Balance);

/// <summary>RFC 9457 problem details as returned by the API.</summary>
public record ProblemJson(int Status, string Title, string? Detail, Dictionary<string, string[]>? Errors);

/// <summary>
/// Checks that are the same for many tests: status code, content type and body shape.
/// NHamcrest matchers are fully qualified because NUnit has classes with the same names.
/// </summary>
public static class ResponseAssertions
{
    public static AccountJson ShouldReturnAccount(this VerifiableResponse response, int expectedStatus = 200) =>
        response
            .StatusCode(expectedStatus)
            .ContentType(NHamcrest.Contains.String("application/json"))
            .DeserializeTo<AccountJson>()!;

    /// <summary>For errors raised by the domain: they always carry a detail message.</summary>
    public static ProblemJson ShouldReturnProblem(this VerifiableResponse response, int expectedStatus, string expectedTitle, string expectedDetail)
    {
        var problem = response.ShouldReturnProblemWithStatus(expectedStatus);

        Assert.Multiple(() =>
        {
            Assert.That(problem.Title, Is.EqualTo(expectedTitle));
            Assert.That(problem.Detail, Is.EqualTo(expectedDetail));
        });

        return problem;
    }

    /// <summary>
    /// For request validation failures caught by ASP.NET before the domain is reached.
    /// These come back as application/json, not application/problem+json, because the
    /// controller declares [Produces("application/json")].
    /// </summary>
    public static ProblemJson ShouldReturnValidationProblem(this VerifiableResponse response)
    {
        var problem = response.ShouldReturnProblemWithStatus(400, "application/json");

        Assert.Multiple(() =>
        {
            Assert.That(problem.Title, Is.EqualTo("One or more validation errors occurred."));
            Assert.That(problem.Errors, Is.Not.Null.And.Not.Empty);
        });

        return problem;
    }

    public static void ShouldReturnEmptyResponse(this VerifiableResponse response, int expectedStatus) =>
        response
            .StatusCode(expectedStatus)
            .ResponseBodyLength(NHamcrest.Is.EqualTo(0));

    private static ProblemJson ShouldReturnProblemWithStatus(
        this VerifiableResponse response, int expectedStatus, string expectedMediaType = "application/problem+json")
    {
        var problem = response
            .StatusCode(expectedStatus)
            .ContentType(NHamcrest.Contains.String(expectedMediaType))
            .DeserializeTo<ProblemJson>()!;

        Assert.That(problem.Status, Is.EqualTo(expectedStatus));

        return problem;
    }
}
