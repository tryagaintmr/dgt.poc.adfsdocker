using Serilog;
using Serilog.Events;
using Serilog.Settings.Configuration;
using System.Diagnostics;
using System.Text;

namespace dgt.poc.adfsdocker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            #region Raw test of the SEQ digest point
            Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));

            try
            {
                using var client = new HttpClient { BaseAddress = new Uri("http://seq:5341") };
                // client.DefaultRequestHeaders.Add("Content-Type", "application/vnd.serilog.clef");

                var content = new StringContent(
                    "{ \"@t\":\"2023-07-05T14:34:32.576Z\",\"@mt\":\"Hello, {User}\",\"User\":\"Alice\" }",
                    Encoding.UTF8,
                    "application/vnd.serilog.clef"
                );

                var response = await client.PostAsync("/api/events/raw", content);

                if (response.IsSuccessStatusCode)
                {
                    Log.Information("Successfully sent log to Seq server.");
                }
                else
                {
                    Log.Error("Could not send log to Seq server. Status Code: " + response.StatusCode);
                }
            } catch(Exception ex)
            {
                throw;
            }
            #endregion

            try
            {

                


                var builder = WebApplication.CreateBuilder(args);
                Log.Logger = new LoggerConfiguration()
                                .MinimumLevel.Debug()
                                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                                .Enrich.FromLogContext()
                                .WriteTo.Console()
                                .WriteTo.Seq("http://seq:5341", apiKey: "your-api-key")
                                .CreateLogger();


                // Important to call at exit so that batched events are flushed.
                builder.Services.AddSingleton<Serilog.ILogger>(sp => Log.Logger);
                builder.Services.AddSingleton<Serilog.Extensions.Hosting.DiagnosticContext>();

                var options = new ConfigurationReaderOptions { SectionName = "Seq" };


                // Add services to the container.
                builder.Host.UseSerilog();
                Log.Error("Starting up use");

                builder.Services.AddControllers();

                builder.Services.AddHsts(options =>
                {
                    options.Preload = true;
                    options.IncludeSubDomains = true;
                    options.MaxAge = TimeSpan.FromDays(60);
                });

                builder.Services.AddHttpsRedirection(options =>
                {
                    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                    options.HttpsPort = 8443;
                });

                builder.Services.Configure<CookiePolicyOptions>(options =>
                {
                    options.MinimumSameSitePolicy = SameSiteMode.Strict;
                    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
                    options.Secure = CookieSecurePolicy.Always;
                });



                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                var app = builder.Build();

                app.UseSerilogRequestLogging();

                Log.Error("Starting up useuseuse");

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseHsts();
                app.UseCookiePolicy();

                app.UseRouting();
                app.UseAuthorization();


                app.MapControllers();

                app.Run();

                Log.Information("Run");
                Log.CloseAndFlush();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}