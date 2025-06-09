using bb1.Components.Models;
using bb1.Services.WDRequestListFormats;
using System.Text.Json;
using bb1.Services.WeatherInterfaces;
using Microsoft.EntityFrameworkCore;
namespace bb1.Services
{

    public class WeatherDataRequestService : IWeatherService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly WeatherDbContext _dbContext;
        private readonly Dictionary<string, IWeatherDataParser> _parsers;

        public WeatherDataRequestService(
            IHttpClientFactory httpClientFactory,
            WeatherDbContext dbContext,
            IEnumerable<IWeatherDataParser> parsers)
        {
            _httpClientFactory = httpClientFactory;
            _dbContext = dbContext;

            _parsers = parsers.ToDictionary(
                p => p.GetType().Name.Replace("Parser", ""),
                StringComparer.OrdinalIgnoreCase);
        }

        public async Task<WeatherDataCollection> FetchWeatherDataAsync(string siteKey,float latitude,float longitude)
        {
            var api = await _dbContext.ApiRecords
        .AsNoTracking()
        .FirstOrDefaultAsync(r => r.Key == siteKey);

            if (api == null)
                throw new InvalidOperationException($"API config for key '{siteKey}' not found.");

            string url = api.BaseApiUrl
                .Replace("{latitude}", latitude.ToString(System.Globalization.CultureInfo.InvariantCulture))
                .Replace("{longitude}", longitude.ToString(System.Globalization.CultureInfo.InvariantCulture));

            if (api.IsEnabled && !string.IsNullOrWhiteSpace(api.ApiKey))
            {
                url = url.Replace("{ApiKey}", api.ApiKey);
            }
            else
            {
                url = url.Replace("{ApiKey}", string.Empty);
            }

            url = url.Replace("&&", "&").TrimEnd('&', '?');

            if (url.StartsWith("$"))
            {
                url = url.Substring(1);
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"API error {(int)response.StatusCode}: {response.ReasonPhrase}. Body: {errorBody}");
            }

            var rawContent = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(rawContent))
                throw new InvalidOperationException($"API '{siteKey}' returned empty response.");

            JsonDocument jsonDoc;
            try
            {
                jsonDoc = JsonDocument.Parse(rawContent);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"API '{siteKey}' response is not valid JSON:\n{rawContent}", ex);
            }

            var root = jsonDoc.RootElement;

            string parserKey = api.ApiParserReference?.Replace("Parser", "") ?? "";

            if (!_parsers.TryGetValue(parserKey, out var parser))
                throw new InvalidOperationException($"Parser '{api.ApiParserReference}' is not registered.");

            return parser.Parse(root);
        }
    }
}
