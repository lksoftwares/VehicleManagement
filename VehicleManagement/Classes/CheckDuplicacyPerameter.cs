namespace VehicleManagement.Classes
{
    public class CheckDuplicacyPerameter
    {

        public string tableName { get; set; }
        public string[] fields { get; set; }
        public string[] values { get; set; }
        public string idField { get; set; } = null;
        public string idValue { get; set; } = null;
    }
}
