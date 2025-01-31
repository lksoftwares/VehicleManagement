using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;
using VehicleManagement.Classes;
using VehicleManagement.Model;
using Newtonsoft.Json;

using static VehicleManagement.Classes.CreateQueryWithPermissions;

namespace VehicleManagement.Controllers
{
    [Authorize]
    [Route("/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private IConfiguration _config;
        private apiResponse Resp = new apiResponse();
        private readonly ConnectionClass _connection;
        LkDataConnection.DataAccess _dc = new LkDataConnection.DataAccess();
        LkDataConnection.SqlQueryResult _query = new LkDataConnection.SqlQueryResult();
        public PermissionController(ConnectionClass connection, IConfiguration configuration)
        {
            _connection = connection;
            LkDataConnection.Connection.Connect();
            LkDataConnection.Connection.ConnectionStr = _connection.GetSqlConnection().ConnectionString;
            _config = configuration;

        }


        [AllowAnonymous]
        [HttpGet]
        [Route("GetMenusLatest/{Role_Id?}")]
        public IActionResult GetMenusLatest(int? Role_Id)
        {
            try
            {
                //            var countQuery = @"WITH Menu AS
                //(
                //    SELECT 
                //        Menu_Id,
                //        Parent_Id,
                //        1 AS Level
                //    FROM 
                //        Menus_Mst
                //    WHERE 
                //        Parent_Id = 0

                //    UNION ALL

                //    SELECT 
                //        m.Menu_Id,
                //        m.Parent_Id,
                //        mh.Level + 1 AS Level
                //    FROM 
                //        Menus_Mst m
                //    INNER JOIN 
                //        Menu mh
                //    ON 
                //        m.Parent_Id = mh.Menu_Id
                //)
                //SELECT MAX(Level) AS Mx
                //FROM Menu;";
                       var connection = new LkDataConnection.Connection();

                //            var LevelCount = connection.bindmethod(countQuery);

                //            DataTable Leveltbl = LevelCount._DataTable;

                //            int maxLevel = Convert.ToInt32(Leveltbl.Rows[0]["Mx"]);


                DataTableToJson dataTableToJson = new DataTableToJson();
                var _maxLevelPerametes = new MaxLevelParameters
                {
                    TableName = "Menus_Mst",
                    MenuIdColumn = "Menu_Id",
                    ParentIdColumn = "Parent_Id"
                };


                int maxLevel = dataTableToJson.GetMaxLevel(_maxLevelPerametes);
                var ImgPath = _config["EnvVariable:ImgPathIcon"];

                PerameteFeilds perameteFeilds = new PerameteFeilds
                {
                    Levels = maxLevel,
                    RoleId = Role_Id ?? 0,
                    //  startLevel = 1,
                    ImagePath = ImgPath
                };
                CreateQueryWithPermissions createQueryWithPermissions = new CreateQueryWithPermissions();
                string query = createQueryWithPermissions.LatestBlankCreateQuery(perameteFeilds);
                // var connection = new LkDataConnection.Connection();
                var result = connection.bindmethod(query);
                if (result == null || result._DataTable == null || result._DataTable.Rows.Count == 0)
                {
                    Resp.StatusCode = StatusCodes.Status200OK;
                    Resp.Message = "No Menus Found";
                    return Ok(Resp);
                }
                DataTable dataTable = result._DataTable;
                var menus = dataTable.AsEnumerable()
   .GroupBy(row => row["Level1"]?.ToString())
   .Select(lev1 => new
   {
       MenuID = lev1.FirstOrDefault()?["levMenuId1"],
       Icon = string.IsNullOrEmpty(lev1.First()["Icon1"]?.ToString())
                                ? null
                                : perameteFeilds.ImagePath + lev1.First()["Icon1"]?.ToString(),
    //   PageName = lev1.First()["PageName1"]?.ToString(),

       MenuName = lev1.Key,
       Roles = lev1
           .Where(row => row["Role_Id"] != DBNull.Value || row["Permission_Id"] != DBNull.Value)
           .Select(row => new
           {
               levOrd1 = row["levOrd1"],
               RoleId = row["Role_Id"],
               RoleName = row["Role_Name"]?.ToString(),
               PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
               PermissionType = row["Permission_Type"]?.ToString()
           })
           .Distinct()
           .ToList(),
       submenu = createQueryWithPermissions.BuildSubMenu(
             new PerameteFeilds
             {
                 Levels = perameteFeilds.Levels,
                 RoleId = perameteFeilds.RoleId,
                 group = lev1,
                 startLevel = 2,
                 ImagePath = perameteFeilds.ImagePath
             })
   })
                    .ToList();
                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = $"Data fetched successfully ";
                Resp.ApiResponse = menus;
                Resp.IsSuccess = true;

                return Ok(Resp);
            }
            catch (Exception ex)
            {
                Resp.StatusCode = StatusCodes.Status500InternalServerError;
                Resp.Message = ex.Message;

                return StatusCode(StatusCodes.Status500InternalServerError, Resp);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetRoleBasedMenus/{id?}")]
        public IActionResult GetRoleBasedMenus(int? id)
        {
            try
            {
                DatatblePerameters datatblePerameters = new DatatblePerameters
                {
                    Role_Id = id ?? 0,
                };

                DataTableToJson dataTableToJson = new DataTableToJson();
                DataTable dataTable = (DataTable)dataTableToJson.QueryToDataTable(datatblePerameters);
                var _maxLevelPerametes = new MaxLevelParameters
                {
                    TableName = "Menus_Mst",
                    MenuIdColumn = "Menu_Id",
                    ParentIdColumn = "Parent_Id"
                };

                int maxlevels = dataTableToJson.GetMaxLevel(_maxLevelPerametes);

                if (dataTable == null || dataTable.Rows.Count == 0)
                {
                    return NotFound(new
                    {
                        StatusCode = StatusCodes.Status404NotFound,
                        Message = "No menus found for the given role."
                    });
                }
                var ImgPath = _config["EnvVariable:ImgPathIcon"];

                string json = dataTableToJson.DataTableToJsonMethod(dataTable);
                var menus = dataTable.AsEnumerable()
                    .Where(row => row["Permission_Id"] != DBNull.Value)
                    .GroupBy(row => row["Level1"]?.ToString())
                    .Select(lev1 => new
                    {
                        MenuName = lev1.Key,
                        Icon = string.IsNullOrEmpty(lev1.First()["Icon1"]?.ToString())
                            ? null
                            : ImgPath + lev1.First()["Icon1"]?.ToString(),
                        PageName = lev1.First()["PageName1"]?.ToString(),
                        Roles = lev1.Select(row => new
                        {
                            RoleId = row["Role_Id"],
                            RoleName = row["Role_Name"]?.ToString(),
                            PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
                            PermissionType = row["Permission_Type"]?.ToString()
                        }).Distinct().ToList(),
                        SubMenus = new CreateQueryWithPermissions().BuildSubMenu(new PerameteFeilds
                        {
                            Levels = maxlevels,
                            RoleId = datatblePerameters.Role_Id,
                            group = lev1,
                            startLevel = 2,
                            ImagePath = ImgPath
                        })
                    })
                    .ToList();


                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = "Fetched successfully";
                Resp.ApiResponse = menus;
                Resp.IsSuccess = true;
                return Ok(

                    Resp
            );
            }

            catch (Exception ex)
            {
                Resp.StatusCode = StatusCodes.Status500InternalServerError;
                Resp.Message = ex.Message;
                return StatusCode(StatusCodes.Status500InternalServerError, Resp);
            }
        }



        //[AllowAnonymous]
        //[HttpGet]
        //[Route("GetRoleBasedMenus/{Role_Id?}")]
        //public IActionResult GetRoleBasedMenus(int? Role_Id)
        //{
        //    try
        //    {
        //        var Count = @"WITH Menu AS
        //    (
        //        SELECT 
        //            Menu_Id,
        //            Parent_Id,
        //            1 AS Level
        //        FROM 
        //            Menus_Mst
        //        WHERE 
        //            Parent_Id IS NULL

        //        UNION ALL

        //        SELECT 
        //            m.Menu_Id,
        //            m.Parent_Id,
        //            mh.Level + 1 AS Level
        //        FROM 
        //            Menus_Mst m
        //        INNER JOIN 
        //            Menu mh
        //        ON 
        //            m.Parent_Id = mh.Menu_Id
        //    )
        //    SELECT MAX(Level) AS Mx
        //    FROM Menu;";
        //        var connection = new LkDataConnection.Connection();

        //        var LevelCount = connection.bindmethod(Count);

        //        DataTable Leveltbl = LevelCount._DataTable;

        //        int maxLevel = Convert.ToInt32(Leveltbl.Rows[0]["Mx"]);
        //        var queryFields = new PerameteFeilds
        //        {
        //            Levels = maxLevel,
        //            RoleId = Role_Id ?? 0,
        //            ImagePath = "http://192.168.1.64:7148/public/Icons/"
        //        };
        //        CreateQueryWithPermissions createMenuQuery = new CreateQueryWithPermissions();
        //        string query = createMenuQuery.CreateMenus_Mst(queryFields);

        //        //   var connection = new LkDataConnection.Connection();
        //        var result = connection.bindmethod(query);

        //        if (result == null || result._DataTable == null || result._DataTable.Rows.Count == 0)
        //        {
        //            Resp.StatusCode = StatusCodes.Status404NotFound;
        //            Resp.Message = "No Menus Found";
        //            return Ok(Resp);
        //        }

        //        DataTable dataTable = result._DataTable;
        //        // var jsonResult = JsonConvert.SerializeObject(dataTable);

        //        //string prettyJson = JsonConvert.SerializeObject(dataTable, Formatting.Indented);


        //        // var jsonData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonResult);


        //        // var menus = jsonData
        //        //     .Where(row => row["Permission_Id"] != DBNull.Value)
        //        //     .GroupBy(row => row["Level1"]?.ToString())
        //        //     .Select(lev1 => new
        //        //     {
        //        //         MenuName = lev1.Key,
        //        //         Icon = string.IsNullOrEmpty(lev1.First()["Icon1"]?.ToString())
        //        //             ? null
        //        //             : queryFields.ImagePath + lev1.First()["Icon1"]?.ToString(),
        //        //         Roles = lev1.Select(row => new
        //        //         {
        //        //             RoleId = row["Role_Id"],
        //        //             RoleName = row["Role_Name"]?.ToString(),
        //        //             PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
        //        //             PermissionType = row["Permission_Type"]?.ToString()
        //        //         }).Distinct().ToList(),
        //        //         SubMenus = createMenuQuery.BuildSubMenu(new PerameteFeilds
        //        //         {
        //        //             Levels = queryFields.Levels,
        //        //             RoleId = queryFields.RoleId,
        //        //            // group = lev1,
        //        //             startLevel = 2,
        //        //             ImagePath = queryFields.ImagePath
        //        //         })
        //        //     })
        //        //     .ToList();

        //        var menus = dataTable.AsEnumerable()
        //            .Where(row => row["Permission_Id"] != DBNull.Value)
        //            .GroupBy(row => row["Level1"]?.ToString())
        //            .Select(lev1 => new
        //            {
        //                MenuName = lev1.Key,
        //                Icon = string.IsNullOrEmpty(lev1.First()["Icon1"]?.ToString())
        //                    ? null
        //                    : queryFields.ImagePath + lev1.First()["Icon1"]?.ToString(),
        //                Roles = lev1.Select(row => new
        //                {
        //                    RoleId = row["Role_Id"],
        //                    RoleName = row["Role_Name"]?.ToString(),
        //                    PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
        //                    PermissionType = row["Permission_Type"]?.ToString()
        //                }).Distinct().ToList(),
        //                SubMenus = createMenuQuery.BuildSubMenu(new PerameteFeilds
        //                {
        //                    Levels = queryFields.Levels,
        //                    RoleId = queryFields.RoleId,
        //                    group = lev1,
        //                    startLevel = 2,
        //                    ImagePath = queryFields.ImagePath
        //                })
        //            })
        //            .ToList();

        //        Resp.StatusCode = StatusCodes.Status200OK;
        //        Resp.Message = "Fetched successfully";
        //        Resp.ApiResponse = menus;
        //        Resp.IsSuccess = true;
        //        return Ok(Resp);
        //    }
        //    catch (Exception ex)
        //    {
        //        Resp.StatusCode = StatusCodes.Status500InternalServerError;
        //        Resp.Message = ex.Message;
        //        return StatusCode(StatusCodes.Status500InternalServerError, Resp);
        //    }
        //}




        //[AllowAnonymous]
        //[HttpGet]
        //[Route("GetRoleBasedMenus/{Role_Id?}")]
        //public IActionResult GetRoleBasedMenuss(int? Role_Id)
        //{
        //    try
        //    {

        //        string query = @"	SELECT     
        //        t1.IconPath as icon1,
        //               t2.IconPath as icon2,
        //        	t3.IconPath as icon3,
        //        	t4.IconPath as icon4,
        //               t1.Order_No as levOrd1,
        //               t2.Order_No as levOrd2,
        //               t3.Order_No as levOrd3,
        //               t4.Order_No as levOrd4,

        //                   t1.Menu_Name AS Level1,     
        //                   t2.Menu_Name AS Level2,  
        //                   t3.Menu_Name AS Level3,       
        //                   t4.Menu_Name AS Level4,   
        //                   mrp.Permission_Id,    
        //                   p.Permission_Type, 
        //                   mrp.Role_Id, 
        //                   r.Role_Name
        //               FROM Menus_Mst AS t1
        //               LEFT JOIN Menus_Mst AS t2 ON t2.Parent_Id = t1.Menu_Id
        //               LEFT JOIN Menus_Mst AS t3 ON t3.Parent_Id = t2.Menu_Id  
        //               LEFT JOIN Menus_Mst AS t4 ON t4.Parent_Id = t3.Menu_Id  
        //               JOIN Menu_Role_Permission_Mst AS mrp 
        //                   ON t1.Menu_Id = mrp.Menu_Id 
        //                   OR t2.Menu_Id = mrp.Menu_Id 
        //                   OR t3.Menu_Id = mrp.Menu_Id 
        //                   OR t4.Menu_Id = mrp.Menu_Id  
        //               JOIN Permission_Mst AS p ON mrp.Permission_Id = p.Permission_Id  
        //               JOIN Role_Mst AS r ON mrp.Role_Id = r.Role_Id ";

        //        if (Role_Id == null || Role_Id != 0)
        //        {
        //            query += $" WHERE r.Role_Id = {Role_Id} AND t1.Parent_Id IS NULL ";
        //        }
        //        query += " ORDER BY   t1.Order_No,     t2.Order_No,  t3.Order_No,    t4.Order_No ";
        //        // var queryFields = new CreateMenuQueryFeilds
        //        // {
        //        //     Levels = 3,
        //        //     RoleId = 2,
        //        //     group = menus, 
        //        //     startLevel = 1,
        //        //     ImagePath = "http://example.com/images/"
        //        // };

        //        // CreateMenuQuery createMenuQuery = new CreateMenuQuery();
        //        //string query = createMenuQuery.CreateMenusQueryFeilds(queryFields);


        //        var connection = new LkDataConnection.Connection();
        //        var result = connection.bindmethod(query);

        //        if (result == null || result._DataTable == null || result._DataTable.Rows.Count == 0)
        //        {
        //            Resp.StatusCode = StatusCodes.Status404NotFound;
        //            Resp.Message = "No Menus Found";
        //            return Ok(Resp);
        //        }

        //        DataTable dataTable = result._DataTable;


        //        var ImagePath = "http://192.168.1.64:7148/public/Icons/";

        //        var menus = dataTable.AsEnumerable()
        //            .Where(row => row["Permission_Id"] != DBNull.Value)
        //            .GroupBy(row => row["Level1"]?.ToString())
        //            .Select(lev1 => new
        //            {
        //                MenuName = lev1.Key,
        //                Icon = string.IsNullOrEmpty(lev1.First()["Icon1"]?.ToString())
        //    ? null
        //    : ImagePath + lev1.First()["Icon1"]?.ToString(),
        //                Roles = lev1
        //                    .Where(row => row["Role_Id"] != DBNull.Value)
        //                    .Select(row => new
        //                    {
        //                        levOrd1 = row["levOrd1"],
        //                        RoleId = row["Role_Id"],
        //                        RoleName = row["Role_Name"]?.ToString(),
        //                        PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
        //                        PermissionType = row["Permission_Type"]?.ToString()
        //                    })
        //                    .Distinct()
        //                    .ToList(),
        //                SubMenus = lev1
        //                    .Where(row => !string.IsNullOrEmpty(row["Level2"]?.ToString()) && row["Permission_Id"] != DBNull.Value)
        //                    .GroupBy(row => row["Level2"]?.ToString())
        //                    .Select(lev2 => new
        //                    {
        //                        MenuName = lev2.Key,
        //                        Icon = string.IsNullOrEmpty(lev2.First()["Icon2"]?.ToString())
        //    ? null
        //    : ImagePath + lev2.First()["Icon2"]?.ToString(),
        //                        Roles = lev2
        //                            .Where(row => row["Role_Id"] != DBNull.Value)
        //                            .Select(row => new
        //                            {
        //                                levOrd2 = row["levOrd2"],
        //                                RoleId = row["Role_Id"],
        //                                RoleName = row["Role_Name"]?.ToString(),
        //                                PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
        //                                PermissionType = row["Permission_Type"]?.ToString()
        //                            })
        //                            .Distinct()
        //                            .ToList(),
        //                        SubMenus = lev2
        //                            .Where(row => !string.IsNullOrEmpty(row["Level3"]?.ToString()) && row["Permission_Id"] != DBNull.Value)
        //                            .GroupBy(row => row["Level3"]?.ToString())
        //                            .Select(lev3 => new
        //                            {
        //                                MenuName = lev3.Key,
        //                                Icon = string.IsNullOrEmpty(lev3.First()["Icon3"]?.ToString())
        //    ? null
        //    : ImagePath + lev3.First()["Icon3"]?.ToString(),
        //                                Roles = lev3
        //                                    .Where(row => row["Role_Id"] != DBNull.Value)
        //                                    .Select(row => new
        //                                    {
        //                                        levOrd3 = row["levOrd3"],
        //                                        RoleId = row["Role_Id"],
        //                                        RoleName = row["Role_Name"]?.ToString(),
        //                                        PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
        //                                        PermissionType = row["Permission_Type"]?.ToString()
        //                                    })
        //                                    .Distinct()
        //                                    .ToList(),
        //                                SubMenus = lev3
        //                                    .Where(row => !string.IsNullOrEmpty(row["Level4"]?.ToString()) && row["Permission_Id"] != DBNull.Value)
        //                                    .Select(row => new
        //                                    {
        //                                        MenuName = row["Level4"]?.ToString(),
        //                                        Icon = string.IsNullOrEmpty(row["Icon4"]?.ToString())
        //    ? null
        //    : ImagePath + row["Icon4"]?.ToString(),
        //                                        Roles = new List<object>
        //                                        {
        //                                                new
        //                                                {
        //                                                    levOrd4 = row["levOrd4"],
        //                                                    RoleId = row["Role_Id"],
        //                                                    RoleName = row["Role_Name"]?.ToString(),
        //                                                    PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
        //                                                    PermissionType = row["Permission_Type"]?.ToString()
        //                                                }
        //                                        }
        //                                    })
        //                                    .ToList()
        //                            })
        //                            .ToList()
        //                    })
        //                    .ToList()
        //            })
        //            .ToList();

        //        //var menus = dataTable.AsEnumerable()
        //        //    .Where(row => row["Permission_Id"] != DBNull.Value)
        //        //    .GroupBy(row => row["Level1"]?.ToString())
        //        //    .Select(lev1 => new
        //        //    {
        //        //        MenuName = lev1.Key,
        //        //        Icon = string.IsNullOrEmpty(lev1.First()["Icon1"]?.ToString())
        //        //            ? null
        //        //            : ImagePath + lev1.First()["Icon1"]?.ToString(),
        //        //        Roles = lev1.Select(row => new
        //        //        {
        //        //            RoleId = row["Role_Id"],
        //        //            RoleName = row["Role_Name"]?.ToString(),
        //        //            PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
        //        //            PermissionType = row["Permission_Type"]?.ToString()
        //        //        }).Distinct().ToList(),
        //        //        SubMenus = createMenuQuery.BuildSubMenu(lev1,2,ImagePath)
        //        //    })
        //        //    .ToList();



        //        Resp.StatusCode = StatusCodes.Status200OK;
        //        Resp.Message = "Fetched successfully";
        //        Resp.ApiResponse = menus;
        //        Resp.IsSuccess = true;
        //        return Ok(Resp);
        //    }
        //    catch (Exception ex)
        //    {
        //        Resp.StatusCode = StatusCodes.Status500InternalServerError;
        //        Resp.Message = ex.Message;
        //        return StatusCode(StatusCodes.Status500InternalServerError, Resp);
        //    }
        //}





        [AllowAnonymous]
        [HttpGet]
        [Route("GetMenusTest2/{Role_Id?}")]
        public IActionResult GetMenusTest(int? Role_Id)
        {
            try
            {
                //var Count = "WITH Menu AS\r\n(\r\n    SELECT \r\n        MenuID,\r\n        ParentID,\r\n        1 AS Level\r\n    FROM \r\n        Menus    WHERE         ParentID IS NULL\r\n\r\n    UNION ALL\r\n\r\n    SELECT \r\n        m.MenuID,\r\n        m.ParentID,\r\n        mh.Level + 1 AS Level   FROM \r\n        Menus m   INNER JOIN \r\n        Menu mh\r\n    ON \r\n        m.ParentID = mh.MenuID ) SELECT MAX(Level) AS Mx FROM Menu;";
                var connection = new LkDataConnection.Connection();

                // var LevelCount = connection.bindmethod(Count);

                //DataTable Leveltbl = LevelCount._DataTable
                DataTableToJson dataTableToJson = new DataTableToJson();
                  var _maxLevelPerametes = new MaxLevelParameters
                {
                    TableName = "Menus",
                    MenuIdColumn = "MenuID",
                    ParentIdColumn = "ParentID"
                  };

                int maxLevel = dataTableToJson.GetMaxLevel(_maxLevelPerametes);

                var ImgPath = _config["EnvVariable:ImgPathIcon"];

               // int maxLevel = Convert.ToInt32(Leveltbl.Rows[0]["Mx"]);
                PerameteFeilds perameteFeilds = new PerameteFeilds
                {
                    Levels = maxLevel,
                    RoleId = Role_Id ?? 0,

                  //  startLevel = 1,
                    ImagePath = ImgPath
                };
                CreateQueryWithPermissions createQueryWithPermissions = new CreateQueryWithPermissions();
                string query = createQueryWithPermissions.CreateQuery(perameteFeilds);
               // var connection = new LkDataConnection.Connection();
                var result = connection.bindmethod(query);
                if (result == null || result._DataTable == null || result._DataTable.Rows.Count == 0)
                {
                    Resp.StatusCode = StatusCodes.Status200OK;
                    Resp.Message = "No Menus Found";
                    return Ok(Resp);
                }
                DataTable dataTable = result._DataTable;
                var menus = dataTable.AsEnumerable()
   .GroupBy(row => row["Level1"]?.ToString())
   .Select(lev1 => new
   {
       MenuID = lev1.FirstOrDefault()?["levMenuId1"],
       Icon = string.IsNullOrEmpty(lev1.First()["Icon1"]?.ToString())
                                ? null
                                : perameteFeilds.ImagePath + lev1.First()["Icon1"]?.ToString(),
       MenuName = lev1.Key,
       Roles = lev1
           .Where(row => row["Role_Id"] != DBNull.Value || row["Permission_Id"] != DBNull.Value)
           .Select(row => new
           {
               levOrd1 = row["levOrd1"],
               RoleId = row["Role_Id"],
               RoleName = row["Role_Name"]?.ToString(),
               PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
               PermissionType = row["Permission_Type"]?.ToString()
           })
           .Distinct()
           .ToList(),
           submenu = createQueryWithPermissions.BuildSubMenu(
             new PerameteFeilds
               {
                   Levels = perameteFeilds.Levels,
                   RoleId = perameteFeilds.RoleId,
                   group = lev1,
                   startLevel = 2,
                   ImagePath = perameteFeilds.ImagePath
               })
                    })
                    .ToList();
                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = $"Data fetched successfully ";
                Resp.ApiResponse = menus;
                Resp.IsSuccess = true;

                return Ok(Resp);
            }
            catch (Exception ex)
            {
                Resp.StatusCode = StatusCodes.Status500InternalServerError;
                Resp.Message = ex.Message;

                return StatusCode(StatusCodes.Status500InternalServerError, Resp);
            }
        }







        [HttpGet]

        [Route("GetAllPermission")]
        public IActionResult GetAllPermission()
        {
            try
            {
                string query = $"select * from Permission_Mst ";
                Console.WriteLine(query);
                var connection = new LkDataConnection.Connection();

                //var result = connection.bindmethod(query);


                //DataTable Table = result._DataTable;
                DataTable Table = _connection.ExecuteQueryWithResult(query);


                var PermissionList = new List<PermissionModel>();

                foreach (DataRow row in Table.Rows)
                {
                    PermissionList.Add(new PermissionModel
                    {
                        Permission_Id = Convert.ToInt32(row["Permission_Id"]),

                        Permission_Type = row["Permission_Type"].ToString()


                    });
                }
                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = $"Data fetched successfully ";
                Resp.ApiResponse = PermissionList;
                Resp.IsSuccess = true;

                return Ok(Resp);



            }
            catch (Exception ex)
            {
                Resp.StatusCode = StatusCodes.Status500InternalServerError;
                Resp.Message = ex.Message;

                return StatusCode(StatusCodes.Status500InternalServerError, Resp);
            }
        }




        [HttpPost]

        [Route("AddPermission")]
        public IActionResult AddPermission([FromBody] PermissionModel permission)
        {
            try
            {
                var duplicacyChecker = new CheckDuplicacy(_connection);
                var duplicacyParameter = new CheckDuplicacyPerameter
                {
                    tableName = "Permission_Mst",
                    fields = new[] { "Permission_Type" },
                    values = new[] { permission.Permission_Type }
                };
                bool isDuplicate = duplicacyChecker.CheckDuplicate(duplicacyParameter);
                if (isDuplicate)
                {
                    Resp.StatusCode = StatusCodes.Status208AlreadyReported;
                    Resp.Message = $"Permission already exists.";
                    Resp.Dup = true;

                    return StatusCode(StatusCodes.Status208AlreadyReported, Resp);
                }
                if (String.IsNullOrEmpty(permission.Permission_Type))
                {
                    Resp.StatusCode = StatusCodes.Status404NotFound;
                    Resp.Message = $"Permission Type Can't be Blank Or Null";

                    return StatusCode(StatusCodes.Status404NotFound, Resp);
                }
                if (!string.IsNullOrEmpty(permission.Permission_Type))
                {
                    permission.Permission_Type = permission.Permission_Type.ToUpper();
                }
                _query = _dc.InsertOrUpdateEntity(permission, "Permission_Mst", -1);

                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = $"Permission Added Successfully";
                Resp.IsSuccess = true;

                return StatusCode(StatusCodes.Status200OK, Resp);

            }
            catch (Exception ex)
            {

                Resp.StatusCode = StatusCodes.Status500InternalServerError;
                Resp.Message = ex.Message;


                return StatusCode(StatusCodes.Status500InternalServerError, Resp);
            }
        }

        [HttpPut]
        [Route("updatePermission/{id}")]

        public IActionResult UpdateRole(int id, [FromBody] PermissionModel permission)
        {
            try
            {

                var roleExists = $"SELECT COUNT(*) FROM Permission_Mst WHERE Permission_Id = {id} ";
                int result = Convert.ToInt32(_connection.ExecuteScalar(roleExists));

                                var ImgPath = _config["EnvVariable:ImgPathIcon"];

                if (result == 0)
                {
                    Resp.StatusCode = StatusCodes.Status404NotFound;
                    Resp.Message = $"Permission ID does not exist.";

                    return StatusCode(StatusCodes.Status404NotFound, Resp);
                }

                var duplicacyChecker = new CheckDuplicacy(_connection);
                var duplicacyParameter = new CheckDuplicacyPerameter
                {
                    tableName = "Permission_Mst",
                    fields = new[] { "Permission_Type" },
                    values = new[] { permission.Permission_Type },
                    idField = "Permission_Id",
                    idValue = id.ToString()
                };

                bool isDuplicate = duplicacyChecker.CheckDuplicate(duplicacyParameter);


                if (isDuplicate)
                {
                    Resp.StatusCode = StatusCodes.Status208AlreadyReported;
                    Resp.Message = $"Permission already exists.";
                    Resp.Dup = true;

                    return StatusCode(StatusCodes.Status208AlreadyReported, Resp);

                }
                if (String.IsNullOrEmpty(permission.Permission_Type))
                {
                    Resp.StatusCode = StatusCodes.Status208AlreadyReported;
                    Resp.Message = $"Permission Can't be Blank Or Null";

                    return StatusCode(StatusCodes.Status208AlreadyReported, Resp);

                }
                if (!string.IsNullOrEmpty(permission.Permission_Type))
                {
                    permission.Permission_Type = permission.Permission_Type.ToUpper();
                }
                _query = _dc.InsertOrUpdateEntity(permission, "Permission_Mst", id, "Permission_Id");
                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = "Permission Updated Successfully";
                Resp.IsSuccess = true;

                return StatusCode(StatusCodes.Status200OK, Resp);

            }
            catch (Exception ex)
            {
                Resp.StatusCode = StatusCodes.Status500InternalServerError;
                Resp.Message = ex.Message;


                return StatusCode(StatusCodes.Status500InternalServerError, Resp);
            }
        }



        [AllowAnonymous]
        [HttpGet]
        [Route("GetAllMenuWithPermissionsTest/{Role_Id?}")]
        public IActionResult GetAllMenuWithPermissionsTest(int? Role_Id)
        {
            try
            {
                try
                {


                    var Count = "WITH Menu AS\r\n(\r\n    SELECT \r\n        MenuID,\r\n        ParentID,\r\n        1 AS Level\r\n    FROM \r\n        Menus    WHERE         ParentID IS NULL\r\n\r\n    UNION ALL\r\n\r\n    SELECT \r\n        m.MenuID,\r\n        m.ParentID,\r\n        mh.Level + 1 AS Level   FROM \r\n        Menus m   INNER JOIN \r\n        Menu mh\r\n    ON \r\n        m.ParentID = mh.MenuID ) SELECT MAX(Level) AS Mx FROM Menu;";
                    var connection = new LkDataConnection.Connection();

                    var LevelCount = connection.bindmethod(Count);

                    DataTable Leveltbl = LevelCount._DataTable;

                  int  maxLevel = Convert.ToInt32(Leveltbl.Rows[0]["Mx"]);
                    var ImgPath = _config["EnvVariable:ImgPathIcon"];

                    var queryFields = new PerameteFeilds
                    {
                        Levels = Convert.ToInt32(maxLevel),
                        RoleId = Role_Id ?? 0,
                       
                        ImagePath = ImgPath
                    };

                    CreateQueryWithPermissions createMenuQuery = new CreateQueryWithPermissions();
                    string query = createMenuQuery.CreateMenusQuery(queryFields);

                    var result = connection.bindmethod(query);

                    if (result == null || result._DataTable == null || result._DataTable.Rows.Count == 0)
                    {
                        Resp.StatusCode = StatusCodes.Status404NotFound;
                        Resp.Message = "No Menus Found";
                        return Ok(Resp);
                    }

                    DataTable dataTable = result._DataTable;

                    var menus = dataTable.AsEnumerable()
                        .Where(row => row["Permission_Id"] != DBNull.Value)
                        .GroupBy(row => row["Level1"]?.ToString())
                        .Select(lev1 => new
                        {
                            MenuID = lev1.FirstOrDefault()?["levMenuId1"],
                            MenuName = lev1.Key,
                            Icon = string.IsNullOrEmpty(lev1.First()["Icon1"]?.ToString())
                                ? null
                                : queryFields.ImagePath + lev1.First()["Icon1"]?.ToString(),
                            Roles = lev1.Select(row => new
                            {
                                RoleId = row["Role_Id"],
                                RoleName = row["Role_Name"]?.ToString(),
                                PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
                                PermissionType = row["Permission_Type"]?.ToString()
                            }).Distinct().ToList(),
                            SubMenus = createMenuQuery.BuildSubMenu(new PerameteFeilds
                            {
                                Levels = queryFields.Levels,
                                RoleId = queryFields.RoleId,
                                group = lev1,
                                startLevel=2,
                                ImagePath = queryFields.ImagePath
                            })
                        })
                        .ToList();

                    Resp.StatusCode = StatusCodes.Status200OK;
                    Resp.Message = "Fetched successfully";
                    Resp.ApiResponse = menus;
                    Resp.IsSuccess = true;
                    return Ok(Resp);
                }
                catch (Exception ex)
                {
                    Resp.StatusCode = StatusCodes.Status500InternalServerError;
                    Resp.Message = ex.Message;
                    return StatusCode(StatusCodes.Status500InternalServerError, Resp);
                }

            }
            catch(Exception ex)
            {
                Resp.StatusCode = StatusCodes.Status500InternalServerError;
                Resp.Message = ex.Message;


                return StatusCode(StatusCodes.Status500InternalServerError, Resp);
            }

        }


        [HttpDelete]
        [Route("deletePermission/{id}")]
        public IActionResult deletePermission(int id)
        {



            try
            {
                var roleExists = $"SELECT COUNT(*) FROM Permission_Mst WHERE Permission_Id = {id} ";
                int result = Convert.ToInt32(_connection.ExecuteScalar(roleExists));


                if (result == 0)
                {
                    Resp.StatusCode = StatusCodes.Status404NotFound;
                    Resp.Message = $"Permission ID does not exist.";

                    return StatusCode(StatusCodes.Status404NotFound, Resp);

                }

                string checkQuery = $"SELECT COUNT(*) AS recordCount FROM Menu_Role_Permission_Mst WHERE permission_Id = {id}";


                int roleIdInUser = Convert.ToInt32(_connection.ExecuteScalar(checkQuery));
                if (roleIdInUser > 0)
                {
                    Resp.StatusCode = StatusCodes.Status404NotFound;
                    Resp.Message = $"Can't delete Exists in another table";

                    return StatusCode(StatusCodes.Status404NotFound, Resp);


                }
                string deleteRoleQuery = $"Delete from Permission_Mst where Permission_Id='{id}'";

                LkDataConnection.Connection.ExecuteNonQuery(deleteRoleQuery);
                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = "Permission Deleted successfully";
                Resp.IsSuccess = true;


                return StatusCode(StatusCodes.Status200OK, Resp);


            }
            catch (Exception ex)
            {
                Resp.StatusCode = StatusCodes.Status500InternalServerError;
                Resp.Message = ex.Message;


                return StatusCode(StatusCodes.Status500InternalServerError, Resp);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetAllMenu")]
        public IActionResult GetAllMenu()
        {
            try
            {
                 string query = $"SELECT t1.MenuName AS lev1, t2.MenuName as lev2, t3.MenuName as lev3, t4.MenuName as lev4 FROM Menus AS t1 LEFT JOIN Menus AS t2 ON t2.ParentID = t1.MenuID LEFT JOIN Menus AS t3 ON t3.ParentID = t2.MenuID LEFT JOIN Menus AS t4 ON t4.ParentID = t3.MenuID WHERE      t1.ParentID IS NULL ";
                //string query = $"SELECT \r\n mrp.Permission_Id,p.Permission_Type,\r\n     t1.MenuID AS lev1_ID, t1.MenuName AS lev1, \r\n     t2.MenuID AS lev2_ID, t2.MenuName AS lev2, \r\n     t3.MenuID AS lev3_ID, t3.MenuName AS lev3, \r\n     t4.MenuID AS lev4_ID, t4.MenuName AS lev4\r\n FROM Menus AS t1\r\n LEFT JOIN Menus AS t2 ON t2.ParentID = t1.MenuID\r\n LEFT JOIN Menus AS t3 ON t3.ParentID = t2.MenuID\r\n LEFT JOIN Menus AS t4 ON t4.ParentID = t3.MenuID\r\n Join [Menu_Role_Permission_Mst] mrp ON t1.MenuID= mrp.Menu_Id\r\n join Permission_Mst p ON mrp.Permission_Id =p.Permission_Id\r\n WHERE t1.ParentID IS NULL\r\n";
                Console.WriteLine(query);

                var connection = new LkDataConnection.Connection();
                var result = connection.bindmethod(query);
                DataTable table = result._DataTable;



                var menuper = table.AsEnumerable()
                    .GroupBy(row => row["lev1"].ToString())
                    .Select(lg1 => new
                    {
                        MenuName = lg1.Key,
                        SubMenus = lg1
                            .GroupBy(row => row["lev2"].ToString()
                            )
                            .Where(lg2 => !string.IsNullOrEmpty(lg2.Key))
                            .Select(lg2 => new
                            {
                                MenuName = lg2.Key,
                                SubMenus = lg2
                                    .GroupBy(row => row["lev3"].ToString())
                                    .Where(lg3 => !string.IsNullOrEmpty(lg3.Key))
                                    .Select(lg3 => new
                                    {
                                        MenuName = lg3.Key,
                                        SubMenus = lg3
                                            .Where(row => !string.IsNullOrEmpty(row["lev4"].ToString()))
                                            .Select(row => new
                                            {
                                                MenuName = row["lev4"].ToString()
                                            })
                                            .ToList()
                                    })
                                    .ToList()
                            })
                            .ToList()
                    })
                    .ToList();






                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = "Data fetched successfully";
                Resp.ApiResponse = menuper;
                Resp.IsSuccess = true;

                return Ok(Resp);
            }
            catch (Exception ex)
            {
                Resp.StatusCode = StatusCodes.Status500InternalServerError;
                Resp.Message = ex.Message;

                return StatusCode(StatusCodes.Status500InternalServerError, Resp);
            }





        }


        [AllowAnonymous]
        [HttpGet]
        [Route("GetAllMenusWithRole/{Role_Id?}")]
        public IActionResult GetAllMenusWithRole(int? Role_Id)
        {
            try
            {


                //                string query = @"SELECT
                //    r.Role_Id,
                //    r.Role_Name,
                //    mrp.Permission_Id,
                //    p.Permission_Type,
                //    t1.MenuID AS levMenuId1,
                //    t1.Order_No AS levOrd1,
                //    t2.MenuID AS levMenuId2,
                //    t2.Order_No AS levOrd2,
                //    t3.MenuID AS levMenuId3,
                //    t3.Order_No AS levOrd3,
                //    t4.MenuID AS levMenuId4,
                //    t4.Order_No AS levOrd4,
                //    t1.MenuName AS Level1,
                //    t2.MenuName AS Level2,
                //    t3.MenuName AS Level3,
                //    t4.MenuName AS Level4
                //FROM Menus AS t1
                //LEFT JOIN Menus AS t2 ON t2.ParentID = t1.MenuID
                //LEFT JOIN Menus AS t3 ON t3.ParentID = t2.MenuID
                //LEFT JOIN Menus AS t4 ON t4.ParentID = t3.MenuID
                //LEFT JOIN Menu_Role_Permission_Mst1 AS mrp 
                //    ON (t1.MenuID = mrp.MenuID OR t2.MenuID = mrp.MenuID OR t3.MenuID = mrp.MenuID OR t4.MenuID = mrp.MenuID)
                //LEFT JOIN Permission_Mst AS p ON mrp.Permission_Id = p.Permission_Id
                //LEFT JOIN Role_Mst AS r ON mrp.Role_Id = r.Role_Id

                //";
                string query = $"\tSELECT\r\n    r.Role_Id,\r\n    r.Role_Name,\r\n    mrp.Permission_Id,\r\n    p.Permission_Type,\r\n    t1.MenuID AS levMenuId1,\r\n    t1.Order_No AS levOrd1,\r\n    t2.MenuID AS levMenuId2,\r\n    t2.Order_No AS levOrd2,\r\n    t3.MenuID AS levMenuId3,\r\n    t3.Order_No AS levOrd3,\r\n    t4.MenuID AS levMenuId4,\r\n    t4.Order_No AS levOrd4,\r\n    t1.MenuName AS Level1,\r\n    t2.MenuName AS Level2,\r\n    t3.MenuName AS Level3,\r\n    t4.MenuName AS Level4\r\nFROM Menus AS t1\r\nLEFT JOIN Menus AS t2 ON t2.ParentID = t1.MenuID\r\nLEFT JOIN Menus AS t3 ON t3.ParentID = t2.MenuID\r\nLEFT JOIN Menus AS t4 ON t4.ParentID = t3.MenuID\r\nLEFT JOIN Menu_Role_Permission_Mst1 AS mrp \r\n    ON (t1.MenuID = mrp.MenuID OR t2.MenuID = mrp.MenuID OR t3.MenuID = mrp.MenuID OR t4.MenuID = mrp.MenuID)";
                if (Role_Id == null || Role_Id != 0)
                {
                    // query += $" WHERE (r.Role_Id = {Role_Id} OR r.Role_Id IS NULL)\r\nAND t1.ParentID IS NULL ";
                    query += $" AND (mrp.Role_Id = {Role_Id} OR mrp.Role_Id IS NULL)";
                }
                // query += " ORDER BY \r\n    t1.Order_No, \r\n    t2.Order_No, \r\n    t3.Order_No, \r\n    t4.Order_No; ";
                query += $"LEFT JOIN Permission_Mst AS p ON mrp.Permission_Id = p.Permission_Id\r\nLEFT JOIN Role_Mst AS r ON mrp.Role_Id = r.Role_Id\r\nWHERE t1.ParentID IS NULL\r\nORDER BY \r\n    t1.Order_No, \r\n    t2.Order_No, \r\n    t3.Order_No, \r\n    t4.Order_No;";

                var connection = new LkDataConnection.Connection();
                var result = connection.bindmethod(query);


                if (result == null || result._DataTable == null || result._DataTable.Rows.Count == 0)
                {
                    Resp.StatusCode = StatusCodes.Status200OK;
                    Resp.Message = "No Menus Found";
                    return Ok(Resp);
                }

                DataTable dataTable = result._DataTable;
                var menus = dataTable.AsEnumerable()
    .GroupBy(row => row["Level1"]?.ToString())
    .Select(lev1 => new
    {
        MenuID = lev1.FirstOrDefault()?["levMenuId1"],
        MenuName = lev1.Key,
        Roles = lev1
            .Where(row => row["Role_Id"] != DBNull.Value || row["Permission_Id"] != DBNull.Value)
            .Select(row => new
            {
                levOrd1 = row["levOrd1"],
                RoleId = row["Role_Id"],
                RoleName = row["Role_Name"]?.ToString(),
                PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
                PermissionType = row["Permission_Type"]?.ToString()
            })
            .Distinct()
            .ToList(),
        SubMenus = lev1
            .GroupBy(row => row["Level2"]?.ToString())
            .Select(lev2 => new
            {
                MenuID = lev2.FirstOrDefault()?["levMenuId2"],
                MenuName = lev2.Key,
                Roles = lev2
                    .Where(row => row["Role_Id"] != DBNull.Value || row["Permission_Id"] != DBNull.Value)
                    .Select(row => new
                    {
                        levOrd2 = row["levOrd2"],
                        RoleId = row["Role_Id"],
                        RoleName = row["Role_Name"]?.ToString(),
                        PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
                        PermissionType = row["Permission_Type"]?.ToString()
                    })
                    .Distinct()
                    .ToList(),
                SubMenus = lev2
                    .GroupBy(row => row["Level3"]?.ToString())
                    .Select(lev3 => new
                    {
                        MenuID = lev3.FirstOrDefault()?["levMenuId3"],
                        MenuName = lev3.Key,
                        Roles = lev3
                            .Where(row => row["Role_Id"] != DBNull.Value || row["Permission_Id"] != DBNull.Value)
                            .Select(row => new
                            {
                                levOrd3 = row["levOrd3"],
                                RoleId = row["Role_Id"],
                                RoleName = row["Role_Name"]?.ToString(),
                                PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
                                PermissionType = row["Permission_Type"]?.ToString()
                            })
                            .Distinct()
                            .ToList(),
                        SubMenus = lev3
                            .Where(row => !string.IsNullOrEmpty(row["Level4"]?.ToString()))
                            .Select(row => new
                            {
                                MenuID = row["levMenuId4"],
                                MenuName = row["Level4"]?.ToString(),
                                Roles = new List<object>
                                {
                                    new
                                    {
                                        levOrd4 = row["levOrd4"],
                                        RoleId = row["Role_Id"],
                                        RoleName = row["Role_Name"]?.ToString(),
                                        PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
                                        PermissionType = row["Permission_Type"]?.ToString()
                                    }
                                }
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .ToList()
    })
    .ToList();

                

                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = "Fetched successfully";
                Resp.ApiResponse = menus;
                Resp.IsSuccess = true;
                return Ok(Resp);
            }
            catch (Exception ex)
            {
                Resp.StatusCode = StatusCodes.Status500InternalServerError;
                Resp.Message = ex.Message;
                return StatusCode(StatusCodes.Status500InternalServerError, Resp);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetAllMenuWithPermissions/{Role_Id?}")]
        public IActionResult GetAllMenusWithRolePermission(int? Role_Id)
        
        {
            try
            {
                string query = @"SELECT      
t1.IconPath as icon1,
        t2.IconPath as icon2,
		t3.IconPath as icon3,
		t4.IconPath as icon4,
        t1.Order_No as levOrd1,
        t2.Order_No as levOrd2,
        t3.Order_No as levOrd3,
        t4.Order_No as levOrd4,

            t1.MenuName AS Level1,     
            t2.MenuName AS Level2,  
            t3.MenuName AS Level3,       
            t4.MenuName AS Level4,   
            mrp.Permission_Id,    
            p.Permission_Type, 
            mrp.Role_Id, 
            r.Role_Name
        FROM Menus AS t1
        LEFT JOIN Menus AS t2 ON t2.ParentID = t1.MenuID
        LEFT JOIN Menus AS t3 ON t3.ParentID = t2.MenuID  
        LEFT JOIN Menus AS t4 ON t4.ParentID = t3.MenuID  
        JOIN Menu_Role_Permission_Mst1 AS mrp 
            ON t1.MenuID = mrp.MenuID 
            OR t2.MenuID = mrp.MenuID 
            OR t3.MenuID = mrp.MenuID 
            OR t4.MenuID = mrp.MenuID  
        JOIN Permission_Mst AS p ON mrp.Permission_Id = p.Permission_Id  
        JOIN Role_Mst AS r ON mrp.Role_Id = r.Role_Id ";

                if (Role_Id == null || Role_Id != 0)
                {
                    query += $" WHERE r.Role_Id = {Role_Id} \r\nAND t1.ParentID IS NULL ";
                }
                query += " ORDER BY \r\n    t1.Order_No, \r\n    t2.Order_No, \r\n    t3.Order_No, \r\n    t4.Order_No;";

                var connection = new LkDataConnection.Connection();
                var result = connection.bindmethod(query);

                if (result == null || result._DataTable == null || result._DataTable.Rows.Count == 0)
                {
                    Resp.StatusCode = StatusCodes.Status200OK;
                    Resp.Message = "No Menus Found";
                    return Ok(Resp);
                }
                var ImagePath = _config["EnvVariable:ImgPathIcon"];

                DataTable dataTable = result._DataTable;
           //     var ImagePath = "http://192.168.1.59:7148/public/Icons/";

                var menus = dataTable.AsEnumerable()
                    .Where(row => row["Permission_Id"] != DBNull.Value)
                    .GroupBy(row => row["Level1"]?.ToString())
                    .Select(lev1 => new
                    {
                        MenuName = lev1.Key,
                       Icon = string.IsNullOrEmpty(lev1.First()["Icon1"]?.ToString())
            ? null
            : ImagePath + lev1.First()["Icon1"]?.ToString(),
                        Roles = lev1
                            .Where(row => row["Role_Id"] != DBNull.Value)
                            .Select(row => new
                            {
                                levOrd1 = row["levOrd1"],
                                RoleId = row["Role_Id"],
                                RoleName = row["Role_Name"]?.ToString(),
                                PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
                                PermissionType = row["Permission_Type"]?.ToString()
                            })
                            .Distinct()
                            .ToList(),
                        SubMenus = lev1
                            .Where(row => !string.IsNullOrEmpty(row["Level2"]?.ToString()) && row["Permission_Id"] != DBNull.Value)
                            .GroupBy(row => row["Level2"]?.ToString())
                            .Select(lev2 => new
                            {
                                MenuName = lev2.Key,
                                Icon = string.IsNullOrEmpty(lev2.First()["Icon2"]?.ToString())
            ? null
            : ImagePath + lev2.First()["Icon2"]?.ToString(),

                                Roles = lev2
                                    .Where(row => row["Role_Id"] != DBNull.Value)
                                    .Select(row => new
                                    {
                                        levOrd2 = row["levOrd2"],
                                        RoleId = row["Role_Id"],
                                        RoleName = row["Role_Name"]?.ToString(),
                                        PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
                                        PermissionType = row["Permission_Type"]?.ToString()
                                    })
                                    .Distinct()
                                    .ToList(),
                                SubMenus = lev2
                                    .Where(row => !string.IsNullOrEmpty(row["Level3"]?.ToString()) && row["Permission_Id"] != DBNull.Value)
                                    .GroupBy(row => row["Level3"]?.ToString())
                                    .Select(lev3 => new
                                    {
                                        MenuName = lev3.Key,
                                        Icon = string.IsNullOrEmpty(lev3.First()["Icon3"]?.ToString())
            ? null
            : ImagePath + lev3.First()["Icon3"]?.ToString(),

                                        Roles = lev3
                                            .Where(row => row["Role_Id"] != DBNull.Value)
                                            .Select(row => new
                                            {
                                                levOrd3 = row["levOrd3"],
                                                RoleId = row["Role_Id"],
                                                RoleName = row["Role_Name"]?.ToString(),
                                                PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
                                                PermissionType = row["Permission_Type"]?.ToString()
                                            })
                                            .Distinct()
                                            .ToList(),
                                        SubMenus = lev3
                                            .Where(row => !string.IsNullOrEmpty(row["Level4"]?.ToString()) && row["Permission_Id"] != DBNull.Value)
                                            .Select(row => new
                                            {
                                                MenuName = row["Level4"]?.ToString(),
                                                Icon = string.IsNullOrEmpty(row["Icon4"]?.ToString())
            ? null
            : ImagePath + row["Icon4"]?.ToString(),

                                                Roles = new List<object>
                                                {
                                                    new
                                                    {
                                                        levOrd4 = row["levOrd4"],
                                                        RoleId = row["Role_Id"],
                                                        RoleName = row["Role_Name"]?.ToString(),
                                                        PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
                                                        PermissionType = row["Permission_Type"]?.ToString()
                                                    }
                                                }
                                            })
                                            .ToList()
                                    })
                                    .ToList()
                            })
                            .ToList()
                    })
                    .ToList();

                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = "Fetched successfully";
                Resp.ApiResponse = menus;
                Resp.IsSuccess = true;
                return Ok(Resp);
            }
            catch (Exception ex)
            {
                Resp.StatusCode = StatusCodes.Status500InternalServerError;
                Resp.Message = ex.Message;
                return StatusCode(StatusCodes.Status500InternalServerError, Resp);
            }
        }


        //[AllowAnonymous]
        //[HttpGet]
        //[Route("GetAllMenuWithPermissions/{Role_Id?}")]
        //public IActionResult GetAllMenusWithRolePermission(int? Role_Id)
        //{
        //    try
        //    {


        //        string query = @"SELECT      
        //t1.Order_No as levOrd1,
        //t2.Order_No as levOrd2,
        //t3.Order_No as levOrd3,
        //t4.Order_No as levOrd4,

        //    t1.MenuName AS Level1,     
        //    t2.MenuName AS Level2,  
        //    t3.MenuName AS Level3,       
        //    t4.MenuName AS Level4,   
        //    mrp.Permission_Id,    
        //    p.Permission_Type, 
        //    mrp.Role_Id, 
        //    r.Role_Name
        //FROM Menus AS t1
        //LEFT JOIN Menus AS t2 ON t2.ParentID = t1.MenuID
        //LEFT JOIN Menus AS t3 ON t3.ParentID = t2.MenuID  
        //LEFT JOIN Menus AS t4 ON t4.ParentID = t3.MenuID  
        //JOIN Menu_Role_Permission_Mst1 AS mrp 
        //    ON t1.MenuID = mrp.MenuID 
        //    OR t2.MenuID = mrp.MenuID 
        //    OR t3.MenuID = mrp.MenuID 
        //    OR t4.MenuID = mrp.MenuID  
        //JOIN Permission_Mst AS p ON mrp.Permission_Id = p.Permission_Id  
        //JOIN Role_Mst AS r ON mrp.Role_Id = r.Role_Id   

        //            ";



        //        if (Role_Id == null || Role_Id != 0)
        //        {
        //            query += $"  WHERE r.Role_Id = {Role_Id} \r\nAND t1.ParentID IS NULL  ";
        //        }
        //        query += "  ORDER BY \r\n    t1.Order_No, \r\n    t2.Order_No, \r\n    t3.Order_No, \r\n    t4.Order_No;  ";

        //        var connection = new LkDataConnection.Connection();
        //        var result = connection.bindmethod(query);


        //        if (result == null || result._DataTable == null || result._DataTable.Rows.Count == 0)
        //        {
        //            Resp.StatusCode = StatusCodes.Status200OK;
        //            Resp.Message = "No Menus Found";
        //            return Ok(Resp);
        //        }

        //        DataTable dataTable = result._DataTable;

        //        var menus = dataTable.AsEnumerable()
        //            .GroupBy(row => row["Level1"]?.ToString())
        //            .Select(lev1 => new
        //            {
        //                MenuName = lev1.Key,
        //                Roles = lev1
        //                    .Where(row => row["Role_Id"] != DBNull.Value || row["Permission_Id"] != DBNull.Value)
        //                    .Select(row => new
        //                    {
        //                        levOrd1 = row["levOrd1"],
        //                        RoleId = row["Role_Id"],
        //                        RoleName = row["Role_Name"]?.ToString(),
        //                        PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
        //                        PermissionType = row["Permission_Type"]?.ToString()
        //                    })
        //                    .Distinct()
        //                    .ToList(),
        //                SubMenus = lev1
        //                    .GroupBy(row => row["Level2"]?.ToString())
        //                    .Select(lev2 => new
        //                    {
        //                        MenuName = lev2.Key,
        //                        Roles = lev2
        //                            .Where(row => row["Role_Id"] != DBNull.Value || row["Permission_Id"] != DBNull.Value)
        //                            .Select(row => new
        //                            {
        //                                levOrd2 = row["levOrd2"],
        //                                RoleId = row["Role_Id"],
        //                                RoleName = row["Role_Name"]?.ToString(),
        //                                PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
        //                                PermissionType = row["Permission_Type"]?.ToString()
        //                            })
        //                            .Distinct()
        //                            .ToList(),
        //                        SubMenus = lev2
        //                            .GroupBy(row => row["Level3"]?.ToString())
        //                            .Select(lev3 => new
        //                            {
        //                                MenuName = lev3.Key,
        //                                Roles = lev3
        //                                    .Where(row => row["Role_Id"] != DBNull.Value || row["Permission_Id"] != DBNull.Value)
        //                                    .Select(row => new
        //                                    {
        //                                        levOrd3 = row["levOrd3"],
        //                                        RoleId = row["Role_Id"],
        //                                        RoleName = row["Role_Name"]?.ToString(),
        //                                        PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
        //                                        PermissionType = row["Permission_Type"]?.ToString()
        //                                    })
        //                                    .Distinct()
        //                                    .ToList(),
        //                                SubMenus = lev3
        //                                    .Where(row => !string.IsNullOrEmpty(row["Level4"]?.ToString()))
        //                                    .Select(row => new
        //                                    {
        //                                        MenuName = row["Level4"]?.ToString(),
        //                                        Roles = new List<object>
        //                                        {
        //                                            new
        //                                            {
        //                                                levOrd4 = row["levOrd4"],
        //                                                RoleId = row["Role_Id"],
        //                                                RoleName = row["Role_Name"]?.ToString(),
        //                                                PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
        //                                                PermissionType = row["Permission_Type"]?.ToString()
        //                                            }
        //                                        }
        //                                    })
        //                                    .ToList()
        //                            })
        //                            .ToList()
        //                    })
        //                    .ToList()
        //            })
        //            .ToList();


        //        Resp.StatusCode = StatusCodes.Status200OK;
        //        Resp.Message = "Fetched successfully";
        //        Resp.ApiResponse = menus;
        //        Resp.IsSuccess = true;
        //        return Ok(Resp);
        //    }
        //    catch (Exception ex)
        //    {
        //        Resp.StatusCode = StatusCodes.Status500InternalServerError;
        //        Resp.Message = ex.Message;
        //        return StatusCode(StatusCodes.Status500InternalServerError, Resp);
        //    }
        //}
























    }



}
