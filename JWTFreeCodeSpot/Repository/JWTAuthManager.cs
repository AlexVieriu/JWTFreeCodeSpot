using Dapper;
using JWTFreeCodeSpot.Model;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JWTFreeCodeSpot.Repository
{
    public class JWTAuthManager : IJWTAuthManager
    {
        private readonly IConfiguration _config;

        public JWTAuthManager(IConfiguration config)
        {
            _config = config;
        }

        public async Task<Response<T>> Execute_Command<T>(string procedure, DynamicParameters sp_params)
        {
            Response<T> response = new();

            using (IDbConnection connection = new SqlConnection(_config.GetConnectionString("Default")))
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                using var transaction = connection.BeginTransaction();
                try
                {

                    var data = await connection.QueryAsync<T>(procedure,
                                                              sp_params,
                                                              commandType: CommandType.StoredProcedure,
                                                              transaction: transaction);

                    response.Data = data.FirstOrDefault();
                    response.Code = sp_params.Get<int>("Retval");
                    response.Message = "Success";
                    transaction.Commit();
                }
                catch(Exception ex)
                {
                    transaction.Rollback();
                    response.Code = 500;
                    response.Message = $"{ex.Message} - {ex.InnerException}";
                }
            };

            return response;
        }       

        public async Task<Response<List<T>>> getUserList<T>()
        {
            Response<List<T>> response = new();

            using IDbConnection db = new SqlConnection(_config.GetConnectionString("Default"));
            string query = "Select * from Users";
            
            var data = await db.QueryAsync<T>(query, null, commandType: CommandType.Text);
            response.Data = data.ToList();
                        
            return response;
        }


        public Response<string> GenerateJWT(User user)
        {
            Response<string> response = new();

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("roles", user.Role),
                new Claim("Date", DateTime.Today.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                _config["JWT:Key"],
                _config["JWT:Key"],
                claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: credentials
                );

            response.Data = new JwtSecurityTokenHandler().WriteToken(token);
            response.Code = 200;
            response.Message = "Token generated";

            return response;
        }
    }
}
