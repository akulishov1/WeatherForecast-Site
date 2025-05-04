using Microsoft.EntityFrameworkCore;

namespace bb1.Components.Models
{
    public class DailyForecast
    {
        public double Temperature { get; set; }
        public double TemperatureFeels { get; set; }
        public double MinTemperature { get; set; }
        public double MaxTemperature { get; set; }
        public double WindSpeed { get; set; }
    }

    

    public class WeatherRecord
    {
        public int Id { get; set; }
        public DateTime CellDate { get; set; }
        public double Temperature { get; set; }
        public double TemperatureFeels { get; set; }
        public double MinTemperature { get; set; }
        public double MaxTemperature { get; set; }
        public double WindSpeed { get; set; }
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
