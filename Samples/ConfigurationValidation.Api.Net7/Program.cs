using ConfigurationValidation.AspNetCore;

namespace ConfigurationValidation.Api.Net7
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // <----- Register validatable configuration(s) you define in your app.
            builder.Services.ConfigureValidatableSetting<SampleLogicConfig>(builder.Configuration.GetSection("LogicConfiguration"));

            // <----- This prevents startup when config is wrong (should see logging)
            // builder.Services.AddConfigurationValidation();

            // <----- Add Health check for configuration (also enable app.UseHeathChecks() below)
            // builder.Services.AddConfigurationHealthCheck(builder.Environment.IsDevelopment());

            builder.Services.AddControllers();


            var app = builder.Build();
            app.UseHttpsRedirection();

            // <----- Displays yellow screen of error when config is wrong (only for root URL ("/")
            app.UseConfigurationValidationErrorPage();

            // <----- Enable together with builder.Services.AddConfgurationHealthCheck() above.
            // app.UseHealthChecks("/health");

            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
