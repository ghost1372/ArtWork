using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtWork.Database.Tables;
public class Art
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string FolderName { get; set; }
    public string FileName { get; set; }
    public string Title { get; set; }
    public string Sig { get; set; }
    public string SimplifiedSig { get; set; }
    public string Gallery { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Wikiartist { get; set; }
}
