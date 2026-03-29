using BoardGamesLibrary.Application.Contracts;
using BoardGamesLibrary.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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
    public async Task<ActionResult<PagedResult<BoardGameResponse>>> List([FromQuery][Range(1, int.MaxValue)] int page = 1, [FromQuery][Range(1, 100)] int pageSize = 20, CancellationToken cancellationToken = default)
    {
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