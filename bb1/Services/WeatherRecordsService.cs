using bb1.Components.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace bb1.Services
{
    public class WeatherRecordsService
    {
        private readonly WeatherDbContext _context;

        public WeatherRecordsService(WeatherDbContext context)
        {
            _context = context;
        }

        public async Task<List<WeatherRecord>> GetDataByDayNumberAsync(int ID)
        {
            return await _context.WeatherRecords.AsNoTracking().Where(record => record.Id == ID).ToListAsync();
        }

        public TimeSpan GetServerTime() => DateTime.Now.TimeOfDay;

        public DateTime ServerDateCheck() => DateTime.Now.Date;
    }
}
