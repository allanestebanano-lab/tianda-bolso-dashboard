using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TiendaBolsos.Core.Modules.Auth.DTOs;
using TiendaBolsos.Core.Modules.Auth.Interfaces;

namespace TiendaBolsos.API.Controllers
{
    [ApiController]

    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;

        public AuthController(IAuthService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var response = await _service.RegisterAsync(dto);

            return Ok(response);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var response = await _service.LoginAsync(dto);

            return Ok(response);
        }
    }
}
