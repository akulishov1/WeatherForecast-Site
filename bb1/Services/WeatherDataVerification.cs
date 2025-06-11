namespace bb1.Services
{
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

            bool anyValid = await _repository.AnyByCityAndFutureDatesAsync(tableReference, cityName, today);
            int countAllCity = await _repository.CountByCityAndFutureDatesAsync(tableReference, cityName, today);

            if (!anyValid || countAllCity < 5)
            {
                await _repository.DeleteAllByCityAsync(tableReference, cityName);
                return false;
            }

            return true;
        }
    }
}
