using Microsoft.AspNetCore.Mvc;
using Api.Core.Entities;
using Api.Application.Interfaces;

namespace Api.Controllers;

[ApiController]
[Route("api/pictures")]
public class PictureController(IPictureService pictureService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPictures()
    {
        try
        {
            var pictures = await pictureService.GetPicturesAsync();
            return Ok(pictures);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Failed to fetch pictures" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> FindById(int id)
    {
        try
        {
            var picture = await pictureService.FindByIdAsync(id);
            if (picture == null)
            {
                return NotFound(new { error = "Picture not found" });
            }

            return Ok(picture);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("hierarchy/{id}")]
    public async Task<IActionResult> FindByHierarchyId(int id)
    {
        try
        {
            var pictures = await pictureService.FindByHierarchyIdAsync(id);
            return Ok(pictures);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Failed to fetch node pictures" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreatePicture([FromBody] Picture req)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { error = "Invalid request body" });
        }

        try
        {
            await pictureService.CreatePictureAsync(req);
            return StatusCode(201, req);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Failed to create picture" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePicture(int id, [FromBody] Picture req)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { error = "Invalid request body" });
        }

        try
        {
            var existing = await pictureService.FindByIdAsync(id);
            if (existing == null)
            {
                return NotFound(new { error = "Picture not found" });
            }

            req.Id = id;
            await pictureService.UpdatePictureAsync(req);
            return Ok(req);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Failed to update picture" });
        }
    }
}