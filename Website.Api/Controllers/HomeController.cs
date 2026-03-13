using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Website.Models;
using Website.Services.Interfaces;

namespace Website.Controllers
{
    [ApiController]
    [Route("api/")]
    public class HomeController : ControllerBase
    {
        private readonly IFinderService _finderService;

        public HomeController(IFinderService finderService)
        {
            _finderService = finderService;
        }
 
        [HttpGet("find-match")]
        public IActionResult Index([FromQuery, BindRequired] FinderRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(ResultDto.Failure(errors));
            }

            var result = _finderService.FindMatches(
                model.StartYear,
                model.EndYear,
                model.DayOfMonth,
                model.TargetDayOfWeek
            );

            if (result.IsFailure)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
