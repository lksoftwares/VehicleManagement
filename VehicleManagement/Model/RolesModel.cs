using System.Text.Json.Serialization;

namespace VehicleManagement.Model
{
    public class RolesModel
    {
        [JsonPropertyName("roleId")]

        public int? Role_Id { get; set; }

        [JsonPropertyName("roleName")]

        public string Role_Name { get; set; }

        [JsonPropertyName("permissionId")]

        public int? Permission_Id { get; set; }
    }
}
