using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using paypart_api_gateway.Models;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Confluent.Kafka.Serialization;
using Hangfire;
using paypart_logger_service.Model;
using Newtonsoft.Json;

namespace paypart_api_gateway.Middleware
{
    public class RequestResponseLoggerMiddleware
    {
        const string TIMESTAMP_FORMAT = "yyyy-MM-dd HH:mm:ss:fff";

        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly IOptions<Settings> _settings;
        private readonly IOptions<MongoSettings> _msettings;


        public RequestResponseLoggerMiddleware(RequestDelegate next, IConfiguration configuration, IOptions<Settings> settings, IOptions<MongoSettings> msettings)
        {
            _next = next;
            _settings = settings;
            _msettings = msettings;
            _configuration = configuration;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Request.EnableRewind();

            var originalRequestBody = context.Request.Body;
            var requestText = await FormatRequest(context.Request);
            var originalResponseBody = context.Response.Body;

            using (var responseBody = new MemoryStream())
            {
                context.Request.Body.Position = 0;
                context.Response.Body = responseBody;
                context.Request.Body = originalRequestBody;

                Stopwatch stopwatch = Stopwatch.StartNew();

                await _next(context);

                stopwatch.Stop();

                var responseText = await FormatResponse(context.Response);
                var timestamp = DateTimeOffset.Now.ToString(TIMESTAMP_FORMAT);
                var elapsed = $"{stopwatch.ElapsedMilliseconds.ToString().PadLeft(5)}ms";
                var status = context.Response.StatusCode.ToString();
                var method = context.Request.Method;
                var path = context.Request.Path + context.Request.QueryString;

                var logItem = new LogDto
                {
                    Elapsed = elapsed,
                    Method = method,
                    Path = path,
                    Request = requestText,
                    Response = responseText,
                    Status = status,
                    TimeStamp = timestamp
                };

                //Append Token
                try
                {
                    if (path.Contains("validateuser"))
                    {
                        if (!string.IsNullOrEmpty(responseText))
                        {
                            var user = JsonConvert.DeserializeObject<User>(responseText);
                            Utility utility = new Utility(_settings);
                            utility.appendToken(context, user);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }


                //Log to Mongo
                try
                {
                    string logText = JsonHelper.toJson(logItem);
                    Console.Write(logText);
                    if (_settings.Value.LogToMongo)
                        await LogToMongoDB(logItem);
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }


                //produce on kafka
                //try
                //{
                //    string logText = JsonHelper.toJson(logItem);

                //    var config = new Dictionary<string, object> { { "bootstrap.servers", _settings.Value.brokerList } };
                //    using (var producer = new Producer<Null, string>(config, null, new StringSerializer(Encoding.UTF8)))
                //    {
                //        var deliveryReport = await producer.ProduceAsync(_settings.Value.logTopic, null, logText);
                //        var result = deliveryReport.Value;
                //        producer.Flush(TimeSpan.FromSeconds(100));

                //    }
                //}
                //catch (Exception ex)
                //{
                //    Console.Write(ex.Message);
                //}

                await responseBody.CopyToAsync(originalResponseBody);
            }
        }

        private async Task<string> FormatRequest(HttpRequest request)
        {
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);

            return $"{request.Scheme} {request.Host}{request.Path} {request.QueryString} {bodyAsText}";
        }

        private async Task<string> FormatResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var text = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            return $"{text}";
        }
        private async Task<LogDto> LogToMongoDB(LogDto logDto)
        {
            LogDto log = new LogDto();

            var logger = new LoggerMongoRepository(_msettings);

            log = await logger.AddLog(logDto);

            return log;
        }
    }
    public static class RequestResponseLoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogger(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestResponseLoggerMiddleware>();
        }
    }

}
