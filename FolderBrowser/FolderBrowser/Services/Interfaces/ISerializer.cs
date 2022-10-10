using FolderBrowser.Models;

namespace FolderBrowser.Services.Interfaces
{
    public interface ISerializer
    {
        public string Serialize(Folder? root);
        public Folder? Deserialize(string path);
    }
}
