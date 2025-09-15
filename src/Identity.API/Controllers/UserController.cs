using Identity.API.Attributes;
using Identity.API.Models;
using Identity.API.Models.Authorization;
using Identity.API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserController> _logger;

    public UserController(UserManager<ApplicationUser> userManager, ILogger<UserController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    [RequirePermission(Permission.Users.Read)]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var users = _userManager.Users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var userDtos = new List<UserDto>();
            
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    Roles = roles.ToList()
                });
            }

            return Ok(ApiResponse<List<UserDto>>.SuccessResult(userDtos, "Users retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving users");
            return StatusCode(500, ApiResponse<List<UserDto>>.ErrorResult("Internal server error occurred"));
        }
    }

    [HttpGet("{id}")]
    [RequirePermission(Permission.Users.Read)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.ErrorResult("User not found"));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = roles.ToList()
            };

            return Ok(ApiResponse<UserDto>.SuccessResult(userDto, "User retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user");
            return StatusCode(500, ApiResponse<UserDto>.ErrorResult("Internal server error occurred"));
        }
    }

    [HttpPut("{id}")]
    [RequirePermission(Permission.Users.Write)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(string id, [FromBody] UpdateUserDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<UserDto>.ErrorResult("Invalid input", errors));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.ErrorResult("User not found"));
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.Email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ApiResponse<UserDto>.ErrorResult("Failed to update user", errors));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = roles.ToList()
            };

            _logger.LogInformation("User {UserId} updated successfully", id);
            return Ok(ApiResponse<UserDto>.SuccessResult(userDto, "User updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user");
            return StatusCode(500, ApiResponse<UserDto>.ErrorResult("Internal server error occurred"));
        }
    }

    [HttpDelete("{id}")]
    [RequirePermission(Permission.Users.Delete)]
    public async Task<ActionResult<ApiResponse>> DeleteUser(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResult("User not found"));
            }

            // Prevent deletion of the current user
            var currentUserId = _userManager.GetUserId(User);
            if (id == currentUserId)
            {
                return BadRequest(ApiResponse.ErrorResult("Cannot delete your own account"));
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ApiResponse.ErrorResult("Failed to delete user", errors));
            }

            _logger.LogInformation("User {UserId} deleted successfully", id);
            return Ok(ApiResponse.SuccessResult("User deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting user");
            return StatusCode(500, ApiResponse.ErrorResult("Internal server error occurred"));
        }
    }
}