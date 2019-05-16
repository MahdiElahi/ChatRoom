using DomainClasses.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Services.Repositories;
using DomainClasses.Chat;
using Utilities;
using Microsoft.AspNetCore.Authorization;
using ViewModels;
using Microsoft.EntityFrameworkCore;

namespace itsaco_RefrencenetCoreCMS.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IGenericRepository<ApplicationUser> _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGenericRepository<ChatMessage> _chatMessageRepository;
        private readonly IGenericRepository<ChatGroup> _chatGroupRepository;
        private static List<ApplicationUser> _onlineUsers = new List<ApplicationUser>();
        public ChatHub(IGenericRepository<ApplicationUser> userRepository,
            IGenericRepository<ChatMessage> chatMessageRepository,
           IGenericRepository<ChatGroup> chatGroupRepository,
            UserManager<ApplicationUser> userManager)
        {
            _chatMessageRepository = chatMessageRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _chatGroupRepository = chatGroupRepository;
        }
        public override async Task OnConnectedAsync()
        {

            var user = await _userManager.GetUserAsync(Context.User);
            _onlineUsers.Add(user);

            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = await _userManager.GetUserAsync(Context.User);
            _onlineUsers.Remove(user);
            await Clients.All.SendAsync("Logout");

            await base.OnDisconnectedAsync(exception);
        }
        public async Task SendMessage(string text, string username)
        {
            var user1 = await _userManager.GetUserAsync(Context.User);
            var user2 = await _userRepository.GetAsync(x => x.UserName == username);

            var message = new ChatMessage()
            {
                Message = text,
                Date = DateTime.Now,
                Type = true,
                UserId_1 = user1.Id,
                UserId_2 = user2.Id,
                IsNew=true
                
            };
            await _chatMessageRepository.AddAsync(message);
            await _chatMessageRepository.SaveChangesAsync();

            var model = new ChatMessageViewModel()
            {
                UserName_1 = user1.UserName,
                UserName_2 = user2.UserName,
                FullName = user1.FullName,
                Message = message.Message,
                Date = message.Date.ToShamsi(),
                UserPhoto = user1.UserPhoto,
                Time = message.Date.ToTime(),
                IsNew=message.IsNew
            };
            await Clients.All.SendAsync("ReceiveMessage", model);

        }
        public async Task Logout()
        {
            var user = await _userManager.GetUserAsync(Context.User);
            _onlineUsers.Remove(user);
            await Clients.All.SendAsync("Logout", user.UserName);

        }
        public async Task ChangeRoom(string username)
        {
            var l = await _chatMessageRepository.GetAllAsync();

            var user1 = await _userManager.GetUserAsync(Context.User);
            var user2 = await _userRepository.GetAsync(x => x.UserName == username);

            var messages = await _chatMessageRepository.GetWithIncludeAsync<ChatMessage>(where: x => (x.UserId_1 == user1.Id && x.UserId_2 == user2.Id) ||
            (x.UserId_1 == user2.Id && x.UserId_2 == user1.Id),
            selector: x => x,
            include: x => x.Include(s => s.ApplicationUser_UserId1).Include(s=>s.ApplicationUser_UserId2));

            var list = new List<ChatMessageViewModel>();
            foreach (var item in messages)
            {

                list.Add(new ChatMessageViewModel()
                {
                    UserName_1 = item.ApplicationUser_UserId1.UserName,
                    FullName =item.ApplicationUser_UserId1.FullName,
                    Message = item.Message,
                    Date = item.Date.ToShamsi(),
                    UserPhoto = item.ApplicationUser_UserId1.UserPhoto,
                    Time = item.Date.ToTime(),
                    IsNew=item.IsNew
                });
            }
            await Clients.Caller.SendAsync("LoadHistory", list);

        }
        public async Task GetUserName()
        {
            var user = await _userManager.GetUserAsync(Context.User);
            await Clients.Caller.SendAsync("GetUserName", user.UserName);
            await GetUsers();

        }
        public async Task GetUsers()
        {
           
            var current_user = await _userManager.GetUserAsync(Context.User);
            var users_model = new List<OnlineUsersViewModel>();
            var countMessage_model = new List<MessageCountViewModel>();
            var allUsers = await _userRepository.GetAllAsync();

         


            foreach (var item in allUsers)
            {
                var model = new OnlineUsersViewModel() { user = item };

                if ( _onlineUsers.Any(x => x.Id == item.Id)) model.Online = true;
                else
                    model.Online = false;   
              
                users_model.Add(model);
            }

            await Clients.All.SendAsync("GetUsers", users_model);

            foreach (var item in allUsers)
            {
                var model = new MessageCountViewModel() { UserName = item.UserName };

                //--------------Get Messages--------------------//
                var messages = await _chatMessageRepository.GetAllAsync(x=>x.UserId_1==item.Id);
                //--------------Its Worked Correctly------------//
                model.Count_Message = messages.Count().ToString();
                countMessage_model.Add(model);

            }

            await Clients.All.SendAsync("GetCountMessage", countMessage_model);

        }
        public async Task<IEnumerable<ChatMessage>> GetCountMessage()
        {
            return await _chatMessageRepository.GetAllAsync();
        }
    }
}
