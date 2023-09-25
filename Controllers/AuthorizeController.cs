using System;
using Microsoft.AspNetCore.Mvc;
using ReppadL.IRepo;
using ReppadL.Model;
using ReppadL.Model.PostModel;

namespace ReppadL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizeController: ControllerBase
	{

		private readonly IAuthorizeRepo _authRepo;

		public AuthorizeController(IAuthorizeRepo authRepo)
		{
			_authRepo = authRepo;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] PostUserRegister user)
		{
			var result = await _authRepo.Register(user);

            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] PostUserLogin user)
        {
            var result = await _authRepo.Login(user);

            return result != null ? Ok(result) : NotFound();
        }
    }
}

