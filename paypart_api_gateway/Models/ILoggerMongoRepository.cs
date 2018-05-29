using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using paypart_api_gateway.Models;

namespace paypart_api_gateway.Models
{
    public interface ILoggerMongoRepository
    {
        Task<LogDto> GetLog(string id);
        Task<LogDto> AddLog(LogDto item);
    }
}
