namespace Api.Extensions
{
    public static class ServiceExtensions
    {

        // Cross Origin Policies Configurations
        public static void ConfigureCors(this IServiceCollection services) =>
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                builder.AllowAnyMethod()
                       .AllowAnyHeader()
                       .WithOrigins("http://localhost:3000"));
            });
    }
}
