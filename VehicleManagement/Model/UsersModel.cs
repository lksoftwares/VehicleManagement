using System.Text.Json.Serialization;

namespace VehicleManagement.Model
{
    public class UsersModel
    {
        [JsonPropertyName("userId")]

        public int? User_Id { get; set; }
        [JsonPropertyName("userName")]


        public string? User_Name { get; set; }
        [JsonPropertyName("userEmail")]

        public string? User_Email { get; set; }
        [JsonPropertyName("userPassword")]

        public string? User_Password { get; set; }
        [JsonPropertyName("user_Status")]

        public int? User_Status { get; set; }
        [JsonPropertyName("contactNo")]

        public int? Contact_Number { get; set; }
        [JsonPropertyName("createdAt")]

        public DateTime? Created_At { get; set; }
        [JsonPropertyName("roleId")]

        public int? Role_Id { get; set; }
        [JsonPropertyName("userRole")]

        public string? userRole { get; set; }
        [JsonPropertyName("image")]

        public IFormFile? Image { get; set; }
      



    }
}
