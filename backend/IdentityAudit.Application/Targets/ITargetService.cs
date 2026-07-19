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

    Task<UpdateTargetResult> UpdateAsync(
        Guid targetId,
        UpdateTargetRequest request,
        CancellationToken cancellationToken = default);

    Task<TestTargetConnectionResult> TestConnectionAsync(
        Guid targetId,
        CancellationToken cancellationToken = default);
}