using Eva.Shared.Network;
using Microsoft.Extensions.Logging;

namespace Eva
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
                    fonts.AddFont("FontAwesomeRegular.otf", "FontRegular");
                    fonts.AddFont("FontAwesomeBrands.otf", "FontBrands");
                    fonts.AddFont("FontAwesomeSolid.otf", "FontSolid");
                });

#if WINDOWS
            builder.Services.AddSingleton<IWifiService, Platforms.Windows.WifiService>();
#endif
#if ANDROID
            builder.Services.AddSingleton<IWifiService, Platforms.Android.WifiService>();
#endif
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
