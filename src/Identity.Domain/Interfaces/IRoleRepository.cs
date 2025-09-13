using Identity.Domain.Entities;

namespace Identity.Domain.Interfaces;

/// <summary>
/// Repository interface for Role entity
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// Gets all roles
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of roles</returns>
    Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a role by ID
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Role or null if not found</returns>
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a role by name
    /// </summary>
    /// <param name="name">Role name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Role or null if not found</returns>
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets roles for a specific user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of roles assigned to the user</returns>
    Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a new role
    /// </summary>
    /// <param name="role">Role to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Added role</returns>
    Task<Role> AddAsync(Role role, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing role
    /// </summary>
    /// <param name="role">Role to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated role</returns>
    Task<Role> UpdateAsync(Role role, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a role
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}