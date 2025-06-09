using bb1.Components.Models;

namespace bb1.Services
{
    public interface IWeatherDataProcessor
    {
        List<T> MapToRecordsSimple<T>(WeatherDataCollection weatherData, Func<T> recordFactory, string cityName) where T : WeatherRecordBase;
    }
    public class WeatherProcessorService : IWeatherDataProcessor
    {
        public List<T> MapToRecordsSimple<T>(WeatherDataCollection weatherData, Func<T> recordFactory, string cityName) where T : WeatherRecordBase
        {
            var records = new List<T>();
            DateTime extractionDate = DateTime.Now.Date;

            int maxDays = 5;
            int slotsPerDay = 4;

            for (int day = 0; day < maxDays; day++)
            {
                int startIndex = day * slotsPerDay;
                var dayForecasts = weatherData.List.Skip(startIndex).Take(slotsPerDay).ToList();

                if (dayForecasts.Count != slotsPerDay)
                    break;

                var temps = dayForecasts.Select(f => Math.Truncate(f.Temperature)).ToArray();
                var winds = dayForecasts.Select(f => Math.Truncate(f.WindSpeed)).ToArray();

                var record = recordFactory();
                record.CellDate = extractionDate;

                record.TemperatureMorn = temps[0];
                record.WindSpeedMorn = winds[0];

                record.TemperatureDay = temps[1];
                record.WindSpeedDay = winds[1];

                record.TemperatureEve = temps[2];
                record.WindSpeedEve = winds[2];

                record.TemperatureNight = temps[3];
                record.WindSpeedNight = winds[3];

                record.AvgTemperature = Math.Truncate(temps.Average());
                record.AvgWindSpeed = Math.Truncate(winds.Average());
                record.MinTemperature = temps.Min();
                record.MaxTemperature = temps.Max();

                record.City = cityName;  // Заполняем название города

                records.Add(record);
                extractionDate = extractionDate.AddDays(1);
            }

            return records;
        }
    }
}