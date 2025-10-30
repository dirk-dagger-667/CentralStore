using FluentValidation;
using LocalStore;
using LocalStore.Domain;
using LocalStore.ProductManagement.CreateProduct;
using LocalStore.ProductManagement.RemoveProduct;
using LocalStore.ProductManagement.UpdateProduct;
using LocalStore.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(); ;

builder.Services.AddDbContext<LocalStoreDbContext>(options
  => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<ICreateProductService, CreateProductsService>();
builder.Services.AddScoped<IRemoveProductService, RemoveProductService>();
builder.Services.AddScoped<IUpdateProductService, UpdateProductService>();

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.ConfigureMassTransit();
builder.Services.Configure<QueueMetadata>(builder.Configuration
  .GetSection(QueueMetadata.SectionName));

ServiceDescriptor[] serviceDescriptors = Assembly.GetExecutingAssembly().DefinedTypes
  .Where(t => t.IsAssignableTo(typeof(IEndpoint)) && t.IsClass && !t.IsAbstract)
  .Select(t => ServiceDescriptor.Transient(typeof(IEndpoint), t))
  .ToArray();

builder.Services.TryAddEnumerable(serviceDescriptors);

var app = builder.Build();

using (var scope = app.Services.CreateAsyncScope())
{
  var db = scope.ServiceProvider.GetRequiredService<LocalStoreDbContext>();
  await db.Database.MigrateAsync();
}

using (var scope = app.Services.CreateScope())
{
  foreach (var endpoint in scope.ServiceProvider.GetServices<IEndpoint>())
  {
    endpoint.MapEndpoint(app);
  }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
