using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using VehicleManagement.Classes;
using VehicleManagement.Model;

namespace VehicleManagement.Controllers
{
    [Authorize]
    [Route("/[controller]")]
    [ApiController]
    public class MenuRolePermissionController : ControllerBase
    {
        private apiResponse Resp = new apiResponse();
        private readonly ConnectionClass _connection;
        LkDataConnection.DataAccess _dc = new LkDataConnection.DataAccess();
        LkDataConnection.SqlQueryResult _query = new LkDataConnection.SqlQueryResult();
        public MenuRolePermissionController(ConnectionClass connection)
        {
            _connection = connection;
            LkDataConnection.Connection.Connect();
            LkDataConnection.Connection.ConnectionStr = _connection.GetSqlConnection().ConnectionString;

        }

        [HttpGet]

        [Route("GetAllMenuRolePermission")]
        public IActionResult GetAllMenuRolePermission()
        {
            try
            {
                string query = $"SELECT    mrp.RMP_Id, m.Menu_Id,   m.Menu_Name, r.Role_Id,    r.Role_Name, p.Permission_Id,    p.Permission_Type FROM    Menu_Role_Permission_Mst mrp JOIN    Menu_Mst m ON mrp.Menu_Id = m.Menu_Id JOIN     Role_Mst r ON mrp.Role_Id = r.Role_Id JOIN    Permission_Mst p ON mrp.Permission_Id = p.Permission_Id";
                Console.WriteLine(query);
                var connection = new LkDataConnection.Connection();

                var result = connection.bindmethod(query);


                DataTable Table = result._DataTable;

                var MenuRolePermissionList = new List<MenuRolePermissionModel>();

                foreach (DataRow row in Table.Rows)
                {
                    MenuRolePermissionList.Add(new MenuRolePermissionModel
                    {
                        RMP_Id = Convert.ToInt32(row["RMP_Id"]),

                        Role_Id = Convert.ToInt32(row["Role_Id"]),
                        Role_Name = row["Role_Name"].ToString(),
                        Permission_Id = Convert.ToInt32(row["Permission_Id"]),
                        Permission_Type = row["Permission_Type"].ToString(),

                        Menu_Id = Convert.ToInt32(row["Menu_Id"]),
                        Menu_Name = row["Menu_Name"].ToString(),





                    });
                }



                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = $"Data fetched successfully ";
                Resp.ApiResponse = MenuRolePermissionList;
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
        [Route("AddMenuRolePermission")]

        public IActionResult AddMenuRolePermission([FromBody] MenuRolePermissionModel menuRolePermission)
        {
            try
            {
                if (menuRolePermission == null ||
                    menuRolePermission.Role_Id <= 0 ||
                    menuRolePermission.MenuID <= 0 ||
                    menuRolePermission.Permission_Id <= 0)
                {
                    Resp.StatusCode = StatusCodes.Status400BadRequest;
                    Resp.Message = "Invalid or missing data. Please provide valid Role_Id, Menu_Id, and Permission_Id.";
                    return StatusCode(StatusCodes.Status400BadRequest, Resp);
                }

                string deleteQuery = $"DELETE FROM Menu_Role_Permission_Mst1 WHERE Role_Id = {menuRolePermission.Role_Id} AND MenuID = {menuRolePermission.MenuID}";


                LkDataConnection.Connection.ExecuteNonQuery(deleteQuery);

                _query = _dc.InsertOrUpdateEntity(menuRolePermission, "Menu_Role_Permission_Mst1", -1);

                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = "Menu Role Permission Added successfully.";
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

        [HttpPost]
        [Route("AddMulMenuRolePermission")]
        public IActionResult AddMulMenuRolePermission([FromBody] List<MenuRolePermissionModel> menuRolePermissions)
        {
            try
            {
                if (menuRolePermissions == null || menuRolePermissions.Count == 0)
                {
                    Resp.StatusCode = StatusCodes.Status400BadRequest;
                    Resp.Message = "Data not found ....";
                    return StatusCode(StatusCodes.Status400BadRequest, Resp);
                }

                foreach (var menuRolePermission in menuRolePermissions)
                {
                    if (menuRolePermission.Role_Id <= 0 ||
                        menuRolePermission.MenuID <= 0 ||
                        menuRolePermission.Permission_Id <= 0 )
                    {
                        Resp.StatusCode = StatusCodes.Status400BadRequest;
                        Resp.Message = "Invalid Role_Id, MenuID or Permission_Id in one of the entries.";
                        return StatusCode(StatusCodes.Status400BadRequest, Resp);
                    }


                    if (menuRolePermission.Role_Id == null)
                    {
                        Resp.StatusCode = StatusCodes.Status400BadRequest;
                        Resp.Message = "RoleId Is Missing  In One Of The Entries. Please Provide RoleId ";
                        return StatusCode(StatusCodes.Status400BadRequest, Resp);
                    }

                    if (menuRolePermission.MenuID == null)
                    {
                        Resp.StatusCode = StatusCodes.Status400BadRequest;
                        Resp.Message = "MenuID Is Missing  In One Of The Entries Please Provide RoleId ";
                        return StatusCode(StatusCodes.Status400BadRequest, Resp);
                    }
                    if (menuRolePermission.Permission_Id == null)
                    {
                        Resp.StatusCode = StatusCodes.Status400BadRequest;
                        Resp.Message = "PermissionId Is Missing  In One Of The Entries Please Provide RoleId ";
                        return StatusCode(StatusCodes.Status400BadRequest, Resp);
                    }

                }

                foreach (var menuRolePermission in menuRolePermissions)
                {
                    string deleteQuery = $"DELETE FROM Menu_Role_Permission_Mst1 WHERE Role_Id = {menuRolePermission.Role_Id} AND MenuID = {menuRolePermission.MenuID} AND Permission_Id = {menuRolePermission.Permission_Id}";
                    LkDataConnection.Connection.ExecuteNonQuery(deleteQuery);
                }

                foreach (var menuRolePermission in menuRolePermissions)
                {
                    _query = _dc.InsertOrUpdateEntity(menuRolePermission, "Menu_Role_Permission_Mst1", -1);
                }

                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = "Menu Role Permissions added successfully.";
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
        [Route("updateMenuRolePermission/{id}")]
        public IActionResult UpdateMenuRolePermission(int id, [FromBody] MenuRolePermissionModel menuRolePermission)
        {
            try
            {
                if (menuRolePermission == null ||
                    menuRolePermission.Role_Id <= 0 ||
                    menuRolePermission.Menu_Id <= 0 ||
                    menuRolePermission.Permission_Id <= 0)
                {
                    Resp.StatusCode = StatusCodes.Status400BadRequest;
                    Resp.Message = "Invalid or missing data. Please provide valid Role_Id, Menu_Id, and Permission_Id.";
                    return StatusCode(StatusCodes.Status400BadRequest, Resp);
                }

                var MenuRolePermissionExists = $"SELECT COUNT(*) FROM Menu_Role_Permission_Mst WHERE RMP_Id = {id}";
                int result = Convert.ToInt32(_connection.ExecuteScalar(MenuRolePermissionExists));

                if (result == 0)
                {
                    Resp.StatusCode = StatusCodes.Status404NotFound;
                    Resp.Message = $"Menu Role Permission does not exist.";
                    return StatusCode(StatusCodes.Status404NotFound, Resp);
                }


                string deleteQuery = $"DELETE FROM Menu_Role_Permission_Mst WHERE Role_Id = {menuRolePermission.Role_Id} AND Menu_Id = {menuRolePermission.Menu_Id} AND RMP_Id != {id}";

                LkDataConnection.Connection.ExecuteNonQuery(deleteQuery);


                _query = _dc.InsertOrUpdateEntity(menuRolePermission, "Menu_Role_Permission_Mst", id, "RMP_Id");


                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = "Menu Role Permission updated successfully.";
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
        [Route("deleteMenuRolePermission/{id}")]
        public IActionResult deleteMenuRolePermission(int id)
        {



            try
            {
                var MenuRolePermissionExists = $"SELECT COUNT(*) FROM Menu_Role_Permission_Mst WHERE RMP_Id = {id} ";
                int result = Convert.ToInt32(_connection.ExecuteScalar(MenuRolePermissionExists));


                if (result == 0)
                {
                    Resp.StatusCode = StatusCodes.Status404NotFound;
                    Resp.Message = $"RMP ID does not exist.";

                    return StatusCode(StatusCodes.Status404NotFound, Resp);

                }

             
                string deleteMenuRolePermissionQuery = $"Delete from Menu_Role_Permission_Mst where RMP_Id='{id}'";

                LkDataConnection.Connection.ExecuteNonQuery(deleteMenuRolePermissionQuery);
                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = "Menu Role Permission Deleted successfully";
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



    }
}
