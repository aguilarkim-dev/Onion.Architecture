using ActionFilters;
using CodeMaze.API.Extensions;
using Contracts.Interfaces;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using NLog;
using Onion.API.Utility;
using Presentation;
using Service.DataShaping;

namespace CodeMaze.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

            // Add services to the container.
            builder.Services.ConfigureCors();
            builder.Services.ConfigureIISIntegration();
            builder.Services.ConfigureLoggerService();
            builder.Services.ConfigureSqlContext(builder.Configuration);
            builder.Services.ConfigureRepositoryManager();
            builder.Services.ConfigureServiceManager();
            builder.Services.AddAutoMapper(typeof(Program));
            builder.Services.RegisterActionFilters();
            builder.Services.AddDataShaper();
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true; //This options remove default invalid payload/state response from [ApiController]
            });
            builder.Services.AddControllers(config =>
                {
                    config.RespectBrowserAcceptHeader = true;
                    config.ReturnHttpNotAcceptable = true;
                    config.InputFormatters.Insert(0, GetJsonPatchInputFormatter()); //https://learn.microsoft.com/en-us/aspnet/core/web-api/jsonpatch?view=aspnetcore-8.0 => To not override System.Text.Json default formatter
                })
                .AddXmlDataContractSerializerFormatters() //Xml OutputFormatter Header => Accept: text/xml
                .AddCustomCSVFormatter() //Custom OutputFormatter for Companies Header => Accept: text/csv
                .AddApplicationPart(typeof(AssemblyReference).Assembly); //Point controllers to Presentaion project
            builder.Services.AddCustomMediaTypes();
            builder.Services.AddScoped<IEmployeeLinks, EmployeeLinks>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            var logger = app.Services.GetRequiredService<ILoggerManager>();
            app.ConfigureExceptionHandler(logger);
            if (app.Environment.IsProduction()) 
            {
                app.UseHsts();
            }


            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });
            app.UseCors("CorsPolicy");

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        private static NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter()
        {
            return new ServiceCollection()
                .AddLogging()
                .AddMvc()
                .AddNewtonsoftJson()
                .Services
                .BuildServiceProvider()
                .GetRequiredService<IOptions<MvcOptions>>()
                .Value
                .InputFormatters
                .OfType<NewtonsoftJsonPatchInputFormatter>()
                .First();
        }
    }
}
