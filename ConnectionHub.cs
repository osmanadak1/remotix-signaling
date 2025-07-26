using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RemotixSignalingServer
{
    public class ConnectionHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> _connections = new();

        public async Task RegisterClient(string clientId)
        {
            _connections[clientId] = Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, clientId);
        }

        public async Task RequestConnection(string requesterId, string targetId)
        {
            if (_connections.TryGetValue(targetId, out var targetConnectionId))
            {
                await Clients.Client(targetConnectionId).SendAsync("ConnectionRequest", requesterId);
            }
        }

        public async Task RespondToConnection(string responderId, string requesterId, bool accepted)
        {
            if (_connections.TryGetValue(requesterId, out var requesterConnectionId))
            {
                await Clients.Client(requesterConnectionId).SendAsync("ConnectionResponse", accepted);
                
                if (accepted)
                {
                    await Clients.Client(requesterConnectionId).SendAsync("EstablishConnection");
                    if (_connections.TryGetValue(responderId, out var responderConnectionId))
                    {
                        await Clients.Client(responderConnectionId).SendAsync("EstablishConnection");
                    }
                }
            }
        }

        public async Task SendScreenData(string senderId, byte[] screenData)
        {
            await Clients.Others.SendAsync("ReceiveScreenData", senderId, screenData);
        }

        public async Task SendMouseCommand(string senderId, string targetId, string action, int x, int y, string button, bool isPressed, int delta)
        {
            if (_connections.TryGetValue(targetId, out var targetConnectionId))
            {
                await Clients.Client(targetConnectionId).SendAsync("ReceiveMouseCommand", senderId, action, x, y, button, isPressed, delta);
            }
        }

        public async Task SendKeyCommand(string senderId, string targetId, string key, bool isKeyDown)
        {
            if (_connections.TryGetValue(targetId, out var targetConnectionId))
            {
                await Clients.Client(targetConnectionId).SendAsync("ReceiveKeyCommand", senderId, key, isKeyDown);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var itemsToRemove = _connections.Where(kvp => kvp.Value == Context.ConnectionId).ToList();
            foreach (var item in itemsToRemove)
            {
                _connections.TryRemove(item.Key, out _);
            }
            
            await base.OnDisconnectedAsync(exception);
        }
    }
}