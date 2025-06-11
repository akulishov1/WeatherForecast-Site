using System.Linq.Expressions;
using bb1.Components.Models;
using bb1.Services.WeatherInterfaces;
using Microsoft.EntityFrameworkCore;

namespace bb1.Services
{
    public class WeatherRecordsService : IWeatherDataRepository
    {
        private readonly WeatherDbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly IWeatherDataProcessor _processor;
        private readonly IDbContextFactory<WeatherDbContext> _contextFactory;
        private readonly IWeatherDRService _weatherService;
        
        public WeatherRecordsService(
        WeatherDbContext dbContext,
        IServiceProvider serviceProvider,
        IWeatherDataProcessor processor,
        IDbContextFactory<WeatherDbContext> contextFactory,
        IWeatherDRService weatherService) 
        {
            _dbContext = dbContext;
            _serviceProvider = serviceProvider;
            _processor = processor;
            _contextFactory = contextFactory;
            _weatherService = weatherService;
        }
        public TimeSpan GetServerTime() => DateTime.Now.TimeOfDay;
        // Получить DbSet по имени таблицы из ApiRecords через Reflection
        public IQueryable<WeatherRecordBase> GetRecordsQueryable(string tableReference)
        {
            var context = _contextFactory.CreateDbContext();

            var property = context.GetType().GetProperty(tableReference);
            if (property == null)
                throw new ArgumentException($"TableReference '{tableReference}' not found in DbContext");

            var dbSet = property.GetValue(context);

            var queryable = dbSet as IQueryable;

            if (queryable == null)
                throw new InvalidOperationException($"Property {tableReference} is not IQueryable");

            return queryable.Cast<WeatherRecordBase>().AsNoTracking(); 
        }
        public async Task<bool> AnyByCityAndFutureDatesAsync(string tableReference, string cityName, DateTime today)
        {
            var query = GetRecordsQueryable(tableReference);
            return await query.AnyAsync(r =>
                EF.Property<string>(r, "City") == cityName &&
                EF.Property<DateTime>(r, "CellDate") >= today);
        }

        public async Task<int> CountByCityAndFutureDatesAsync(string tableReference, string cityName, DateTime today)
        {
            var query = GetRecordsQueryable(tableReference);
            return await query.CountAsync(r =>
                EF.Property<string>(r, "City") == cityName &&
                EF.Property<DateTime>(r, "CellDate") >= today);
        }

        public async Task DeleteAllByCityAsync(string tableReference, string cityName)
        {
            var query = GetRecordsQueryable(tableReference).Where(r => EF.Property<string>(r, "City") == cityName);
            var entities = await query.ToListAsync();

            if (entities.Count == 0)
                return;

            var property = _dbContext.GetType().GetProperty(tableReference);
            if (property == null)
                throw new ArgumentException($"TableReference '{tableReference}' not found in DbContext");

            var dbSet = property.GetValue(_dbContext);

            // Получаем точный метод RemoveRange(IEnumerable<TEntity>)
            var entityType = dbSet!.GetType().GenericTypeArguments[0];
            var removeRangeMethod = dbSet.GetType().GetMethod("RemoveRange", new Type[] { typeof(IEnumerable<>).MakeGenericType(entityType) });
            if (removeRangeMethod == null)
                throw new InvalidOperationException("RemoveRange method not found");

            // Приводим entities к IEnumerable<TEntity>
            var castMethod = typeof(Enumerable).GetMethod("Cast")!.MakeGenericMethod(entityType);
            var castedEntities = castMethod.Invoke(null, new object[] { entities });

            removeRangeMethod.Invoke(dbSet, new object[] { castedEntities });

            await _dbContext.SaveChangesAsync();
        }
        public async Task<string> GetTableReferenceBySiteIdAsync(int siteId)
        {
            using var context = _contextFactory.CreateDbContext();
            var record = await context.ApiRecords.FirstOrDefaultAsync(r => r.Id == siteId);
            if (record == null)
                throw new Exception($"Site with Id {siteId} not found");

            return record.TableReference;
        }
        public async Task<List<CityRecord>> GetListOfAllCitiesAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.CityRecords
                .OrderBy(r => r.Id)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<(double Latitude, double Longitude)?> GetCityCoordinatesAsync(int cityId)
        {
            using var context = _contextFactory.CreateDbContext();
            var coords = await context.CityRecords
                .AsNoTracking()
                .Where(c => c.Id == cityId)
                .Select(c => new { c.Latitude, c.Longitude })
                .FirstOrDefaultAsync();

            if (coords == null)
                return null;

            return (coords.Latitude, coords.Longitude);
        }
        public async Task<List<ApiRecord>> GetListOfAllSitesAsync()
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.ApiRecords
                .OrderBy(r => r.Id)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<bool> EnsureWeatherDataUpToDateAsync(int siteId, int cityId)
        {
            using var context = _contextFactory.CreateDbContext();
            var record = await context.ApiRecords.FirstOrDefaultAsync(r => r.Id == siteId);
            if (record == null)
                throw new Exception($"Site with Id {siteId} not found");

            var tableReference = record.TableReference;
            var apiKey = record.Key;

            var city = await context.CityRecords.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cityId);
            if (city == null)
                throw new Exception($"City with Id {cityId} not found");

            var verificationService = _serviceProvider.GetRequiredService<WeatherDataVerification>();
            var isValid = await verificationService.IsTableValidAsync(tableReference, city.CityName);
            if (isValid)
                return true;

            float latitude = (float)city.Latitude;
            float longitude = (float)city.Longitude;

            var data = await _weatherService.FetchWeatherDataAsync(apiKey, latitude, longitude);
            await SaveWeatherDataAsync(tableReference, data, city.CityName);

            var recheck = await verificationService.IsTableValidAsync(tableReference, city.CityName);
            return recheck;
        }

        public async Task<Dictionary<int, List<WeatherRecordBase>>> GetWeatherDataListAsync(int siteId, int cityId)
        {
            var tableReference = await GetTableReferenceBySiteIdAsync(siteId);

            var city = await _contextFactory.CreateDbContext().CityRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == cityId);
            if (city == null)
                throw new Exception($"City with Id {cityId} not found");

            var cityName = city.CityName;

            var query = GetRecordsQueryable(tableReference);

            // Фильтруем по названию города (City)
            var filteredQuery = query.Where(r => EF.Property<string>(r, "City") == cityName);

            var records = await filteredQuery
                .OrderBy(r => r.CellDate)
                .ToListAsync();

            var grouped = records
                .GroupBy(r => (r.CellDate - DateTime.Today).Days + 1)
                .Where(g => g.Key >= 1 && g.Key <= 5)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(r => r.CellDate).ToList()
                );

            return grouped;
        }
        public async Task SaveWeatherDataAsync(string tableReference, WeatherDataCollection data, string cityName)
        {
            var property = _dbContext.GetType().GetProperty(tableReference);
            if (property == null)
                throw new ArgumentException($"TableReference '{tableReference}' not found in DbContext");

            var entityType = property.PropertyType.GenericTypeArguments[0];

            // Получаем обобщённый метод MapToRecordsSimple
            var mapMethodGeneric = typeof(IWeatherDataProcessor).GetMethod("MapToRecordsSimple");
            if (mapMethodGeneric == null)
                throw new InvalidOperationException("MapToRecordsSimple method not found");

            // Создаём конкретный метод с нужным типом
            var mapMethod = mapMethodGeneric.MakeGenericMethod(entityType);

            // Создаём делегат Func<entityType> для создания новых экземпляров сущности
            var newExpr = Expression.New(entityType);
            var lambda = Expression.Lambda(typeof(Func<>).MakeGenericType(entityType), newExpr);
            var recordFactoryTyped = lambda.Compile();

            // Вызываем MapToRecordsSimple, получаем список записей
            var records = mapMethod.Invoke(_processor, new object[] { data, recordFactoryTyped, cityName })!;

            var dbSet = property.GetValue(_dbContext);

            // Получаем метод AddRangeAsync<TEntity>(IEnumerable<TEntity>, CancellationToken)
            var addRangeMethod = dbSet.GetType().GetMethod("AddRangeAsync", new Type[] { typeof(IEnumerable<>).MakeGenericType(entityType), typeof(CancellationToken) });
            if (addRangeMethod == null)
                throw new InvalidOperationException("AddRangeAsync method not found");

            // Запускаем AddRangeAsync
            var task = (Task)addRangeMethod.Invoke(dbSet, new object[] { records, CancellationToken.None })!;
            await task;

            await _dbContext.SaveChangesAsync();
        }
    }
}
