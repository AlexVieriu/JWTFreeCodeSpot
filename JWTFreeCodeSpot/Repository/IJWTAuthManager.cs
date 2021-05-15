using Dapper;
using JWTFreeCodeSpot.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JWTFreeCodeSpot.Repository
{
    public interface IJWTAuthManager
    {
        Response<string> GenerateJWT(User user);
        Task<Response<T>> Execute_Command<T>(string query, DynamicParameters parameters);
        Task<Response<List<T>>> getUserList<T>();
    }
}
