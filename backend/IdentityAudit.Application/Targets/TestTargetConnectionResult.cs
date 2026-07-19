namespace IdentityAudit.Application.Targets;

public sealed class TestTargetConnectionResult
{
    public bool TargetFound { get; init; }

    public bool Succeeded { get; init; }

    public string Message { get; init; } = string.Empty;

    public DateTimeOffset TestedAt { get; init; }

    public static TestTargetConnectionResult Success(
        string message)
    {
        return new TestTargetConnectionResult
        {
            TargetFound = true,
            Succeeded = true,
            Message = message,
            TestedAt = DateTimeOffset.UtcNow
        };
    }

    public static TestTargetConnectionResult Failure(
        string message)
    {
        return new TestTargetConnectionResult
        {
            TargetFound = true,
            Succeeded = false,
            Message = message,
            TestedAt = DateTimeOffset.UtcNow
        };
    }

    public static TestTargetConnectionResult NotFound(
        string message)
    {
        return new TestTargetConnectionResult
        {
            TargetFound = false,
            Succeeded = false,
            Message = message,
            TestedAt = DateTimeOffset.UtcNow
        };
    }
}