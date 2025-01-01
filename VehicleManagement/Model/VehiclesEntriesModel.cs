using System.Security.Cryptography.Xml;
using System.Text.Json.Serialization;

namespace VehicleManagement.Model
{
    public class VehiclesEntriesModel
    {
        [JsonPropertyName("entryId")]

        public int Entry_Id { get; set; }
        [JsonPropertyName("vehicleId")]

        public int Vehicle_Id { get; set; }
        [JsonPropertyName("entryTime")]

        public DateTime Entry_Time { get; set; }
        [JsonPropertyName("exitTime")]

        public DateTime Exit_Time { get; set; }
        [JsonPropertyName("entryStatus")]

        public string Entry_Status { get; set; }
        [JsonPropertyName("handleByUser")]

        public int HandleByUser { get; set; }


        
    }
}
