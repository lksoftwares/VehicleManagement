using System.Text.Json.Serialization;

namespace VehicleManagement.Model
{
    public class MenuRolePermissionModel
    {
        [JsonPropertyName("RMPId")]


        public int? RMP_Id { get; set; }
        [JsonPropertyName("RoleId")]
        public int? Role_Id { get; set; }
        [JsonPropertyName("MenuId")]

        public int? Menu_Id { get; set; }
        [JsonPropertyName("PermissionId")]

        public int? Permission_Id { get; set; }
        [JsonPropertyName("RoleName")]

        public string? Role_Name { get; set; }
        [JsonPropertyName("MenuName")]

        public string? Menu_Name { get; set; }
        [JsonPropertyName("PermissionType")]

        public string? Permission_Type { get; set; }



    }




}
