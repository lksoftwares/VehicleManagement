﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;
using VehicleManagement.Classes;
using VehicleManagement.Model;

namespace VehicleManagement.Controllers
{
    [Authorize]
    [Route("/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private apiResponse Resp = new apiResponse();
        private readonly ConnectionClass _connection;
        LkDataConnection.DataAccess _dc = new LkDataConnection.DataAccess();
        LkDataConnection.SqlQueryResult _query = new LkDataConnection.SqlQueryResult();
        public PermissionController(ConnectionClass connection)
        {
            _connection = connection;
            LkDataConnection.Connection.Connect();
            LkDataConnection.Connection.ConnectionStr = _connection.GetSqlConnection().ConnectionString;

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

                var result = connection.bindmethod(query);


                DataTable Table = result._DataTable;

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
        [HttpGet]



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
                string query = @"
  SELECT
    r.Role_Id,
    r.Role_Name,
    mrp.Permission_Id,
    p.Permission_Type,
    t1.Order_No AS levOrd1,
    t2.Order_No AS levOrd2,
    t3.Order_No AS levOrd3,
    t4.Order_No AS levOrd4,
    
    t1.MenuName AS Level1,
    t2.MenuName AS Level2,
    t3.MenuName AS Level3,
    t4.MenuName AS Level4
FROM Menus AS t1

LEFT JOIN Menus AS t2 ON t2.ParentID = t1.MenuID

LEFT JOIN Menus AS t3 ON t3.ParentID = t2.MenuID

LEFT JOIN Menus AS t4 ON t4.ParentID = t3.MenuID


LEFT JOIN Menu_Role_Permission_Mst1 AS mrp 
    ON (t1.MenuID = mrp.MenuID OR t2.MenuID = mrp.MenuID OR t3.MenuID = mrp.MenuID OR t4.MenuID = mrp.MenuID)


LEFT JOIN Permission_Mst AS p ON mrp.Permission_Id = p.Permission_Id

LEFT JOIN Role_Mst AS r ON mrp.Role_Id = r.Role_Id
       
 
        ";

                if (Role_Id == null || Role_Id != 0)
                {
                    query += $" WHERE (r.Role_Id = {Role_Id} OR r.Role_Id IS NULL)\r\nAND t1.ParentID IS NULL ";
                }
                query += " ORDER BY \r\n    t1.Order_No, \r\n    t2.Order_No, \r\n    t3.Order_No, \r\n    t4.Order_No; ";

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



        //[AllowAnonymous]

        //[HttpGet]
        //[Route("GetAllMenusWithRole/{Role_Id?}")]
        //public IActionResult GetAllMenusWithRole(int Role_Id)
        //{
        //    try

        //    {

        //        string query = "SELECT\r\n\t   r.Role_Id,\r\n    r.Role_Name,\r\n\t    mrp.Permission_Id,\r\n    p.Permission_Type,\r\n    t1.Order_No AS levOrd1,\r\n    t2.Order_No AS levOrd2,\r\n    t3.Order_No AS levOrd3,\r\n    t4.Order_No AS levOrd4,\r\n    \r\n    t1.MenuName AS Level1,\r\n    t2.MenuName AS Level2,\r\n    t3.MenuName AS Level3,\r\n    t4.MenuName AS Level4\r\n    \r\n\r\n \r\n\r\nFROM Menus AS t1\r\nLEFT JOIN Menus AS t2 ON t2.ParentID = t1.MenuID\r\nLEFT JOIN Menus AS t3 ON t3.ParentID = t2.MenuID\r\nLEFT JOIN Menus AS t4 ON t4.ParentID = t3.MenuID\r\n\r\n\r\nLEFT JOIN Menu_Role_Permission_Mst1 AS mrp \r\n    ON (t1.MenuID = mrp.MenuID \r\n        OR t2.MenuID = mrp.MenuID \r\n        OR t3.MenuID = mrp.MenuID \r\n        OR t4.MenuID = mrp.MenuID)\r\n\r\nLEFT JOIN Permission_Mst AS p ON mrp.Permission_Id = p.Permission_Id\r\n\r\nLEFT JOIN Role_Mst AS r ON mrp.Role_Id = r.Role_Id ";



        //        if (Role_Id == null || Role_Id != 0)
        //        {
        //            query += $" WHERE r.Role_Id = {Role_Id}  OR r.Role_Id IS NULL ";
        //        }
        //        //query += " ORDER BY \r\n    t1.Order_No, \r\n    t2.Order_No, \r\n    t3.Order_No, \r\n    t4.Order_No; ";

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
        //.GroupBy(row => row["Level1"]?.ToString())
        //.Select(lev1 => new
        //{
        //    MenuName = lev1.Key,
        //    Roles = lev1
        //        .Where(row => row["Role_Id"] != DBNull.Value || row["Permission_Id"] != DBNull.Value)
        //        .Select(row => new
        //        {
        //            levOrd1 = row["levOrd1"],
        //            RoleId = row["Role_Id"],
        //            RoleName = row["Role_Name"]?.ToString(),
        //            PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
        //            PermissionType = row["Permission_Type"]?.ToString()
        //        })
        //        .Distinct()
        //        .ToList(),
        //    SubMenus = lev1
        //        .GroupBy(row => row["Level2"]?.ToString())
        //        .Select(lev2 => new
        //        {
        //            MenuName = lev2.Key,
        //            Roles = lev2
        //                .Where(row => row["Role_Id"] != DBNull.Value || row["Permission_Id"] != DBNull.Value)
        //                .Select(row => new
        //                {
        //                    levOrd2 = row["levOrd2"],
        //                    RoleId = row["Role_Id"],
        //                    RoleName = row["Role_Name"]?.ToString(),
        //                    PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
        //                    PermissionType = row["Permission_Type"]?.ToString()
        //                })
        //                .Distinct()
        //                .ToList(),
        //            SubMenus = lev2
        //                .GroupBy(row => row["Level3"]?.ToString())
        //                .Select(lev3 => new
        //                {
        //                    MenuName = lev3.Key,
        //                    Roles = lev3
        //                        .Where(row => row["Role_Id"] != DBNull.Value || row["Permission_Id"] != DBNull.Value)
        //                        .Select(row => new
        //                        {
        //                            levOrd3 = row["levOrd3"],
        //                            RoleId = row["Role_Id"],
        //                            RoleName = row["Role_Name"]?.ToString(),
        //                            PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
        //                            PermissionType = row["Permission_Type"]?.ToString()
        //                        })
        //                        .Distinct()
        //                        .ToList(),
        //                    SubMenus = lev3
        //                        .Where(row => !string.IsNullOrEmpty(row["Level4"]?.ToString()))
        //                        .Select(row => new
        //                        {
        //                            MenuName = row["Level4"]?.ToString(),
        //                            Roles = new List<object>
        //                            {
        //                            new
        //                            {
        //                                levOrd4 = row["levOrd4"],
        //                                RoleId = row["Role_Id"],
        //                                RoleName = row["Role_Name"]?.ToString(),
        //                                PermissionId = row["Permission_Id"] != DBNull.Value ? row["Permission_Id"] : null,
        //                                PermissionType = row["Permission_Type"]?.ToString()
        //                            }
        //                            }
        //                        })
        //                        .ToList()
        //                })
        //                .ToList()
        //        })
        //        .ToList()
        //})
        //.ToList();



        //        Resp.StatusCode = StatusCodes.Status200OK;
        //        Resp.Message = " Fetched successfully ";
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
