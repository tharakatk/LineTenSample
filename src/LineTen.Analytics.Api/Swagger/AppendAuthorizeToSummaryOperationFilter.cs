using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LineTen.Analytics.Api.Swagger
{
    public class PolicySelectorWithLabel<T> where T : Attribute
    {
        public Func<IEnumerable<T>, IEnumerable<string>> Selector { get; set; }

        public string Label { get; set; }
    }
    internal static class OperationFilterContextExtensions
    {
        public static ICollection<T> GetControllerAndActionAttributes<T>(this OperationFilterContext context) where T : Attribute
        {
            var controllerAttributes = context.MethodInfo.DeclaringType.GetTypeInfo().GetCustomAttributes<T>();
            var actionAttributes = context.MethodInfo.GetCustomAttributes<T>();

            var result = new List<T>(controllerAttributes);
            result.AddRange(actionAttributes);
            return result;
        }
    }
    public class AppendAuthorizeToSummaryOperationFilter<T> : IOperationFilter where T : Attribute
    {
        private readonly IEnumerable<PolicySelectorWithLabel<T>> policySelectors;

        /// <summary>
        /// Constructor for AppendAuthorizeToSummaryOperationFilter
        /// </summary>
        /// <param name="policySelectionCondition">Selects which attributes have policies. e.g. (a => !string.IsNullOrEmpty(a.Policy))</param>
        /// <param name="policySelector">Used to select the authorization policy from the attribute e.g. (a => a.Policy)</param>
        public AppendAuthorizeToSummaryOperationFilter(IEnumerable<PolicySelectorWithLabel<T>> policySelectors)
        {
            this.policySelectors = policySelectors;
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.GetControllerAndActionAttributes<AllowAnonymousAttribute>().Any())
            {
                return;
            }

            var authorizeAttributes = context.GetControllerAndActionAttributes<T>();

            if (authorizeAttributes.Any())
            {
                var authorizationDescription = new StringBuilder(" (Auth");

                foreach (var policySelector in policySelectors)
                {
                    AppendPolicies(authorizeAttributes, authorizationDescription, policySelector);
                }

                operation.Summary += " ---- " + authorizationDescription.ToString().TrimEnd(';') + ")";
            }
        }

        private static void AppendPolicies(IEnumerable<T> authorizeAttributes, StringBuilder authorizationDescription, PolicySelectorWithLabel<T> policySelector)
        {
            var policies = policySelector.Selector(authorizeAttributes)
                .OrderBy(policy => policy);

            if (policies.Any())
            {
                authorizationDescription.Append($" {policySelector.Label}: {string.Join(", ", policies)};");
            }
        }
    }
    public class AppendAuthorizeToSummaryOperationFilter : IOperationFilter
    {
        private readonly AppendAuthorizeToSummaryOperationFilter<AuthorizeAttribute> _filter;

        public AppendAuthorizeToSummaryOperationFilter()
        {
            var policySelector = new PolicySelectorWithLabel<AuthorizeAttribute>
            {
                Label = "policies",
                Selector = authAttributes =>
                    authAttributes
                        .Where(a => !string.IsNullOrEmpty(a.Policy))
                        .Select(a => a.Policy)
            };

            var rolesSelector = new PolicySelectorWithLabel<AuthorizeAttribute>
            {
                Label = "roles",
                Selector = authAttributes =>
                    authAttributes
                        .Where(a => !string.IsNullOrEmpty(a.Roles))
                        .Select(a => a.Roles)
            };

            _filter = new AppendAuthorizeToSummaryOperationFilter<AuthorizeAttribute>(new[] { policySelector, rolesSelector }.AsEnumerable());
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            _filter.Apply(operation, context);
        }
    }
}
