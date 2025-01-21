namespace VehicleManagement.Model
{
    public class InsertUpdateEntityModel
    {
        public int  table { get; set; }
        public object entity { get; set; }
        public int id { get; set; }
        public string idPropertyName { get; set; } = null;
        public string imgFolderpath { get; set; } = null;



    }
}
