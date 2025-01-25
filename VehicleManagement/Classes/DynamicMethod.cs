using System.Data;

namespace VehicleManagement.Classes
{
    public class DynamicMethod
    {
    }
    public class DynamicMethodPerameter()
    {
        public int Permission_Id { get; set; }
        public int Role_Id { get; set; }
        public string RoleTableName { get; set; }
        public string PermissionTableName { get; set; }
        public string PermissionRoleMenusTableName { get; set; }
        public string RoleMenus { get; set; }
        public int Levels { get; set; }
        public int RoleId { get; set; }
        public IGrouping<string, DataRow> group { get; set; }
        public int startLevel { get; set; }
        public string ImagePath { get; set; }

    }
}
