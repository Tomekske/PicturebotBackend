using Api.Controllers; // For CreateNodeRequest DTO
using Api.Data.Models;
using Api.Data.Enums;
using Api.Data.Repositories;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Api.Services;

public class HierarchyService
{
    private readonly HierarchyRepository _repo;
    private readonly PictureRepository _pictureRepo;
    private readonly ILogger<HierarchyService> _logger;

    public HierarchyService(HierarchyRepository repo, PictureRepository pictureRepo, ILogger<HierarchyService> logger)
    {
        _repo = repo;
        _pictureRepo = pictureRepo;
        _logger = logger;
    }

    public async Task<Hierarchy> CreateNodeAsync(CreateNodeRequest req)
    {
        // Convert string type to Enum
        if (!Enum.TryParse<HierarchyType>(req.Type, true, out var typeEnum))
        {
            throw new ArgumentException("Invalid hierarchy type");
        }

        // Duplicate Check
        if (typeEnum == HierarchyType.Folder)
        {
            // Handle root folders (ParentID 0 or null)
            int? pid = req.ParentId == 0 ? null : req.ParentId;
            if (await _repo.FindDuplicateAsync(pid, req.Name, typeEnum))
            {
                throw new InvalidOperationException("a folder with this name already exists here");
            }
        }

        var newNode = new Hierarchy
        {
            ParentId = req.ParentId == 0 ? null : req.ParentId,
            Name = req.Name,
            Type = typeEnum,
            SubFolders = req.SubFolders
        };

        // Album Logic: UUID and Disk creation
        if (typeEnum == HierarchyType.Album)
        {
            newNode.Uuid = Guid.NewGuid().ToString(); // V7 is currently standard Guid.CreateVersion7 in .NET 9 preview, standard Guid is fine
            
            if (!string.IsNullOrEmpty(req.SourcePath))
            {
                string libraryRoot = @"M:\Picturebot-Test"; // Configurable path ideally
                string albumRoot = Path.Combine(libraryRoot, newNode.Uuid);

                var standardFolders = new[] { "RAWs", "JPGs" };
                foreach (var fName in standardFolders)
                {
                    newNode.SubFolders.Add(new SubFolder
                    {
                        Name = fName,
                        Location = Path.Combine(albumRoot, fName)
                    });
                }

                // Create Directories
                Directory.CreateDirectory(albumRoot);
                foreach (var sub in newNode.SubFolders)
                {
                    Directory.CreateDirectory(sub.Location);
                }
            }
        }

        await _repo.CreateAsync(newNode);

        // Import Process
        if (typeEnum == HierarchyType.Album && !string.IsNullOrEmpty(req.SourcePath))
        {
            // Fire and forget, or await depending on requirement. Usually async background task.
            // For now, we await it to match Go behavior.
            await ProcessAndImportPicturesAsync(req.SourcePath, newNode);
        }

        return newNode;
    }

    public async Task<List<Hierarchy>> GetFullHierarchyAsync()
    {
        var allNodes = await _repo.FindAllAsync();
        var nodeMap = allNodes.ToDictionary(n => n.Id);
        var rootNodes = new List<Hierarchy>();

        foreach (var node in allNodes)
        {
            // Clear children first because EF might have tracked them
            node.Children = new List<Hierarchy>(); 

            if (node.ParentId.HasValue && nodeMap.ContainsKey(node.ParentId.Value))
            {
                nodeMap[node.ParentId.Value].Children.Add(node);
            }
            else
            {
                rootNodes.Add(node);
            }
        }

        return rootNodes;
    }

    // -- Import Logic --

    private async Task ProcessAndImportPicturesAsync(string sourceDir, Hierarchy hierarchy)
    {
        _logger.LogInformation("Starting import for album {AlbumName}", hierarchy.Name);
        
        var dirInfo = new DirectoryInfo(sourceDir);
        if (!dirInfo.Exists) return;

        // Group files by BaseName
        var files = dirInfo.GetFiles().Where(f => !f.Attributes.HasFlag(FileAttributes.Directory));
        var groups = files.GroupBy(f => Path.GetFileNameWithoutExtension(f.Name))
                          .Select(g => new { 
                              BaseName = g.Key, 
                              Files = g.ToList(), 
                              SortTime = g.Min(f => f.LastWriteTime) // Simple sort logic
                          })
                          .OrderBy(g => g.SortTime)
                          .ToList();

        var subFolderMap = hierarchy.SubFolders.ToDictionary(sf => sf.Name, sf => sf);
        int counter = 1;

        foreach (var group in groups)
        {
            string newIndexStr = counter.ToString("D6"); // 000001
            
            foreach (var file in group.Files)
            {
                string ext = file.Extension;
                string upperExt = ext.ToUpperInvariant();
                string targetFolder = "JPGs";
                PictureType pType = PictureType.Display;

                if (new[] { ".ARW", ".CR2", ".NEF" }.Contains(upperExt))
                {
                    targetFolder = "RAWs";
                    pType = PictureType.Raw;
                }

                if (!subFolderMap.ContainsKey(targetFolder)) continue;

                var destFolder = subFolderMap[targetFolder];
                string newFileName = newIndexStr + ext;
                string destPath = Path.Combine(destFolder.Location, newFileName);

                // Copy File
                File.Copy(file.FullName, destPath, true);

                // Calculations
                int sharpness = 0;
                long pHash = 0;

                if (pType == PictureType.Display)
                {
                    // Call Image Processing Helpers
                    try 
                    {
                        sharpness = CalculateSobelSharpness(destPath);
                        // pHash = CalculatePHash(destPath); // Requires specific pHash lib
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Image processing failed for {File}: {Error}", destPath, ex.Message);
                    }
                }

                var pic = new Picture
                {
                    Index = newIndexStr,
                    FileName = newFileName,
                    Extension = ext,
                    Type = pType,
                    Location = destPath,
                    SubFolderId = destFolder.Id,
                    Sharpness = sharpness,
                    PHash = pHash
                };

                await _pictureRepo.CreateAsync(pic);
            }
            counter++;
        }
    }

    // Simplified Sobel implementation using ImageSharp
    private int CalculateSobelSharpness(string path)
    {
        using var image = Image.Load<L8>(path); // Load as Grayscale directly
        
        // Resize for performance (similar to Go implementation)
        if (image.Width > 600)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(600, 0),
                Mode = ResizeMode.Max
            }));
        }

        // Simplistic gradient calculation placeholder
        // A full Sobel kernel convolution manually in C# is verbose.
        // For production, consider using OpenCVSharp or a dedicated filter in ImageSharp.
        return 0; // Placeholder
    }
}