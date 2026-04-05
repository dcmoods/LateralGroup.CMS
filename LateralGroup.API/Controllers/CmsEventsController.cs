using LateralGroup.API.Authentication;
using LateralGroup.API.Contracts.Cms;
using LateralGroup.Application.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LateralGroup.API.Controllers;


[ApiController]
[Route("cms/events")]
[Authorize(Policy = AuthConstants.CmsPolicy)]
public class CmsEventsController : ControllerBase
{
    private readonly ICmsEventProcessor _eventProcessor;

    public CmsEventsController(ICmsEventProcessor eventProcessor)
    {
        _eventProcessor = eventProcessor ?? throw new ArgumentNullException(nameof(eventProcessor));
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] List<CmsEventRequest> request, CancellationToken cancellationToken)
    {

        if (request is null || !request.Any())
        {
            return BadRequest("Request body is required.");
        }

        var processInput = request
            .Select((item, index) => item.ToProcessInput(index))
            .ToList();
            
        var result = await _eventProcessor.ProcessAsync(processInput, cancellationToken);

        return Ok(result);
    }
}
