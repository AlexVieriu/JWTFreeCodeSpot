using Dapper;
using JWTFreeCodeSpot_Original.Models;
using JWTFreeCodeSpot_Original.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace JWTFreeCodeSpot_Original.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IJWTAuthManager _auth;

        public AccountController(IJWTAuthManager auth)
        {
            _auth = auth;
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginModel user)
        {
            if (!ModelState.IsValid)
                return BadRequest("Parameter is missing");

            DynamicParameters d_params = new();
            d_params.Add("email", user.email);
            d_params.Add("password", user.password);
            d_params.Add("retVal", DbType.Int32, direction: ParameterDirection.Output);

            var result = _auth.Execute_Command<User>("sp_loginUser", d_params);
            if (result.code == 200)
            {
                var token = _auth.GenerateJWT(result.Data);
                return Ok(token);
            }

            return NotFound(result.Data);
        }

        [HttpGet("UserList")]
        [Authorize(Roles = "User")]
        public IActionResult getAllUsers()
        {
            var result = _auth.getUserList<User>();
            return Ok(result);
        }

        [HttpPost("Register")]
        [Authorize(Roles = "Admin")]
        public IActionResult Register([FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest("Parameters are missing");

            DynamicParameters d_params = new();
            d_params.Add("email", user.Email);
            d_params.Add("username", user.Username);
            d_params.Add("password", user.Password);
            d_params.Add("role", user.Role);
            d_params.Add("retVal", DbType.Int32, direction: ParameterDirection.Output);

            var result = _auth.Execute_Command<User>("sp_registerUser", d_params);
            if (result.code == 200)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpDelete("Delete")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(string id)
        {
            if (id == string.Empty)
                return BadRequest("Parameter is missing");

            DynamicParameters d_params = new();
            d_params.Add("userid", id);
            d_params.Add("retVal", DbType.String, direction: ParameterDirection.Output);

            var result = _auth.Execute_Command<User>("sp_deleteUser", d_params);

            if (result.code == 200)
                return Ok(result);

            return BadRequest(result);
        }
    }
}
