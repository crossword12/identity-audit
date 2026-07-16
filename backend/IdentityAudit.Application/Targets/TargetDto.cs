using IdentityAudit.Domain.Enums;

namespace IdentityAudit.Application.Targets;

public sealed record TargetDto(
    Guid Id,
    string Name,
    TargetType Type,
    bool IsEnabled,
    string? ConfigurationJson,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? LastCollectedAt);