using System.IO;
using API.Extensions;
using API.Helpers;
using API.Middleware;
using AutoMapper;
using Infrastructure.Data;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace API
{
    public class Startup
    {
        private readonly IConfiguration _config;
        public Startup(IConfiguration config)
        {
            _config = config;
        }

        // These are convention based method names for development and production mode
        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            services.AddDbContext<StoreContext>(option => 
                option.UseSqlServer(_config.GetConnectionString("DefaultConnection")));
            services.AddDbContext<AppIdentityDbContext>(option => 
                option.UseSqlServer(_config.GetConnectionString("IdentityConnection"))); 

            //call configure services for development
            ConfigureServices(services);
        }

        public void ConfigureProductionServices(IServiceCollection services)
        {
             var defaultConnectionString = _config.GetConnectionString("DefaultConnection");
             var identityConnectionString = _config.GetConnectionString("IdentityConnection");

             services.AddDbContext<StoreContext>(option => 
                option.UseMySql(defaultConnectionString, ServerVersion.AutoDetect(defaultConnectionString)));

            services.AddDbContext<AppIdentityDbContext>(option => 
                option.UseMySql(identityConnectionString, ServerVersion.AutoDetect(identityConnectionString))); 

            //call configure services for production mode
            ConfigureServices(services);
        }   

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfiles).Assembly);
            services.AddControllers();           

            //redis configuaration
            services.AddSingleton<IConnectionMultiplexer>(c => {
                var configuration = ConfigurationOptions.Parse(_config.GetConnectionString("Redis"), true);

                return ConnectionMultiplexer.Connect(configuration);
            });

            //extended application services
            services.AddApplicationServices();

            //extended identity services
            services.AddIdentityServices(_config);
                
           //Swagger
           services.AddSwaggerDocumentation();
           services.AddCors(opt => {
               opt.AddPolicy("CorsPolicy", policy => {
                   policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200");
               });
           });
        }
        

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseMiddleware<ExceptionMiddleware>();

            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
               app.UseSwaggerDocumentation();
            }

            //Called when the endpoint doesn't match a particular request made by the user (404)
            app.UseStatusCodePagesWithReExecute("/errors/{0}");

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions{
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "Content")
                ), RequestPath="/content"
            });

            app.UseCors("CorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToController("Index", "Fallback");
            });
        }
    }
}
