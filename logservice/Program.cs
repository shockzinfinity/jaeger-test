using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders.Thrift;
using OpenTracing;
using OpenTracing.Contrib.NetCore.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTracing();
builder.Services.AddSingleton<ITracer>(sp =>
{
  var serviceName = sp.GetRequiredService<IWebHostEnvironment>().ApplicationName;
  var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

  //var reporter = new RemoteReporter.Builder().WithLoggerFactory(loggerFactory).WithSender(new UdpSender()).Build();
  var reporter = new RemoteReporter.Builder()
    .WithLoggerFactory(loggerFactory)
    .WithSender(new HttpSender(builder.Configuration.GetConnectionString("Jaeger", "http-thrift") ?? "http://localhost:14268/api/traces"))
    .Build();

  var tracer = new Tracer.Builder(serviceName)
                   .WithSampler(new ConstSampler(true))
                   .WithReporter(reporter)
                   .Build();

  return tracer;
});

builder.Services.Configure<HttpHandlerDiagnosticOptions>(options => options.OperationNameResolver = request => $"{request.Method.Method}: {request?.RequestUri?.AbsoluteUri}");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
