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
        private readonly ICmsAdminService _cmsAdminService;

        public ContentItemsController(
            ICmsContentQueryService cmsContentQueryService,
            ICmsAdminService cmsAdminService)
        {
            _cmsContentQueryService = cmsContentQueryService ?? throw new ArgumentNullException(nameof(cmsContentQueryService));
            _cmsAdminService = cmsAdminService ?? throw new ArgumentNullException(nameof(cmsAdminService));
        }


        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var userIsAdmin = User.IsInRole(AuthConstants.AdminRole);
            var result = await _cmsContentQueryService.GetAllAsync(
                userIsAdmin,
                cancellationToken);

            var response = userIsAdmin
                    ? result.Select(item => item.ToAdminResponse())
                    : result.Select(item => item.ToResponse());

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            var userIsAdmin = User.IsInRole(AuthConstants.AdminRole);
            var result = await _cmsContentQueryService.GetByIdAsync(
                id,
                userIsAdmin,
                cancellationToken);

            if (result == null)
                return NotFound();

            
            var response = userIsAdmin ? result.ToAdminResponse() : result.ToResponse();
            return Ok(response);
        }

        [HttpPost("{id}/disable")]
        [Authorize(Policy = AuthConstants.AdminPolicy)]
        public async Task<IActionResult> Disable(string id, CancellationToken cancellation)
        {
            var result = await _cmsAdminService.DisableAsync(id, cancellation);
            return result switch
            {
                CmsAdminActionResult.NotFound => NotFound(),
                CmsAdminActionResult.NoChange => NoContent(),
                CmsAdminActionResult.Updated => NoContent(),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        [HttpPost("{id}/enable")]
        [Authorize(Policy = AuthConstants.AdminPolicy)]
        public async Task<IActionResult> Enable(string id, CancellationToken cancellation)
        {
            var result = await _cmsAdminService.EnableAsync(id, cancellation);
            return result switch
            {
                CmsAdminActionResult.NotFound => NotFound(),
                CmsAdminActionResult.NoChange => NoContent(),
                CmsAdminActionResult.Updated => NoContent(),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }
    }
}
