using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Storage;
using RecipesApi.Models;
using RecipesApi.Services;
using System;
using System.IO;
using System.Reflection;

namespace RecipesApi
{
    /// <summary>
    /// Startup
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Startup constructor
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Startup));
            services.AddControllers();
         
            // Stopped using Oracle MySql Entity Framework lib: https://github.com/dotnet/efcore/issues/17788
            // Using Pomelo instead which support NetCore 3+
            // Lazy loading DBSets only when needed
            services.AddDbContext<DbContext>(options =>
                //.UseLazyLoadingProxies()
                options
                    .UseLoggerFactory(ConsoleLoggerFactory)
                    .EnableSensitiveDataLogging(true)
                    .UseMySql(
                    Configuration.GetConnectionString("DefaultConnection"), 
                    mySqlOptions => mySqlOptions.ServerVersion(new ServerVersion(new Version(8, 0, 18), ServerType.MySql)))
                );
            services.AddScoped<IEntityService<Recipe>,RecipesService>();
            services.AddScoped<IEntityService<Category>,CategoryService>();
            services.AddScoped<IEntityService<RecipeCategory>, RecipeCategoryService>();
            services.AddScoped<IEntityService<Ingredient>,IngredientsService>();
            services.AddScoped<IEntityService<Unit>,UnitsService>();
            services.AddScoped<IEntityService<Media>,MediaService>();
            services.AddScoped<IEntityService<Instruction>,InstructionsService>();
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Recipe API", Version = "v1" });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            logger.LogInformation("Service is starting");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Recipe API V1");
                // Uncomment below to have app root to point to swagger
                //c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static readonly ILoggerFactory ConsoleLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddFilter((category, level) =>
            category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information)
            .AddConsole();
        });
    }
}
