using System.Text.Json.Serialization;

namespace VehicleManagement.Model
{
    public class PermissionModel
    {
        [JsonPropertyName("PermissionId")]

        public int? Permission_Id { get; set; }
        [JsonPropertyName("PermissionType")]

        public string? Permission_Type { get; set; }


    }
}
