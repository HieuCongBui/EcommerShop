using Identity.API.Attributes;
using Identity.API.Models;
using Identity.API.Models.Authorization;
using Identity.API.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoleController(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    ILogger<RoleController> logger) : ControllerBase
{
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ILogger<RoleController> _logger = logger;

    [HttpGet]
    [RequirePermission(Permission.Users.Read)]
    public async Task<ActionResult<ApiResponse<List<RoleDto>>>> GetRoles()
    {
        try
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var roleDtos = roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name!,
                Description = r.Description,
                CreatedAt = r.CreatedAt
            }).ToList();

            return Ok(ApiResponse<List<RoleDto>>.SuccessResult(roleDtos, "Roles retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving roles");
            return StatusCode(500, ApiResponse<List<RoleDto>>.ErrorResult("Internal server error occurred"));
        }
    }

    [HttpPost]
    [RequirePermission(Permission.System.Admin)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> CreateRole([FromBody] CreateRoleDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<RoleDto>.ErrorResult("Invalid input", errors));
            }

            var existingRole = await _roleManager.FindByNameAsync(model.Name);
            if (existingRole != null)
            {
                return BadRequest(ApiResponse<RoleDto>.ErrorResult("Role with this name already exists"));
            }

            var role = new ApplicationRole
            {
                Name = model.Name,
                Description = model.Description,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ApiResponse<RoleDto>.ErrorResult("Failed to create role", errors));
            }

            var roleDto = new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                CreatedAt = role.CreatedAt
            };

            _logger.LogInformation("Role {RoleName} created successfully", role.Name);
            return Ok(ApiResponse<RoleDto>.SuccessResult(roleDto, "Role created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating role");
            return StatusCode(500, ApiResponse<RoleDto>.ErrorResult("Internal server error occurred"));
        }
    }

    [HttpPost("{userId}/assign-role")]
    [RequirePermission(Permission.Users.Write)]
    public async Task<ActionResult<ApiResponse>> AssignRoleToUser(string userId, [FromBody] AssignRoleDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.ErrorResult("Invalid input", errors));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResult("User not found"));
            }

            var role = await _roleManager.FindByNameAsync(model.RoleName);
            if (role == null)
            {
                return NotFound(ApiResponse.ErrorResult("Role not found"));
            }

            if (await _userManager.IsInRoleAsync(user, model.RoleName))
            {
                return BadRequest(ApiResponse.ErrorResult("User is already in this role"));
            }

            var result = await _userManager.AddToRoleAsync(user, model.RoleName);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ApiResponse.ErrorResult("Failed to assign role", errors));
            }

            _logger.LogInformation("Role {RoleName} assigned to user {UserId}", model.RoleName, userId);
            return Ok(ApiResponse.SuccessResult("Role assigned successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning role to user");
            return StatusCode(500, ApiResponse.ErrorResult("Internal server error occurred"));
        }
    }

    [HttpDelete("{userId}/remove-role")]
    [RequirePermission(Permission.Users.Write)]
    public async Task<ActionResult<ApiResponse>> RemoveRoleFromUser(string userId, [FromBody] AssignRoleDto model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse.ErrorResult("Invalid input", errors));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(ApiResponse.ErrorResult("User not found"));
            }

            if (!await _userManager.IsInRoleAsync(user, model.RoleName))
            {
                return BadRequest(ApiResponse.ErrorResult("User is not in this role"));
            }

            var result = await _userManager.RemoveFromRoleAsync(user, model.RoleName);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(ApiResponse.ErrorResult("Failed to remove role", errors));
            }

            _logger.LogInformation("Role {RoleName} removed from user {UserId}", model.RoleName, userId);
            return Ok(ApiResponse.SuccessResult("Role removed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing role from user");
            return StatusCode(500, ApiResponse.ErrorResult("Internal server error occurred"));
        }
    }

    [HttpGet("permissions")]
    [RequirePermission(Permission.System.Admin)]
    public ActionResult<ApiResponse<List<PermissionDto>>> GetPermissions()
    {
        try
        {
            var permissions = Permission.GetAllPermissions().Select(p => new PermissionDto
            {
                Name = p,
                Description = GetPermissionDescription(p)
            }).ToList();

            return Ok(ApiResponse<List<PermissionDto>>.SuccessResult(permissions, "Permissions retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving permissions");
            return StatusCode(500, ApiResponse<List<PermissionDto>>.ErrorResult("Internal server error occurred"));
        }
    }

    private static string GetPermissionDescription(string permission)
    {
        return permission switch
        {
            Permission.Users.Read => "View users",
            Permission.Users.Write => "Create and update users",
            Permission.Users.Delete => "Delete users",
            Permission.Catalog.Read => "View catalog items",
            Permission.Catalog.Write => "Create and update catalog items",
            Permission.Catalog.Delete => "Delete catalog items",
            Permission.Orders.Read => "View orders",
            Permission.Orders.Write => "Create and update orders",
            Permission.Orders.Delete => "Delete orders",
            Permission.Profile.Read => "View own profile",
            Permission.Profile.Write => "Update own profile",
            Permission.System.Admin => "System administration access",
            _ => permission
        };
    }
}