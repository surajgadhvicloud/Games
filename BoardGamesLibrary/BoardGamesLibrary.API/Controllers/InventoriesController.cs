using BoardGamesLibrary.Application.Contracts;
using BoardGamesLibrary.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BoardGamesLibrary.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager,DataEntry")]
[Route("api/inventories")]
public class InventoriesController(IInventoryService inventoryService) : ControllerBase
{
    [Authorize(Roles = "Admin,Manager")]
    [HttpPost]
    public async Task<ActionResult<InventoryResponse>> Create([FromBody] CreateInventoryRequest request, CancellationToken cancellationToken)
    {
        var result = await inventoryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByBoardGameId), new { boardGameId = result.BoardGameId }, result);
    }

    [Authorize(Roles = "Admin,Manager")]
    [HttpPut("{boardGameId:int}")]
    public async Task<ActionResult<InventoryResponse>> Update(int boardGameId, [FromBody] UpdateInventoryRequest request, CancellationToken cancellationToken)
    {
        var result = await inventoryService.UpdateAsync(boardGameId, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<InventoryResponse>>> List([FromQuery][Range(1, int.MaxValue)] int page = 1, [FromQuery][Range(1, 100)] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await inventoryService.ListAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{boardGameId:int}")]
    public async Task<ActionResult<InventoryResponse>> GetByBoardGameId(int boardGameId, CancellationToken cancellationToken)
    {
        var result = await inventoryService.GetByBoardGameIdAsync(boardGameId, cancellationToken);
        return Ok(result);
    }
}