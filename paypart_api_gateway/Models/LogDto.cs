using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace paypart_api_gateway.Models
{
    public class LogDto
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; }
        public string Elapsed { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string Status { get; set; }
        public string TimeStamp { get; set; }
    }
}
