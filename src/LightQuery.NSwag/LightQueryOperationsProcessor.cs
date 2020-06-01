using LightQuery.EntityFrameworkCore;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.Linq;
using System.Reflection;

namespace LightQuery.NSwag
{
    public class LightQueryOperationsProcessor : IOperationProcessor
    {
        public bool Process(OperationProcessorContext context)
        {
            if (context.MethodInfo.GetCustomAttributes()
                .Any(a => a is LightQueryAttribute
                    || a is AsyncLightQueryAttribute))
            {
                context.OperationDescription
                    .Operation
                    .Parameters
                    .Add(new OpenApiParameter
                    {
                        Name = "sort",
                        Kind = OpenApiParameterKind.Query,
                        Description = "sort",
                        Schema = new NJsonSchema.JsonSchema
                        {
                            Type = NJsonSchema.JsonObjectType.String
                        }
                    });
                context.OperationDescription
                    .Operation
                    .Parameters
                    .Add(new OpenApiParameter
                    {
                        Name = "thenSort",
                        Kind = OpenApiParameterKind.Query,
                        Description = "then sort",
                        Schema = new NJsonSchema.JsonSchema
                        {
                            Type = NJsonSchema.JsonObjectType.String
                        }
                    });
                context.OperationDescription
                    .Operation
                    .Parameters
                    .Add(new OpenApiParameter
                    {
                        Name = "pageSize",
                        Kind = OpenApiParameterKind.Query,
                        Description = "page size",
                        Schema = new NJsonSchema.JsonSchema
                        {
                            Type = NJsonSchema.JsonObjectType.Integer
                        }
                    });
                context.OperationDescription
                    .Operation
                    .Parameters
                    .Add(new OpenApiParameter
                    {
                        Name = "page",
                        Kind = OpenApiParameterKind.Query,
                        Description = "page",
                        Schema = new NJsonSchema.JsonSchema
                        {
                            Type = NJsonSchema.JsonObjectType.Integer
                        }
                    });
            }

            return true;
        }
    }
}
