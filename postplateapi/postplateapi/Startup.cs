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
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {

        string[] allowedDomains = {
                "https://sostshi.com",
                "https://betnow-32807.web.app",
                "http://localhost:4200",
                "http://localhost:57749",
                "http://localhost:5001"
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

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

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