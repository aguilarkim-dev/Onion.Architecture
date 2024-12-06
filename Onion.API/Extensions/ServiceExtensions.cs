using Contracts.Interfaces;
using LoggerService;
using Repository;
using Service.Contracts.Interfaces;
using Service;
using Microsoft.EntityFrameworkCore;
using Onion.API;
using ActionFilters;
using Service.DataShaping;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc;
using Onion.API.Utility;

namespace CodeMaze.API.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                     builder.AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader()
                     .WithExposedHeaders("X-Pagination"));
            });
        }

        public static void ConfigureIISIntegration(this IServiceCollection services)
        {
            services.Configure<IISOptions>(options =>
            {

            });
        }


        public static void ConfigureLoggerService(this IServiceCollection services) 
        {
            services.AddSingleton<ILoggerManager, LoggerManager>();
        }

        public static void ConfigureRepositoryManager(this IServiceCollection services)
        {
            services.AddScoped<IRepositoryManager, RepositoryManager>();
        }

        public static void ConfigureServiceManager(this IServiceCollection services)
        {
            services.AddScoped<IServiceManager, ServiceManager>();
        }

        public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration)
        {
            //**old way
            //services.AddDbContext<RepositoryContext>(opts =>
            //    opts.UseSqlServer(configuration.GetConnectionString("sqlConnection")));

            //**AddDbContext + UseSqlServer
            services.AddSqlServer<RepositoryContext>(configuration.GetConnectionString("sqlConnection"));
        }

        public static IMvcBuilder AddCustomCSVFormatter(this IMvcBuilder builder)
        {
            return builder.AddMvcOptions(config => config.OutputFormatters.Add(new CsvOutputFormatter()));
        }

        public static void RegisterActionFilters(this IServiceCollection services)
        {
            services.AddScoped<ValidationFilterAttribute>();
            services.AddScoped<ValidateMediaTypeAttribute>();
        }

        public static void AddDataShaper(this IServiceCollection services)
        {
            services.AddScoped(typeof(IDataShaper<>), typeof(DataShaper<>));
        }

        public static void AddCustomMediaTypes(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(config =>
            {
                var systemTextJsonOutputFormatter = config.OutputFormatters
                    .OfType<SystemTextJsonOutputFormatter>()?
                    .FirstOrDefault();

                if (systemTextJsonOutputFormatter != null)
                {
                    systemTextJsonOutputFormatter.SupportedMediaTypes
                    .Add("application/vnd.codemaze.hateoas+json");
                }

                var xmlOutputFormatter = config.OutputFormatters
                    .OfType<XmlDataContractSerializerOutputFormatter>()?
                    .FirstOrDefault();

                if (xmlOutputFormatter != null)
                {
                    xmlOutputFormatter.SupportedMediaTypes
                    .Add("application/vnd.codemaze.hateoas+xml");
                }
            });
        }

        public static void UseHypermediaAsTheEngineOfApplicationState(this IServiceCollection services) //HATEOAS
        {
            services.AddScoped<IEmployeeLinks, EmployeeLinks>();
            services.AddScoped<ICompanyLinks, CompanyLinks>();
        }


    }


}
