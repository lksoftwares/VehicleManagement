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

     //   [AllowAnonymous]

        [HttpGet]

        [Route("getallrole")]

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


                bool isDuplicate = duplicacyChecker.CheckDuplicate("Role_mst",
                    new[] { "Role_Name" },
                    new[] { role.Role_Name });


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
                var duplicacyChecker = new CheckDuplicacy(_connection);

                bool isDuplicate = duplicacyChecker.CheckDuplicate("Role_mst",
                 new[] { "Role_Name" },
                 new[] { role.Role_Name },
                 "Role_Id", Role_ID.ToString());

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
