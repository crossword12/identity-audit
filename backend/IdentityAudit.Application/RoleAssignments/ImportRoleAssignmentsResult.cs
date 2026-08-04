namespace IdentityAudit.Application.RoleAssignments;

public sealed class ImportRoleAssignmentsResult
{
    public bool Succeeded { get; init; }

    public string? ErrorMessage { get; init; }

    public IReadOnlyList<RoleAssignmentDto> Assignments
    {
        get;
        init;
    } = Array.Empty<RoleAssignmentDto>();

    public int ImportedCount => Assignments.Count;

    public static ImportRoleAssignmentsResult Success(
        IReadOnlyList<RoleAssignmentDto> assignments)
    {
        return new ImportRoleAssignmentsResult
        {
            Succeeded = true,
            Assignments = assignments
        };
    }

    public static ImportRoleAssignmentsResult Failure(
        string errorMessage)
    {
        return new ImportRoleAssignmentsResult
        {
            Succeeded = false,
            ErrorMessage = errorMessage
        };
    }
}