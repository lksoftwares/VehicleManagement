//using System.Data;
//using System.Linq;

//namespace VehicleManagement.Classes
//{
//    public class RolePermissionMenusStructure
//    {
//        public static object GenerateMenuJson(RolePermissionMenusPerameter rolePermissionMenusPerameter)
//        {
//            Func<IEnumerable<DataRow>, int, object> processLevel = null;
//            processLevel = (rows, levelIndex) =>
//            {
//                if (levelIndex >= rolePermissionMenusPerameter.Levels.Count) return null;

//                var levelName = rolePermissionMenusPerameter.Levels[levelIndex];
//                return rows.GroupBy(row => row[levelName]?.ToString())
//                           .Where(group => !string.IsNullOrEmpty(group.Key))
//                           .Select(group => new
//                           {
//                               MenuName = group.Key,
//                               Roles = group
//                                   .Select(row => new
//                                   {
//                                       //RoleId = row[rolePermissionMenusPerameter.Role_Id],
//                                       //RoleName = row[rolePermissionMenusPerameter.Role_Name]?.ToString(),
//                                       //PermissionId = row[rolePermissionMenusPerameter.Permission_Id],
//                                       //PermissionType = row[rolePermissionMenusPerameter.Permission_Type]?.ToString()
//                                       rolePermissionMenusPerameter.FeildsKeys= rolePermissionMenusPerameter.FeildsValues


//                                   })
//                                   .Distinct()
//                                   .ToList(),
//                               SubMenus = processLevel(
//                                   group.Where(row => levelIndex + 1 < rolePermissionMenusPerameter.Levels.Count && !string.IsNullOrEmpty(row[rolePermissionMenusPerameter.Levels[levelIndex + 1]]?.ToString())),
//                                   levelIndex + 1
//                               )
//                           })
//                           .ToList();
//            };

//            return processLevel(rolePermissionMenusPerameter.dataTable.AsEnumerable(), 0);
//        }



//    }
//    public class RolePermissionMenusPerameter
//    {
//       // public string Role_Id { get; set; }
//        //public string Permission_Id { get; set; }
//        //public string Permission_Type { get; set; }
//        //public string Role_Name { get; set; }
//        public List<string> FeildsKeys { get; set; }
//        public List<string> FeildsValues { get; set; }

//        public DataTable dataTable { get; set; }
//        public List<string> Levels { get; set; }

//    }
//}
using System.Data;
using System.Linq;

namespace VehicleManagement.Classes
{
    public class RolePermissionMenusStructure
    {
        public  object GenerateMenuJson(RolePermissionMenusPerameter rolePermissionMenusPerameter)
        {
            Func<IEnumerable<DataRow>, int, object> processLevel = null;

            processLevel = (rows, levelIndex) =>
            {
                if (levelIndex >= rolePermissionMenusPerameter.Levels.Count) return null;

                var levelName = rolePermissionMenusPerameter.Levels[levelIndex];

                return rows.GroupBy(row => row[levelName]?.ToString())
                           .Where(group => !string.IsNullOrEmpty(group.Key))
                           .Select(group => new
                           {
                               MenuName = group.Key,
                               Roles = group
                                   .Select(row =>
                                   {
                                       var roleData = rolePermissionMenusPerameter.FieldKeys
                                           .Zip(rolePermissionMenusPerameter.FieldValues, (key, value) => new
                                           {
                                               Key = key,
                                               Value = row[value]?.ToString()
                                           })
                                           .ToDictionary(k => k.Key, v => v.Value);

                                       return roleData;
                                   })
                                   .Distinct()
                                   .ToList(),
                               SubMenus = processLevel(
                                   group.Where(row => levelIndex + 1 < rolePermissionMenusPerameter.Levels.Count &&
                                                      !string.IsNullOrEmpty(row[rolePermissionMenusPerameter.Levels[levelIndex + 1]]?.ToString())),
                                   levelIndex + 1
                               )
                           })
                           .ToList();
            };

            return processLevel(rolePermissionMenusPerameter.DataTable.AsEnumerable(), 0);
        }
    }

    public class RolePermissionMenusPerameter
    {
        public List<string> FieldKeys { get; set; } 
        public List<string> FieldValues { get; set; } 
        public DataTable DataTable { get; set; }
        public List<string> Levels { get; set; }
    }
}
