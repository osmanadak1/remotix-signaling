using Microsoft.AspNetCore.SignalR;

namespace RemotixSignalingServer.Hubs
{
    public class ConnectionHub : Hub
    {
        // Store connected clients
        private static readonly Dictionary<string, string> ConnectedClients = new Dictionary<string, string>();

        // Register a client with their unique hardware ID
        public async Task RegisterClient(string clientId)
        {
            ConnectedClients[clientId] = Context.ConnectionId;
            await Groups.AddToGroupAsync(Context.ConnectionId, clientId);
            
            Console.WriteLine($"Client registered: {clientId} -> {Context.ConnectionId}");
        }

        // Request connection to another client
        public async Task RequestConnection(string requesterId, string targetId)
        {
            if (ConnectedClients.TryGetValue(targetId, out string targetConnectionId))
            {
                // Send connection request to target client
                await Clients.Client(targetConnectionId).SendAsync("ConnectionRequest", requesterId);
                Console.WriteLine($"Connection request sent from {requesterId} to {targetId}");
            }
            else
            {
                // Target client not found
                if (ConnectedClients.TryGetValue(requesterId, out string requesterConnectionId))
                {
                    await Clients.Client(requesterConnectionId).SendAsync("ConnectionResponse", false);
                }
                Console.WriteLine($"Target client {targetId} not found");
            }
        }

        // Respond to connection request
        public async Task RespondToConnection(string responderId, string requesterId, bool accepted)
        {
            if (ConnectedClients.TryGetValue(requesterId, out string requesterConnectionId))
            {
                // Send response back to requester
                await Clients.Client(requesterConnectionId).SendAsync("ConnectionResponse", accepted);
                
                if (accepted)
                {
                    // Notify both clients to establish connection
                    if (ConnectedClients.TryGetValue(responderId, out string responderConnectionId))
                    {
                        await Clients.Client(requesterConnectionId).SendAsync("EstablishConnection");
                        await Clients.Client(responderConnectionId).SendAsync("EstablishConnection");
                    }
                }
                
                Console.WriteLine($"Connection response: {responderId} -> {requesterId} = {accepted}");
            }
        }

        // Handle client disconnection
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Remove client from connected clients list
            var clientToRemove = ConnectedClients.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (!clientToRemove.Equals(default(KeyValuePair<string, string>)))
            {
                ConnectedClients.Remove(clientToRemove.Key);
                Console.WriteLine($"Client disconnected: {clientToRemove.Key}");
            }
            
            await base.OnDisconnectedAsync(exception);
        }
    }
}