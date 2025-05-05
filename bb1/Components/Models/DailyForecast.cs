using Microsoft.EntityFrameworkCore;

namespace bb1.Components.Models
{
    public class DailyForecast
    {
        public double Temperature { get; set; }
        public double WindSpeed { get; set; }
    }

    

    public class WeatherRecord
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

        // Aggregated summary
        public double AvgTemperature { get; set; }
        public double AvgWindSpeed { get; set; }
        public double MinTemperature { get; set; }
        public double MaxTemperature { get; set; }
    }
    

    public class WeatherDbContext : DbContext
    {
        public WeatherDbContext(DbContextOptions<WeatherDbContext> options)
            : base(options)
        {

        }
        public DbSet<WeatherRecord> WeatherRecords { get; set; }

    }
}
