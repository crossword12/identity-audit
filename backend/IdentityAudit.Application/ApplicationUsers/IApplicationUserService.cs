namespace IdentityAudit.Application.ApplicationUsers;

public interface IApplicationUserService
{
    Task<IReadOnlyCollection<ApplicationUserDto>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<ApplicationUserDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<ApplicationUserOperationResult> CreateAsync(
        CreateApplicationUserRequest request,
        CancellationToken cancellationToken);

    Task<ApplicationUserOperationResult> UpdateAsync(
        Guid id,
        Guid currentUserId,
        UpdateApplicationUserRequest request,
        CancellationToken cancellationToken);

    Task<ApplicationUserOperationResult> UpdateRolesAsync(
        Guid id,
        Guid currentUserId,
        UpdateApplicationUserRolesRequest request,
        CancellationToken cancellationToken);
}

public enum ApplicationUserErrorType
{
    None,
    NotFound,
    Conflict,
    Validation,
    Forbidden
}

public sealed record ApplicationUserOperationResult(
    bool Succeeded,
    ApplicationUserDto? User,
    ApplicationUserErrorType ErrorType,
    string? ErrorMessage)
{
    public static ApplicationUserOperationResult Success(
        ApplicationUserDto user)
    {
        return new ApplicationUserOperationResult(
            true,
            user,
            ApplicationUserErrorType.None,
            null);
    }

    public static ApplicationUserOperationResult Failure(
        ApplicationUserErrorType errorType,
        string errorMessage)
    {
        return new ApplicationUserOperationResult(
            false,
            null,
            errorType,
            errorMessage);
    }
}