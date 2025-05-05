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
            // Remove previous data (you may want to handle this differently in production)
            await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE WeatherRecords");

            DateTime extractionDate = DateTime.Now.Date;

            // Loop through 5 days
            for (int i = 0; i < 5; i++)
            {
                // Calculate the starting index for the current group of 4 forecasts.
                int startIndex = i * 4;
                var dayForecasts = weatherData.List.Skip(startIndex).Take(4).ToList();

                // Ensure we have exactly 4 records for the day.
                if (dayForecasts.Count != 4)
                    continue;

                // Remove fractional parts rather than rounding.
                double tempMorn = Math.Truncate(dayForecasts[0].Temperature - 273.15);
                double tempDay = Math.Truncate(dayForecasts[1].Temperature - 273.15);
                double tempEve = Math.Truncate(dayForecasts[2].Temperature - 273.15);
                double tempNight = Math.Truncate(dayForecasts[3].Temperature - 273.15);

                // Remove fractions for wind speeds.
                double windMorn = Math.Truncate(dayForecasts[0].WindSpeed);
                double windDay = Math.Truncate(dayForecasts[1].WindSpeed);
                double windEve = Math.Truncate(dayForecasts[2].WindSpeed);
                double windNight = Math.Truncate(dayForecasts[3].WindSpeed);

                // Calculate summary stats with the truncated values.
                double avgTemp = Math.Truncate((tempMorn + tempDay + tempEve + tempNight) / 4);
                double avgWind = Math.Truncate((windMorn + windDay + windEve + windNight) / 4);
                double minTemp = new double[] { tempMorn, tempEve, tempNight, tempDay }.Min();
                double maxTemp = new double[] { tempMorn, tempEve, tempNight, tempDay }.Max();

                // Create a new record with the “Morn”, “Day”, “Eve”, “Night” values and summaries.
                var weatherRecord = new WeatherRecord
                {
                    CellDate = extractionDate,
                    AvgTemperature = avgTemp,
                    AvgWindSpeed = avgWind,
                    TemperatureMorn = tempMorn,
                    WindSpeedMorn = windMorn,
                    TemperatureDay = tempDay,
                    WindSpeedDay = windDay,
                    TemperatureEve = tempEve,
                    WindSpeedEve = windEve,
                    TemperatureNight = tempNight,
                    WindSpeedNight = windNight,
                    MinTemperature = minTemp,
                    MaxTemperature = maxTemp
                };

                await _dbContext.WeatherRecords.AddAsync(weatherRecord);

                // Move the date to the next day.
                extractionDate = extractionDate.AddDays(1);
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
