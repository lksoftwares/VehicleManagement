using System.Text.Json.Serialization;

namespace VehicleManagement.Classes
{
    public class apiResponse
    {

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }
        [JsonPropertyName("message")]

        public string Message { get; set; }
        [JsonPropertyName("apiResponse")]

        public object ApiResponse { get; set; }
        [JsonPropertyName("isSuccess")]

        public bool IsSuccess { get; set; } = false;
        public bool Dup { get; set; } = false;

    }
}
