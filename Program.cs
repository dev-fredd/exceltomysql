using Exceltomysql.Application.Services;
using Exceltomysql.Domain.Ports;
using Exceltomysql.Domain.Utils;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddScoped<ILoadInfoService,LoadInforServiceImpl>();
builder.Services.AddScoped<FileHelper>(); 
builder.Services.AddScoped<ExcelHelper>(); 
builder.Services.AddScoped<MySqlUtilHelper>(); 
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();


