//using System.Data;

//namespace VehicleManagement.Classes
//{
//    public class CreateQueryWithPermissions
//    {
//        public string CreateMenus_Mst(PerameteFeilds createMenuQueryFeilds)
//        {
//            var baseQuery = @"
//        SELECT     
//            t1.IconPath AS icon1,
//            t1.Order_No AS levOrd1,
//            t1.Menu_Name AS Level1,
//		    t1.Menu_Id AS levMenuId1,
//            t1.Page_Name AS PageName1,

//            mrp.Permission_Id,
//            p.Permission_Type,
//            mrp.Role_Id,
//            r.Role_Name ";

//            string joinQuery = "";


//            for (int i = 2; i <= createMenuQueryFeilds.Levels; i++)
//            {
//                baseQuery += $", t{i}.IconPath AS icon{i}, t{i}.Order_No AS levOrd{i}, t{i}.Menu_Name AS Level{i}, t{i}.Menu_Id AS levMenuId{i} ,  t{i}.Page_Name AS PageName{i} ";
//                joinQuery += $" LEFT JOIN Menus_Mst AS t{i} ON t{i}.Parent_Id = t{i - 1}.Menu_Id ";
//            }

//            var query = @$"
//        {baseQuery}
//        FROM Menus_Mst AS t1
//        {joinQuery}
//        JOIN Menu_Role_Permission_Mst AS mrp 
//            ON ";

//            query += string.Join(" OR ", Enumerable.Range(1, createMenuQueryFeilds.Levels).Select(i => $"t{i}.Menu_Id = mrp.Menu_Id"));

//            query += @"
//        JOIN Permission_Mst AS p ON mrp.Permission_Id = p.Permission_Id
//        JOIN Role_Mst AS r ON mrp.Role_Id = r.Role_Id ";

//            if (createMenuQueryFeilds.RoleId != null && createMenuQueryFeilds.RoleId != 0)
//            {
//                query += $" WHERE r.Role_Id = {createMenuQueryFeilds.RoleId}   AND t1.Parent_Id = 0";
//            }

//            query += " ORDER BY " + string.Join(", ", Enumerable.Range(1, createMenuQueryFeilds.Levels).Select(i => $"t{i}.Order_No"));

//            return query;
//        }

//        public string CreateMenusQuery(PerameteFeilds createMenuQueryFeilds)
//        {


//            var baseQuery = @"
//        SELECT     
//            t1.IconPath AS icon1,
//            t1.Order_No AS levOrd1,
//            t1.MenuName AS Level1,
//		    t1.MenuID AS levMenuId1,


//            mrp.Permission_Id,
//            p.Permission_Type,
//            mrp.Role_Id,
//            r.Role_Name ";

//            string joinQuery = "";


//            for (int i = 2; i <= createMenuQueryFeilds.Levels; i++)
//            {
//                baseQuery += $", t{i}.IconPath AS icon{i}, t{i}.Order_No AS levOrd{i}, t{i}.MenuName AS Level{i}, t{i}.MenuID AS levMenuId{i} ";
//                joinQuery += $" LEFT JOIN Menus AS t{i} ON t{i}.ParentId = t{i - 1}.MenuID ";
//            }

//            var query = @$"
//        {baseQuery}
//        FROM Menus AS t1
//        {joinQuery}
//        JOIN Menu_Role_Permission_Mst1 AS mrp 
//            ON ";

//            query += string.Join(" OR ", Enumerable.Range(1, createMenuQueryFeilds.Levels).Select(i => $"t{i}.MenuID = mrp.MenuID"));

//            query += @"
//        JOIN Permission_Mst AS p ON mrp.Permission_Id = p.Permission_Id
//        JOIN Role_Mst AS r ON mrp.Role_Id = r.Role_Id ";

//            if (createMenuQueryFeilds.RoleId != null && createMenuQueryFeilds.RoleId != 0)
//            {
//                query += $" WHERE r.Role_Id = {createMenuQueryFeilds.RoleId}   AND t1.ParentID IS NULL";
//            }

//            query += " ORDER BY " + string.Join(", ", Enumerable.Range(1, createMenuQueryFeilds.Levels).Select(i => $"t{i}.Order_No"));

//            return query;
//        }

//        public string CreateQuery(PerameteFeilds perameteFeilds)
//        {
//            var baseQuery = @"
//        SELECT
//            t1.IconPath AS icon1,

//            r.Role_Id,
//            r.Role_Name,
//            mrp.Permission_Id,
//            p.Permission_Type";

//            string joinQuery = "";

//            for (int i = 1; i <= perameteFeilds.Levels; i++)
//            {
//                baseQuery += $@",
//t{i}.IconPath AS icon{i},
//            t{i}.MenuID AS levMenuId{i},
//            t{i}.Order_No AS levOrd{i},
//            t{i}.MenuName AS Level{i}  ";

//                if (i > 1)
//                {
//                    joinQuery += $@"
//            LEFT JOIN Menus AS t{i} ON t{i}.ParentID = t{i - 1}.MenuID";
//                }
//            }

//            var query = @$"
//        {baseQuery}
//        FROM Menus AS t1
//        {joinQuery}
//        LEFT JOIN Menu_Role_Permission_Mst1 AS mrp
//            ON ({string.Join(" OR ", Enumerable.Range(1, perameteFeilds.Levels).Select(i => $"t{i}.MenuID = mrp.MenuID"))})
//            AND (mrp.Role_Id = {perameteFeilds.RoleId} OR mrp.Role_Id IS NULL)
//        LEFT JOIN Permission_Mst AS p ON mrp.Permission_Id = p.Permission_Id
//        LEFT JOIN Role_Mst AS r ON mrp.Role_Id = r.Role_Id
//        WHERE t1.ParentID IS NULL
//        ORDER BY {string.Join(", ", Enumerable.Range(1, perameteFeilds.Levels).Select(i => $"t{i}.Order_No"))};";

//            return query;

//        }


//        public string LatestBlankCreateQuery(PerameteFeilds perameteFeilds)
//        {
//            var baseQuery = @"
//        SELECT
//            t1.IconPath AS icon1,
//            t1.Page_Name AS PageName1,

//            r.Role_Id,
//            r.Role_Name,
//            mrp.Permission_Id,
//            p.Permission_Type";

//            string joinQuery = "";

//            for (int i = 1; i <= perameteFeilds.Levels; i++)
//            {
//                baseQuery += $@",
//t{i}.IconPath AS icon{i},
//            t{i}.Menu_Id AS levMenuId{i},
//            t{i}.Order_No AS levOrd{i},
//            t{i}.Menu_Name AS Level{i},
//  t{i}.Page_Name AS PageName{i}";

//                if (i > 1)
//                {
//                    joinQuery += $@"
//            LEFT JOIN Menus_Mst AS t{i} ON t{i}.Parent_id = t{i - 1}.Menu_Id";
//                }
//            }

//            var query = @$"
//        {baseQuery}
//        FROM Menus_Mst AS t1
//        {joinQuery}
//        LEFT JOIN Menu_Role_Permission_Mst AS mrp
//            ON ({string.Join(" OR ", Enumerable.Range(1, perameteFeilds.Levels).Select(i => $"t{i}.Menu_Id = mrp.Menu_Id"))})
//            AND (mrp.Role_Id = {perameteFeilds.RoleId} OR mrp.Role_Id IS NULL)
//        LEFT JOIN Permission_Mst AS p ON mrp.Permission_Id = p.Permission_Id
//        LEFT JOIN Role_Mst AS r ON mrp.Role_Id = r.Role_Id
//        WHERE t1.Parent_Id = 0
//        ORDER BY {string.Join(", ", Enumerable.Range(1, perameteFeilds.Levels).Select(i => $"t{i}.Order_No"))};";

//            return query;

//        }

//        public List<object> BuildSubMenu(PerameteFeilds createMenuQueryFeilds)
//        {
//            if (createMenuQueryFeilds.startLevel > createMenuQueryFeilds.Levels) return null;

//            return createMenuQueryFeilds.group
//                .GroupBy(row => row[$"Level{createMenuQueryFeilds.startLevel}"]?.ToString())
//                .Where(lev => !string.IsNullOrEmpty(lev.Key)) 
//                .Select(levGroup => new
//                {
//                    MenuID = levGroup.FirstOrDefault()?[$"levMenuId{createMenuQueryFeilds.startLevel}"],
//                    Icon = string.IsNullOrEmpty(levGroup.First()[$"icon{createMenuQueryFeilds.startLevel}"]?.ToString())
//                        ? null
//                        : createMenuQueryFeilds.ImagePath + levGroup.First()[$"icon{createMenuQueryFeilds.startLevel}"]?.ToString(),
//                   PageName = levGroup.FirstOrDefault()?[$"PageName{createMenuQueryFeilds.startLevel}"],

//                    MenuName = levGroup.Key,
//                    Roles = levGroup
//                        .Where(row => row["Role_Id"] != DBNull.Value || row["Permission_Id"] != DBNull.Value)
//                        .Select(row => new
//                        {
//                            OrderNo = row[$"levOrd{createMenuQueryFeilds.startLevel}"],
//                            RoleId = row["Role_Id"],
//                            RoleName = row["Role_Name"]?.ToString(),
//                            PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
//                            PermissionType = row["Permission_Type"]?.ToString()
//                        })
//                        .Distinct()
//                        .ToList(),
//                    SubMenus = BuildSubMenu(new PerameteFeilds
//                    {
//                        Levels = createMenuQueryFeilds.Levels,
//                        RoleId = createMenuQueryFeilds.RoleId,
//                        group = levGroup,
//                        startLevel = createMenuQueryFeilds.startLevel + 1,
//                        ImagePath = createMenuQueryFeilds.ImagePath
//                    })
//                })
//                .ToList<object>();
//        }

//        public class PerameteFeilds()
//        {
//            public int Levels { get; set; }
//            public int RoleId { get; set; }
//            public IGrouping<string, DataRow> group { get; set; }
//            public int startLevel { get; set; }
//            public string ImagePath { get; set; }
//            //public IGrouping<string, Dictionary<string, object>> group { get; set; } 

//        }





//    }
//}
using System.Data;

namespace VehicleManagement.Classes
{
    public class CreateQueryWithPermissions
    {
        public string CreateMenus_Mst(PerameteFeilds createMenuQueryFeilds)
        {
          
            var tableNames = new Dictionary<string, string>
    {
        { "Menus", "Menus_Mst" },
        { "Roles", "Role_Mst" },
        { "Permissions", "Permission_Mst" },
        { "RolePermissions", "Menu_Role_Permission_Mst" }
    };


        
            if (createMenuQueryFeilds.TableNames != null)
            {
                foreach (var key in createMenuQueryFeilds.TableNames.Keys)
                {
                    if (tableNames.ContainsKey(key))
                    {
                        tableNames[key] = createMenuQueryFeilds.TableNames[key];
                    }
                }
            }

          
            string menusTable = tableNames["Menus"];
            string rolesTable = tableNames["Roles"];
            string permissionsTable = tableNames["Permissions"];
            string rolePermissionTable = tableNames["RolePermissions"];

            var baseQuery = $@"
    SELECT
        t1.IconPath AS icon1,
        t1.Order_No AS levOrd1,
        t1.Menu_Name AS Level1,
        t1.Menu_Id AS levMenuId1,
        t1.Page_Name AS PageName1,

        mrp.Permission_Id,
        p.Permission_Type,
        mrp.Role_Id,
        r.Role_Name ";

            string joinQuery = "";

            for (int i = 2; i <= createMenuQueryFeilds.Levels; i++)
            {
                baseQuery += $", t{i}.IconPath AS icon{i}, t{i}.Order_No AS levOrd{i}, t{i}.Menu_Name AS Level{i}, t{i}.Menu_Id AS levMenuId{i}, t{i}.Page_Name AS PageName{i} ";
                joinQuery += $" LEFT JOIN {menusTable} AS t{i} ON t{i}.Parent_Id = t{i - 1}.Menu_Id ";
            }

            var query = $@"
    {baseQuery}
    FROM {menusTable} AS t1
    {joinQuery}
    JOIN {rolePermissionTable} AS mrp 
        ON {string.Join(" OR ", Enumerable.Range(1, createMenuQueryFeilds.Levels).Select(i => $"t{i}.Menu_Id = mrp.Menu_Id"))}
    JOIN {permissionsTable} AS p ON mrp.Permission_Id = p.Permission_Id
    JOIN {rolesTable} AS r ON mrp.Role_Id = r.Role_Id ";

            if (createMenuQueryFeilds.RoleId != 0)
            {
                query += $" WHERE r.Role_Id = {createMenuQueryFeilds.RoleId} AND t1.Parent_Id = 0";
            }

            query += " ORDER BY " + string.Join(", ", Enumerable.Range(1, createMenuQueryFeilds.Levels).Select(i => $"t{i}.Order_No"));

            return query;
        }


        //  public string CreateMenus_Mst(PerameteFeilds createMenuQueryFeilds)
        //  {
        //      var baseQuery = @"
        //  SELECT     
        //      t1.IconPath AS icon1,
        //      t1.Order_No AS levOrd1,
        //      t1.Menu_Name AS Level1,
        //t1.Menu_Id AS levMenuId1,
        //      t1.Page_Name AS PageName1,

        //      mrp.Permission_Id,
        //      p.Permission_Type,
        //      mrp.Role_Id,
        //      r.Role_Name ";

        //      string joinQuery = "";


        //      for (int i = 2; i <= createMenuQueryFeilds.Levels; i++)
        //      {
        //          baseQuery += $", t{i}.IconPath AS icon{i}, t{i}.Order_No AS levOrd{i}, t{i}.Menu_Name AS Level{i}, t{i}.Menu_Id AS levMenuId{i} ,  t{i}.Page_Name AS PageName{i} ";
        //          joinQuery += $" LEFT JOIN Menus_Mst AS t{i} ON t{i}.Parent_Id = t{i - 1}.Menu_Id ";
        //      }

        //      var query = @$"
        //  {baseQuery}
        //  FROM Menus_Mst AS t1
        //  {joinQuery}
        //  JOIN Menu_Role_Permission_Mst AS mrp 
        //      ON ";

        //      query += string.Join(" OR ", Enumerable.Range(1, createMenuQueryFeilds.Levels).Select(i => $"t{i}.Menu_Id = mrp.Menu_Id"));

        //      query += @"
        //  JOIN Permission_Mst AS p ON mrp.Permission_Id = p.Permission_Id
        //  JOIN Role_Mst AS r ON mrp.Role_Id = r.Role_Id ";

        //      if (createMenuQueryFeilds.RoleId != null && createMenuQueryFeilds.RoleId != 0)
        //      {
        //          query += $" WHERE r.Role_Id = {createMenuQueryFeilds.RoleId}   AND t1.Parent_Id = 0";
        //      }

        //      query += " ORDER BY " + string.Join(", ", Enumerable.Range(1, createMenuQueryFeilds.Levels).Select(i => $"t{i}.Order_No"));

        //      return query;
        //  }

        public string CreateMenusQuery(PerameteFeilds createMenuQueryFeilds)
        {
         

            var baseQuery = @"
        SELECT     
            t1.IconPath AS icon1,
            t1.Order_No AS levOrd1,
            t1.MenuName AS Level1,
		    t1.MenuID AS levMenuId1,
       

            mrp.Permission_Id,
            p.Permission_Type,
            mrp.Role_Id,
            r.Role_Name ";

            string joinQuery = "";


            for (int i = 2; i <= createMenuQueryFeilds.Levels; i++)
            {
                baseQuery += $", t{i}.IconPath AS icon{i}, t{i}.Order_No AS levOrd{i}, t{i}.MenuName AS Level{i}, t{i}.MenuID AS levMenuId{i} ";
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

        public string CreateQuery(PerameteFeilds perameteFeilds)
        {
            var baseQuery = @"
        SELECT
            t1.IconPath AS icon1,

            r.Role_Id,
            r.Role_Name,
            mrp.Permission_Id,
            p.Permission_Type";

            string joinQuery = "";

            for (int i = 1; i <= perameteFeilds.Levels; i++)
            {
                baseQuery += $@",
t{i}.IconPath AS icon{i},
            t{i}.MenuID AS levMenuId{i},
            t{i}.Order_No AS levOrd{i},
            t{i}.MenuName AS Level{i}  ";

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


        public string LatestBlankCreateQuery(PerameteFeilds perameteFeilds)
        {
               var tableNames = new Dictionary<string, string>
    {
        { "Menus", "Menus_Mst" },
        { "Roles", "Role_Mst" },
        { "Permissions", "Permission_Mst" },
        { "RolePermissions", "Menu_Role_Permission_Mst" }
    };


            if (perameteFeilds.TableNames != null)
            {
                foreach (var key in perameteFeilds.TableNames.Keys)
                {
                    if (tableNames.ContainsKey(key))
                    {
                        tableNames[key] = perameteFeilds.TableNames[key];
                    }
                }
            }


            string menusTable = tableNames["Menus"];
            string rolesTable = tableNames["Roles"];
            string permissionsTable = tableNames["Permissions"];
            string rolePermissionTable = tableNames["RolePermissions"];
            var baseQuery = @"
        SELECT
            t1.IconPath AS icon1,
            t1.Page_Name AS PageName1,

            r.Role_Id,
            r.Role_Name,
            mrp.Permission_Id,
            p.Permission_Type";

            string joinQuery = "";

            for (int i = 1; i <= perameteFeilds.Levels; i++)
            {
                baseQuery += $@",
t{i}.IconPath AS icon{i},
            t{i}.Menu_Id AS levMenuId{i},
            t{i}.Order_No AS levOrd{i},
            t{i}.Menu_Name AS Level{i},
  t{i}.Page_Name AS PageName{i}";

                if (i > 1)
                {
                    joinQuery += $@"
            LEFT JOIN {menusTable} AS t{i} ON t{i}.Parent_id = t{i - 1}.Menu_Id";
                }
            }

            var query = @$"
        {baseQuery}
        FROM {menusTable} AS t1
        {joinQuery}
        LEFT JOIN {rolePermissionTable} AS mrp
            ON ({string.Join(" OR ", Enumerable.Range(1, perameteFeilds.Levels).Select(i => $"t{i}.Menu_Id = mrp.Menu_Id"))})
            AND (mrp.Role_Id = {perameteFeilds.RoleId} OR mrp.Role_Id IS NULL)
        LEFT JOIN {permissionsTable} AS p ON mrp.Permission_Id = p.Permission_Id
        LEFT JOIN {rolesTable} AS r ON mrp.Role_Id = r.Role_Id
        WHERE t1.Parent_Id = 0
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
                    Icon = string.IsNullOrEmpty(levGroup.First()[$"icon{createMenuQueryFeilds.startLevel}"]?.ToString())
                        ? null
                        : createMenuQueryFeilds.ImagePath + levGroup.First()[$"icon{createMenuQueryFeilds.startLevel}"]?.ToString(),
                    PageName = levGroup.FirstOrDefault()?[$"PageName{createMenuQueryFeilds.startLevel}"],

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
            public Dictionary<string, string> TableNames { get; set; }
            //public IGrouping<string, Dictionary<string, object>> group { get; set; } 

        }





    }
}
