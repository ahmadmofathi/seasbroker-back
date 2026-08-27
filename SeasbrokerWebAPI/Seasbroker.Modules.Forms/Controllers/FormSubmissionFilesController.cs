using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Modules.Forms.Application.Constants;
using Seasbroker.Modules.Forms.Application.DTOs;
using Seasbroker.Modules.Forms.Application.Services;

namespace Seasbroker.Modules.Forms.Controllers;

/// <summary>Lets an authenticated admin download a file attached to a form submission.</summary>
[ApiController]
[Authorize(Policy = FormsConstants.SuperuserPolicy)]
[Tags("Forms")]
[Route("api/forms/submissions/{submissionId}/files/{fileId}")]
public class FormSubmissionFilesController : ControllerBase
{
    private readonly SeasbrokerDbContext _dbContext;
    private readonly IFileStorageService _fileStorage;

    public FormSubmissionFilesController(SeasbrokerDbContext dbContext, IFileStorageService fileStorage)
    {
        _dbContext = dbContext;
        _fileStorage = fileStorage;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PocketBaseErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid submissionId, Guid fileId, CancellationToken cancellationToken)
    {
        var file = await _dbContext.FormSubmissionFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == fileId && f.FormSubmissionId == submissionId, cancellationToken);

        if (file is null)
        {
            return NotFound(new PocketBaseErrorResponse { Message = "File not found.", Status = StatusCodes.Status404NotFound });
        }

        var stream = _fileStorage.OpenRead(file.StoragePath);
        return File(stream, string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType, file.FileName);
    }
}
