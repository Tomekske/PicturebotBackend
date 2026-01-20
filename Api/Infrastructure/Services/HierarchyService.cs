using System.Diagnostics;
using Api.Application.DTOs;
using Api.Application.Interfaces;
using Api.Core.Entities;
using Api.Core.Enums;
using Api.Core.Interfaces;
using CoenM.ImageHash.HashAlgorithms;
using OpenCvSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Size = OpenCvSharp.Size;

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
            var sw = Stopwatch.StartNew();
            await ProcessAndImportPicturesAsync(req.SourcePath, newNode);
            logger.LogInformation("Import took {TotalSeconds:F0} seconds ({Min} min and {Sec} s)", 
                sw.Elapsed.TotalSeconds, 
                sw.Elapsed.Minutes, 
                sw.Elapsed.Seconds);
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
                var pHash = 0UL;

                if (pType == PictureType.Display)
                {
                    try
                    {
                        pHash = HashImage(destPath);
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

    private static ulong HashImage(string filename)
    {
        var hashAlgorithm = new PerceptualHash();
        
        using (var image = Image.Load<Rgba32>(filename))
        {
            return hashAlgorithm.Hash(image);
        }
    }

    private int CalculateSobelSharpness(string path)
    {
        // 1. Load image as Grayscale immediately (ImreadModes.Grayscale)
        using OpenCvSharp.Mat src = Cv2.ImRead(path, ImreadModes.Grayscale);

        // 2. Resize if needed
        // OpenCV resizing is extremely fast.
        if (src.Width > 600)
        {
            double scale = 600.0 / src.Width;
            Cv2.Resize(src, src, new Size(0, 0), scale, scale);
        }

        // 3. Compute Sobel gradients
        // We compute derivatives in x and y directions
        using Mat dx = new Mat();
        using Mat dy = new Mat();

        // ddepth: CV_16S (16-bit signed) to avoid overflow during calculation
        Cv2.Sobel(src, dx, MatType.CV_16S, 1, 0, ksize: 3);
        Cv2.Sobel(src, dy, MatType.CV_16S, 0, 1, ksize: 3);

        // 4. Calculate Magnitude
        // Convert back to absolute values and add them (Approximation) or use Magnitude function
        using Mat absDx = new Mat();
        using Mat absDy = new Mat();
        Cv2.ConvertScaleAbs(dx, absDx);
        Cv2.ConvertScaleAbs(dy, absDy);

        using Mat edges = new Mat();
        Cv2.AddWeighted(absDx, 0.5, absDy, 0.5, 0, edges);

        // 5. Get the average intensity (The Sharpness Score)
        OpenCvSharp.Scalar mean = Cv2.Mean(edges);

        return (int)mean.Val0;
    }
}