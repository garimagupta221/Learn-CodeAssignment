using Microsoft.OpenApi;
using System.Reflection;

namespace PrmServer.Extensions;

public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token below. Example: **eyJhbGci...**"
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
