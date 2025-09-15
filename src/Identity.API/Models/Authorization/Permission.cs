namespace Identity.API.Models.Authorization;

public static class Permission
{
    public static class Users
    {
        public const string Read = "users.read";
        public const string Write = "users.write";
        public const string Delete = "users.delete";
    }
    
    public static class Catalog
    {
        public const string Read = "catalog.read";
        public const string Write = "catalog.write";
        public const string Delete = "catalog.delete";
    }
    
    public static class Orders
    {
        public const string Read = "orders.read";
        public const string Write = "orders.write";
        public const string Delete = "orders.delete";
    }
    
    public static class Profile
    {
        public const string Read = "profile.read";
        public const string Write = "profile.write";
    }
    
    public static class System
    {
        public const string Admin = "system.admin";
    }
    
    public static IEnumerable<string> GetAllPermissions()
    {
        return new[]
        {
            Users.Read, Users.Write, Users.Delete,
            Catalog.Read, Catalog.Write, Catalog.Delete,
            Orders.Read, Orders.Write, Orders.Delete,
            Profile.Read, Profile.Write,
            System.Admin
        };
    }
}