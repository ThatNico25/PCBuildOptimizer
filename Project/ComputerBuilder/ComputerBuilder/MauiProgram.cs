using ComputerBuilder.Services;
using Microsoft.Extensions.Logging;

namespace ComputerBuilder
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<IBestBuildResultApiClient>(sp =>
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };

                var httpClientValuePrediction = new HttpClient(handler)
                {
                    BaseAddress = new Uri("https://localhost:59214/") 
                };


                var httpClientDataClassification = new HttpClient(handler)
                {
                    BaseAddress = new Uri("https://localhost:59264/") 
                };

                return new BestBuildResultApiClient(httpClientValuePrediction, httpClientDataClassification);
            });

            builder.Services.AddTransient<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
