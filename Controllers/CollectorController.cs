using Microsoft.AspNetCore.Mvc;
using System.Data;
using SolarABC.Models;

namespace SolarABC.Controllers;

[ApiController]
[Route("/")]
public class CollectorController : ControllerBase
{
    private readonly ILogger<CollectorController> _logger;
    public CollectorController(ILogger<CollectorController> logger)
    {
        this._logger = logger;
    }

    [HttpGet("/Stats")]
    public ActionResult<Stats> GetStatistics(
        [FromQuery] PTC parabolicTC, double h1, double h2, double h3, double h4, double? mDot, double? Cp
    )
    {
        try
        {
            //Specific Enthalpy values
            double[] h = { h1, h2, h3, h4 };
            Stats statistics = mDot == null || Cp == null ?
                               new Stats(collector: parabolicTC, h: h) :
                               new Stats(collector: parabolicTC, h: h, fluidFlowRate: mDot, fluidSpecificHeat: Cp);
            return statistics;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, parabolicTC);
        }
    }
}