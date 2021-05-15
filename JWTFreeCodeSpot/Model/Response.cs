namespace JWTFreeCodeSpot.Model
{
    public class Response<T> 
    {
        // T = User
        public T Data { get; set; }
        public string Message { get; set; }
        public int Code { get; set; }
    }
}
