using Microsoft.AspNetCore.Mvc;
using System.Data;
using SolarABC.Models;

namespace SolarABC.Controllers;

[ApiController]
[Route("controller")]
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

    [HttpGet("/ET-150")]
    public ActionResult<Statistics> GetEutotrought(
        [FromQuery] EntryData etData
    )
    {
        PTC ptc = new PTC(5.8, 12, 800, etData.Cp, etData.Mi, 300, 0.095, 0.88, 0.8, 0.20, 0.0066, 0.0070, 0.0120, 0.0125);
        Statistics sts = new Statistics(ptc, etData);
        return sts;
    }
}