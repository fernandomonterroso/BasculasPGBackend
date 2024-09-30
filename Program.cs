using BasculasPG.DataAccess;
using BasculasPG.Handlers;
using BasculasPG.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // Permitir cualquier origen
              .AllowAnyMethod()   // Permitir cualquier m�todo (GET, POST, PUT, DELETE, etc.)
              .AllowAnyHeader();  // Permitir cualquier encabezado
    });
});

// Configuraci�n del servicio de autenticaci�n JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false, // Desactiva si no usas Issuer
        ValidateAudience = false, // Desactiva si no usas Audience
        ValidateLifetime = true, // Valida expiraci�n del token
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
    };

    // Evento para modificar la respuesta en caso de error de autorizaci�n
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            // Evitar la respuesta 401 predeterminada
            context.HandleResponse();

            // Definir el JSON personalizado para respuesta 401
            var result = new
            {
                success = false,
                message = "No autorizado. Se requiere un token v�lido."
            };

            // Establecer el c�digo de estado y el contenido JSON
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
        }
    };
});

builder.Services.AddScoped<MySqlConnectionFactory>();
builder.Services.AddScoped<MySqlDbManager>();
builder.Services.AddScoped<GeneralHandler>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
