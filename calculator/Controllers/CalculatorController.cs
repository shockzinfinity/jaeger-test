using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenTracing;

namespace Calculator.Controllers;

[Route("[controller]")]
[ApiController]
public class CalculatorController : ControllerBase
{
  private readonly IHttpClientFactory _clientFactory;
  private readonly ITracer _tracer;

  public CalculatorController(IHttpClientFactory clientFactory, ITracer tracer)
  {
    _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
    _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
  }

  [HttpGet("log")]
  public async Task<ActionResult> ComputeLog(int n, int x)
  {
    var actionName = ControllerContext.ActionDescriptor.DisplayName;
    using var scope = _tracer.BuildSpan(actionName).StartActive(true);
    var client = _clientFactory.CreateClient("logService");
    var response = await client.GetAsync($"/log/compute?n={n}&x={x}");

    return response.IsSuccessStatusCode
      ? Ok(Convert.ToDouble(await response.Content.ReadAsStringAsync()))
      : Problem("Log service failed");
  }
}
