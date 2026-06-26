using System.Text;
using InvoicesWebService.Infrastructure;
using InvoicesWebService.Infrastructure.Data;
using InvoicesWebService.Infrastructure.Repositories;
using InvoicesWebService.Infrastructure.Repositories.Interfaces;
using InvoicesWebService.Services;
using InvoicesWebService.Services.Authorization;
using InvoicesWebService.Services.DraftServices;
using InvoicesWebService.Services.Interfaces;
using InvoicesWebService.Services.Kafka;
using Messaging.Kafka;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Shared;
using Prometheus;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared.Contracts.Events;
using Shared.Entities;

var builder = WebApplication.CreateBuilder(args);

//db
builder.Services.AddSingleton<AuditInterceptor>();
builder.Services.AddSingleton<EfCoreMetricsInterceptor>();
builder.Services.AddDbContextFactory<AppDbContext>();

builder.Services.AddDbContext<UserDbContext>();
builder.Services.AddIdentity<User, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<UserDbContext>()
    .AddDefaultTokenProviders();

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

//repo
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IDraftInvoiceRepository, DraftInvoiceRepository>();
builder.Services.AddScoped<IRawTransactionReader, RawTransactionReader>();

//services
builder.Services.AddScoped<IProcessingErrorService, ProcessingErrorService>();
builder.Services.AddScoped<IDraftInvoiceCreationService, DraftInvoiceCreationCreationService>();
builder.Services.AddScoped<IDraftInvoiceService, DraftInvoiceService>();
builder.Services.AddScoped<IPositionRoleSyncService, PositionRoleSyncService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.Configure<JwtConfiguration>(builder.Configuration.GetSection("Jwt"));

//kafka
builder.Services.AddConsumer<AggregationReadyEvent, AggregationReadyConsumer>
    (builder.Configuration.GetSection("Kafka:AggregationGroup"));

//controllers
builder.Services.AddControllers();

//authentication
builder.Services.Configure<JwtConfiguration>(builder.Configuration.GetSection("Jwt"));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters.ValidIssuer = builder.Configuration["Jwt:Issuer"];
        options.TokenValidationParameters.ValidAudience = builder.Configuration["Jwt:Audience"];
        options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]));
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CanViewInvoices", policy => 
        policy.RegisterPermissions("invoices:read"))

    .AddPolicy("CanManageInvoices", policy => 
        policy.RegisterPermissions("invoices:create", "invoices:update", "invoices:delete"))

    .AddPolicy("CanApproveInvoices", policy => 
        policy.RequireRole("Accountant", "Admin")
            .RegisterPermissions("invoices:approve"));

builder.Services.AddAuthorization();

//swagger
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

//openTelemetry
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeScopes = true;
    logging.IncludeFormattedMessage = true;
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("InvoicesWebService"))
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddNpgsqlInstrumentation()
        .AddMeter("InvoiceWebService")
        .AddPrometheusExporter())
    .WithTracing(tracing => 
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddNpgsql()
            .AddSource("InvoiceWebService")
            .AddOtlpExporter(o => 
            {
                o.Endpoint = new Uri("http://tempo:4318");
                o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            }));

var app = builder.Build();

app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.MapMetrics();

if (app.Environment.IsDevelopment())
{
    await app.SeedRolesAndPermissionsAsync();
    
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(o => o.DisplayRequestDuration());
}

app.UseRouting();
app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();

app.Run();