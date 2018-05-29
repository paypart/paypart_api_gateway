using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocelot.DependencyInjection;
using CacheManager.Core;
using Ocelot.Middleware;
using ConfigurationBuilder = Microsoft.Extensions.Configuration.ConfigurationBuilder;
using paypart_api_gateway.Middleware;
using paypart_api_gateway.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace paypart_api_gateway
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("secrets/appsettings.apigateway.json", optional: true, reloadOnChange: true)
            .AddJsonFile("secrets/configuration.json")
            .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
			 //Cors setup
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                builder => builder.AllowAnyOrigin()
                             .AllowAnyMethod()
                             .AllowAnyHeader()
                             .AllowCredentials());
            });
			
            services.Configure<Settings>(options =>
            {
                options.Aud = Configuration.GetSection("AppSettings:Aud").Value;
                options.Iss = Configuration.GetSection("AppSettings:Iss").Value;
                options.Secret = Configuration.GetSection("AppSettings:Secret").Value;
                options.timespan = Convert.ToInt32(Configuration.GetSection("AppSettings:timespan").Value);
                options.brokerList = Configuration.GetSection("AppSettings:brokerList").Value;
                options.logTopic = Configuration.GetSection("AppSettings:logTopic").Value;
                options.LogToMongo = Convert.ToBoolean(Configuration.GetSection("AppSettings:LogToMongo").Value);

                options.mongoCollection = Configuration.GetSection("MongoDBConnStrings:Collection").Value;
                options.mongoConnstring = Configuration.GetSection("MongoDBConnStrings:ConnectionString").Value;
                options.mongoDatabase = Configuration.GetSection("MongoDBConnStrings:Database").Value;

            });

            services.Configure<MongoSettings>(options =>
            {
                options.connectionString = Configuration.GetSection("MongoDBConnStrings:ConnectionString").Value;
                options.database = Configuration.GetSection("MongoDBConnStrings:Database").Value;
                options.collection = Configuration.GetSection("MongoDBConnStrings:Collection").Value;
            });

            var audienceConfig = Configuration.GetSection("AppSettings");

            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(audienceConfig["Secret"]));
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = audienceConfig["Iss"],
                ValidateAudience = true,
                ValidAudience = audienceConfig["Aud"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true,
            };


            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = "TestKey";
            })
            .AddJwtBearer("TestKey", x =>
            {
                x.RequireHttpsMetadata = false;
                x.TokenValidationParameters = tokenValidationParameters;
            });

            services.AddOcelot(Configuration);
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IHostingEnvironment env,
            ILoggerFactory loggerFactory, IOptions<Settings> settings)
        {
			app.UseCors("CorsPolicy");
            var configuration = new OcelotMiddlewareConfiguration();
            app.UseRequestResponseLogger();
            app.UseAuthentication();
            await app.UseOcelot(configuration);
        }
    }
}
