using System.Reflection;
using Exceltomysql.Application.Services;
using Exceltomysql.Domain.Ports;
using Exceltomysql.Domain.Utils;
using Exceltomysql.Infrastructure.Swagger;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddScoped<ILoadInfoService, LoadInforServiceImpl>();
builder.Services.AddScoped<FileHelper>();
builder.Services.AddScoped<ExcelHelper>();
builder.Services.AddScoped<MySqlUtilHelper>();
// builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // options.SwaggerDoc("v1", new OpenApiInfo
    // {

    //     Title = "Your API",
    //     Version = "v1",
    //     Description = "API for handling file uploads",
    // });

    // options.OperationFilter<FileUploadOperationFilter>();
    // options.SchemaFilter<FormFileSchemaFilter>();
    // options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});
var app = builder.Build();

// app.UseRouting();
if (app.Environment.IsDevelopment())
{
    // app.UseDeveloperExceptionPage();
    app.UseSwagger();
    // app.UseSwaggerUI();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("./v1/swagger.json", "Your API V1");
    });
}
app.MapControllers();
app.Run();


