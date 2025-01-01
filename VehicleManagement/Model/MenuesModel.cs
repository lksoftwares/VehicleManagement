using System.Text.Json.Serialization;

namespace VehicleManagement.Model
{
    public class MenuesModel
    {
        [JsonPropertyName("menuId")]

        public int? Menu_Id { get; set; }
        [JsonPropertyName("menuName")]

        public string? Menu_Name { get; set; }
        [JsonPropertyName("menuURL")]

        public string? Menu_URL { get; set; }
        [JsonPropertyName("roleId")]

        public int? Role_Id { get; set; }
    }
}
