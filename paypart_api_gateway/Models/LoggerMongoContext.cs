using Microsoft.Extensions.Options;
using MongoDB.Driver;
using paypart_api_gateway.Models;

namespace paypart_logger_service.Model
{
    public class LoggerMongoContext
    {
        private readonly IMongoDatabase _database = null;
        private readonly IOptions<MongoSettings> _settings;

        public LoggerMongoContext(IOptions<MongoSettings> settings)
        {
            _settings = settings;
            var client = new MongoClient(_settings.Value.connectionString);
            if (client != null)
                _database = client.GetDatabase(_settings.Value.database);
        }

        public IMongoCollection<LogDto> Logger
        {
            get
            {
                return _database.GetCollection<LogDto>(_settings.Value.collection);
            }
        }
    }
}
