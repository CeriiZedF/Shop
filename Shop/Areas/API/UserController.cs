using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Shop.DAL.Repository.IRepository;

namespace Shop.Areas.API
{
    [Route("api/[controller]/")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet("GetAll")]
        // api/User/GetAll
        public async Task<ActionResult> GetAll()
        {
            var users = await _userRepository.GetAll();
            return Ok(JsonSerializer.Serialize(users));
        }
    }
}
