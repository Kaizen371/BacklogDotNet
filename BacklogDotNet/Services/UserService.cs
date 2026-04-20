using BacklogDotNet.DTO;
using MySqlConnector;

namespace BacklogDotNet.Services;

public class UserService(MySqlDataSource dataSource)
{
    public async Task<UserEntity?> GetUser(string externalID)
    {
        
        using var connection = await dataSource.OpenConnectionAsync();

       
        using var command =
            new MySqlCommand(
                "SELECT username, firstname, lastname, email, role, password, BIN_TO_UUID(external_id) as id FROM Users WHERE external_id = UUID_TO_BIN(@externalID)",
                connection);
        command.Parameters.AddWithValue("@externalID", externalID);

        
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
            
            return new UserEntity(
                reader.GetGuid("id"),
                reader.GetString("firstname"),
                reader.GetString("lastname"),
                reader.GetString("username"),
                reader.GetString("password"),
                reader.GetString("email"),
                reader.GetString("role"));

        return null;
    }

    public async Task<UserEntity?> GetByEmail(string email)
    {
        
        using var connection = await dataSource.OpenConnectionAsync();

        
        using var command =
            new MySqlCommand(
                "SELECT username, firstname, lastname, email, role, password, BIN_TO_UUID(external_id) as id FROM Users WHERE email = @email",
                connection);
        command.Parameters.AddWithValue("@email", email);

        
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
            
            return new UserEntity(
                reader.GetGuid("id"),
                reader.GetString("firstname"),
                reader.GetString("lastname"),
                reader.GetString("username"),
                reader.GetString("password"),
                reader.GetString("email"),
                reader.GetString("role"));

        return null;
    }
}