using Microsoft.AspNetCore.SignalR;

namespace ChatWebApp.Hubs
{
    public class ChatHub : Hub
    {
        // Sends a message to a specific chat group
        public async Task SendMessage(string chatId, string sender, string message)
        {
            await Clients.Group(chatId).SendAsync("ReceiveMessage", sender, message);
        }

        // Adds a user to a chat group
        public async Task JoinChat(string chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        }

        // Removes a user from a chat group (optional, e.g., on disconnect)
        public async Task LeaveChat(string chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
        }
    }
}
