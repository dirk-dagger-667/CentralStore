using CentralStore.Domain;
using CentralStore.ProductManagement.CreateProduct;
using CentralStore.ProductManagement.RemoveProduct;
using CentralStore.ProductManagement.UpdateProduct;
using CentralStore.Shared;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace CentralStore
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);
      // Add services to the container.
      // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
      builder.Services.AddOpenApi();
      builder.Services.AddSwaggerGen();

      builder.Services.ConfigureHttpJsonOptions(options =>
      {
        options.SerializerOptions.PropertyNameCaseInsensitive = true;
      });

      builder.Services.AddScoped<ICreateProductService, CreateProductsService>();
      builder.Services.AddScoped<IRemoveProductService, RemoveProductService>();
      builder.Services.AddScoped<IUpdateProductService, UpdateProductService>();
      builder.Services.AddScoped<IMassTransitSendResolver, EndpointUriResolver>();

      builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

      if (!builder.Environment.IsEnvironment(SharedConstants.IntegrationTestsEnvironement))
      {
        builder.Services.AddDbContext<CentralStoreDbContext>(options
        => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

        builder.ConfigureMassTransit();
      }

      builder.Services.Configure<QueueMetadata>(builder.Configuration
        .GetSection(QueueMetadata.SectionName));

      ServiceDescriptor[] serviceDescriptors = Assembly.GetExecutingAssembly().DefinedTypes
        .Where(t => t.IsAssignableTo(typeof(IEndpoint)) && t.IsClass && !t.IsAbstract)
        .Select(t => ServiceDescriptor.Transient(typeof(IEndpoint), t))
        .ToArray();

      builder.Services.TryAddEnumerable(serviceDescriptors);

      var app = builder.Build();

      if (!app.Environment.IsEnvironment(SharedConstants.IntegrationTestsEnvironement))
      {
        using (var scope = app.Services.CreateAsyncScope())
        {
          var db = scope.ServiceProvider.GetRequiredService<CentralStoreDbContext>();
          await db.Database.MigrateAsync();
        }
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
        app.UseDeveloperExceptionPage();
      }

      app.UseHttpsRedirection();
      app.Run();
    }
  }
}