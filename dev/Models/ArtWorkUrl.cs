namespace ArtWork.Models;
public class ArtWorkUrl
{
    public string ImageUrl { get; set; }
    public string JsonUrl { get; set; }

    public ArtWorkUrl(string jsonUrl, string imageUrl)
    {
        this.ImageUrl = imageUrl;
        this.JsonUrl = jsonUrl;
    }
}
