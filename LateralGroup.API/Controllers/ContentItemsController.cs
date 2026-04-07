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

            //if (userIsAdmin)
            //{
            //    var response = result.Select(item => item.ToAdminResponse());
            //    return Ok(response);
            //}
            //else
            //{
            //    var response = result.Select(item => item.ToResponse());
            //    return Ok(response);
            //}

            var response = userIsAdmin
                    ? result.Select(item => item.ToAdminResponse())
                    : result.Select(item => item.ToResponse());

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {

            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Id cannot be null or empty.");

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
            if (id == null)
                return BadRequest("Id cannot be null.");

            var updated = await _cmsAdminService.DisableAsync(id, cancellation);
            return updated ? NoContent() : NotFound();
        }

        [HttpPost("{id}/enable")]
        [Authorize(Policy = AuthConstants.AdminPolicy)]
        public async Task<IActionResult> Enable(string id, CancellationToken cancellation)
        {
            if (id == null)
                return BadRequest("Id cannot be null.");
            var updated = await _cmsAdminService.EnableAsync(id, cancellation);
            return updated ? NoContent() : NotFound();
        }
    }
}
