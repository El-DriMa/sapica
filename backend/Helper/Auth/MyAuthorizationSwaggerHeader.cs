using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace sapica_backend.Helper.Auth
{
    public class MyAuthorizationSwaggerHeader : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            //operation.Parameters.Add(new OpenApiParameter
            //{
            //    Name = "my-auth-token",
            //    In = ParameterLocation.Header,
            //    Description = "upisati token preuzet iz autentikacijacontrollera"
            //});
        }
    }
}
