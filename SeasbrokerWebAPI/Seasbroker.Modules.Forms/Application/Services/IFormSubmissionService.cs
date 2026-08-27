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
}
