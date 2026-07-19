namespace IdentityAudit.Application.Targets;

public sealed class UpdateTargetResult
{
    public bool Succeeded { get; init; }

    public string? ErrorMessage { get; init; }

    public TargetDto? Target { get; init; }

    public static UpdateTargetResult Success(TargetDto target)
    {
        return new UpdateTargetResult
        {
            Succeeded = true,
            Target = target
        };
    }

    public static UpdateTargetResult Failure(string errorMessage)
    {
        return new UpdateTargetResult
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };
    }
}