namespace Extensions
{
    public static class LoggingExtensions
    {
        public static void ConfigureLogging(this IServiceCollection services, IConfiguration config, string env)
        {
            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConfiguration(config.GetSection("Logging"));
                logging.AddConsole();

#if DEBUG
                logging.AddDebug();
#endif

                logging.SetMinimumLevel(LogLevel.Information);
            });
        }
    }
}
