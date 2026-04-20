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

        //Get items
        group.MapGet("", async (ClaimsPrincipal ClaimsPrincipal, ItemsService itemsService) =>
        {
            var externalUserId = UserEndpoints.GetUserID(ClaimsPrincipal);

            if (externalUserId == null) return TypedResults.Unauthorized();

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

        //Add items
        group.MapPost("", async (ClaimsPrincipal ClaimsPrincipal, ItemsService itemsService, ItemModel itemModel) =>
        {
            var externalUserId = UserEndpoints.GetUserID(ClaimsPrincipal);

            if (externalUserId == null) return TypedResults.Unauthorized();

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

            var newItem = await itemsService.addItem(item);

            var newItemModel = itemModel with { ID = newItem.ID };

            return (IResult)TypedResults.Ok(newItemModel);
        }).RequireAuthorization();

        //remove items
        group.MapDelete("/{ID}", async (ItemsService itemsService, string ID) =>
        {
            if (await itemsService.removeItem(ID) > 0) return TypedResults.Ok();

            return (IResult)TypedResults.NotFound();
        }).RequireAuthorization();

        group.MapPut("/{ID}", async (ItemsService itemsService, ItemModel itemModel, ClaimsPrincipal claimsPrincipal) =>
        {
            var externalUserId = UserEndpoints.GetUserID(claimsPrincipal);

            if (externalUserId == null) return TypedResults.Unauthorized();

            var item = new ItemEntity(
                itemModel.platform,
                itemModel.title,
                itemModel.status,
                itemModel.rating,
                externalUserId,
                itemModel.production,
                itemModel.mediaCategory,
                itemModel.ordinate,
                itemModel.ID);

            await itemsService.editItem(item);


            return (IResult)TypedResults.Ok(itemModel);
        }).RequireAuthorization();
    }
}