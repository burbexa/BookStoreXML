using BookStoreXML.Endpoints;
using BookStoreXML.Models;
using BookStoreXML.Services;
using BookStoreXML.Services.Reports;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Options
builder.Services.Configure<XmlStoreOptions>(
    builder.Configuration.GetSection(XmlStoreOptions.SectionName));

// Register XmlStoreOptions instance for direct injection
builder.Services.AddSingleton<XmlStoreOptions>(provider =>
    provider.GetRequiredService<IOptions<XmlStoreOptions>>().Value);

// Services
builder.Services.AddSingleton<IBookRepository, BookXmlRepository>();
builder.Services.AddSingleton<IHtmlReportService, RazorReportService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.RegisterEndpoints();

app.Run();
