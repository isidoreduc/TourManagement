﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using TourManagement.API.Services;

namespace TourManagement.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(setupAction =>
            {
                setupAction.ReturnHttpNotAcceptable = true;
                // acquire a formatter needed to create a custom media type (for HttpGet)
                var jsonOutputFormatter = setupAction.OutputFormatters.OfType<JsonOutputFormatter>().FirstOrDefault();
                // create custom/vendor media types and adds it to the list of supported ones
                if (jsonOutputFormatter != null)
                {
                    jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.isidore.tour+json");
                    jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.isidore.tourwithestimatedprofits+json");
                    jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.isidore.tourwithshows+json");
                    jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.isidore.tourwithestimatedprofitsandshows+json");
                    jsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.isidore.showcollection+json");
                    jsonOutputFormatter.SupportedMediaTypes.Add("application/json-patch+json");
                }

                // for HttpPost
                var jsonInputFormatter = setupAction.InputFormatters.OfType<JsonInputFormatter>().FirstOrDefault();
                // create custom/vendor media types and adds it to the list of supported ones
                if (jsonInputFormatter != null)
                {
                    jsonInputFormatter.SupportedMediaTypes.Add("application/vnd.isidore.tourforcreation+json");
                    jsonInputFormatter.SupportedMediaTypes.Add("application/vnd.isidore.tourwithmanagerforcreation+json");
                    jsonInputFormatter.SupportedMediaTypes.Add("application/vnd.isidore.tourwithshowsforcreation+json");
                    jsonInputFormatter.SupportedMediaTypes.Add("application/vnd.isidore.tourwithmanagerandshowsforcreation+json");
                    jsonInputFormatter.SupportedMediaTypes.Add("application/vnd.isidore.showcollectionforcreation+json");

                }

            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
                options.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver();
            });

            // Configure CORS so the API allows requests from JavaScript.  
            // For demo purposes, all origins/headers/methods are allowed.  
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOriginsHeadersAndMethods",
                    builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            // register the DbContext on the container, getting the connection string from
            // appsettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration["ConnectionStrings:TourDB"];
            services.AddDbContext<TourManagementContext>(o => o.UseSqlServer(connectionString));

            // register the repository
            services.AddScoped<ITourManagementRepository, TourManagementRepository>();

            // register an IHttpContextAccessor so we can access the current
            // HttpContext in services by injecting it
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // register the user info service
            services.AddScoped<IUserInfoService, UserInfoService>();
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
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
                    });
                });
            }

            AutoMapper.Mapper.Initialize(config =>
            {
                // mapping for output / delivering resources to client
                config.CreateMap<Entities.Tour, Dtos.Tour>()
                    .ForMember(d => d.Band, o => o.MapFrom(s => s.Band.Name));
                config.CreateMap<Entities.Tour, Dtos.TourWithEstimatedProfits>()
                     .ForMember(d => d.Band, o => o.MapFrom(s => s.Band.Name));
                config.CreateMap<Entities.Tour, Dtos.TourWithShows>()
                     .ForMember(d => d.Band, o => o.MapFrom(s => s.Band.Name));
                config.CreateMap<Entities.Tour, Dtos.TourWithEstimatedProfitsAndShows>()
                      .ForMember(d => d.Band, o => o.MapFrom(s => s.Band.Name));
                //
                config.CreateMap<Entities.Band, Dtos.Band>();
                config.CreateMap<Entities.Manager, Dtos.Manager>();
                config.CreateMap<Entities.Show, Dtos.Show>();
                // mapping for input / database persistance
                config.CreateMap<Dtos.TourForCreation, Entities.Tour>();
                config.CreateMap<Dtos.TourWithManagerForCreation, Entities.Tour>();
                config.CreateMap<Dtos.TourWithShowsForCreation, Entities.Tour>();
                config.CreateMap<Dtos.TourWithManagerAndShowsForCreation, Entities.Tour>();
                config.CreateMap<Dtos.ShowForCreation, Entities.Show>();

                config.CreateMap<Entities.Tour, Dtos.TourForUpdate>().ReverseMap();

            });

            // Enable CORS
            app.UseCors("AllowAllOriginsHeadersAndMethods");
            app.UseMvc();
        }
    }
}
