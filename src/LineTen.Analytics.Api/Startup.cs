using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using LineTen.Core;
using LineTen.DataAccess.EntityFramework.Repository;
using LineTen.Analytics.Api.Filters;
using LineTen.Analytics.Api.HealthChecks;
using LineTen.Analytics.Api.Mapper;
using LineTen.Analytics.Api.Options;
using LineTen.Analytics.Api.Swagger;
using LineTen.Analytics.Data.Database;
using LineTen.Analytics.Data.Extensions;
using LineTen.Analytics.Data.Options;
using LineTen.Analytics.Service.Mediator.Command;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace LineTen.Analytics.Api
{
    /// <summary>
    /// Startup class invoked by the webhost
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// web host configuration
        /// </summary>
        public IConfiguration Configuration { get; }
      
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container. 
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            //Add application insights
            services.AddApplicationInsightsTelemetry();
            
            //// Add kubernetes AI enricher
            //services.AddApplicationInsightsKubernetesEnricher();

            ConfigureHealthChecks(services);

            ConfigureDatabase(services);

            ConfigureWebApi(services);

            ConfigureAuth(services);

            ConfigureDependencyInjection(services);

            ConfigureSwaggerGen(services);
        }

        /// <summary>
        /// Configure health check probes
        /// </summary>
        /// <param name="services"></param>
        public virtual void ConfigureHealthChecks(IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck<DatabaseConnectivityHealthCheck>("database_connectivity_health_check")
                .AddMemoryHealthCheck("memory");
        }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "LineTen.Analytics.Api"); });

            app.UseHttpsRedirection();

            // Adds a StatusCodePages middleware with a default response handler that checks for responses with status codes 
            // between 400 and 599 that do not have a body.
            app.UseStatusCodePages();

            // Enables default file mapping conventions on the current path (index.html etc.) for the host when not detected in the querystring
            app.UseDefaultFiles();

            // Enables static file serving for the current request path (index.html etc.)
            app.UseStaticFiles();
            
            // Make sure the CORS middleware is ahead of any SignalR registration
            app.UseCors(builder =>
            {

                builder.AllowAnyMethod()
                    .AllowCredentials()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed(s => { return true; }) //TODO This could be loaded via DI options
                    .WithExposedHeaders("Content-Disposition", "WWW-Authenticate");
            });

            app.UseRouting();

            // Both of the following calls are required to leverage the [Authorize] attribute on Controllers and Actions
            // Extension methods to add authentication capabilities to an HTTP application pipeline.
            app.UseAuthentication();
            // Extension methods to add authorization capabilities to an HTTP application pipeline.
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //Health checks - see https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    ResponseWriter =  WriteResponse,
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    }
                });
                //endpoints.MapHub<YourHub>("/your-desired-hub-path");
            });

            // Create an instance of the container here for convenience
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                //Get the fully initialized ExampleDbContext
                var applicationDbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
                //This code is also executed by the LineTen.Analytics.Api.Tests.Integration project and doesn't support migrations
                if (applicationDbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
                {
                    //Ensure the database is up-to date relative to the current project - EF uses the __EFMigrationsHistory table to determine this
                    applicationDbContext.Database.Migrate();
                }
                var mapper = serviceScope.ServiceProvider.GetService<IMapper>();
                //Some errors can occur at runtime that would otherwise not get caught during compilation - this forces AutoMapper to check the configuration before accepting requests
                mapper.ConfigurationProvider.AssertConfigurationIsValid();
            }
        }
        private static Task WriteResponse(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = "application/json";

            var json = new JObject(
                new JProperty("status", result.Status.ToString()),
                new JProperty("results", new JObject(result.Entries.Select(pair =>
                    new JProperty(pair.Key, new JObject(
                        new JProperty("status", pair.Value.Status.ToString()),
                        new JProperty("description", pair.Value.Description),
                        new JProperty("data", new JObject(pair.Value.Data.Select(
                            p => new JProperty(p.Key, p.Value))))))))));

            return context.Response.WriteAsync(
                json.ToString(Formatting.Indented));
        }

        private void ConfigureSwaggerGen(IServiceCollection services)
        {
            //Use Swashbuckle package to generate a swagger doc that is compliant with OAS3.
            services.AddSwaggerGen(options =>
                {
                    //we need to tell swagger to document all enums in camel case, as per the NewtonSoft convention.
                    options.DescribeAllParametersInCamelCase();

                    var jwtAuthentication = new JwtAuthentication();
                    Configuration.Bind(nameof(JwtAuthentication), jwtAuthentication);
                    options.AddSecurityDefinition(SwaggerConstants.SecurityDefinitionBearer,
                    new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            ClientCredentials = new OpenApiOAuthFlow
                            {
                                TokenUrl = new Uri(jwtAuthentication.IdentityProvider + "/connect/token")
                            }
                        },
                        Description = "Client credentials flow"
                    });

                    //Decorate the swagger doc with the relevant authorization responses, so that SwaggerUi knows to add padlocks and schemes to the relevant endpoints
                    options.OperationFilter<AuthorizeOperationFilter>();
                    // Set the comments path for the Swagger JSON and UI.
                    options.IncludeXmlComments(XmlCommentsFilePath, true);
                    // Adds "(Auth)" to the summary so that you can see which endpoints have Authorization - has to be after IncludeXmlComments
                    options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                })
                //Tell swashbuckle to use whatever other defaults we've established from the NewtonSoft library earlier.
                .AddSwaggerGenNewtonsoftSupport();
        }
        /// <summary>
        /// Add all your custom services to the project here.
        /// </summary>
        /// <param name="services"></param>
        private static void ConfigureDependencyInjection(IServiceCollection services)
        {
            //Some transient services - similar to asking the container to `new` one up for you each time

            //Adds repository abstraction for the ExampleDbContext
            services.AddTransient(typeof(IRepository<>), typeof(ApplicationRepository<>));

            //Sets up AutoMapper for DI and also registers a mapping profile
            services.AddAutoMapper(typeof(WeatherForecastProfile));

            //Sets up MediatR for in-proc messaging
            services.AddMediatR(Assembly.GetExecutingAssembly());

            //Register all MediatR handlers
            services.RegisterAllTypes(typeof(IRequestHandler<,>), new[] { typeof(CreateWeatherForecastHandler).Assembly} );

        }

        /// <summary>
        /// web api configuration
        /// </summary>
        /// <param name="services"></param>
        protected void ConfigureWebApi(IServiceCollection services)
        {
            //Forces the ASP.NET routing implementation to lowercase everything for consistency
            services.AddRouting(x =>
            {
                x.LowercaseUrls = true;
                x.LowercaseQueryStrings = true;
            });

            services.AddControllers(x =>
                {
                    x.Filters.Add<GlobalTransactionScopeActionFilterAttribute>();
                })
                //Required for TestServer in test assembly to function correctly --https://stackoverflow.com/questions/43669633/why-is-testserver-not-able-to-find-controllers-when-controller-is-in-separate-as?noredirect=1#comment74386164_43669633;;
                .AddApplicationPart(Assembly.GetAssembly(typeof(Startup)))
                //Overrides the default System.Text.Json implementation for the more tried and tested NewtonsoftJson library
                .AddNewtonsoftJson(x =>
                {
                    x.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver(); //Camel case all the props
                    x.SerializerSettings.Formatting = Formatting.Indented; //Indent the JSON for readability
                    x.SerializerSettings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy())); //convert all enums to strings
                });

            services.AddMvcCore(x =>
            {
                x.SuppressAsyncSuffixInActionNames = false; //https://github.com/microsoft/aspnet-api-versioning/issues/558
            });
        }
        /// <summary>
        /// Configure JWT Authentication / Authorization
        /// </summary>
        /// <param name="services"></param>
        protected virtual void ConfigureAuth(IServiceCollection services)
        {
            
            services.Configure<JwtAuthentication>(Configuration.GetSection(nameof(JwtAuthentication)));
            services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(); //configuration in ConfigureJwtBearerOptions

            services.AddAuthorization(options =>
            {
                options.AddPolicy("lineten.analytics.read", y =>
                {
                    y.RequireAuthenticatedUser();
                    y.RequireClaim("scope", "lineten.analytics.write", "manage");
                });
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("lineten.analytics.write", y =>
                {
                    y.RequireAuthenticatedUser();
                    y.RequireClaim("scope", "lineten.analytics.write", "manage");
                });
            });
            ////Setup JWT Authentication
            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    //This maps the bearer token to the ASP.NET claims identity when detected in the request headers.
            //    //Further configuration here will adjust the security level
            //    .AddJwtBearer(options =>
            //    {
            //        //These TokenValidationParameters essentially bypass any signature validation and will allow any valid JWT to be deserialized by the .NET runtime, and converted to a ClaimsIdentity
            //        //Not setting TokenValidationParameters will cause the runtime to use the defaults
            //        options.TokenValidationParameters = new TokenValidationParameters
            //        {
            //            ValidateIssuer = false,
            //            ValidateAudience = false,
            //            ValidateIssuerSigningKey = false,
            //            SignatureValidator = delegate (string token,
            //                TokenValidationParameters parameters)
            //            {
            //                var jwt = new JwtSecurityToken(token);
            //                return jwt;
            //            },
            //            ValidateLifetime = false,
            //            ClockSkew = TimeSpan.Zero,
            //            RequireSignedTokens = false
            //        };
            //    });
        }

        //public virtual void ConfigureSignalR(IServiceCollection services)
        //{
            //services.AddSignalR()
                //.AddNewtonsoftJsonProtocol(x => x.PayloadSerializerSettings = DefaultJsonSerializerSettings)
                // We can 'upgrade' to AzureSignalR with this extension method in the Microsoft.Azure.SignalR nuget package
                //.AddAzureSignalR()
                //;
        //}

        /// <summary>
        /// Provides DbContext configuration - IRepository is registered elsewhere to make this simpler to override in InMemoryDbStartup.
        /// </summary>
        /// <param name="services"></param>
        protected virtual void ConfigureDatabase(IServiceCollection services)
        {
            var dbOptions = new DbOptions();
            //Similar to services.Configure, except Configuration.Bind allows us to use it straight away when the DI container isn't initialized.
            Configuration.Bind(nameof(dbOptions), dbOptions);
            services.AddApplicationDbContext(LoggerFactory, dbOptions);
        }

        /// <summary>
        /// Enables EF to write database queries to the attached debugger
        /// </summary>
        public static readonly ILoggerFactory LoggerFactory
            = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder.AddDebug();
            });
        /// <summary>
        /// Returns the path to the XMLDoc comments extracted from the repo
        /// This is configured in $\LineTen.Analytics.Api\LineTen.Analytics.Api.csproj
        /// </summary>
        private static string XmlCommentsFilePath
        {
            get
            {
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var fileName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
                return Path.Combine(basePath, fileName);
            }
        }
    }
}
