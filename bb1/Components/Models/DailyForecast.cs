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
    }

    // Entity for OpenWeather source
    public class OpenWeatherRecord : WeatherRecordBase
    {
    }

    // Entity for OpenMeteo source
    [Table("OpenMeteoRecords")]
    public class OpenMeteoRecord : WeatherRecordBase
    {
    }

    // DbContext with both tables
    public class WeatherDbContext : DbContext
    {
        public WeatherDbContext(DbContextOptions<WeatherDbContext> options)
            : base(options)
        {
        }

        public DbSet<OpenWeatherRecord> OpenWeatherRecords { get; set; }
        public DbSet<OpenMeteoRecord> OpenMeteoRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OpenWeatherRecord>().ToTable("OpenWeatherRecords")
                .HasKey(r => r.Id);

            modelBuilder.Entity<OpenMeteoRecord>().ToTable("OpenMeteoRecords")
                .HasKey(r => r.Id);
        }
    }
}
