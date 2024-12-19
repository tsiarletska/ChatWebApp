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

        // GET
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId"));

            var chats = await _context.Chat
                .Include(c => c.AdminUser)
                .Include(c => c.ParticipantUser)
                .Where(c => c.AdminUserId == userId || c.ParticipantUserId == userId)
                .ToListAsync();

            ViewBag.UserId = userId;
            return View(chats);
        }


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

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> GetOrCreateChat(int adminUserId, int participantUserId)
        {
            if (adminUserId == participantUserId)
                return Json(new { success = false, message = "Cannot create a chat with yourself." });

            var existingChat = await _context.Chat
                .FirstOrDefaultAsync(c =>
                    (c.AdminUserId == adminUserId && c.ParticipantUserId == participantUserId) ||
                    (c.AdminUserId == participantUserId && c.ParticipantUserId == adminUserId));

            if (existingChat != null)
                return Json(new { success = true, chatId = existingChat.ChatId });

            var newChat = new Chat
            {
                AdminUserId = adminUserId,
                ParticipantUserId = participantUserId
            };

            _context.Chat.Add(newChat);
            await _context.SaveChangesAsync();

            return Json(new { success = true, chatId = newChat.ChatId });
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int chatId, string text)
        {
            if (string.IsNullOrEmpty(text))
                return View("ChatMessages", new { success = false, error = "Message text is required." });

            if (!HttpContext.Session.TryGetValue("UserId", out var userIdBytes))
                return RedirectToAction("Login", "Account"); // Redirect to login if not logged in

            int senderId = int.Parse(System.Text.Encoding.UTF8.GetString(userIdBytes));

            var sender = await _context.User.FindAsync(senderId);
            if (sender == null)
                return View("ChatMessages", new { success = false, error = "Sender does not exist." });

            var chat = await _context.Chat.FindAsync(chatId);
            if (chat == null)
                return View("ChatMessages", new { success = false, error = "Chat does not exist." });

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

            return RedirectToAction("ChatMessages", new { chatId = chatId });
        }

        [HttpPost]
        public async Task<IActionResult> SendMessageToUser(int recipientId, string message)
        {
            if (!HttpContext.Session.TryGetValue("UserId", out var userIdBytes))
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if not logged in
            }

            int senderId = int.Parse(System.Text.Encoding.UTF8.GetString(userIdBytes));

            var recipient = await _context.User.FindAsync(recipientId);
            if (recipient == null)
            {
                ViewBag.Error = "The recipient does not exist.";
                return RedirectToAction("SearchUser");
            }

            var existingChat = await _context.Chat.FirstOrDefaultAsync(c =>
                (c.AdminUserId == senderId && c.ParticipantUserId == recipientId) ||
                (c.AdminUserId == recipientId && c.ParticipantUserId == senderId));

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

            return RedirectToAction("ChatMessages", new { chatId = existingChat.ChatId });
        }

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

            ViewBag.ChatId = chatId; 
            return View(messages);
        }

    }
}


