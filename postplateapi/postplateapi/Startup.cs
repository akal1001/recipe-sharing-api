using BusinessLogicLayer;
using DataAccessLayer;
using postplateapi.OpenAiManagers;
using postplateapi.UnsplashManager;

namespace postplateapi;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddCors(); // <-- ADD THIS
        services.AddScoped<PostPlateManager>();
        services.AddScoped<PostPlatesDataAccess>();
        services.AddScoped<IngredientPropertyDataAccess>();
        services.AddScoped<InsertHealthProfileDataAccess>();
        services.AddScoped<ConnictionString>();
        services.AddScoped<OpenAiManager>();
        services.AddHttpClient<UnsplashService>();
        services.AddScoped<EncryptionHelper.EncryptionHelper>();

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {

        string[] allowedDomains = {
                "http://localhost:4200",
                "http://localhost:57749",
                "https://postplate-6fc40.web.app"
            };

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        //Apply CORS Policy
        app.UseCors(builder =>
            builder.WithOrigins(allowedDomains)
                   .AllowAnyHeader()
                   .AllowAnyMethod()
        );

      //  app.UseMiddleware<ApiKeyMiddleware>();

       

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
            });
        });
    }
}