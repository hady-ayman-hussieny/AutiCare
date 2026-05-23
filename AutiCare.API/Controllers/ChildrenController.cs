using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutiCare.API.Controllers;

[Route("api/children")]
[Authorize(Roles = "Parent")]
public class ChildrenController : BaseController
{
    private readonly IChildService _childService;
    public ChildrenController(IChildService childService) => _childService = childService;

    [HttpGet]
    public async Task<IActionResult> GetMyChildren()
    {
        var children = await _childService.GetMyChildrenAsync(GetUserId());
        return Ok(children);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetChild(int id)
    {
        var child = await _childService.GetByIdAsync(id,GetUserId());
        return child == null ? NotFound() : Ok(child);
    }

    [HttpPost]
    public async Task<IActionResult> AddChild(CreateChildRequest request)
    {
        var child = await _childService.CreateAsync(GetUserId(), request);
        return CreatedAtAction(nameof(GetChild), new { id = child.ChildId }, child);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateChild(int id, UpdateChildRequest request)
    {
        var child = await _childService.UpdateAsync(id, GetUserId(), request);
        return Ok(child);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteChild(int id)
    {
        await _childService.DeleteAsync(id, GetUserId());
        return NoContent();
    }
}
