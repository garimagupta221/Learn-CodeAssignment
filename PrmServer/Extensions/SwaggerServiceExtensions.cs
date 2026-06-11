using Microsoft.OpenApi;

namespace PrmServer.Extensions;

public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token in the field below."
            });

            c.AddSecurityRequirement(doc =>
            {
                var requirement = new OpenApiSecurityRequirement();
                requirement.Add(new OpenApiSecuritySchemeReference("Bearer", doc), new List<string>());
                return requirement;
            });
        });

        return services;
    }
}
