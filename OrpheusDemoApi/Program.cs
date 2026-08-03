using OrpheusDemoApi.Data;
using OrpheusDemoApi.Endpoints;
using OrpheusPostgreSQLDDLHelper;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------------------------
// This single line is all it takes to wire Orpheus into an ASP.NET Core application. It reads the
// connection named "PostgreSQL" out of the OrpheusConfiguration section of appsettings.json and
// registers a pooled connection factory, the PostgreSQL DDL helper and IOrpheusDatabase itself.
//
// The sibling packages expose AddOrpheusSqlServer and AddOrpheusMySql with identical signatures, so
// switching engines here plus swapping the project reference is the whole port.
// ---------------------------------------------------------------------------------------------
builder.Services.AddOrpheusPostgreSql(builder.Configuration, "PostgreSQL");

// One connected database per HTTP request. See Data/OrpheusSession.cs for why this exists.
builder.Services.AddScoped<OrpheusSession>();

builder.Services.AddSingleton<DemoDataSeeder>();
builder.Services.AddHostedService<SchemaInitializer>();

builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.WriteIndented = true);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options => options.WithTitle("OrpheusORM Demo API"));

app.MapCustomerEndpoints();
app.MapProductEndpoints();
app.MapSalesOrderEndpoints();
app.MapReportEndpoints();

app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

app.Run();
