using BoardGamesLibrary.Application.Contracts;
using BoardGamesLibrary.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BoardGamesLibrary.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager,DataEntry")]
[Route("api/gameissues")]
public class GameIssuesController(IGameIssueService gameIssueService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<GameIssueResponse>> Create([FromBody] CreateGameIssueRequest request, CancellationToken cancellationToken)
    {
        var result = await gameIssueService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<GameIssueResponse>> Update(int id, [FromBody] UpdateGameIssueRequest request, CancellationToken cancellationToken)
    {
        var result = await gameIssueService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<GameIssueResponse>>> List([FromQuery][Range(1, int.MaxValue)] int page = 1, [FromQuery][Range(1, 100)] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await gameIssueService.ListAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GameIssueResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await gameIssueService.GetAsync(id, cancellationToken);
        return Ok(result);
    }
}