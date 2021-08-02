using Dapper;
using JWTFreeCodeSpot_Original.Models;
using System.Collections.Generic;

namespace JWTFreeCodeSpot_Original.Repository
{
    public interface IJWTAuthManager
    {
        Response<string> GenerateJWT(User user);
        Response<T> Execute_Command<T>(string query, DynamicParameters sp_params);
        Response<List<T>> getUserList<T>();
    }
}
