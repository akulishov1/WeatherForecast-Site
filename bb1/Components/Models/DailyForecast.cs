using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace bb1.Components.Models
{
    public class WeatherDataCollection
    {
        public List<DailyForecast> List { get; set; } = new();
    }

    public class DailyForecast
    {
        public double Temperature { get; set; }
        public double WindSpeed { get; set; }
    }

    // Shared base class for all weather records
    public abstract class WeatherRecordBase
    {
        public int Id { get; set; }
        public DateTime CellDate { get; set; }

        public double TemperatureMorn { get; set; }
        public double WindSpeedMorn { get; set; }

        public double TemperatureDay { get; set; }
        public double WindSpeedDay { get; set; }

        public double TemperatureEve { get; set; }
        public double WindSpeedEve { get; set; }

        public double TemperatureNight { get; set; }
        public double WindSpeedNight { get; set; }

        public double AvgTemperature { get; set; }
        public double AvgWindSpeed { get; set; }
        public double MinTemperature { get; set; }
        public double MaxTemperature { get; set; }
        public string City {  get; set; }
    }
    public class ApiRecord
    {
        public int Id { get; set; }
        public string Key { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string BaseApiUrl { get; set; } = null!;
        public bool IsEnabled { get; set; }
        public string? ApiKey { get; set; }
        public string? TableReference { get; set; }
        public string? ApiParserReference { get; set; }
    }
    public class CityRecord
    {
        public int Id { get; set; }
        public string CityName { set; get; } = null!;
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }
    // Entity for OpenWeather source
    [Table("OpenWeatherRecords")]
    public class OpenWeatherRecord : WeatherRecordBase
    {
    }

    // Entity for OpenMeteo source
    [Table("OpenMeteoRecords")]
    public class OpenMeteoRecord : WeatherRecordBase
    {
    }

    public class WeatherDbContext : DbContext
    {
        public WeatherDbContext(DbContextOptions<WeatherDbContext> options)
            : base(options)
        {
        }

        public DbSet<OpenWeatherRecord> OpenWeatherRecords { get; set; }
        public DbSet<OpenMeteoRecord> OpenMeteoRecords { get; set; }
        public DbSet<ApiRecord> ApiRecords { get; set; }
        public DbSet<CityRecord> CityRecords { get; set; }
    }
}
