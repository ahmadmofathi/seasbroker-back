using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Quote.Application.DTOs;
using System.Text.Json.Serialization;

namespace SeasbrokerWebAPI.Controllers;

public class SettingRecordDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public string Created { get; set; } = string.Empty;

    [JsonPropertyName("updated")]
    public string Updated { get; set; } = string.Empty;

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public class CreateSettingRequest
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

public class UpdateSettingRequest
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}

[ApiController]
[Route("api/collections/settings/records")]
public class SettingsRecordsController : ControllerBase
{
    private readonly SeasbrokerDbContext _dbContext;

    public SettingsRecordsController(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PocketBaseListResponse<SettingRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] string? filter,
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SystemSettings.AsNoTracking();

        var keyFilter = TryParseKeyFilter(filter);
        if (!string.IsNullOrEmpty(keyFilter))
        {
            query = query.Where(s => s.Key == keyFilter);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        
        var pageIndex = page < 1 ? 1 : page;
        var pageSize = perPage < 1 ? 50 : perPage;

        var items = await query
            .OrderBy(s => s.Key)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SettingRecordDto
            {
                Id = s.Id.ToString(),
                Created = s.Created.ToString("o"),
                Updated = s.Updated.ToString("o"),
                Key = s.Key,
                Value = s.Value
            })
            .ToListAsync(cancellationToken);

        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

        return Ok(new PocketBaseListResponse<SettingRecordDto>
        {
            Page = pageIndex,
            PerPage = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = items
        });
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SettingRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> View(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var guidId))
        {
            return NotFound(new PocketBaseErrorResponse { Message = "Setting not found", Status = 404 });
        }

        var setting = await _dbContext.SystemSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == guidId, cancellationToken);

        if (setting is null)
        {
            return NotFound(new PocketBaseErrorResponse { Message = "Setting not found", Status = 404 });
        }

        return Ok(new SettingRecordDto
        {
            Id = setting.Id.ToString(),
            Created = setting.Created.ToString("o"),
            Updated = setting.Updated.ToString("o"),
            Key = setting.Key,
            Value = setting.Value
        });
    }

    [HttpPost]
    [Authorize(Policy = "Superuser")]
    [ProducesResponseType(typeof(SettingRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSettingRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Key))
        {
            return BadRequest(new PocketBaseErrorResponse { Message = "Key is required", Status = 400 });
        }

        var exists = await _dbContext.SystemSettings.AnyAsync(s => s.Key == request.Key, cancellationToken);
        if (exists)
        {
            return BadRequest(new PocketBaseErrorResponse { Message = $"Setting with key '{request.Key}' already exists", Status = 400 });
        }

        var setting = new SystemSetting
        {
            Key = request.Key,
            Value = request.Value
        };

        _dbContext.SystemSettings.Add(setting);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new SettingRecordDto
        {
            Id = setting.Id.ToString(),
            Created = setting.Created.ToString("o"),
            Updated = setting.Updated.ToString("o"),
            Key = setting.Key,
            Value = setting.Value
        });
    }

    [HttpPatch("{id}")]
    [Authorize(Policy = "Superuser")]
    [ProducesResponseType(typeof(SettingRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateSettingRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var guidId))
        {
            return NotFound(new PocketBaseErrorResponse { Message = "Setting not found", Status = 404 });
        }

        var setting = await _dbContext.SystemSettings.FirstOrDefaultAsync(s => s.Id == guidId, cancellationToken);
        if (setting is null)
        {
            return NotFound(new PocketBaseErrorResponse { Message = "Setting not found", Status = 404 });
        }

        setting.Value = request.Value;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new SettingRecordDto
        {
            Id = setting.Id.ToString(),
            Created = setting.Created.ToString("o"),
            Updated = setting.Updated.ToString("o"),
            Key = setting.Key,
            Value = setting.Value
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Superuser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var guidId))
        {
            return NotFound(new PocketBaseErrorResponse { Message = "Setting not found", Status = 404 });
        }

        var setting = await _dbContext.SystemSettings.FirstOrDefaultAsync(s => s.Id == guidId, cancellationToken);
        if (setting is null)
        {
            return NotFound(new PocketBaseErrorResponse { Message = "Setting not found", Status = 404 });
        }

        _dbContext.SystemSettings.Remove(setting);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static string? TryParseKeyFilter(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return null;
        }

        const string prefix = "key = ";
        var trimmed = filter.Trim();

        if (!trimmed.StartsWith(prefix, StringComparison.Ordinal))
        {
            return null;
        }

        return trimmed[prefix.Length..].Trim().Trim('"').Trim('\'');
    }
}
