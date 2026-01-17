using Microsoft.AspNetCore.Mvc;
using Api.Application.DTOs;
using Api.Application.Interfaces;

namespace Api.Controllers;

[ApiController]
[Route("api/hierarchy")]
public class HierarchyController : ControllerBase
{
    private readonly IHierarchyService _hierarchyService;
    private readonly IPictureService _pictureService;
    
    public HierarchyController(IHierarchyService hierarchyService, IPictureService pictureService)
    {
        _hierarchyService = hierarchyService;
        _pictureService = pictureService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateNode([FromBody] CreateNodeRequest req)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { error = "Invalid request body" });
        }

        try
        {
            var node = await _hierarchyService.CreateNodeAsync(req);
            return CreatedAtAction(nameof(GetHierarchy), new { id = node.Id }, node);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Failed to create node" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetHierarchy()
    {
        try
        {
            var tree = await _hierarchyService.GetFullHierarchyAsync();
            return Ok(tree);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Failed to build hierarchy" });
        }
    }

    [HttpGet("{id}/grouped-pictures")]
    public async Task<IActionResult> GetGroupedPictures(int id)
    {
        try
        {
            var groupedPictures = await _pictureService.GroupSimilarPicturesAsync(id, 8);
            return Ok(groupedPictures);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Failed to group pictures" });
        }
    }
}