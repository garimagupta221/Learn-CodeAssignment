using Microsoft.AspNetCore.Mvc;
using PrmServer.Repositories.Interfaces;

namespace PrmServer.Controllers
{
    [ApiController]
    [Route("api/activity-tags")]
    public class ActivityTagController : ControllerBase
    {
        private readonly IActivityTagRepository _activityTagRepository;

        public ActivityTagController(IActivityTagRepository activityTagRepository)
        {
            _activityTagRepository = activityTagRepository;
        }

        /// <summary>
        /// Retrieves all activity tags.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tags = await _activityTagRepository.GetAllAsync();
            return Ok(tags);
        }
    }
}
