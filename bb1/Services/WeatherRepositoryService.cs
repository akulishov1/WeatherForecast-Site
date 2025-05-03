using bb1.Components.Models;
using Microsoft.EntityFrameworkCore;
using static bb1.Services.WeatherDataRequestService;

namespace bb1.Services
{
    public class WeatherRepositoryService
    {
        private readonly WeatherDbContext _dbContext;

        public async Task<bool> IsTableEmptyAsync()
        {
            return !await _dbContext.WeatherRecords.AnyAsync();
        }
        public WeatherRepositoryService(WeatherDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<bool> IsWeatherDataUpToDateAsync()
        {
            var todayDate = DateTime.Now.Date;

            var record = await _dbContext.WeatherRecords
                .Where(r => r.Id == 1)
                .Select(r => r.CellDate)
                .FirstOrDefaultAsync();

            return record == todayDate;
        }
        public async Task SaveWeatherDataAsync(WeatherData weatherData)
        {
            await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE WeatherRecords");

            DateTime ExtractionDataDate = DateTime.Now.Date; // Start with today's date

            foreach (var forecast in weatherData.List)
            {
                var weatherRecord = new WeatherRecord
                {
                    CellDate = ExtractionDataDate, // Assign the correct date
                    Temperature = forecast.Temperature - 273.15,
                    TemperatureFeels = forecast.TemperatureFeels - 273.15,
                    MinTemperature = forecast.MinTemperature - 273.15,
                    MaxTemperature = forecast.MaxTemperature - 273.15,
                    WindSpeed = forecast.WindSpeed
                };

                await _dbContext.WeatherRecords.AddAsync(weatherRecord);
                ExtractionDataDate = ExtractionDataDate.AddDays(1); // Move to the next day
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
