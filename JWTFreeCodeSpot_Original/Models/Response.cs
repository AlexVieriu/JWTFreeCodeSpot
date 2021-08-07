
namespace JWTFreeCodeSpot_Original.Models
{
    public class Response<T>
    {
        public T Data { get; set; }
        public string message { get; set; }
        public int code { get; set; }
    }
}
