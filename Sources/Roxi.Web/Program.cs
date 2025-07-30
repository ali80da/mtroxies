/* MTRoxies */

using Microsoft.OpenApi.Models;
using Roxi.Core.Services.Proxies;
using Roxi.Web.Middleware.MainGates;

var builder = WebApplication.CreateBuilder(args);
{


    // Add services to the container.

    builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });


    // Configuring Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "Roxi API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please enter a valid API key",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
        });
    });



    // Add Dependencies (from Roxi.Core)


}
var app = builder.Build();
{


    // Configure The HTTP Request Pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else if (app.Environment.IsProduction())
    {


        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();


    // Add Custom Gate Middleware 4 request/response
    app.UseMiddleware<RequestGateMiddleware>();
    app.UseMiddleware<ResponseGateMiddleware>();

    app.MapControllers();
    //app.UseEndpoints(endpoints => endpoints.MapControllers());

}
app.Run();