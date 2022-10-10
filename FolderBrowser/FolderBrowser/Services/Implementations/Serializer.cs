using FolderBrowser.Models;
using FolderBrowser.Services.Interfaces;
using Newtonsoft.Json;

namespace FolderBrowser.Services.Implementations
{
    /// <summary>
    /// Service for serializing and deserializing directory system in json format.
    /// </summary>
    public class Serializer : ISerializer
    {
        public string Serialize(Folder? root)
        {
            string json = JsonConvert.SerializeObject(root, Formatting.Indented,
                new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects });
            return json;
        }
        
        public Folder? Deserialize(string path)
        {
            using StreamReader reader = new StreamReader(path);
            string json = reader.ReadToEnd();
            var root = JsonConvert.DeserializeObject<Folder?>(json);
            return root;
        }
    }
}
