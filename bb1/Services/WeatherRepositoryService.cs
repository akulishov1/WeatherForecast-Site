using bb1.Services.WDSources;
using bb1.Components.Models;
using Microsoft.EntityFrameworkCore;
using static bb1.Services.WeatherDataRequestService;

namespace bb1.Services
{
    public class WeatherRepositoryService
    {

        private readonly WeatherDbContext _dbContext;
        private readonly IWeatherService _weatherService;

        public WeatherRepositoryService(WeatherDbContext dbContext, IWeatherService weatherService)
        {
            _dbContext = dbContext;
            _weatherService = weatherService;
        }
        public async Task<bool> IsTableEmptyAsync(int siteId)
        {
            return siteId switch
            {
                (int)WeatherSource.OpenWeather => !await _dbContext.OpenWeatherRecords.AnyAsync(),
                (int)WeatherSource.OpenMeteo => !await _dbContext.OpenMeteoRecords.AnyAsync(),
                _ => throw new ArgumentException("Unsupported site ID")
            };
        }
        public async Task EnsureWeatherDataUpToDateAsync(int SelectedSiteID)
        {
            bool isUpToDate = await IsWeatherDataUpToDateAsync(SelectedSiteID);

            if (!isUpToDate)
            {
                var weatherData = await _weatherService.FetchWeatherDataAsync(SelectedSiteID);
                await SaveWeatherDataAsync(SelectedSiteID, weatherData);
            }
        }

        public async Task<bool> IsWeatherDataUpToDateAsync(int SelectedSiteID)
        {
            var todayDate = DateTime.Now.Date;

            return SelectedSiteID switch
            {
                (int)WeatherSource.OpenWeather => await _dbContext.OpenWeatherRecords.AnyAsync(r => r.CellDate == todayDate),
                (int)WeatherSource.OpenMeteo => await _dbContext.OpenMeteoRecords.AnyAsync(r => r.CellDate == todayDate),
                _ => throw new ArgumentException("Unsupported site ID")
            };
        }
        public async Task SaveWeatherDataAsync(int SelectedSiteID, WeatherDataCollection weatherData)
        {
            DateTime extractionDate = DateTime.Now.Date;

            switch (SelectedSiteID)
            {
                case (int)WeatherSource.OpenWeather:
                    var openWeatherRecords = MapToOpenWeatherRecords(weatherData);

                    foreach (var entry in _dbContext.ChangeTracker.Entries<OpenWeatherRecord>())
                    {
                        entry.State = EntityState.Detached;
                    }

                    // Clear old data
                    await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE OpenWeatherRecords");
                    // Add new data
                    await _dbContext.OpenWeatherRecords.AddRangeAsync(openWeatherRecords);
                    break;

                case (int)WeatherSource.OpenMeteo:
                    var openMeteoRecords = MapToOpenMeteoRecords(weatherData);

                    foreach (var entry in _dbContext.ChangeTracker.Entries<OpenMeteoRecord>())
                    {
                        entry.State = EntityState.Detached;
                    }

                    await _dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE OpenMeteoRecords");
                    await _dbContext.OpenMeteoRecords.AddRangeAsync(openMeteoRecords);
                    break;

                default:
                    throw new ArgumentException("Unsupported site ID");
            }

            await _dbContext.SaveChangesAsync();
        }

        // Helper to create List<WeatherRecordBase> for insert
        private List<OpenWeatherRecord> MapToOpenWeatherRecords(WeatherDataCollection weatherData)
        {
            var records = new List<OpenWeatherRecord>();

            DateTime extractionDate = DateTime.Now.Date;

            for (int i = 0; i < 5; i++)
            {
                int startIndex = i * 4;
                var dayForecasts = weatherData.List.Skip(startIndex).Take(4).ToList();

                if (dayForecasts.Count != 4)
                    continue;

                var temp = dayForecasts.Select(f => Math.Truncate(f.Temperature)).ToArray();
                var wind = dayForecasts.Select(f => Math.Truncate(f.WindSpeed)).ToArray();

                double avgTemp = Math.Truncate(temp.Average());
                double avgWind = Math.Truncate(wind.Average());
                double minTemp = temp.Min();
                double maxTemp = temp.Max();

                var record = new OpenWeatherRecord
                {
                    CellDate = extractionDate,
                    AvgTemperature = avgTemp,
                    AvgWindSpeed = avgWind,

                    TemperatureMorn = temp[0],
                    WindSpeedMorn = wind[0],

                    TemperatureDay = temp[1],
                    WindSpeedDay = wind[1],

                    TemperatureEve = temp[2],
                    WindSpeedEve = wind[2],

                    TemperatureNight = temp[3],
                    WindSpeedNight = wind[3],

                    MinTemperature = minTemp,
                    MaxTemperature = maxTemp
                };

                records.Add(record);
                extractionDate = extractionDate.AddDays(1);
            }

            return records;
        }

        private List<OpenMeteoRecord> MapToOpenMeteoRecords(WeatherDataCollection weatherData)
        {
            var records = new List<OpenMeteoRecord>();
            DateTime extractionDate = DateTime.Now.Date;

            var filteredList = weatherData.List;

            int maxDays = 5;       // Max 5 days
            int slotsPerDay = 4;   // morning, day, evening, night

            for (int dayIndex = 0; dayIndex < maxDays; dayIndex++)
            {
                var dayTemps = new double[slotsPerDay];
                var dayWinds = new double[slotsPerDay];

                bool hasEnoughData = true;

                for (int slot = 0; slot < slotsPerDay; slot++)
                {
                    // Calculate the index in the filteredList:
                    // Start at item 6 (index 5),
                    // then take every 6th item for each slot
                    int dataIndex = 5 + dayIndex * 6 + slot * 6;

                    if (dataIndex >= filteredList.Count)
                    {
                        hasEnoughData = false;
                        break;
                    }

                    dayTemps[slot] = filteredList[dataIndex].Temperature;
                    dayWinds[slot] = filteredList[dataIndex].WindSpeed;
                }

                if (!hasEnoughData)
                    break;

                var record = new OpenMeteoRecord
                {
                    CellDate = extractionDate,
                    AvgTemperature = Math.Truncate(dayTemps.Average()),
                    AvgWindSpeed = Math.Truncate(dayWinds.Average()),

                    TemperatureMorn = Math.Truncate(dayTemps[0]),
                    WindSpeedMorn = Math.Truncate(dayWinds[0]),

                    TemperatureDay = Math.Truncate(dayTemps[1]),
                    WindSpeedDay = Math.Truncate(dayWinds[1]),

                    TemperatureEve = Math.Truncate(dayTemps[2]),
                    WindSpeedEve = Math.Truncate(dayWinds[2]),

                    TemperatureNight = Math.Truncate(dayTemps[3]),
                    WindSpeedNight = Math.Truncate(dayWinds[3]),

                    MinTemperature = Math.Truncate(dayTemps.Min()),
                    MaxTemperature = Math.Truncate(dayTemps.Max())
                };

                records.Add(record);
                extractionDate = extractionDate.AddDays(1);
            }

            return records;
        }

    }
}
