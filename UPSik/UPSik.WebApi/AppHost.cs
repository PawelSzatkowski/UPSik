using Microsoft.AspNetCore;
using System;
using Unity.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Unity;

namespace UPSik.WebApi
{
    public class AppHost
    {
        private IWebHost _webApiHost;

        private readonly IUnityContainer _container;

        public AppHost(IUnityContainer container)
        {
            _container = container;
        }

        public void Start()
        {
            _webApiHost = WebHost
                .CreateDefaultBuilder()
                .UseUnityServiceProvider(_container)
                .ConfigureServices(services =>
                {
                    services.AddMvc();
                    services.AddSwaggerGen(SwaggerDocsConfig);
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                    app.UseCors();
                    app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UPSik WebApi v1");
                        c.RoutePrefix = string.Empty;
                    });
                })
                .UseUrls("http://*:10500")
                .Build();

            _webApiHost.RunAsync();
        }

        public void Stop()
        {
            _webApiHost.StopAsync().Wait();
        }

        private static void SwaggerDocsConfig(SwaggerGenOptions genOptions)
        {
            genOptions.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Version = "v1",
                    Title = "UPSik.WebApi",
                    Description = "WebApi for UPSik delivery company",
                    TermsOfService = new Uri("https://webapiexamples.project.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Pawel Szatkowski",
                        Email = "pawszatk@gmail.com",
                    },
                    License = new OpenApiLicense
                    {
                        Name = "This WebApi license",
                        Url = new Uri("https://webapiexamples.project.com/license")
                    }
                });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            genOptions.IncludeXmlComments(xmlPath);
        }
    }
}
