using ChatWebApp.Data;
using ChatWebApp.Hubs; // Import the SignalR Hub namespace
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChatWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllersWithViews();

            // Configure DbContext for MySQL
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
                ).LogTo(Console.WriteLine, LogLevel.Information)
            );

            // Configure Distributed Memory Cache for Session Management
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = ".ChatApp.Session";
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Add SignalR service
            builder.Services.AddSignalR();

            var app = builder.Build();

            // Apply database migrations and seed data
            using (var scope = app.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var logger = serviceProvider.GetRequiredService<ILogger<Seed>>();

                try
                {
                    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
                    context.Database.Migrate();
                    Seed.SeedData(app);
                }
                catch (Exception ex)
                {
                    logger.LogError($"An error occurred during migration or seeding: {ex.Message}");
                }
            }

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Add Session Middleware here
            app.UseSession();

            app.UseAuthorization();

            // Map the SignalR hub
            app.MapHub<ChatHub>("/chathub");

            // Map default controller routes
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
