using Api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Api.Core.Entities;

namespace Api.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController(ISettingsService settingsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        try
        {
            var settings = await settingsService.GetSettingsAsync();
            return Ok(settings);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Failed to fetch settings" });
        }
    }
    
    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] Settings req)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { error = "Invalid request body" });
        }

        try
        {
            await settingsService.UpdateSettingsAsync(req);
            return Ok(req);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "Failed to update settings" });
        }
    }
}