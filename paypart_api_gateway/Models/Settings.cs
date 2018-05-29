
using System;
using System.Linq;

namespace paypart_api_gateway.Models
{
    public class Settings
    {
        public string mongoUrl;
        public string mongoConnstring;
        public string mongoDatabase;
        public string mongoCollection { get; set; }
        public string addBillerTopic;
        public int redisCancellationToken;
        public int pLength;
        public string notifyUrl;
        public string resetNotifyBody;
        public string resetNotifySubject;
        public string resetNotifynewpass;

        public string Secret;
        public string Iss;
        public string Aud;
        public int timespan;
        public bool LogToMongo { get; set; }

        public string brokerList;
        public string logTopic;
    }
}
