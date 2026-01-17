using Api.Application.DTOs;
using Api.Application.Interfaces; 
using Api.Core.Entities;
using Api.Core.Enums;
using Api.Core.Interfaces; 
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Api.Infrastructure.Services;

public class HierarchyService : IHierarchyService
{
    private readonly IHierarchyRepository _repo;
    private readonly IPictureRepository _pictureRepo;
    private readonly ILogger<HierarchyService> _logger;

    public HierarchyService(
        IHierarchyRepository repo, 
        IPictureRepository pictureRepo, 
        ILogger<HierarchyService> logger)
    {
        _repo = repo;
        _pictureRepo = pictureRepo;
        _logger = logger;
    }

    public async Task<Hierarchy> CreateNodeAsync(CreateNodeRequest req)
    {
        if (!Enum.TryParse<HierarchyType>(req.Type, true, out var typeEnum))
        {
            throw new ArgumentException("Invalid hierarchy type");
        }

        // Duplicate Check
        if (typeEnum == HierarchyType.Folder)
        {
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

        if (typeEnum == HierarchyType.Album)
        {
            newNode.Uuid = Guid.NewGuid().ToString(); 
            
            if (!string.IsNullOrEmpty(req.SourcePath))
            {
                string libraryRoot = @"M:\Picturebot-Test"; 
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

                Directory.CreateDirectory(albumRoot);
                foreach (var sub in newNode.SubFolders)
                {
                    Directory.CreateDirectory(sub.Location);
                }
            }
        }

        await _repo.CreateAsync(newNode);

        if (typeEnum == HierarchyType.Album && !string.IsNullOrEmpty(req.SourcePath))
        {
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

    private async Task ProcessAndImportPicturesAsync(string sourceDir, Hierarchy hierarchy)
    {
        _logger.LogInformation("Starting import for album {AlbumName}", hierarchy.Name);
        
        var dirInfo = new DirectoryInfo(sourceDir);
        if (!dirInfo.Exists) return;

        var files = dirInfo.GetFiles().Where(f => !f.Attributes.HasFlag(FileAttributes.Directory));
        var groups = files.GroupBy(f => Path.GetFileNameWithoutExtension(f.Name))
                          .Select(g => new { 
                              BaseName = g.Key, 
                              Files = g.ToList(), 
                              SortTime = g.Min(f => f.LastWriteTime)
                          })
                          .OrderBy(g => g.SortTime)
                          .ToList();

        var subFolderMap = hierarchy.SubFolders.ToDictionary(sf => sf.Name, sf => sf);
        int counter = 1;

        foreach (var group in groups)
        {
            string newIndexStr = counter.ToString("D6");
            
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

                File.Copy(file.FullName, destPath, true);

                int sharpness = 0;
                long pHash = 0;

                if (pType == PictureType.Display)
                {
                    try 
                    {
                        sharpness = CalculateSobelSharpness(destPath);
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

    private int CalculateSobelSharpness(string path)
    {
        using var image = Image.Load<L8>(path); 
        
        if (image.Width > 600)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(600, 0),
                Mode = ResizeMode.Max
            }));
        }
        return 0; 
    }
}