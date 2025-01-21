using System.Text.Json.Serialization;

namespace VehicleManagement.Model
{
    public class MenusModel
    {
        [JsonPropertyName("menuId")]

        public int? Menu_Id { get; set; }
        [JsonPropertyName("menuName")]

        public string? Menu_Name { get; set; }
        [JsonPropertyName("parentId")]

        public int? Parent_Id { get; set; }
        [JsonPropertyName("IconPath")]

        public IFormFile? IconPath { get; set; }
        public string? IconUrl { get; set; }




    }
}
