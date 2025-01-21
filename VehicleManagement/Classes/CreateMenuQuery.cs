using Azure.Core;
using System.Data;

namespace VehicleManagement.Classes
{
    public class CreateMenuQuery
    {


        public string CreateMenusQuery(CreateMenuQueryFeilds createMenuQueryFeilds)
        {
            var baseQuery = @"
        SELECT     
            t1.IconPath AS icon1,
            t1.Order_No AS levOrd1,
            t1.MenuName AS Level1,
            mrp.Permission_Id,
            p.Permission_Type,
            mrp.Role_Id,
            r.Role_Name ";

            string joinQuery = "";


            for (int i = 2; i <= createMenuQueryFeilds.Levels; i++)
            {
                baseQuery += $", t{i}.IconPath AS icon{i}, t{i}.Order_No AS levOrd{i}, t{i}.MenuName AS Level{i} ";
                joinQuery += $" LEFT JOIN Menus AS t{i} ON t{i}.ParentId = t{i - 1}.MenuID ";
            }

            var query = @$"
        {baseQuery}
        FROM Menus AS t1
        {joinQuery}
        JOIN Menu_Role_Permission_Mst1 AS mrp 
            ON ";

            query += string.Join(" OR ", Enumerable.Range(1, createMenuQueryFeilds.Levels).Select(i => $"t{i}.MenuID = mrp.MenuID"));

            query += @"
        JOIN Permission_Mst AS p ON mrp.Permission_Id = p.Permission_Id
        JOIN Role_Mst AS r ON mrp.Role_Id = r.Role_Id ";

            if (createMenuQueryFeilds.RoleId != null && createMenuQueryFeilds.RoleId != 0)
            {
                query += $" WHERE r.Role_Id = {createMenuQueryFeilds.RoleId}   AND t1.ParentID IS NULL";
            }

            query += " ORDER BY " + string.Join(", ", Enumerable.Range(1, createMenuQueryFeilds.Levels).Select(i => $"t{i}.Order_No"));

            return query;
        }

        public List<object> BuildSubMenu(CreateMenuQueryFeilds createMenuQueryFeilds)
        {
            if (createMenuQueryFeilds.startLevel > createMenuQueryFeilds.Levels) return null; 

            return createMenuQueryFeilds.group.Where(row => !string.IsNullOrEmpty(row[$"Level{createMenuQueryFeilds.startLevel}"]?.ToString()) && row["Permission_Id"] != DBNull.Value)
                .GroupBy(row => row[$"Level{createMenuQueryFeilds.startLevel}"]?.ToString())
                .Select(subGroup => new
                {
                    MenuName = subGroup.Key,
                    Icon = string.IsNullOrEmpty(subGroup.First()[$"Icon{createMenuQueryFeilds.startLevel}"]?.ToString())
                        ? null
                        : createMenuQueryFeilds.ImagePath + subGroup.First()[$"Icon{createMenuQueryFeilds.startLevel}"]?.ToString(),
                    Roles = subGroup.Select(row => new
                    {
                        RoleId = row["Role_Id"],
                        RoleName = row["Role_Name"]?.ToString(),
                        PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
                        PermissionType = row["Permission_Type"]?.ToString()
                    }).Distinct().ToList(),
                    //  SubMenus = BuildSubMenu(subGroup, createMenuQueryFeilds.startLevel + 1, createMenuQueryFeilds.ImagePath)
                    

                    SubMenus = BuildSubMenu(new CreateMenuQueryFeilds
                    {
                        Levels = createMenuQueryFeilds.Levels,
                        RoleId = createMenuQueryFeilds.RoleId,
                        group = subGroup,
                        startLevel = createMenuQueryFeilds.startLevel + 1,
                        ImagePath = createMenuQueryFeilds.ImagePath
                    })
                }).ToList<object>();
           
        }






    }
    public class CreateMenuQueryFeilds()
    {
        public int Levels { get; set; }
        public int RoleId { get; set; }
        public  IGrouping<string, DataRow> group { get; set; }
        public int startLevel { get; set; }
        public string ImagePath { get; set; }

    }
}
