using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EMRO.Common.RealTimeNotification
{
    public class SignalRHub : Hub
    {
        private static Dictionary<string, string> connectionsNgroup = new Dictionary<string, string>();

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (connectionsNgroup.ContainsKey(Context.ConnectionId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, connectionsNgroup[Context.ConnectionId]);
                connectionsNgroup.Remove(Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task BroadcastText(string text)
        {
            if (connectionsNgroup.ContainsKey(Context.ConnectionId))
            {
                await Clients.OthersInGroup(connectionsNgroup[Context.ConnectionId]).SendAsync("ReceiveText", text);
            }
        }

        public async Task JoinGroup(string group)
        {
            if (connectionsNgroup.ContainsKey(Context.ConnectionId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, connectionsNgroup[Context.ConnectionId]);
                connectionsNgroup.Remove(Context.ConnectionId);
            }
            connectionsNgroup.Add(Context.ConnectionId, group);
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            //Notify all when new user join the group
            await Clients.OthersInGroup(connectionsNgroup[Context.ConnectionId]).SendAsync("group-" + group, "new user join");
        }

        public async Task BroadcastChat(ChatMessage message)
        {
            if (connectionsNgroup.ContainsKey(Context.ConnectionId))
            {
                await Clients.Groups(connectionsNgroup[Context.ConnectionId]).SendAsync("ReceiveChat", message);
            }
            //await Clients.All.MessageReceivedFromHub(message);
        }
    }

    public class ChatMessage
    {
        public string Text { get; set; }
        public string ConnectionId { get; set; }
        public DateTime DateTime { get; set; }
    }
}
