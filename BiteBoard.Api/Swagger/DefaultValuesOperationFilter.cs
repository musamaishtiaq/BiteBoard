using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BiteBoard.API.Swagger;

public class DefaultValuesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            return;

        foreach (var parameter in operation.Parameters)
        {
            if (parameter.Name == "version" && parameter.In == ParameterLocation.Path)
            {
                parameter.Schema.Default = new Microsoft.OpenApi.Any.OpenApiString("1.0");
                parameter.Required = false;
            }
        }
    }
}