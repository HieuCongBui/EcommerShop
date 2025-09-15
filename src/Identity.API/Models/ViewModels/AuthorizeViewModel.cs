namespace Identity.API.Models.ViewModels;

public class AuthorizeViewModel
{
    public string? ApplicationName { get; set; }
    public string? Scope { get; set; }
    public List<string> Scopes { get; set; } = new();
    public List<ScopeDescription> ScopeDescriptions { get; set; } = new();
}

public class ScopeDescription
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}