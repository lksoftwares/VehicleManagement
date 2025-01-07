using System.Text.Json.Serialization;

namespace VehicleManagement.Model
{
    public class UsershiftModel
    {
        [JsonPropertyName("shiftId")]

        public int? Shift_Id { get; set; }
        [JsonPropertyName("shiftName")]

        public string? Shift_Name { get; set; }
        [JsonPropertyName("startTime")]

        public TimeSpan? Start_Time { get; set; }
        [JsonPropertyName("endTime")]

        public TimeSpan? End_Time { get; set; }
        [JsonPropertyName("graceTime")]

        public float? Grace_Time { get; set; }
        [JsonPropertyName("createdAt")]

        public DateTime? Created_At { get; set; }
        [JsonPropertyName("shiftStatus")]

        public bool? Shift_Status { get; set; }


    }
}
