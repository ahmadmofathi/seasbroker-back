using Seasbroker.Modules.Matching.Application.Abstractions;
using Seasbroker.Modules.Matching.Application.Commands;
using Seasbroker.Modules.Matching.Application.DTOs;
using Seasbroker.Modules.Matching.Application.Queries;

namespace Seasbroker.Modules.Matching.Application.Services;

public interface IMatchingRunService
{
    Task<MatchingRunResultDto> RunAsync(
        RunMatchingCommand command,
        CancellationToken cancellationToken = default);
}

public interface IMatchLifecycleService
{
    Task<MatchRecordDto> CreateManualAsync(
        CreateManualMatchCommand command,
        CancellationToken cancellationToken = default);

    Task<MatchRecordDto> ExpireAsync(
        ExpireMatchCommand command,
        CancellationToken cancellationToken = default);

    Task<MatchRecordDto> CancelAsync(
        CancelMatchCommand command,
        CancellationToken cancellationToken = default);
}

public interface IMatchRecordsService
{
    Task<PocketBaseListResponse<MatchRecordDto>> GetAllAsync(
        GetMatchesQuery query,
        CancellationToken cancellationToken = default);

    Task<MatchRecordDto> GetByIdAsync(
        GetMatchByIdQuery query,
        CancellationToken cancellationToken = default);
}

public interface IMatchingRuleRecordsService
{
    Task<IReadOnlyList<MatchingRuleRecordDto>> GetAllAsync(
        GetMatchingRulesQuery query,
        CancellationToken cancellationToken = default);

    Task<MatchingRuleRecordDto> UpdateAsync(
        UpdateMatchingRuleCommand command,
        CancellationToken cancellationToken = default);
}

public class MatchingRunService : IMatchingRunService
{
    private readonly ICommandHandler<RunMatchingCommand, MatchingRunResultDto> _runHandler;

    public MatchingRunService(ICommandHandler<RunMatchingCommand, MatchingRunResultDto> runHandler)
    {
        _runHandler = runHandler;
    }

    public Task<MatchingRunResultDto> RunAsync(
        RunMatchingCommand command,
        CancellationToken cancellationToken = default) =>
        _runHandler.HandleAsync(command, cancellationToken);
}

public class MatchLifecycleService : IMatchLifecycleService
{
    private readonly ICommandHandler<CreateManualMatchCommand, MatchRecordDto> _createManualHandler;
    private readonly ICommandHandler<ExpireMatchCommand, MatchRecordDto> _expireHandler;
    private readonly ICommandHandler<CancelMatchCommand, MatchRecordDto> _cancelHandler;

    public MatchLifecycleService(
        ICommandHandler<CreateManualMatchCommand, MatchRecordDto> createManualHandler,
        ICommandHandler<ExpireMatchCommand, MatchRecordDto> expireHandler,
        ICommandHandler<CancelMatchCommand, MatchRecordDto> cancelHandler)
    {
        _createManualHandler = createManualHandler;
        _expireHandler = expireHandler;
        _cancelHandler = cancelHandler;
    }

    public Task<MatchRecordDto> CreateManualAsync(
        CreateManualMatchCommand command,
        CancellationToken cancellationToken = default) =>
        _createManualHandler.HandleAsync(command, cancellationToken);

    public Task<MatchRecordDto> ExpireAsync(
        ExpireMatchCommand command,
        CancellationToken cancellationToken = default) =>
        _expireHandler.HandleAsync(command, cancellationToken);

    public Task<MatchRecordDto> CancelAsync(
        CancelMatchCommand command,
        CancellationToken cancellationToken = default) =>
        _cancelHandler.HandleAsync(command, cancellationToken);
}

public class MatchRecordsService : IMatchRecordsService
{
    private readonly IQueryHandler<GetMatchesQuery, PocketBaseListResponse<MatchRecordDto>> _getAllHandler;
    private readonly IQueryHandler<GetMatchByIdQuery, MatchRecordDto> _getByIdHandler;

    public MatchRecordsService(
        IQueryHandler<GetMatchesQuery, PocketBaseListResponse<MatchRecordDto>> getAllHandler,
        IQueryHandler<GetMatchByIdQuery, MatchRecordDto> getByIdHandler)
    {
        _getAllHandler = getAllHandler;
        _getByIdHandler = getByIdHandler;
    }

    public Task<PocketBaseListResponse<MatchRecordDto>> GetAllAsync(
        GetMatchesQuery query,
        CancellationToken cancellationToken = default) =>
        _getAllHandler.HandleAsync(query, cancellationToken);

    public Task<MatchRecordDto> GetByIdAsync(
        GetMatchByIdQuery query,
        CancellationToken cancellationToken = default) =>
        _getByIdHandler.HandleAsync(query, cancellationToken);
}

public class MatchingRuleRecordsService : IMatchingRuleRecordsService
{
    private readonly IQueryHandler<GetMatchingRulesQuery, IReadOnlyList<MatchingRuleRecordDto>> _getAllHandler;
    private readonly ICommandHandler<UpdateMatchingRuleCommand, MatchingRuleRecordDto> _updateHandler;

    public MatchingRuleRecordsService(
        IQueryHandler<GetMatchingRulesQuery, IReadOnlyList<MatchingRuleRecordDto>> getAllHandler,
        ICommandHandler<UpdateMatchingRuleCommand, MatchingRuleRecordDto> updateHandler)
    {
        _getAllHandler = getAllHandler;
        _updateHandler = updateHandler;
    }

    public Task<IReadOnlyList<MatchingRuleRecordDto>> GetAllAsync(
        GetMatchingRulesQuery query,
        CancellationToken cancellationToken = default) =>
        _getAllHandler.HandleAsync(query, cancellationToken);

    public Task<MatchingRuleRecordDto> UpdateAsync(
        UpdateMatchingRuleCommand command,
        CancellationToken cancellationToken = default) =>
        _updateHandler.HandleAsync(command, cancellationToken);
}
