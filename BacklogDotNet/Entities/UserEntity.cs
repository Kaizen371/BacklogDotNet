namespace BacklogDotNet.DTO;

public class UserEntity
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }


    public UserEntity(Guid id, string firstName, string lastName, string username, string password, string email, string Role)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Username = username;
        Password = password;
        Email = email;
        Role = Role;
    }
}