using FluentValidation;
using Seasbroker.Modules.Approval.Application.Abstractions;
using Seasbroker.Modules.Approval.Application.Commands;
using Seasbroker.Modules.Approval.Application.DTOs;
using Seasbroker.Modules.Approval.Application.Helpers;
using Seasbroker.Modules.Approval.Application.Services;
using Seasbroker.Modules.Approval.Application.Validators;

namespace Seasbroker.Modules.Approval.Application.Handlers.Commands;

public class ApproveMatchCommandHandler : ICommandHandler<ApproveMatchCommand, MatchApprovalRecordDto>
{
    private readonly IMatchApprovalService _matchApprovalService;
    private readonly IValidator<ApproveMatchCommand> _validator;

    public ApproveMatchCommandHandler(
        IMatchApprovalService matchApprovalService,
        IValidator<ApproveMatchCommand> validator)
    {
        _matchApprovalService = matchApprovalService;
        _validator = validator;
    }

    public async Task<MatchApprovalRecordDto> HandleAsync(
        ApproveMatchCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateCommandAsync(command, cancellationToken);
        return await _matchApprovalService.ApproveAsync(command, cancellationToken);
    }
}

public class RejectMatchCommandHandler : ICommandHandler<RejectMatchCommand, MatchApprovalRecordDto>
{
    private readonly IMatchApprovalService _matchApprovalService;
    private readonly IValidator<RejectMatchCommand> _validator;

    public RejectMatchCommandHandler(
        IMatchApprovalService matchApprovalService,
        IValidator<RejectMatchCommand> validator)
    {
        _matchApprovalService = matchApprovalService;
        _validator = validator;
    }

    public async Task<MatchApprovalRecordDto> HandleAsync(
        RejectMatchCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateCommandAsync(command, cancellationToken);
        return await _matchApprovalService.RejectAsync(command, cancellationToken);
    }
}

public class CancelMatchCommandHandler : ICommandHandler<CancelMatchCommand, MatchApprovalRecordDto>
{
    private readonly IMatchApprovalService _matchApprovalService;
    private readonly IValidator<CancelMatchCommand> _validator;

    public CancelMatchCommandHandler(
        IMatchApprovalService matchApprovalService,
        IValidator<CancelMatchCommand> validator)
    {
        _matchApprovalService = matchApprovalService;
        _validator = validator;
    }

    public async Task<MatchApprovalRecordDto> HandleAsync(
        CancelMatchCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateCommandAsync(command, cancellationToken);
        return await _matchApprovalService.CancelApprovedAsync(command, cancellationToken);
    }
}

public class CompleteMatchCommandHandler : ICommandHandler<CompleteMatchCommand, MatchApprovalRecordDto>
{
    private readonly IMatchApprovalService _matchApprovalService;
    private readonly IValidator<CompleteMatchCommand> _validator;

    public CompleteMatchCommandHandler(
        IMatchApprovalService matchApprovalService,
        IValidator<CompleteMatchCommand> validator)
    {
        _matchApprovalService = matchApprovalService;
        _validator = validator;
    }

    public async Task<MatchApprovalRecordDto> HandleAsync(
        CompleteMatchCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateCommandAsync(command, cancellationToken);
        return await _matchApprovalService.CompleteAsync(command, cancellationToken);
    }
}
