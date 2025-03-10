using DotNetty.Common.Internal.Logging;

namespace JT1078NetCore.Common
{
    public static class CommonHelper
    {
        static CommonHelper()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(ProcessDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
        }

        public static string ProcessDirectory
        {
            get
            {
#if NETSTANDARD2_0 || NETCOREAPP3_1_OR_GREATER || NET5_0_OR_GREATER
                return AppContext.BaseDirectory;
#else
                return AppDomain.CurrentDomain.BaseDirectory;
#endif
            }
        }

        public static IConfigurationRoot Configuration { get; }

        public static void SetConsoleLogger() => InternalLoggerFactory.DefaultFactory = LoggerFactory.Create(builder => builder.AddConsole());
    }
}
