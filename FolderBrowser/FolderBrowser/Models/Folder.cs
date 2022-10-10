using Newtonsoft.Json;

namespace FolderBrowser.Models
{
    public class Folder
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string? Name { get; set; }

        public ICollection<Folder>? Children { get; set; }

        public Folder? Parent { get; set; }
        [JsonIgnore]
        public int? ParentId { get; set; }
    }
}
