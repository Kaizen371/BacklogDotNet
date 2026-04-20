using BacklogDotNet.DTO;
using MySqlConnector;

namespace BacklogDotNet.Services;

public class ItemsService(MySqlDataSource dataSource)
{
    public async Task<ItemEntity> addItem(ItemEntity item)
    {
        using var connection = dataSource.OpenConnection();

        var command =
            new MySqlCommand(
                "INSERT INTO items (title, platform, status, rating, userID, production, mediaCategory, ordinate) VALUES(@title, @platform, @status, @rating, UUID_TO_BIN(@userID), @production, @mediaCategory, @ordinate)",
                connection);

        command.Parameters.AddWithValue("@title", item.Title);
        command.Parameters.AddWithValue("@platform", item.Platform);
        command.Parameters.AddWithValue("@status", item.Status);
        command.Parameters.AddWithValue("@rating", item.Rating);
        command.Parameters.AddWithValue("@userID", item.UserID);
        command.Parameters.AddWithValue("@production", item.Production);
        command.Parameters.AddWithValue("@mediaCategory", item.MediaCategory);
        command.Parameters.AddWithValue("@ordinate", item.Ordinate);


        command.ExecuteNonQuery();

        var idCommand = new MySqlCommand("SELECT LAST_INSERT_ID();", connection);

        var itemID = idCommand.ExecuteScalar().ToString();

        item.ID = itemID;

        return item;
    }

    public async Task<int> removeItem(string ID)
    {
        using var connection = await dataSource.OpenConnectionAsync();

        var command = new MySqlCommand("DELETE FROM items WHERE ID = @ID", connection);

        command.Parameters.AddWithValue("@ID", ID);
        return command.ExecuteNonQuery();
    }

    public async Task<ItemEntity> editItem(ItemEntity item)
    {
        using var connection = await dataSource.OpenConnectionAsync();

        using var command = new MySqlCommand(
            "UPDATE Items SET Title = @title, Platform = @platform, Status = @status, Rating = @rating, Production = @production, MediaCategory = @mediaCategory, Ordinate = @ordinate WHERE ID = @ID AND userID = UUID_TO_BIN(@userID)",
            connection);
        
        command.Parameters.AddWithValue("@title", item.Title);
        command.Parameters.AddWithValue("@platform", item.Platform);
        command.Parameters.AddWithValue("@status", item.Status);
        command.Parameters.AddWithValue("@rating", item.Rating);
        command.Parameters.AddWithValue("@production", item.Production);
        command.Parameters.AddWithValue("@mediaCategory", item.MediaCategory);
        command.Parameters.AddWithValue("@ordinate", item.Ordinate);
        command.Parameters.AddWithValue("@ID", item.ID);
        command.Parameters.AddWithValue("@userID", item.UserID);
        command.ExecuteNonQuery();


        return item;
    }

    public async Task<List<ItemEntity>> GetItems(string externalID)
    {
        using var connection = await dataSource.OpenConnectionAsync();

        var command =
            new MySqlCommand(
                "SELECT platform, title, status, rating, BIN_TO_UUID(userID) as user_id, production, mediaCategory, ordinate, id FROM Items WHERE userId = UUID_TO_BIN(@externalID)",
                connection);

        command.Parameters.AddWithValue("@externalID", externalID);

        using var reader = command.ExecuteReader();

        var items = new List<ItemEntity>();

        while (reader.Read())
        {
            var newItem = new ItemEntity(
                reader.GetString("platform"),
                reader.GetString("title"),
                reader.GetString("status"),
                reader.GetInt32("rating"),
                reader.GetString("user_id"),
                reader.GetString("production"),
                reader.GetString("mediaCategory"),
                reader.GetInt32("ordinate"),
                reader.GetInt32("id").ToString());

            items.Add(newItem);
        }


        return items;
    }
}