using BoardGamesLibrary.Application.Contracts;
using BoardGamesLibrary.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGamesLibrary.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager,DataEntry")]
[Route("api/members")]
public class MembersController(IMemberService memberService) : ControllerBase
{
    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    public async Task<ActionResult<MemberResponse>> Create([FromBody] CreateMemberRequest request, CancellationToken cancellationToken)
    {
        var result = await memberService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<MemberResponse>> Update(int id, [FromBody] UpdateMemberRequest request, CancellationToken cancellationToken)
    {
        var result = await memberService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<MemberResponse>>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (page < 1) return BadRequest("Page must be greater than or equal to 1.");
        if (pageSize < 1 || pageSize > 100) return BadRequest("PageSize must be between 1 and 100.");
        var result = await memberService.ListAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MemberResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await memberService.GetAsync(id, cancellationToken);
        return Ok(result);
    }
}
