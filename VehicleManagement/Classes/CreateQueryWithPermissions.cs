using System.Data;

namespace VehicleManagement.Classes
{
    public class CreateQueryWithPermissions
    {

        public string CreateQuery(PerameteFeilds perameteFeilds)
        {
            var baseQuery = @"
        SELECT
            r.Role_Id,
            r.Role_Name,
            mrp.Permission_Id,
            p.Permission_Type";

            string joinQuery = "";

            for (int i = 1; i <= perameteFeilds.Levels; i++)
            {
                baseQuery += $@",
            t{i}.MenuID AS levMenuId{i},
            t{i}.Order_No AS levOrd{i},
            t{i}.MenuName AS Level{i}";

                if (i > 1)
                {
                    joinQuery += $@"
            LEFT JOIN Menus AS t{i} ON t{i}.ParentID = t{i - 1}.MenuID";
                }
            }

            var query = @$"
        {baseQuery}
        FROM Menus AS t1
        {joinQuery}
        LEFT JOIN Menu_Role_Permission_Mst1 AS mrp
            ON ({string.Join(" OR ", Enumerable.Range(1, perameteFeilds.Levels).Select(i => $"t{i}.MenuID = mrp.MenuID"))})
            AND (mrp.Role_Id = {perameteFeilds.RoleId} OR mrp.Role_Id IS NULL)
        LEFT JOIN Permission_Mst AS p ON mrp.Permission_Id = p.Permission_Id
        LEFT JOIN Role_Mst AS r ON mrp.Role_Id = r.Role_Id
        WHERE t1.ParentID IS NULL
        ORDER BY {string.Join(", ", Enumerable.Range(1, perameteFeilds.Levels).Select(i => $"t{i}.Order_No"))};";

            return query;

        }



        public List<object> BuildSubMenu(PerameteFeilds createMenuQueryFeilds)
        {
            if (createMenuQueryFeilds.startLevel > createMenuQueryFeilds.Levels) return null;

            return createMenuQueryFeilds.group
                .GroupBy(row => row[$"Level{createMenuQueryFeilds.startLevel}"]?.ToString())
                .Where(lev => !string.IsNullOrEmpty(lev.Key)) 
                .Select(levGroup => new
                {
                    MenuID = levGroup.FirstOrDefault()?[$"levMenuId{createMenuQueryFeilds.startLevel}"],
                    MenuName = levGroup.Key,
                    Roles = levGroup
                        .Where(row => row["Role_Id"] != DBNull.Value || row["Permission_Id"] != DBNull.Value)
                        .Select(row => new
                        {
                            OrderNo = row[$"levOrd{createMenuQueryFeilds.startLevel}"],
                            RoleId = row["Role_Id"],
                            RoleName = row["Role_Name"]?.ToString(),
                            PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
                            PermissionType = row["Permission_Type"]?.ToString()
                        })
                        .Distinct()
                        .ToList(),
                    SubMenus = BuildSubMenu(new PerameteFeilds
                    {
                        Levels = createMenuQueryFeilds.Levels,
                        RoleId = createMenuQueryFeilds.RoleId,
                        group = levGroup,
                        startLevel = createMenuQueryFeilds.startLevel + 1,
                        ImagePath = createMenuQueryFeilds.ImagePath
                    })
                })
                .ToList<object>();
        }

        public class PerameteFeilds()
        {
            public int Levels { get; set; }
            public int RoleId { get; set; }
            public IGrouping<string, DataRow> group { get; set; }
            public int startLevel { get; set; }
            public string ImagePath { get; set; }

        }





    }
}
