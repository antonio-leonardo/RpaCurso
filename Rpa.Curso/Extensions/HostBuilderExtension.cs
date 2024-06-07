using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Rpa.Curso.Extensions
{
    public static class HostBuilderExtension
    {
        public static IHostBuilder ConfigureAppSettings(this IHostBuilder host)
        {
            host.ConfigureAppConfiguration((ctx, builder) =>
            {
                builder.AddJsonFile("appsettings.json", false, true);
                builder.AddEnvironmentVariables();
            });
            return host;
        }
    }
}