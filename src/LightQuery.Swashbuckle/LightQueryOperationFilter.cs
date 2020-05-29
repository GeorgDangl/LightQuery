using System.Linq;
using System.Reflection;
using LightQuery.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LightQuery.Swashbuckle
{
    /// <summary>
    /// Generates operation filter for LightQuery's paramters
    /// Swashbuckle <see cref="IOperationFilter"/>
    /// </summary>
    /// <remarks>OpenAPI Support</remarks>
    public class LightQueryOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (((ControllerActionDescriptor)context.ApiDescription.ActionDescriptor).MethodInfo.GetCustomAttributes().Any(a => a is AsyncLightQueryAttribute || a is LightQueryAttribute))
            {
                // sort
                operation.Parameters.Add(new OpenApiParameter()
                {
                    Name = "sort",
                    In = ParameterLocation.Query,
                    Description = "sort",
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Example = new OpenApiString("CreatedAt desc")
                    }
                });

                // thenSort
                operation.Parameters.Add(new OpenApiParameter()
                {
                    Name = "thenSort",
                    In = ParameterLocation.Query,
                    Description = "then sort",
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Example = new OpenApiString("UpdatedAt desc")
                    }
                });

                // pageSize
                operation.Parameters.Add(new OpenApiParameter()
                {
                    Name = "pageSize",
                    In = ParameterLocation.Query,
                    Description = "page size",
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = "integer",
                        Example = new OpenApiInteger(10)
                    }
                });

                // page
                operation.Parameters.Add(new OpenApiParameter()
                {
                    Name = "page",
                    In = ParameterLocation.Query,
                    Description = "page",
                    Required = false,
                    Schema = new OpenApiSchema
                    {
                        Type = "integer",
                        Example = new OpenApiInteger(1)
                    }
                });
            }
        }
    }
}