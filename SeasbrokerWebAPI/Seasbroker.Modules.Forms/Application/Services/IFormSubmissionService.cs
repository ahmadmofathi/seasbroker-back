using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Seasbroker.Modules.Forms.Application.DTOs;

namespace Seasbroker.Modules.Forms.Application.Services;

public interface IFormSubmissionService
{
    Task<SubmitFormResponse> SubmitAsync(
        string formKey,
        Dictionary<string, JsonElement> rawValues,
        IFormFileCollection files,
        CancellationToken cancellationToken = default);

    /// <summary>Loads a customer's own request in the form it was registered with, if it can still be edited.</summary>
    Task<RequestEditFormDto> LoadForEditAsync(string? number, string? email, CancellationToken cancellationToken = default);

    /// <summary>Saves a customer's changes to their own request, if it can still be edited.</summary>
    Task UpdateAsync(
        string? number,
        string? email,
        Dictionary<string, JsonElement> rawValues,
        CancellationToken cancellationToken = default);
}
