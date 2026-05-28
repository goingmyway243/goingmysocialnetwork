using GoingMy.Upload.Application.Commands;
using GoingMy.Upload.Application.Dtos;
using GoingMy.Upload.Application.Queries;
using GoingMy.Upload.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoingMy.Upload.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UploadsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(MediaFileDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [RequestSizeLimit(110 * 1024 * 1024)] // 110 MB max
    public async Task<ActionResult<MediaFileDto>> Upload(
        IFormFile file,
        [FromForm] MediaPurpose purpose = MediaPurpose.PostMedia,
        CancellationToken ct = default)
    {
        if (file.Length == 0)
            return BadRequest("File is empty.");

        var userId = User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User ID claim not found.");

        var command = new UploadFileCommand(
            FileStream: file.OpenReadStream(),
            FileName: file.FileName,
            ContentType: file.ContentType,
            FileSizeBytes: file.Length,
            UserId: userId,
            Purpose: purpose);

        try
        {
            var result = await mediator.Send(command, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("batch")]
    [ProducesResponseType(typeof(IReadOnlyList<MediaFileDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [RequestSizeLimit(440 * 1024 * 1024)] // 4 × 110 MB
    public async Task<ActionResult<IReadOnlyList<MediaFileDto>>> UploadBatch(
        IFormFileCollection files,
        [FromForm] MediaPurpose purpose = MediaPurpose.PostMedia,
        CancellationToken ct = default)
    {
        if (files.Count == 0)
            return BadRequest("No files provided.");

        if (files.Count > 4)
            return BadRequest("Cannot upload more than 4 files at once.");

        var userId = User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User ID claim not found.");

        var results = new List<MediaFileDto>();
        foreach (var file in files)
        {
            if (file.Length == 0) continue;

            var command = new UploadFileCommand(
                FileStream: file.OpenReadStream(),
                FileName: file.FileName,
                ContentType: file.ContentType,
                FileSizeBytes: file.Length,
                UserId: userId,
                Purpose: purpose);

            try
            {
                results.Add(await mediator.Send(command, ct));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        return StatusCode(StatusCodes.Status201Created, results);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MediaFileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaFileDto>> GetById(string id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetMediaFileQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("by-filekey/{fileKey}")]
    [ProducesResponseType(typeof(MediaFileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaFileDto>> GetByFileKey(string fileKey, CancellationToken ct)
    {
        var result = await mediator.Send(new GetMediaFileByKeyQuery(fileKey), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var userId = User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("User ID claim not found.");

        try
        {
            await mediator.Send(new DeleteFileCommand(id, userId), ct);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
