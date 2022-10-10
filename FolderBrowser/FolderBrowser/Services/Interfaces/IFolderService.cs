using FolderBrowser.Models;

namespace FolderBrowser.Services.Interfaces
{
    public interface IFolderService
    {
        public void Add(Folder folder);
        public void AddRange(IEnumerable<Folder> folders);
        public void Update(Folder folder);
        public void Remove(Folder folder);
        public void RemoveAll();
        public Folder? GetById(int id);
        public List<Folder> GetAll();
        public List<Folder> GetChildren(Folder folder);
        public void Export();
        public int Import(Folder root);
    }
}
