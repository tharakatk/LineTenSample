using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LineTen.Analytics.Api.Swagger
{
    /// <summary>
    /// Instructs swagger to put the "padlock" on the api operations
    /// </summary>
    public class AuthorizeOperationFilter : IOperationFilter
    {
        /// <inheritdoc />
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var authorizeAttributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                .Union(context.MethodInfo.GetCustomAttributes(true))
                .OfType<AuthorizeAttribute>().ToList();

            if (!authorizeAttributes.Any())
            {
                //No need to do anything
                return;
            }

            //For when a user is logged out
            operation.Responses.TryAdd(StatusCodes.Status401Unauthorized.ToString(), new OpenApiResponse { Description = "Unauthorized" });

            //For when a user is forbidden from taking an action
            operation.Responses.TryAdd(StatusCodes.Status403Forbidden.ToString(), new OpenApiResponse { Description = "Forbidden" });

            var customAuthenticationSchemes = authorizeAttributes.Where(y => !string.IsNullOrEmpty(y.AuthenticationSchemes)).Select(x => x.AuthenticationSchemes).ToList();
            if (!customAuthenticationSchemes.Any())
            {
                customAuthenticationSchemes.Add(SwaggerConstants.SecurityDefinitionBearer);
            }

            foreach (var customAuthenticationScheme in customAuthenticationSchemes)
            {
                var openApiSecurityScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = customAuthenticationScheme
                    }
                };
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [openApiSecurityScheme] = new string[] { }
                });
            }
        }
    }
}
