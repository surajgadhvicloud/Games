using BoardGamesLibrary.Application.Contracts;
using BoardGamesLibrary.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGamesLibrary.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager,DataEntry")]
[Route("api/boardgames")]
public class BoardGamesController(IBoardGameService boardGameService) : ControllerBase
{
    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    public async Task<ActionResult<BoardGameResponse>> Create([FromBody] CreateBoardGameRequest request, CancellationToken cancellationToken)
    {
        var result = await boardGameService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<BoardGameResponse>> Update(int id, [FromBody] UpdateBoardGameRequest request, CancellationToken cancellationToken)
    {
        var result = await boardGameService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<BoardGameResponse>>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (page < 1) return BadRequest("Page must be greater than or equal to 1.");
        if (pageSize < 1 || pageSize > 100) return BadRequest("PageSize must be between 1 and 100.");
        var result = await boardGameService.ListAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BoardGameResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await boardGameService.GetAsync(id, cancellationToken);
        return Ok(result);
    }
}