using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Quote.Application.DTOs;
using System.Text.Json.Serialization;

namespace SeasbrokerWebAPI.Controllers;

public class FaqRecordDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public string Created { get; set; } = string.Empty;

    [JsonPropertyName("updated")]
    public string Updated { get; set; } = string.Empty;

    [JsonPropertyName("heading")]
    public string Heading { get; set; } = string.Empty;

    [JsonPropertyName("para")]
    public string Para { get; set; } = string.Empty;

    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; }
}

public class CreateFaqRequest
{
    [JsonPropertyName("heading")]
    public string Heading { get; set; } = string.Empty;

    [JsonPropertyName("para")]
    public string Para { get; set; } = string.Empty;

    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; }
}

public class UpdateFaqRequest
{
    [JsonPropertyName("heading")]
    public string Heading { get; set; } = string.Empty;

    [JsonPropertyName("para")]
    public string Para { get; set; } = string.Empty;

    [JsonPropertyName("sortOrder")]
    public int SortOrder { get; set; }
}

[ApiController]
[Route("api/collections/faqs/records")]
public class FaqsRecordsController : ControllerBase
{
    private readonly SeasbrokerDbContext _dbContext;

    public FaqsRecordsController(SeasbrokerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PocketBaseListResponse<FaqRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Faqs.AsNoTracking();

        var totalItems = await query.CountAsync(cancellationToken);
        
        var pageIndex = page < 1 ? 1 : page;
        var pageSize = perPage < 1 ? 50 : perPage;

        var items = await query
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.Heading)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FaqRecordDto
            {
                Id = f.Id.ToString(),
                Created = f.Created.ToString("o"),
                Updated = f.Updated.ToString("o"),
                Heading = f.Heading,
                Para = f.Para,
                SortOrder = f.SortOrder
            })
            .ToListAsync(cancellationToken);

        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

        return Ok(new PocketBaseListResponse<FaqRecordDto>
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
    [ProducesResponseType(typeof(FaqRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> View(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var guidId))
        {
            return NotFound(new PocketBaseErrorResponse { Message = "FAQ not found", Status = 404 });
        }

        var faq = await _dbContext.Faqs
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == guidId, cancellationToken);

        if (faq is null)
        {
            return NotFound(new PocketBaseErrorResponse { Message = "FAQ not found", Status = 404 });
        }

        return Ok(new FaqRecordDto
        {
            Id = faq.Id.ToString(),
            Created = faq.Created.ToString("o"),
            Updated = faq.Updated.ToString("o"),
            Heading = faq.Heading,
            Para = faq.Para,
            SortOrder = faq.SortOrder
        });
    }

    [HttpPost]
    [Authorize(Policy = "Superuser")]
    [ProducesResponseType(typeof(FaqRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateFaqRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Heading))
        {
            return BadRequest(new PocketBaseErrorResponse { Message = "Heading is required", Status = 400 });
        }

        var faq = new Faq
        {
            Heading = request.Heading,
            Para = request.Para,
            SortOrder = request.SortOrder
        };

        _dbContext.Faqs.Add(faq);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new FaqRecordDto
        {
            Id = faq.Id.ToString(),
            Created = faq.Created.ToString("o"),
            Updated = faq.Updated.ToString("o"),
            Heading = faq.Heading,
            Para = faq.Para,
            SortOrder = faq.SortOrder
        });
    }

    [HttpPatch("{id}")]
    [Authorize(Policy = "Superuser")]
    [ProducesResponseType(typeof(FaqRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateFaqRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var guidId))
        {
            return NotFound(new PocketBaseErrorResponse { Message = "FAQ not found", Status = 404 });
        }

        var faq = await _dbContext.Faqs.FirstOrDefaultAsync(f => f.Id == guidId, cancellationToken);
        if (faq is null)
        {
            return NotFound(new PocketBaseErrorResponse { Message = "FAQ not found", Status = 404 });
        }

        faq.Heading = request.Heading;
        faq.Para = request.Para;
        faq.SortOrder = request.SortOrder;
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new FaqRecordDto
        {
            Id = faq.Id.ToString(),
            Created = faq.Created.ToString("o"),
            Updated = faq.Updated.ToString("o"),
            Heading = faq.Heading,
            Para = faq.Para,
            SortOrder = faq.SortOrder
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
            return NotFound(new PocketBaseErrorResponse { Message = "FAQ not found", Status = 404 });
        }

        var faq = await _dbContext.Faqs.FirstOrDefaultAsync(f => f.Id == guidId, cancellationToken);
        if (faq is null)
        {
            return NotFound(new PocketBaseErrorResponse { Message = "FAQ not found", Status = 404 });
        }

        _dbContext.Faqs.Remove(faq);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
