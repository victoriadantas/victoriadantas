using Quartz;
using Microsoft.EntityFrameworkCore;
using PaymentOrganizer.Api.Domain.Entities;
using PaymentOrganizer.Api.Infrastructure;
using PaymentOrganizer.Api.Integrations;
using PaymentOrganizer.Api.Jobs;
using PaymentOrganizer.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=payments.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// App Services
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<SyncService>();

// Integrations (simuladas)
builder.Services.AddSingleton<IProviderClient, GympassClient>();
builder.Services.AddSingleton<IProviderClient, TotalPassClient>();

// Quartz
builder.Services.AddQuartz(q =>
{
    var dailyKey = new Quartz.JobKey("DailySyncJob");
    q.AddJob<DailySyncJob>(opts => opts.WithIdentity(dailyKey));
    q.AddTrigger(t => t
        .ForJob(dailyKey)
        .WithIdentity("DailySyncJob-trigger")
        .WithCronSchedule("0 0 2 * * ?")); // 02:00 UTC diariamente

    var monthlyKey = new Quartz.JobKey("MonthlyBillingJob");
    q.AddJob<MonthlyBillingJob>(opts => opts.WithIdentity(monthlyKey));
    q.AddTrigger(t => t
        .ForJob(monthlyKey)
        .WithIdentity("MonthlyBillingJob-trigger")
        .WithCronSchedule("0 0 1 1 * ?")); // dia 1 às 01:00 UTC
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

var app = builder.Build();

// Criar banco/tabelas conforme o modelo (sem migrations por enquanto)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Minimal APIs para aprender CRUD
app.MapGet("/employees", async (ApplicationDbContext db) =>
{
    var employees = await db.Employees
        .Include(e => e.Subscriptions)
        .ToListAsync();
    return Results.Ok(employees);
});

app.MapPost("/employees", async (ApplicationDbContext db, Employee employee) =>
{
    db.Employees.Add(employee);
    await db.SaveChangesAsync();
    return Results.Created($"/employees/{employee.Id}", employee);
});

app.MapGet("/subscriptions", async (ApplicationDbContext db) =>
{
    var subs = await db.Subscriptions.Include(s => s.Employee).ToListAsync();
    return Results.Ok(subs);
});

app.MapGet("/payments", async (ApplicationDbContext db) =>
{
    var payments = await db.Payments
        .Include(p => p.Subscription)
        .ThenInclude(s => s.Employee)
        .ToListAsync();
    return Results.Ok(payments);
});

app.MapPost("/payments/{id:int}/mark-paid", async (int id, PaymentService svc) =>
{
    var updated = await svc.MarkAsPaidAsync(id, DateOnly.FromDateTime(DateTime.UtcNow));
    return updated == 1 ? Results.Ok() : Results.NotFound();
});

app.MapPost("/admin/generate-month", async (PaymentService svc) =>
{
    var now = DateTime.UtcNow;
    var target = new DateOnly(now.Year, now.Month, 1);
    var created = await svc.GenerateMonthlyPaymentsAsync(target);
    return Results.Ok(new { created });
});

app.MapPost("/admin/sync", async (SyncService svc) =>
{
    var now = DateTime.UtcNow;
    var month = new DateOnly(now.Year, now.Month, 1);
    var updated = await svc.SyncMonthAsync(month);
    return Results.Ok(new { updated });
});

app.Run();
