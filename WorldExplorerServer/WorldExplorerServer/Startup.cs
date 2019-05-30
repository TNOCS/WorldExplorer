using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using WorldExplorerClient;
using WorldExplorerServer.Hubs;
using WorldExplorerServer.Services;

namespace WorldExplorerServer
{
    public class LogTraceWriter : ITraceWriter
    {
        
        public LogTraceWriter()

        {
           
        }

        public TraceLevel LevelFilter => TraceLevel.Verbose;

        public void Trace(TraceLevel level, string message, Exception ex)
        {
            
        }
    }

   

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
           
        }

        public IConfiguration Configuration { get; }

        public void errorMsg(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs pErr)
        {
            
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR().AddJsonProtocol(options =>
            {
                options.PayloadSerializerSettings.Error += errorMsg;
               // options.PayloadSerializerSettings.ConstructorHandling = Newtonsoft.Json.ConstructorHandling.AllowNonPublicDefaultConstructor;
                options.PayloadSerializerSettings.TraceWriter = new LogTraceWriter();
                options.PayloadSerializerSettings.ContractResolver = new AvroJsonSerializer();
                options.PayloadSerializerSettings.TypeNameHandling = TypeNameHandling.Objects;
                //options.PayloadSerializerSettings
                // Edit or replace 'options.PayloadSerializerSettings' here!
            }).AddHubOptions<WorldExplorerHub>(options =>
            {
                options.EnableDetailedErrors = true;
                
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.AddCors(o => o.AddPolicy("WorldExplorerCorsPolicy", builder =>
            {
                builder
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowAnyOrigin();
                //.WithOrigins("http://localhost:4200");
            }));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            app.UseSignalR(routes =>
            {
                routes.MapHub<WorldExplorerHub>("/WorldExplorerHub");
            });
           

            /*
            app.UseOwin(addToPipeline =>
            {
                addToPipeline(next =>
                {
                    return null;
    
                   // var builder = new AppBuilder();
                   // builder.UseCors(CorsOptions.AllowAll);
                   // builder.MapSignalR();

                   // var appFunc = builder.Build(typeof(Func<IDictionary<string, object>, Task>)) as Func<IDictionary<string, object>, Task>;

                    // return appFunc;
                });
            }); */
            
            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });

            app.UseCors("WorldExplorerCorsPolicy");

        }
    }
}
