namespace Femah.Core.Api
{
    public class ApiResponse
    {
        public string Body { get; set; }
        public int HttpStatusCode { get; set; }

        public ApiResponse()
        {
            Body = string.Empty;
        }

    }
}
