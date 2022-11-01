using FolderBrowser.Models;
using FolderBrowser.Services.Interfaces;
using FolderBrowser.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FolderBrowser.Controllers
{
    public class HomeController : Controller
    {
        ILogger<HomeController> logger;
        IWebHostEnvironment appEnvironment;
        IFolderService folderService;
        ISerializer serializer;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment appEnvironment, 
            IFolderService folderService, ISerializer serializer)
        {
            this.logger = logger;
            this.appEnvironment = appEnvironment;
            this.folderService = folderService;
            this.serializer = serializer;
        }

        /// <summary>
        /// Action for start page, where user is able to choose a catalogue for browsing from the list, which stores in wwwroot/Files folder on server,
        /// adding new files with directories or generating files based on directory tree of the project.
        /// </summary>
        /// <returns>ViewResult</returns>
        [HttpGet]
        public IActionResult Index()
        {
            string path = Path.Combine(appEnvironment.WebRootPath, "Files");
            var files = Directory.GetFiles(path)
                .Select(f => new FileViewModel { Name = Path.GetFileNameWithoutExtension(f), FullName = Path.GetFileName(f)})
                .ToList();

            return View(files);
        }

        /// <summary>
        /// Action for importing json-files with directories tree.
        /// At first previous file, which was used for browsing exports to the wwwroot/Files.
        /// Then defined json-file deserializes from json to Folder objects and loads to the database.
        /// </summary>
        /// <param name="fileName">Name for the file, which has to be imported from the wwwroot/Files</param>
        /// <returns>Redirection to the browsing page</returns>
        [HttpGet]
        public IActionResult Import(string fileName)
        {
            try
            {
                folderService.Export();

                string path = Path.Combine(appEnvironment.WebRootPath, "Files", fileName);
                var root = serializer.Deserialize(path);
                if (root == null)
                {
                    throw new Exception("Не вдалося десеріалізувати файл");
                }

                int id = folderService.Import(root);
                var node = folderService.GetById(id);

                return Redirect($"Browse/{node?.Name}");
            }
            catch (Exception err)
            {
                return RedirectToAction("Index", "Error", new { message = err.Message });
            }
        }

        [Route("Home/Browse/{*path}")]
        [HttpGet]
        public IActionResult Browse()
        {
            try
            {
                var path = RouteData.Values["path"]?.ToString();

                var currentFolder = folderService.Parse(path);

                if (currentFolder == null)
                {
                    throw new Exception("Не вдалося знайти папку");
                }

                ViewBag.CurrentFolder = currentFolder.Name;

                var children = folderService.GetChildren(currentFolder)
                    .Select(f => new FolderBrowseViewModel { Name = f.Name, Path = $"{currentFolder.Name}/{f.Name}" }).ToList();

                foreach (var i in children)
                {
                    logger.LogCritical(i.Name);
                }

                return View(children);
            }
            catch (Exception err)
            {
                return RedirectToAction("Index", "Error", new { message = err.Message });
            }
        }

        /// <summary>
        /// Action for downloading json-file from the server.
        /// </summary>
        /// <param name="fileName">A name of file to be exported</param>
        /// <returns>Json-file</returns>
        [HttpPost]
        public IActionResult Export(string fileName)
        {
            string filePath = Path.Combine(appEnvironment.WebRootPath, "Files", fileName);
            string fileType = "application/json";

            return PhysicalFile(filePath, fileType, fileName);
        }

        /// <summary>
        /// Action for uploading new json-file to the server.
        /// </summary>
        /// <param name="file">Json-file</param>
        /// <returns>Redirection to the start page with updated list of files</returns>
        [HttpPost]
        public IActionResult UploadFromFile(IFormFile file)
        {
            try
            {
                if (file != null)
                {
                    var fileExt = Path.GetExtension(file.FileName).Substring(1);
                    if (fileExt != "json")
                    {
                        throw new Exception("Файл має бути у форматі json");
                    }

                    string path = Path.Combine(appEnvironment.WebRootPath, "Files", file.FileName);
                    using (var fileStream = new FileStream(path, FileMode.OpenOrCreate))
                    {
                        file.CopyTo(fileStream);
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    throw new Exception("Не вдалося завантажити файл");
                }
            }
            catch (Exception err)
            {
                return RedirectToAction("Index", "Error", new { message = err.Message });
            }
        }

        /// <summary>
        /// Action for generating directory tree from the project catalogue.
        /// After checking the existence of the directory on server adds found folders to list using breadth-first search algorithm.
        /// And finally generated list of Folder objects serializes and exports to the wwwroot/Files folder.
        /// </summary>
        /// <param name="path">The path to the root folder in the project caralogue, default is root path</param>
        /// <param name="name">A name of file to be saved, default name is os.json</param>
        /// <returns>Redirection to the start page with updated list of files</returns>
        [HttpPost]
        public IActionResult UploadFromOS(string path, string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = string.Empty;
                }
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = "os";
                }
                string fullPath = Path.Combine(appEnvironment.ContentRootPath, path);
                if (Directory.Exists(fullPath))
                {
                    var rootDir = new DirectoryInfo(fullPath);
                    var root = new Folder { Name = rootDir.Name, Children = new List<Folder>() };
                    var folders = new List<Folder>();
                    var queue = new Queue<(DirectoryInfo, Folder)>();
                    queue.Enqueue((rootDir, root));

                    while (queue.Count > 0)
                    {
                        var node = queue.Dequeue();

                        folders.Add(node.Item2);
                        logger.LogCritical(node.Item1.GetDirectories().Count().ToString());
                        foreach (var subDir in node.Item1.GetDirectories())
                        {
                            var sub = new Folder { Name = subDir.Name, Children = new List<Folder>() };
                            sub.Parent = node.Item2;
                            node.Item2.Children?.Add(sub);
                            queue.Enqueue((subDir, sub));
                        }
                    }

                    path = Path.Combine(appEnvironment.WebRootPath, "Files", name + ".json");
                    using (StreamWriter writer = new StreamWriter(path, false))
                    {
                        var json = serializer.Serialize(root);
                        writer.Write(json);
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    throw new Exception("Такого каталогу не існує");
                }
            }
            catch (Exception err)
            {
                return RedirectToAction("Index", "Error", new { message = err.Message });
            }
        }
    }
}
