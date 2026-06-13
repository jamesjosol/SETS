using Reposi;
using SETS.Server.Hubs;
using SETS.Server.Services;

var builder = WebApplication.CreateBuilder(args);


SetsConnection.Initialize(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR(options =>  
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.HandshakeTimeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddSingleton<IUserPresenceService, UserPresenceService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:39722")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseSession();          
app.UseAuthorization();
app.MapControllers();
app.MapHub<ContingencyHub>("/hubs/contingency");
app.MapHub<AnnouncementHub>("/hubs/announcement");
app.MapHub<NotificationHub>("/hubs/notification");
app.MapHub<PresenceHub>("/hubs/presence");
app.MapFallbackToFile("/index.html");

app.Run();