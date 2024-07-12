using DASAPI.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Text;
using ZOI.BAL.Services;
using ZOI.BAL.Services.Interface;
using static ZOI.BAL.Utilities.Constants;

namespace DASAPI.SignalR
{
    public class CommHub : Hub

    {

        private readonly string _commGroup;
        private readonly IDictionary<string, string> _members;

        private readonly ITemplateService _templateService;


        public CommHub(IDictionary<string, string> Members, ITemplateService templateService)
        {
            _commGroup = "ZOI Communication Hub";
            _members = Members;
            _templateService = templateService;
        }


        public override async Task OnConnectedAsync()
        {

            UserHandler.ConnectedIds.Add(new ConnectedUser { ConnectionID = Context.ConnectionId });

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var itemToRemove = UserHandler.ConnectedIds.Single(r => r.ConnectionID == Context.ConnectionId);

            DocumentCollection documentCollection = new DocumentCollection();
            documentCollection.ObjectID = itemToRemove.DocID;
            documentCollection.CollectionName = itemToRemove.CollectionName;


            UserHandler.ConnectedIds.Remove(itemToRemove);

            var userList = UserHandler.ConnectedIds.Where(e => e.DocID == itemToRemove.DocID).ToList();
            List<string> userID = new List<string>();

            foreach (var value in userList)
            {
                userID.Add(value.ConnectionID);
            }

            var data = await _templateService.GetTemplateDetailsByID(documentCollection);

            await Clients.Clients(userID).SendAsync("GetTemplateDetailsByID", data, UserHandler.ConnectedIds);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task GetTemplateDetailsByID(DocumentCollection model)
        {
            var updateValue = UserHandler.ConnectedIds.Single(r => r.ConnectionID == Context.ConnectionId);
            updateValue.DocID = model.ObjectID;
            updateValue.UserName = model.CreatedBy;
            updateValue.UserID = model.CreatedByID;
            updateValue.CollectionName = model.CollectionName;

            var userList = UserHandler.ConnectedIds.Where(e => e.DocID == model.ObjectID).ToList();


            List<string> userID = new List<string>();

            foreach (var value in userList)
            {
                userID.Add(value.ConnectionID);
            }
            var data = await _templateService.GetTemplateDetailsByID(model);

            await Clients.Clients(userID).SendAsync("GetTemplateDetailsByID", data, userList);
        }

        public async Task GetPreviewTemplateDetailsByID(DocumentCollection model)
        {
            var userList = UserHandler.ConnectedIds;

            List<string> userID = new List<string>();

            foreach (var value in userList)
            {
                userID.Add(value.ConnectionID);
            }
            var data = await _templateService.GetTemplateDetailsByID(model);

            await Clients.Clients(userID).SendAsync("GetPreviewTemplateDetailsByID",data, userList, Context.ConnectionId);
        }

        public async Task GetCollectionListByUserID(DocumentCollection model)
        {
            var data = await _templateService.GetCollectionListByUserID(model);

            await Clients.Clients(Context.ConnectionId).SendAsync("GetCollectionListByUserID", data);
        }

    }
}

public static class UserHandler
{
    public static List<ConnectedUser> ConnectedIds = new List<ConnectedUser>();

}

public class ConnectedUser
{
    public string ConnectionID { get; set; }

    public string? UserName { get; set; }

    public string? UserID { get; set; }

    public string? DocID { get; set; }

    public string? CollectionName { get; set; }
}
