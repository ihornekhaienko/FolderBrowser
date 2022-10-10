using FolderBrowser.Services.Implementations;
using FolderBrowser.Services.Interfaces;

namespace FolderBrowser.Services.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddFolderService(this IServiceCollection services)
        {
            services.AddScoped<IFolderService, FolderService>();
        }

        public static void AddJsonSerializer(this IServiceCollection services)
        {
            services.AddScoped<ISerializer, Serializer>();
        }
    }
}
