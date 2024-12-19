using ChatWebApp.Data;
using ChatWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatWebApp.Controllers
{
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Display the list of chats
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId"));

            // Retrieve all chats where the logged-in user is either AdminUser or ParticipantUser
            var chats = await _context.Chat
                .Include(c => c.AdminUser)
                .Include(c => c.ParticipantUser)
                .Where(c => c.AdminUserId == userId || c.ParticipantUserId == userId)
                .ToListAsync();

            // Pass the chats and logged-in userId to the view
            ViewBag.UserId = userId;
            return View(chats);
        }


        // 1. GET: Search for a user by nickname
        [HttpGet]
        public async Task<IActionResult> SearchUser(string nickname)
        {
            if (string.IsNullOrEmpty(nickname))
            {
                ViewBag.Error = "Please provide a nickname to search.";
                return View();
            }

            var user = await _context.User.FirstOrDefaultAsync(u => u.Nickname == nickname);

            if (user == null)
            {
                ViewBag.Error = "User not found.";
                return View();
            }

            // Pass the found user details to the view
            return View(user);
        }


        // 2. POST: Create or retrieve a chat between two users
        [HttpPost]
        public async Task<IActionResult> GetOrCreateChat(int adminUserId, int participantUserId)
        {
            if (adminUserId == participantUserId)
                return Json(new { success = false, message = "Cannot create a chat with yourself." });

            // Check if chat already exists
            var existingChat = await _context.Chat
                .FirstOrDefaultAsync(c =>
                    (c.AdminUserId == adminUserId && c.ParticipantUserId == participantUserId) ||
                    (c.AdminUserId == participantUserId && c.ParticipantUserId == adminUserId));

            if (existingChat != null)
                return Json(new { success = true, chatId = existingChat.ChatId });

            // Create a new chat
            var newChat = new Chat
            {
                AdminUserId = adminUserId,
                ParticipantUserId = participantUserId
            };

            _context.Chat.Add(newChat);
            await _context.SaveChangesAsync();

            return Json(new { success = true, chatId = newChat.ChatId });
        }

        // 3. POST: Send a message
        [HttpPost]
        public async Task<IActionResult> SendMessage(int chatId, string text)
        {
            if (string.IsNullOrEmpty(text))
                return View("ChatMessages", new { success = false, error = "Message text is required." });

            // Retrieve the logged-in user's ID from the session
            if (!HttpContext.Session.TryGetValue("UserId", out var userIdBytes))
                return RedirectToAction("Login", "Account"); // Redirect to login if not logged in

            int senderId = int.Parse(System.Text.Encoding.UTF8.GetString(userIdBytes));

            // Check if the sender exists
            var sender = await _context.User.FindAsync(senderId);
            if (sender == null)
                return View("ChatMessages", new { success = false, error = "Sender does not exist." });

            // Check if the chat exists
            var chat = await _context.Chat.FindAsync(chatId);
            if (chat == null)
                return View("ChatMessages", new { success = false, error = "Chat does not exist." });

            // Save the message
            var message = new Message
            {
                ChatId = chatId,
                SenderId = senderId,
                Text = text,
                SentAt = DateTime.UtcNow,
                Status = "Sent"
            };

            _context.Message.Add(message);
            await _context.SaveChangesAsync();

            // Redirect back to the ChatMessages view after saving
            return RedirectToAction("ChatMessages", new { chatId = chatId });
        }

        [HttpPost]
        public async Task<IActionResult> SendMessageToUser(int recipientId, string message)
        {
            // Retrieve the sender's UserId from the session
            if (!HttpContext.Session.TryGetValue("UserId", out var userIdBytes))
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if not logged in
            }

            int senderId = int.Parse(System.Text.Encoding.UTF8.GetString(userIdBytes));

            // Check if the recipient exists
            var recipient = await _context.User.FindAsync(recipientId);
            if (recipient == null)
            {
                ViewBag.Error = "The recipient does not exist.";
                return RedirectToAction("SearchUser");
            }

            // Check if a chat between the two users exists
            var existingChat = await _context.Chat.FirstOrDefaultAsync(c =>
                (c.AdminUserId == senderId && c.ParticipantUserId == recipientId) ||
                (c.AdminUserId == recipientId && c.ParticipantUserId == senderId));

            // If no chat exists, create one
            if (existingChat == null)
            {
                existingChat = new Chat
                {
                    AdminUserId = senderId,
                    ParticipantUserId = recipientId
                };
                _context.Chat.Add(existingChat);
                await _context.SaveChangesAsync();
            }

            // Save the message
            var newMessage = new Message
            {
                ChatId = existingChat.ChatId,
                SenderId = senderId,
                Text = message,
                SentAt = DateTime.UtcNow,
                Status = "Sent"
            };
            _context.Message.Add(newMessage);
            await _context.SaveChangesAsync();

            // Redirect to the ChatMessages view to see the conversation
            return RedirectToAction("ChatMessages", new { chatId = existingChat.ChatId });
        }


        // 4. GET: Retrieve all messages for a chat



        // GET: Retrieve chat messages as JSON (for APIs)
        [HttpGet]
        public async Task<IActionResult> GetChatMessages(int chatId)
        {
            var messages = await _context.Message
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.SentAt)
                .Include(m => m.Sender) // To get the sender's details
                .ToListAsync();

            var formattedMessages = messages.Select(m => new
            {
                m.MessageId,
                SenderNickname = m.Sender?.Nickname,
                m.Text,
                SentAt = m.SentAt?.ToString("g")
            });

            return Json(new { success = true, messages = formattedMessages });
        }



        [HttpGet]
        public async Task<IActionResult> ChatMessages(int chatId)
        {
            var messages = await _context.Message
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.SentAt)
                .Include(m => m.Sender)
                .ToListAsync();

            ViewBag.ChatId = chatId; // Pass the chatId to the view
            return View(messages);
        }



    }
}


