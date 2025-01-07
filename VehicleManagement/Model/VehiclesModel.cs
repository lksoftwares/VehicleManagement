using System;
using System.Text.Json.Serialization;
using System.Xml;

namespace VehicleManagement.Model
{
    public class VehiclesModel
    {
        [JsonPropertyName("vehicleId")]

        public int? Vehicle_Id { get; set; }
        [JsonPropertyName("vehicleNo")]

        public string? Vehicle_No { get; set; }
       
        [JsonPropertyName("ownerName")]

        public string? Owner_Name { get; set; }
        [JsonPropertyName("contactNumber")]

        public string? Contact_Number { get; set; }
        [JsonPropertyName("vehicle_Status")]

        public bool? Vehicle_Status { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime? Created_At { get; set; }

       
    }
}
