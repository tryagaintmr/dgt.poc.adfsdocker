using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Serilog;
using Serilog.Events;
using Serilog.Settings.Configuration;
using System.Diagnostics;

namespace dgt.poc.webadfsdocker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
            string seqUrl = "http://seq:5341";

            ConfigurationManager configuration = builder.Configuration;
            Log.Logger = new LoggerConfiguration()
                                .MinimumLevel.Debug()
                                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                                .Enrich.FromLogContext()
                                .WriteTo.Console()
                                .WriteTo.Seq(seqUrl, apiKey: "your-api-key")
                                .CreateLogger();


            // Important to call at exit so that batched events are flushed.
            builder.Services.AddSingleton<Serilog.ILogger>(sp => Log.Logger);
            builder.Services.AddSingleton<Serilog.Extensions.Hosting.DiagnosticContext>();

            var options = new ConfigurationReaderOptions { SectionName = "Seq" };


            // Add services to the container.
            builder.Host.UseSerilog();
            var realm = configuration["wsfed:realm"];

            builder.Services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = WsFederationDefaults.AuthenticationScheme;
            }).AddWsFederation(options =>
            {
                options.Wtrealm = realm;
                options.MetadataAddress = configuration["wsfed:metadata"];
                //options.BackchannelHttpHandler = new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator };
            }).AddCookie();

            // Add services to the container.
            builder.Services.AddRazorPages();
            //builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/signin-wsfed"))
                {
                    var form = await context.Request.ReadFormAsync();
                    var token = form["wresult"];
                    Console.WriteLine(token);
                }
                await next.Invoke();
            });
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}