namespace bb1.Components.Models
{
    public class SiteID
    {
        public int id { get; set; }
        public string name { get; set; } = "";
    }
    public static class SiteDataSelectionList
    {
        public static List<SiteID> SiteSelection { get; } = new()
        {
            new SiteID {id = 1, name = "OpenWeatherMap" },
            new SiteID {id = 2, name = "Open-Meteo"}
        };
    }
}
