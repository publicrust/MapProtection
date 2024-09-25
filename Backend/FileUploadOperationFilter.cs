using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace Backend
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileParameters = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile))
                .ToList();

            if (fileParameters.Any())
            {
                // Удаляем параметры из Query, т.к. IFormFile не работает с QueryString
                operation.Parameters.Clear();

                // Добавляем поддержку multipart/form-data
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties =
                                {
                                    ["mapFile"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary",
                                        Description = "Upload a .map file"
                                    }
                                },
                                Required = new HashSet<string> { "mapFile" }
                            }
                        }
                    }
                };

                // Добавляем описание в документацию операции, указывая тип файла
                if (operation.Description == null)
                {
                    operation.Description = "This endpoint allows you to upload a .map file.";
                }
                else
                {
                    operation.Description += " Only .map files are allowed.";
                }
            }
        }
    }
}
