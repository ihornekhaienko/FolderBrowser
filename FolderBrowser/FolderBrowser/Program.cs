using FolderBrowser.Data;
using FolderBrowser.Services.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FolderBrowser
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string connection = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection));

            builder.Services.AddControllersWithViews();

            builder.Services.AddFolderService();

            builder.Services.AddJsonSerializer();

            var app = builder.Build();

            if (!builder.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error/Index");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}