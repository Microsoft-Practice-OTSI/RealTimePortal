using RealTimePortal.API.Data;
using RealTimePortal.API.Extensions;
using RealTimePortal.API.Hubs;
using RealTimePortal.API.Logging;
using RealTimePortal.API.Middleware;
using RealTimePortal.API.Services;
using RealTimePortal.Application;
using RealTimePortal.Infrastructure;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// Logging
// --------------------------------------------------

builder.Host.AddApiLogging();

// --------------------------------------------------
// Services
// --------------------------------------------------

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

builder.Services.AddSignalR();

builder.Services.AddInfrastructure(
    builder.Configuration);


builder.Services.AddApiServices(
    builder.Configuration);


builder.Services.AddScoped<ProcessService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<ChatService>();
//builder.Services.AddScoped<ProgressService>();
builder.Services.AddScoped<IProgressGateway, ProgressGateway>();
builder.Services.AddScoped<IDashboardGateway, DashboardGateway>();
builder.Services.AddScoped<IChatGateway, ChatGateway>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<ChatUserService>();
//builder.Services.AddScoped<IUserRepository, UserRepository>();
//================================
// Application Pipeline
//================================
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await AuthSeeder.SeedAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("DefaultPolicy");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<RealTimeHub>("/hubs/realtime");

app.Run();
