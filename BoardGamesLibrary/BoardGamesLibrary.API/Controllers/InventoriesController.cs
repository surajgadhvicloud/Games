using BoardGamesLibrary.Application.Contracts;
using BoardGamesLibrary.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<PagedResponse<InventoryResponse>>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (page < 1) return BadRequest("Page must be greater than or equal to 1.");
        if (pageSize < 1 || pageSize > 100) return BadRequest("PageSize must be between 1 and 100.");
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