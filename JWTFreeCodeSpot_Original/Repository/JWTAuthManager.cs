using Dapper;
using JWTFreeCodeSpot_Original.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace JWTFreeCodeSpot_Original.Repository
{
    public class JWTAuthManager : IJWTAuthManager
    {
        private readonly IConfiguration _config;

        public JWTAuthManager(IConfiguration config)
        {
            _config = config;
        }

        public Response<string> GenerateJWT(User user)
        {
            var response = new Response<string>();

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtAuth:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]{
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("roles", user.Role),
                new Claim("Date", DateTime.Now.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                _config["JwtAuth:Issuer"],
                _config["JwtAuth:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(5),
                signingCredentials: credentials
                );

            response.Data = new JwtSecurityTokenHandler().WriteToken(token);
            response.code = 200;
            response.message = "Token generated";

            return response;
        }

        public Response<T> Execute_Command<T>(string query, DynamicParameters sp_params)
        {
            var response = new Response<T>();

            using (IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("default")))
            {
                if (dbConnection.State == ConnectionState.Closed)
                    dbConnection.Open();

                using var transation = dbConnection.BeginTransaction();
                try
                {
                    response.Data = dbConnection.Query<T>(query,
                                                          sp_params,
                                                          commandType: CommandType.StoredProcedure,
                                                          transaction: transation).FirstOrDefault();

                    response.code = sp_params.Get<int>("retVal");
                    response.message = "Success";
                    transation.Commit();
                }
                catch (Exception e)
                {
                    transation.Rollback();
                    response.code = 500;
                    response.message = $"{e.Message} - {e.InnerException}";
                }

                return response;
            }
        }

        public Response<List<T>> getUserList<T>()
        {
            var response = new Response<List<T>>();
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString("default"));

            string query = "Select userid, username, email, [role], reg_date FROM tbl_users";
            response.Data = connection.Query<T>(query, null, commandType: CommandType.Text).ToList();

            return response;
        }
    }
}
