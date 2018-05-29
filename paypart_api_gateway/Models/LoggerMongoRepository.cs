using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using paypart_api_gateway.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace paypart_logger_service.Model
{
    public class LoggerMongoRepository : ILoggerMongoRepository
    {
        private readonly LoggerMongoContext _context = null;
        private readonly IOptions<MongoSettings> _settings;
        public LoggerMongoRepository(IOptions<MongoSettings> settings)
        {
            _settings = settings;
            _context = new LoggerMongoContext(_settings);
        }

        public async Task<LogDto> GetLog(string id)
        {
            var filter = Builders<LogDto>.Filter.Eq("Id", id);
            return await _context.Logger
                                 .Find(filter)
                                 .FirstOrDefaultAsync();
        }

        public async Task<LogDto> AddLog(LogDto item)
        {
            await _context.Logger.InsertOneAsync(item);
            return await GetLog(item.Id.ToString());
        }
    }
}
