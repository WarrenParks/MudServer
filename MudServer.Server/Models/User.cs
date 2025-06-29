public class User
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty; // Password/authentication key
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public bool IsRegistered { get; set; } = false;
}