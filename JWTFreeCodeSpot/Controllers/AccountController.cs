using Dapper;
using JWTFreeCodeSpot.Model;
using JWTFreeCodeSpot.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Threading.Tasks;
using System;

namespace JWTFreeCodeSpot.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IJWTAuthManager _auth;

        public AccountController(IJWTAuthManager auth)
        {
            _auth = auth;
        }        

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel user)
        {
            if (!ModelState.IsValid)
                return BadRequest("Parameters are missing");

            DynamicParameters p = new();

            p.Add("UserName", user.UserName);
            p.Add("Password", user.Password);

            p.Add("retVal", DbType.String, direction: ParameterDirection.Output);

            var result =  await _auth.Execute_Command<User>("sp_loginUser", p);
            if(result.Code == 200)
            {
                var token = _auth.GenerateJWT(result.Data);
                return Ok(token);
            }

            return NotFound(result.Data);
        }


        [HttpGet("UserList")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _auth.getUserList<User>();

            return Ok(result);
        }


        [HttpPost("Register")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest("Parameters are missing");

            DynamicParameters p = new();

            p.Add("UserName", user.UserName);
            p.Add("Password", user.Password);
            p.Add("Email", user.Email);
            p.Add("Date", DateTime.Now);
            p.Add("Role", user.Role);            

            p.Add("Retval", DbType.Int32, direction: ParameterDirection.Output);

            var result = await _auth.Execute_Command<User>("sp_RegisterUser", p);

            if(result.Code == 200)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("Delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Paramater is missing");

            DynamicParameters p = new();
            
            p.Add("IdUser", id);
            p.Add("ReVal", DbType.String, direction: ParameterDirection.Output);

            var result = await _auth.Execute_Command<User>("sp_DeleteUser", p);
            if (result.Code == 200)
                return Ok(result);

            return NotFound(result);
        }
    }
}
