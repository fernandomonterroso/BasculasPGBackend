namespace BasculasPG.Models
{
    public class RespuestaHttp
    {
        public Boolean success { get; set; }
        public string message { get; set; }
        public dynamic data { get; set; }

        public RespuestaHttp(Boolean success, string message, dynamic data = null)
        {
            this.success = success;
            this.message = message;
            this.data = data;
        }

        public static RespuestaHttp BuildResponse(bool success, string message, dynamic data = null)
        {
            return new RespuestaHttp(success, message, data);
        }
    }
}
