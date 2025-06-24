public class NewsData
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public string newUrl {get; set;}

    //Burası ai ile doldurulacak
    public string newSummary {get; set;}


    public void PrintNews()
    {
        Console.WriteLine($"Title: {Title}");
        Console.WriteLine($"Description: {Description}");
        Console.WriteLine($"ImageUrl: {ImageUrl}");
        Console.WriteLine($"newUrl: {newUrl}");
        Console.WriteLine($"newSummary: {newSummary}");
    }
}

public enum NewsSource
{
    BBC,
    CNN,
    GLOBAL,
    TASS,
    NEWYORK
}