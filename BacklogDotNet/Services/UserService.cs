using System.Data.SqlTypes;
using BacklogDotNet.DTO;
using BacklogDotNet.Models;
using MySqlConnector;

namespace BacklogDotNet.Services;

public class UserService(MySqlDataSource dataSource)
{
    public async Task<UserEntity?> GetUser(string externalID)
    {
        // 1. Create a connection from the data source
        using var connection = await dataSource.OpenConnectionAsync();

        // 2. Define your SQL query
        using var command = new MySqlCommand("SELECT username, firstname, lastname, email, role, password, BIN_TO_UUID(external_id) as id FROM Users WHERE external_id = UUID_TO_BIN(@externalID)", connection);
        command.Parameters.AddWithValue("@externalID", externalID);

        // 3. Execute and Read
        using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            // 4. Map the database columns to your DTO
            return new UserEntity(
                reader.GetGuid("id"), 
                reader.GetString("firstname"), 
                reader.GetString("lastname"), 
                reader.GetString("username"),
                reader.GetString("password"), 
                reader.GetString("email"),
                reader.GetString("role"));

        }

        return null;
    }
    public async Task<UserEntity?> GetByEmail(string email)
    {
        // 1. Create a connection from the data source
        using var connection = await dataSource.OpenConnectionAsync();

        // 2. Define your SQL query
        using var command = new MySqlCommand("SELECT username, firstname, lastname, email, role, password, BIN_TO_UUID(external_id) as id FROM Users WHERE email = @email", connection);
        command.Parameters.AddWithValue("@email", email);

        // 3. Execute and Read
        using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            // 4. Map the database columns to your DTO
            return new UserEntity(
                reader.GetGuid("id"), 
                reader.GetString("firstname"), 
                reader.GetString("lastname"), 
                reader.GetString("username"),
                reader.GetString("password"), 
                reader.GetString("email"),
                reader.GetString("role"));

        }

        return null;
    }
}