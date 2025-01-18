using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Runtime.Intrinsics.Arm;
using VehicleManagement.Classes;
using VehicleManagement.Model;

namespace VehicleManagement.Controllers
{
    [Authorize]
    [Route("/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private apiResponse Resp = new apiResponse();
        private readonly ConnectionClass _connection;
         LkDataConnection.DataAccess _dc= new LkDataConnection.DataAccess();
       LkDataConnection.SqlQueryResult _query= new LkDataConnection.SqlQueryResult();
        public RoleController(ConnectionClass connection)
        {
            _connection = connection;
            LkDataConnection.Connection.Connect();
            LkDataConnection.Connection.ConnectionStr = _connection.GetSqlConnection().ConnectionString;

        }

     [AllowAnonymous]

        [HttpGet]

        [Route("GetAllRole")]
        public IActionResult GetAllRole()
        {
            try
            {
                string query = $"select * from Role_mst ORDER BY Role_Name ASC";
                Console.WriteLine(query);
                var connection = new LkDataConnection.Connection();

               // var result1 = connection.bindmethod(query);
              //  var result = _connection.ExecuteQueryWithResult(query);

               // DataTable Table = result._DataTable;
                DataTable Table= _connection.ExecuteQueryWithResult(query);

                var RoleList = new List<RolesModel>();

                foreach (DataRow row in Table.Rows)
                {
                    RoleList.Add(new RolesModel
                    {
                        Role_Id = Convert.ToInt32(row["Role_Id"]),
                        Role_Name = row["Role_Name"].ToString(),
                        Permission_Id = Convert.ToInt32(row["Permission_Id"]),


                    });
                }

              

                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = $"Data fetched successfully ";
                Resp.ApiResponse = RoleList;
                Resp.IsSuccess = true;

                return Ok(Resp);
            }
            catch (Exception ex)
            {
            
                Resp.StatusCode = StatusCodes.Status500InternalServerError;
                Resp.Message = ex.Message;

            return StatusCode(StatusCodes.Status500InternalServerError,Resp);

            }

        }

        [HttpPost]

        [Route("AddRole")]
        public IActionResult AddRole([FromBody] RolesModel role)
        {
            try
            {
                var duplicacyChecker = new CheckDuplicacy(_connection);
                var duplicacyParameter = new CheckDuplicacyPerameter
                {
                    tableName = "Role_mst",
                    fields = new[] { "Role_Name" },
                    values = new[] { role.Role_Name }     
                };
                bool isDuplicate = duplicacyChecker.CheckDuplicate(duplicacyParameter);
                if (isDuplicate)
                {
                    Resp.StatusCode = StatusCodes.Status208AlreadyReported;
                    Resp.Message = $"RoleName already exists.";
                    Resp.Dup = true;

                    return StatusCode(StatusCodes.Status208AlreadyReported, Resp);
                }
                if (String.IsNullOrEmpty(role.Role_Name))
                {
                    Resp.StatusCode = StatusCodes.Status404NotFound;
                    Resp.Message = $"RoleName Can't be Blank Or Null";

                    return StatusCode(StatusCodes.Status404NotFound, Resp);
                }
                _query = _dc.InsertOrUpdateEntity(role, "Role_mst", -1);

                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = $"RoleName Added Successfully";
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
        [Route("updateRole/{Role_ID}")]

        public IActionResult UpdateRole(int Role_ID, [FromBody] RolesModel role)
        {
            try
            {

                var roleExists = $"SELECT COUNT(*) FROM Role_mst WHERE Role_Id = {Role_ID} ";
                int result = Convert.ToInt32(_connection.ExecuteScalar(roleExists));


                if (result==0)
                {
                    Resp.StatusCode = StatusCodes.Status404NotFound;
                    Resp.Message = $"Role ID does not exist.";

                    return StatusCode(StatusCodes.Status404NotFound, Resp);
                }

                var duplicacyChecker = new CheckDuplicacy(_connection);
                var duplicacyParameter = new CheckDuplicacyPerameter
                {
                    tableName = "Role_mst",
                    fields = new[] { "Role_Name" },
                    values = new[] { role.Role_Name },
                    idField = "Role_Id",
                    idValue = Role_ID.ToString()
                };

                bool isDuplicate = duplicacyChecker.CheckDuplicate(duplicacyParameter);
         

                if (isDuplicate)
                {
                    Resp.StatusCode = StatusCodes.Status208AlreadyReported;
                    Resp.Message = $"RoleName already exists.";
                    Resp.Dup = true;

                    return StatusCode(StatusCodes.Status208AlreadyReported, Resp);

                }
                if (String.IsNullOrEmpty(role.Role_Name))
                {
                    Resp.StatusCode = StatusCodes.Status208AlreadyReported;
                    Resp.Message = $"RoleName Can't be Blank Or Null";

                    return StatusCode(StatusCodes.Status208AlreadyReported, Resp);

                }
                _query = _dc.InsertOrUpdateEntity(role, "Role_mst", Role_ID, "Role_Id");
                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = "RoleName Updated Successfully";
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
        [Route("deleteRole/{id}")]
        public IActionResult DeleteRoleName(int id)
        {

         

            try
            {
                var roleExists = $"SELECT COUNT(*) FROM Role_mst WHERE Role_Id = {id} ";
                int result = Convert.ToInt32(_connection.ExecuteScalar(roleExists));


                if (result == 0)
                {
                    Resp.StatusCode = StatusCodes.Status404NotFound;
                    Resp.Message = $"Role ID does not exist.";

                    return StatusCode(StatusCodes.Status404NotFound, Resp);
                  
                }

                string checkQuery = $"SELECT COUNT(*) AS recordCount FROM User_mst WHERE Role_Id = {id}";


                int roleIdInUser = Convert.ToInt32(_connection.ExecuteScalar(checkQuery));
                    if (roleIdInUser > 0)
                    {
                    Resp.StatusCode = StatusCodes.Status404NotFound;
                    Resp.Message = $"Can't delete Exists in another table";

                    return StatusCode(StatusCodes.Status404NotFound, Resp);
                   

                }
                string deleteRoleQuery = $"Delete from Role_mst where Role_Id='{id}'";

                LkDataConnection.Connection.ExecuteNonQuery(deleteRoleQuery);
                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = "RoleName Deleted successfully";
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
