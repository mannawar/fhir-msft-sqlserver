using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Microsoft.EntityFrameworkCore;
using NetCrudApp.Data;
using NetCrudApp.Entity;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        IList<JsonConverter> fhirConverters = opt.JsonSerializerOptions.ForFhir(ModelInfo.ModelInspector).Converters;
        IList<JsonConverter> convertersToAdd = new List<JsonConverter>(fhirConverters);
        foreach(JsonConverter fhirConverter in convertersToAdd)
        {
            opt.JsonSerializerOptions.Converters.Add(fhirConverter);
        }
        opt.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddSingleton<FhirClient>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var baseUrl = configuration["FhirServer:BaseUrl"];
    return new FhirClient(baseUrl);
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(); // added this line to converse with fhir server microsoft



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
