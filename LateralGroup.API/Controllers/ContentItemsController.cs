using LateralGroup.API.Authentication;
using LateralGroup.API.Contracts.Cms;
using LateralGroup.Application.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LateralGroup.API.Controllers
{
    [ApiController]
    [Route("api/content-items")]
    [Authorize(Policy = AuthConstants.ConsumerPolicy)]
    public class ContentItemsController : ControllerBase
    {
        private readonly ICmsContentQueryService _cmsContentQueryService;

        public ContentItemsController(ICmsContentQueryService cmsContentQueryService)
        {
            _cmsContentQueryService = cmsContentQueryService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _cmsContentQueryService.GetAllAsync(
                User.IsInRole(AuthConstants.AdminRole),
                cancellationToken);

            var response = result.Select(item => item.ToResponse());
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            
            if (string.IsNullOrWhiteSpace(id))  
                return BadRequest("Id cannot be null or empty.");

            var result = await _cmsContentQueryService.GetByIdAsync(
                id,
                User.IsInRole(AuthConstants.AdminRole),
                cancellationToken);
     
            if (result == null)
                return NotFound();

            var response = result.ToResponse();
            return Ok(response);
        }
    }
}
