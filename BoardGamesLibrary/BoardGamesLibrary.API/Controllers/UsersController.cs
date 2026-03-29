using BoardGamesLibrary.Application.Contracts;
using BoardGamesLibrary.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoardGamesLibrary.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/users")]
public class UsersController(IUserService userService) : ControllerBase
{
	[HttpPost]
	public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
	{
		var result = await userService.CreateAsync(request, cancellationToken);
		return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
	}

	[HttpPut("{id:int}")]
	public async Task<ActionResult<UserResponse>> Update(int id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
	{
		var result = await userService.UpdateAsync(id, request, cancellationToken);
		return Ok(result);
	}

	[HttpGet]
	public async Task<ActionResult<PagedResponse<UserResponse>>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
	{
		if (page < 1) return BadRequest("Page must be greater than or equal to 1.");
		if (pageSize < 1 || pageSize > 100) return BadRequest("PageSize must be between 1 and 100.");
		var result = await userService.ListAsync(page, pageSize, cancellationToken);
		return Ok(result);
	}

	[HttpGet("{id:int}")]
	public async Task<ActionResult<UserResponse>> GetById(int id, CancellationToken cancellationToken)
	{
		var result = await userService.GetAsync(id, cancellationToken);
		return Ok(result);
	}
}