using AutoMapper;
using NetCrud.Rest.Example.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using NetCrud.Rest.Data;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetCrud.Rest.Example.Definations;
using NetCrud.Rest.Core;
using NetCrud.Rest.EntityFramework;

namespace NetCrud.Rest.Example
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
            services.AddControllers(options =>
            {
                options.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
               
            }).AddNewtonsoftJson(option =>
            {
                option.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                option.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            }).AddMvcOptions(options =>
            {
                var jsonOutputFormater = options.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>();

                if (jsonOutputFormater != null)
                {
                    jsonOutputFormater.First().SupportedMediaTypes.Add("application/vnd.Nv.hateoas+json");
                }
            });



            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "NetCrud_Rest_Api", Version = "v1" });
            });

            services.AddDbContext<NetCrudDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Default")));
            
            //Register Service, Repository and DataShaper for Purchase Entity
            services.AddScoped<IRepository<Purchase>, Repository<Purchase, NetCrudDbContext>>();
            services.AddScoped<IEntityService<Purchase, int>, PurchaseService>();
            services.AddScoped<IDataShaper<Purchase>, DataShaper<Purchase>>();

            //Register Service, Repository and DataShaper for Customer Entity
            services.AddScoped<IRepository<Customer>, Repository<Customer, NetCrudDbContext>>();
            services.AddScoped<IEntityService<Customer, int>, EntityService<Customer>>();
            services.AddScoped<IDataShaper<Customer>, DataShaper<Customer>>();

            //Register Service, Repository and DataShaper for Product Entity
            services.AddScoped<IRepository<Product>, Repository<Product, NetCrudDbContext>>();
            services.AddScoped<IEntityService<Product, int>, EntityService<Product>>();
            services.AddScoped<IDataShaper<Product>, DataShaper<Product>>();

            services.AddScoped<IUnitOfWork, UnitOfWork<NetCrudDbContext>>();

            services.AddHttpContextAccessor();
           

            //services.AddCors(options =>
            //{
            //    options.AddPolicy(MyAllowSpecificOrigins,
            //    builder =>
            //    {
            //        builder
            //        .WithOrigins(
            //            "http://localhost:5004"
            //            )
            //            .AllowAnyHeader()
            //            .AllowAnyMethod()
            //            .WithExposedHeaders("X-Pagination");
            //    });
            //});
        }

        private static NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter()
        {
            var builder = new ServiceCollection()
                .AddLogging()
                .AddMvc()
                .AddNewtonsoftJson()
                .Services.BuildServiceProvider();

            return builder
                .GetRequiredService<IOptions<MvcOptions>>()
                .Value
                .InputFormatters
                .OfType<NewtonsoftJsonPatchInputFormatter>()
                .First();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            this.InitializeDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NPSApi v1"));

                this.SeedDatabase(app);
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public virtual void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<NetCrudDbContext>().Database.Migrate();
            }
        }

        public void SeedDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<NetCrudDbContext>();
                context.Seed().GetAwaiter().GetResult();
            }
        }
    }
}
