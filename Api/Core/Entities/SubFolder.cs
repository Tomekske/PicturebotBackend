using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Api.Core.Enums;

namespace Api.Core.Entities;

[Table("sub_folders")]
public class SubFolder
{
    [Key]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("location")]
    public string Location { get; set; } = string.Empty;

    // Belongs To Relation (Hierarchy)
    [Column("hierarchy_id")]
    public int HierarchyId { get; set; }

    [ForeignKey(nameof(HierarchyId))]
    public Hierarchy? Hierarchy { get; set; }

    // Has Many Relation (Pictures)
    public ICollection<Picture> Pictures { get; set; } = new List<Picture>();

    /// <summary>
    /// Helper method to create a Picture instance based on filename conventions.
    /// Ported from Go logic.
    /// </summary>
    public Picture CreatePicture(string filename, string index)
    {
        var ext = Path.GetExtension(filename);
        var upperExt = ext.ToUpperInvariant();
        var pType = PictureType.Unknown;

        switch (upperExt)
        {
            case ".JPG":
            case ".JPEG":
                pType = PictureType.Display;
                break;
            case ".ARW":
            case ".NEF":
                pType = PictureType.Raw;
                break;
        }

        var fullPath = Path.Combine(this.Location, filename);

        return new Picture
        {
            Index = index,
            FileName = filename,
            Extension = ext,
            Type = pType,
            Location = fullPath,
            SubFolderId = this.Id
        };
    }
}