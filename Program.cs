var builder = WebApplication.CreateBuilder(args);

// Controller desteğini ekleyin
builder.Services.AddControllers();

var app = builder.Build();

// Basit bir test endpoint'i
app.MapGet("/", () => "Deprem Projesi API çalışıyor!");

// API Controller'ı devreye al
app.MapControllers();

app.Run();
