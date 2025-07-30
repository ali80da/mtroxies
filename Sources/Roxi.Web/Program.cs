/* MTRoxies */

using Roxi.Web.Middleware.MainGates;

var builder = WebApplication.CreateBuilder(args);
{


    // Add services to the container.

    builder.Services.AddControllers();
    // Configuring Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();


}
var app = builder.Build();
{


    // Configure The HTTP Request Pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();


    // Add Custom Gate Middleware 4 request/response
    app.UseMiddleware<RequestGateMiddleware>();
    app.UseMiddleware<ResponseGateMiddleware>();

    app.MapControllers();

}
app.Run();