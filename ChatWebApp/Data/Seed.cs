using ChatWebApp.Data;
using ChatWebApp.Models;
using ChatWebApp.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace ChatWebApp.Data
{
    public class Seed
    {
        public static void SeedData(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

                context.Database.Migrate();

                if (!context.User.Any())
                {
                    context.User.AddRange(new List<User>
                    {
                        new User { UserId = 100, Nickname = "bulbul", Password = "123456", Email = "bulbul@mail.com", CreatedAt = DateTime.Parse("2024-11-26") },
                        new User { UserId = 200, Nickname = "burlbum", Password = "678901", Email = "burumbul@mail.com", CreatedAt = DateTime.Parse("2024-11-27") }
                    });
                }

                if (!context.Chat.Any())
                {
                    context.Chat.AddRange(new List<Chat>
                    {
                        new Chat { ChatId = 1100200, AdminUserId = 200, ParticipantUserId = 100 }
                    });
                }

                if (!context.Message.Any())
                {
                    context.Message.AddRange(new List<Message>
                    {
                        new Message { MessageId = 1, ChatId = 1100200, SenderId = 100, Text = "Hi! It's Bulbul.", SentAt = DateTime.Parse("2024-11-28"), Status = "Unread" }
                    });
                }

                if (!context.AuthToken.Any())
                {
                    context.AuthToken.AddRange(new List<AuthToken>
                    {
                        new AuthToken { TokenId = 1, UserId = 100, TokenValue = "ABCD1234EFGH5678", ExpiresAt = DateTime.Parse("2024-11-30") },
                        new AuthToken { TokenId = 2, UserId = 200, TokenValue = "WXYZ9876JKLM4321", ExpiresAt = DateTime.Parse("2024-12-01") }
                    });
                }

                context.SaveChanges();
            }
        }
    }
}
