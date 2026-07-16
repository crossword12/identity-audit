namespace IdentityAudit.Application.Targets;

public interface ITargetService
{
    Task<IReadOnlyList<TargetDto>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<TargetDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<TargetDto> CreateAsync(
        CreateTargetRequest request,
        CancellationToken cancellationToken);
}