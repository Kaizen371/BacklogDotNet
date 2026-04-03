using Microsoft.AspNetCore.Components.Web;

namespace BacklogDotNet.DTO;

public class ItemEntity
{
    public string Platform { get; set; }
    public string Title { get; set; }
    public string Status { get; set; }
    public int Rating { get; set; }
    public string UserID { get; set; }
    public string Production { get; set; }
    public string MediaCategory { get; set; }
    public int Ordinate { get; set; }
    
    public string ID {get;set;}
    
    
    public ItemEntity(
        string platform, 
        string title, 
        string status, 
        int rating, 
        string userID, 
        string production, 
        string mediaCategory, 
        int ordinate,
        string id)
    {
        Platform = platform;
        Title = title;
        Status = status;
        Rating = rating;
        UserID = userID;
        Production = production;
        MediaCategory = mediaCategory;
        Ordinate = ordinate;
        ID = id;
    }
}

