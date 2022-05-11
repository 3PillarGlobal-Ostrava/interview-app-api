using Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Logging;
using Serilog;
using WebApi.Modules;
using WebApi.Exceptions;

namespace WebApi;

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
        Log.Debug("ConfigureServices => Using AddApplicationInsightsTelemetry");
        services.AddApplicationInsightsTelemetry();

        Log.Debug("ConfigureServices => Setting AddControllers");
        services.AddControllers();

        Log.Debug("ConfigureServices => Setting AddHealthChecks");
        services.AddHealthChecks();

        Log.Debug("ConfigureServices => Setting AddVersioning");
        services.AddVersioning();

        Log.Debug("ConfigureServices => Setting AddSwaggerGen");
        services.AddSwaggerGen();

        Log.Debug("ConfigureServices => Setting AddUseCases");
        services.AddUseCases();

        Log.Debug("ConfigureServices => Setting AddPersistence");
        services.AddPersistence(Configuration);

        Log.Debug("ConfigureServices => Setting AddMapper");
        services.AddMapper();

        services.AddControllers(config =>
        {
            config.Filters.Add<BusinessExceptionFilter>();
        });

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
        }
        )
        .AddCookie()
        .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.ClientId = Configuration["Authentication:Google:ClientID"];
            options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
            options.ClaimActions.MapJsonKey("urn:google:pic", "picture", "url");
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
    {
        Log.Debug("IsDevelopment: {IsDevelopment}",env.IsDevelopment());
        if (env.IsDevelopment())
        {
            Log.Debug("Using UseDeveloperExceptionPage");
            app.UseDeveloperExceptionPage();
            Log.Debug("Running seed");
            app.ApplicationServices.GetRequiredService<MyDbContext>().Seed().Wait();
        }

        Log.Debug("Setting middleware => {Middleware}",nameof(TimeElapsedDiagnosticsMiddleware));
        app.UseMiddleware<TimeElapsedDiagnosticsMiddleware>();

        Log.Debug("Setting UseHttpsRedirection");
        app.UseHttpsRedirection();

        Log.Debug("Setting UseVersionedSwagger End: {SwaggerEnv}", env);
        app.UseVersionedSwagger(provider, this.Configuration, env);

        Log.Debug("Setting UseRouting");
        app.UseRouting();

        Log.Debug("Setting UseCors");
        app.UseCors(Configuration);

        Log.Debug("Setting UseAuthorization");
        app.UseAuthorization();

        Log.Debug("Setting UseAuthentication");
        app.UseAuthentication();

        Log.Debug("Setting UseEndpoints");
        app.UseEndpoints(endpoints =>
        {
            Log.Debug("Setting endpoints => MapControllers");
            endpoints.MapControllers();

            Log.Debug("Setting endpoints => add health check");
            endpoints.MapHealthChecks("/health");
        });
    }
}
