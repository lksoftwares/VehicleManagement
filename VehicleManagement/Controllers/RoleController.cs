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
    public class RoleController : ControllerBase
    {
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
            string query = $"select * from Role_mst ORDER BY Role_Name ASC";
            Console.WriteLine(query);
            var connection = new LkDataConnection.Connection();

            var result = connection.bindmethod(query);


            DataTable Table = result._DataTable;

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
          

            return Ok(RoleList);
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
                    return StatusCode(StatusCodes.Status208AlreadyReported, new { message = "RoleName already exists.", DUP = true });
                }
                if (String.IsNullOrEmpty(role.Role_Name))
                {
                    return StatusCode(StatusCodes.Status200OK, new { message = "RoleName Can't be Blank Or Null", DUP = false });
                }
                _query = _dc.InsertOrUpdateEntity(role, "Role_mst", -1);
                return StatusCode(StatusCodes.Status200OK, new { message = "RoleName Added Successfully", DUP = false });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
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
                    return StatusCode(StatusCodes.Status404NotFound, new { message = "Role ID does not exist.", DUP = false });
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
                    return StatusCode(StatusCodes.Status208AlreadyReported, new { message = "RoleName already exists.", DUP = true });

                }
                if (String.IsNullOrEmpty(role.Role_Name))
                {
                    return StatusCode(StatusCodes.Status200OK, new { message = "RoleName Can't be Blank Or Null", DUP = true });

                }
                _query = _dc.InsertOrUpdateEntity(role, "Role_mst", Role_ID, "Role_Id");
                return StatusCode(StatusCodes.Status200OK, new { message = "RoleName Updated Successfully", DUP = false });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
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
                    return StatusCode(StatusCodes.Status404NotFound, new { message = "Role ID does not exist.", DUP = false });
                }

                string checkQuery = $"SELECT COUNT(*) AS recordCount FROM User_mst WHERE Role_Id = {id}";


                int roleIdInUser = Convert.ToInt32(_connection.ExecuteScalar(checkQuery));
                    if (roleIdInUser > 0)
                    {
                        return Ok("Can't delete Exists in another table  ");
                    }
                    string deleteRoleQuery = $"Delete from Role_mst where Role_Id='{id}'";

                LkDataConnection.Connection.ExecuteNonQuery(deleteRoleQuery);
                return Ok("RoleName Deleted successfully");

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
            }
        }

    }
}
