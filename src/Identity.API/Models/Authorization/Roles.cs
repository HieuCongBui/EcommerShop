namespace Identity.API.Models.Authorization;

public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
    
    public static IEnumerable<string> GetAllRoles()
    {
        return new[] { Admin, User };
    }
}