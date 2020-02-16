using BlazorBoilerplate.Shared.DataInterfaces;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Shared.Dto.Sample;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Hubs
{
    /// <summary>
    /// The SignalR hub
    /// </summary>
    public class ChatHub : Hub
    {
        private IMessageStore MessageService { get; set; }

        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(IMessageStore messageService, UserManager<ApplicationUser> userManager)
        {
            MessageService = messageService;
            _userManager = userManager;
        }

        /// <summary>
        /// connectionId-to-username lookup
        /// </summary>
        /// <remarks>
        /// Needs to be static as the chat is created dynamically a lot
        /// </remarks>
        private static readonly Dictionary<string, string> userLookup = new Dictionary<string, string>();

        /// <summary>
        /// Deletes a message
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteMessage(int id)
        {
            await MessageService.DeleteById(id);
        }

        /// <summary>
        /// Send a message to all clients
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessage(string message)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(Context.User.Identity.Name);

            MessageDto newMessage = new MessageDto()
            {
                Text = message,
                UserName = user.UserName,
                UserID = user.Id,
                When = DateTime.UtcNow
            };

            await MessageService.AddMessage(newMessage);

            #region Customized

            await Clients.All.SendAsync("ReceiveMessage", 0, user.UserName, message, newMessage.When);

            #endregion Customized
        }

        /// <summary>
        /// Register username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task Register(string username)
        {
            string currentId = Context.ConnectionId;

            if (!userLookup.ContainsKey(currentId))
            {
                // maintain a lookup of connectionId-to-username
                userLookup.Add(currentId, username);
                // re-use existing message for now
                await Clients.AllExcept(currentId).SendAsync("ReceiveMessage", 0, username, $"{username} joined the chat", DateTime.Now);
            }
        }

        /// <summary>
        /// Log connection
        /// </summary>
        /// <returns></returns>
        public override Task OnConnectedAsync()
        {
            Console.WriteLine("Connected");
            List<MessageDto> messages = MessageService.GetMessages();

            foreach (MessageDto message in messages)
            {
                Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", message.Id, message.UserName, message.Text, message.When);
            }

            return base.OnConnectedAsync();
        }

        /// <summary>
        /// Log disconnection
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception e)
        {
            Console.WriteLine($"Disconnected {e?.Message}");
            // try to get connection
            string id = Context.ConnectionId;
            if (userLookup.TryGetValue(id, out string username))
            {
                userLookup.Remove(id);
                await Clients.AllExcept(Context.ConnectionId).SendAsync("ReceiveMessage", 0, username, $"{username} has left the chat", DateTime.Now);
            }
            await base.OnDisconnectedAsync(e);
        }
    }
}