using Core.Entities.Identity;
using StackExchange.Redis;
using AutoMapper;

var builder = WebApplication.CreateBuilder(args);

//Add Services to the container
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);

builder.Services.AddDbContext<StoreContext>(option =>
                option.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<AppIdentityDbContext>(option =>
    option.UseNpgsql(builder.Configuration.GetConnectionString("IdentityConnection")));

//redis configuaration
builder.Services.AddSingleton<IConnectionMultiplexer>(c =>
{
    var configuration = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis"), true);

    return ConnectionMultiplexer.Connect(configuration);
});

//extended application services
builder.Services.AddApplicationServices();

//extended identity services
builder.Services.AddIdentityServices(builder.Configuration);

//Swagger
builder.Services.AddSwaggerDocumentation();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200");
    });
});


//Configure http request pipeline
var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (builder.Environment.IsDevelopment())
{
    //app.UseDeveloperExceptionPage();
    app.UseSwaggerDocumentation();
}

//Called when the endpoint doesn't match a particular request made by the user (404)
app.UseStatusCodePagesWithReExecute("/errors/{0}");

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Content")
    ),
    RequestPath = "/content"
});

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToController("Index", "Fallback");

//Migrations and Database Seeding
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var loggerFactory = services.GetRequiredService<ILoggerFactory>();

try
{
    var context = services.GetRequiredService<StoreContext>();
    await context.Database.MigrateAsync();
    await StoreContextSeed.SeedAsync(context, loggerFactory);

    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    var identityContext = services.GetRequiredService<AppIdentityDbContext>();
    await identityContext.Database.MigrateAsync();
    await AppIdentityDbContextSeed.SeedUsersAsync(userManager, roleManager);
}
catch (System.Exception ex)
{
    var logger = loggerFactory.CreateLogger<Program>();
    logger.LogError(ex, "An error occurred during migration.");
}

await app.RunAsync();

// namespace API
// {
//     public class Program
//     {
//         public static async Task Main(string[] args)
//         {
//             var host = CreateHostBuilder(args).Build();
//             using var scope = host.Services.CreateScope();
//             var services = scope.ServiceProvider;
//             var loggerFactory = services.GetRequiredService<ILoggerFactory>();

//             try
//             {
//                 var context = services.GetRequiredService<StoreContext>();
//                 await context.Database.MigrateAsync();
//                 await StoreContextSeed.SeedAsync(context, loggerFactory);

//                 var userManager = services.GetRequiredService<UserManager<AppUser>>();
//                 var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
//                 var identityContext = services.GetRequiredService<AppIdentityDbContext>();
//                 await identityContext.Database.MigrateAsync();
//                 await AppIdentityDbContextSeed.SeedUsersAsync(userManager, roleManager);
//             }
//             catch (System.Exception ex)
//             {
//                 var logger = loggerFactory.CreateLogger<Program>();
//                 logger.LogError(ex, "An error occurred during migration.");
//             }

//             await host.RunAsync();
//         }

//         public static IHostBuilder CreateHostBuilder(string[] args) =>
//             Host.CreateDefaultBuilder(args)
//                 .ConfigureWebHostDefaults(webBuilder =>
//                 {
//                     webBuilder.UseStartup<Startup>();
//                 });
//     }
// }
