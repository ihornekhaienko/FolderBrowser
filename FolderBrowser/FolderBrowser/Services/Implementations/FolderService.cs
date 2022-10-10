using FolderBrowser.Data;
using FolderBrowser.Models;
using FolderBrowser.Services.Interfaces;

namespace FolderBrowser.Services.Implementations
{
    /// <summary>
    /// The service for working with Folders table in the database.
    /// </summary>
    public class FolderService : IFolderService
    {
        private readonly AppDbContext db;
        private readonly IWebHostEnvironment appEnvironment;
        private readonly ISerializer serializer;

        public FolderService(AppDbContext db, IWebHostEnvironment appEnvironment, ISerializer serializer)
        {
            this.db = db;
            this.appEnvironment = appEnvironment;
            this.serializer = serializer;
        }

        public void Add(Folder folder)
        {
            db.Folders.Add(folder);
            db.SaveChanges();
        }

        public void AddRange(IEnumerable<Folder> folders)
        {
            db.Folders.AddRange(folders);
            db.SaveChanges();
        }

        public void Remove(Folder folder)
        {
            db.Folders.Remove(folder);
            db.SaveChanges();
        }

        public void RemoveAll()
        {
            db.Folders.RemoveRange(db.Folders);
            db.SaveChanges();
        }

        public List<Folder> GetAll()
        {
            return db.Folders.ToList();
        }

        public List<Folder> GetChildren(Folder folder)
        {
            if (folder.Children != null)
            {
                return folder.Children.ToList();
            }
            else
            {
                return new List<Folder>();
            }
        }

        public Folder? GetById(int id)
        {
            return GetAll().Where(f => f.Id == id).FirstOrDefault();
        }

        public void Update(Folder folder)
        {
            db.Folders.Update(folder);
            db.SaveChanges();
        }

        /// <summary>
        /// Serializes current database state to the json-file and exports it to the wwwroot/Files directory.
        /// </summary>
        public void Export()
        {
            string json = serializer.Serialize(GetAll().FirstOrDefault());
            string fileName = "snapshot.json";
            string path = Path.Combine(appEnvironment.WebRootPath, "Files", fileName);
            using StreamWriter writer = new StreamWriter(path);
            writer.Write(json);
        }

        /// <summary>
        /// Replaces current database with new directory system.
        /// </summary>
        /// <param name="root">Root Folder object</param>
        /// <returns>Id of the root Folder object</returns>
        public int Import(Folder root)
        {
            RemoveAll();
            var queue = new Queue<Folder>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                if (node != null)
                {
                    db.Folders.Add(node);

                    if (node.Children != null)
                    {
                        foreach (var child in node.Children)
                        {
                            queue.Enqueue(child);
                        }
                    }
                }
            }

            db.SaveChanges();

            int id = GetAll().First().Id;
            return id;
        }
    }
}
