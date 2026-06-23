
using Microsoft.Extensions.Configuration;

namespace UJStudentGorvenanceStudentWeb.Helper
{
    public static class AppSettings
    {
        public static bool IsTestEnvironment => bool.Parse(ConfigFile("Environment:IsLocal")!);
        public static string? SrcApi => ConfigFile("Api:SrcApi");
        public static string? TokenIssuer => ConfigFile("Jwt:Issuer");
        public static string? ItsApi => ConfigFile("ExternalApi:ItsApi");


        private static string? ConfigFile(string appSetting)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var configuration = new ConfigurationBuilder()
                                          .SetBasePath(Directory.GetCurrentDirectory())
                                          .AddJsonFile(basePath + "/appsettings.json").Build();

                return configuration[appSetting];
        }

    }
}
