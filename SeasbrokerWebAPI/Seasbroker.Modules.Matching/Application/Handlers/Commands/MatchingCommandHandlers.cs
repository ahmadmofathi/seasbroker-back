using FluentValidation;
using Seasbroker.Modules.Matching.Application.Abstractions;
using Seasbroker.Modules.Matching.Application.Commands;
using Seasbroker.Modules.Matching.Application.DTOs;
using Seasbroker.Modules.Matching.Application.Helpers;
using Seasbroker.Modules.Matching.Application.Services;
using Seasbroker.Modules.Matching.Application.Validators;

namespace Seasbroker.Modules.Matching.Application.Handlers.Commands;

public class RunMatchingCommandHandler : ICommandHandler<RunMatchingCommand, MatchingRunResultDto>
{
    private readonly IMatchingEngineService _matchingEngineService;
    private readonly IValidator<RunMatchingCommand> _validator;

    public RunMatchingCommandHandler(
        IMatchingEngineService matchingEngineService,
        IValidator<RunMatchingCommand> validator)
    {
        _matchingEngineService = matchingEngineService;
        _validator = validator;
    }

    public async Task<MatchingRunResultDto> HandleAsync(
        RunMatchingCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateCommandAsync(command, cancellationToken);

        if (!string.IsNullOrWhiteSpace(command.CargoListingId))
        {
            var cargoId = MatchingDomainHelper.ParseGuidOrNotFound(command.CargoListingId, "cargo listing");
            return await _matchingEngineService.RunForCargoAsync(cargoId, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(command.VesselId))
        {
            var vesselId = MatchingDomainHelper.ParseGuidOrNotFound(command.VesselId, "vessel");
            return await _matchingEngineService.RunForVesselAsync(vesselId, cancellationToken);
        }

        return await _matchingEngineService.RunBatchAsync(cancellationToken);
    }
}

public class CreateManualMatchCommandHandler : ICommandHandler<CreateManualMatchCommand, MatchRecordDto>
{
    private readonly IMatchService _matchService;
    private readonly IValidator<CreateManualMatchCommand> _validator;

    public CreateManualMatchCommandHandler(
        IMatchService matchService,
        IValidator<CreateManualMatchCommand> validator)
    {
        _matchService = matchService;
        _validator = validator;
    }

    public async Task<MatchRecordDto> HandleAsync(
        CreateManualMatchCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateCommandAsync(command, cancellationToken);

        return await _matchService.CreateManualAsync(
            command.CargoListingId,
            command.VesselId,
            command.Score,
            command.MatchReason,
            cancellationToken);
    }
}

public class ExpireMatchCommandHandler : ICommandHandler<ExpireMatchCommand, MatchRecordDto>
{
    private readonly IMatchService _matchService;
    private readonly IValidator<ExpireMatchCommand> _validator;

    public ExpireMatchCommandHandler(
        IMatchService matchService,
        IValidator<ExpireMatchCommand> validator)
    {
        _matchService = matchService;
        _validator = validator;
    }

    public async Task<MatchRecordDto> HandleAsync(
        ExpireMatchCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateCommandAsync(command, cancellationToken);
        return await _matchService.ExpireAsync(command.MatchId, cancellationToken);
    }
}

public class CancelMatchCommandHandler : ICommandHandler<CancelMatchCommand, MatchRecordDto>
{
    private readonly IMatchService _matchService;
    private readonly IValidator<CancelMatchCommand> _validator;

    public CancelMatchCommandHandler(
        IMatchService matchService,
        IValidator<CancelMatchCommand> validator)
    {
        _matchService = matchService;
        _validator = validator;
    }

    public async Task<MatchRecordDto> HandleAsync(
        CancelMatchCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateCommandAsync(command, cancellationToken);
        return await _matchService.CancelAsync(command.MatchId, cancellationToken);
    }
}

public class UpdateMatchingRuleCommandHandler : ICommandHandler<UpdateMatchingRuleCommand, MatchingRuleRecordDto>
{
    private readonly IMatchingRuleService _matchingRuleService;
    private readonly IValidator<UpdateMatchingRuleCommand> _validator;

    public UpdateMatchingRuleCommandHandler(
        IMatchingRuleService matchingRuleService,
        IValidator<UpdateMatchingRuleCommand> validator)
    {
        _matchingRuleService = matchingRuleService;
        _validator = validator;
    }

    public async Task<MatchingRuleRecordDto> HandleAsync(
        UpdateMatchingRuleCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateCommandAsync(command, cancellationToken);

        return await _matchingRuleService.UpdateAsync(
            command.RuleId,
            command.Weight,
            command.IsActive,
            command.Configuration,
            cancellationToken);
    }
}
