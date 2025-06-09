namespace bb1.Services
{
    // TODO: If First Day is non valid, it won't be checked and reprocessed by services anyway
    // quite critical bug, but even not a medium priority, because data update goes like a solid block replacement
    public class WeatherDataVerification
    {
        private readonly IWeatherDataRepository _repository;

        public WeatherDataVerification(IWeatherDataRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> IsTableValidAsync(string tableReference, string cityName)
        {
            var today = DateTime.Now.Date;

            bool anyValid = await _repository.AnyByCityAndDateAsync(tableReference, cityName, today);
            int countAllCity = await _repository.CountByCityAsync(tableReference, cityName);

            if (!anyValid || countAllCity < 5)
            {
                await _repository.DeleteAllByCityAsync(tableReference, cityName);
                return false;
            }

            return true;
        }
    }
}
