using System.Security.Claims;
using BacklogDotNet.DTO;
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
                    item.Production,
                    item.MediaCategory,
                    item.Ordinate,
                    item.ID);

                itemList.Add(itemmodel);
            }


            return (IResult)TypedResults.Ok(itemList);
        }).RequireAuthorization();

        group.MapPost("", async (ClaimsPrincipal ClaimsPrincipal, ItemsService itemsService, ItemModel itemModel) =>
        {
            var externalUserId = UserEndpoints.GetUserID(ClaimsPrincipal);

            if (externalUserId == null)
            {
                return (IResult)TypedResults.Unauthorized();
            }
            
            var item = new ItemEntity(
                itemModel.platform, 
                itemModel.title,
                itemModel.status,
                itemModel.rating,
                externalUserId,
                itemModel.production,
                itemModel.mediaCategory,
                itemModel.ordinate,
                null);

            ItemEntity newItem = await itemsService.addItem(item);

            var newItemModel = itemModel with { ID = newItem.ID };
            
            return (IResult)TypedResults.Ok(newItemModel);

        }).RequireAuthorization();
        
        /*group.MapDelete("", async (ClaimsPrincipal claimsPrincipal, ItemsService itemsService, ) =>)*/
    }
}