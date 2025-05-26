namespace bb1.Services.WDSources
{
    public enum WeatherSource
    {
        OpenWeather = 1,
        OpenMeteo = 2
    }

    public static class WeatherTableMap
    {
        public static string GetTableName(int siteId) => siteId switch
        {
            (int)WeatherSource.OpenWeather => "OpenWeatherRecords",
            (int)WeatherSource.OpenMeteo => "OpenMeteoRecords",
            _ => throw new ArgumentException("Unsupported site ID")
        };
    }
}
