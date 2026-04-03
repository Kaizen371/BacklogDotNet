using System.Security.Claims;
using BacklogDotNet.Models;
using BacklogDotNet.Services;

namespace BacklogDotNet.EndPoints;

public static class ItemEndpoints
{
    public static void MapItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/items").WithTags("Items");

        group.MapGet("", async (ClaimsPrincipal ClaimsPrincipal, ItemsService itemsService) =>
        {
            
            var externalUserId = UserEndpoints.GetUserID(ClaimsPrincipal);
            
            if (externalUserId == null)
            {
                return (IResult)TypedResults.Unauthorized();
            }

            var myItems = await itemsService.GetItems(externalUserId);
            
            var itemList = new List<ItemModel>();

            foreach (var item in myItems)
            {
                var itemmodel = new ItemModel(
                    item.Title, 
                    item.Platform, 
                    item.Status, 
                    item.Rating, 
                    item.UserID,
                    item.Production,
                    item.MediaCategory,
                    item.Ordinate,
                    item.ID);
                
                itemList.Add(itemmodel);
            }
            

            return (IResult)TypedResults.Ok(itemList);
        }).RequireAuthorization();
    }
}