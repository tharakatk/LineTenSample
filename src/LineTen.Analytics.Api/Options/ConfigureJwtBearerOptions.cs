using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LineTen.Analytics.Api.Options
{
    /// <summary>
    /// https://www.meziantou.net/jwt-authentication-with-asp-net-core.htm
    /// </summary>
    internal class ConfigureJwtBearerOptions : IPostConfigureOptions<JwtBearerOptions>
    {
        private readonly IOptions<JwtAuthentication> _jwtAuthentication;

        public ConfigureJwtBearerOptions(IOptions<JwtAuthentication> jwtAuthentication)
        {
            _jwtAuthentication = jwtAuthentication ?? throw new ArgumentNullException(nameof(jwtAuthentication));
        }

        public void PostConfigure(string name, JwtBearerOptions options)
        {
            var jwtAuthentication = _jwtAuthentication.Value;
            options.Events = new JwtBearerEvents()
            {
                OnAuthenticationFailed = async delegate(AuthenticationFailedContext context)
                {
                    Trace.WriteLine(context.Exception);
                    await Task.CompletedTask;
                },
            };
            options.MetadataAddress = $"{jwtAuthentication.IdentityProvider}/.well-known/openid-configuration";
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidAudiences = new List<string>
                {
                    "LineTen.Analytics.Api"
                },
                ValidIssuers = new List<string>
                {
                    jwtAuthentication.IdentityProvider
                },
                ValidateAudience = false, //not validating audiences for now
            };
            // The JwtBearer scheme knows how to extract the token from the Authorization header
            // but we will need to manually extract it from the query string in the case of requests to a SignalR Hub
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = ctx =>
                {
                    if (ctx.Request.Query.ContainsKey("access_token")){
                        ctx.Token = ctx.Request.Query["access_token"];
                    }
                    return Task.CompletedTask;
                }
            };
        }
    }
}
