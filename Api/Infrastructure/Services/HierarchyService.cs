using Api.Application.DTOs;
using Api.Application.Interfaces;
using Api.Core.Entities;
using Api.Core.Enums;
using Api.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Api.Infrastructure.Services;

public class HierarchyService(
    IHierarchyRepository repo,
    IPictureRepository pictureRepo,
    ILogger<HierarchyService> logger
) : IHierarchyService
{
    public async Task<Hierarchy> CreateNodeAsync(CreateNodeRequest req)
    {
        if (!Enum.TryParse<HierarchyType>(req.Type, true, out var typeEnum))
        {
            throw new ArgumentException("Invalid hierarchy type");
        }

        if (typeEnum == HierarchyType.Folder)
        {
            int? pid = req.ParentId == 0 ? null : req.ParentId;
            if (await repo.FindDuplicateAsync(pid, req.Name, typeEnum))
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
            newNode.Uuid = Guid.CreateVersion7().ToString();

            if (!string.IsNullOrEmpty(req.SourcePath))
            {
                const string libraryRoot = @"M:\Picturebot-Test";
                var albumRoot = Path.Combine(libraryRoot, newNode.Uuid);

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

        await repo.CreateAsync(newNode);

        if (typeEnum == HierarchyType.Album && !string.IsNullOrEmpty(req.SourcePath))
        {
            await ProcessAndImportPicturesAsync(req.SourcePath, newNode);
        }

        return newNode;
    }

    public async Task<List<Hierarchy>> GetFullHierarchyAsync()
    {
        var allNodes = await repo.FindAllAsync();
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
        logger.LogInformation("Starting import for album {AlbumName}", hierarchy.Name);

        var dirInfo = new DirectoryInfo(sourceDir);
        if (!dirInfo.Exists) return;

        var files = dirInfo.GetFiles().Where(f => !f.Attributes.HasFlag(FileAttributes.Directory));
        var groups = files.GroupBy(f => Path.GetFileNameWithoutExtension(f.Name))
            .Select(g => new
            {
                BaseName = g.Key,
                Files = g.ToList(),
                SortTime = g.Min(f => f.LastWriteTime)
            })
            .OrderBy(g => g.SortTime)
            .ToList();

        var subFolderMap = hierarchy.SubFolders.ToDictionary(sf => sf.Name, sf => sf);
        var counter = 1;

        foreach (var group in groups)
        {
            var newIndexStr = counter.ToString("D6");

            foreach (var file in group.Files)
            {
                var ext = file.Extension;
                var upperExt = ext.ToUpperInvariant();
                var targetFolder = "JPGs";
                var pType = PictureType.Display;

                if (new[] { ".ARW", ".CR2", ".NEF" }.Contains(upperExt))
                {
                    targetFolder = "RAWs";
                    pType = PictureType.Raw;
                }

                if (!subFolderMap.ContainsKey(targetFolder)) continue;

                var destFolder = subFolderMap[targetFolder];
                var newFileName = newIndexStr + ext;
                var destPath = Path.Combine(destFolder.Location, newFileName);

                File.Copy(file.FullName, destPath, true);

                var sharpness = 0;
                var pHash = 0;

                if (pType == PictureType.Display)
                {
                    try
                    {
                        sharpness = CalculateSobelSharpness(destPath);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning("Image processing failed for {File}: {Error}", destPath, ex.Message);
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

                await pictureRepo.CreateAsync(pic);
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