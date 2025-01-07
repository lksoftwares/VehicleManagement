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
    public class MenusController : ControllerBase
    {

        private readonly ConnectionClass _connection;
        LkDataConnection.DataAccess _dc = new LkDataConnection.DataAccess();
        LkDataConnection.SqlQueryResult _query = new LkDataConnection.SqlQueryResult();
        public MenusController(ConnectionClass connection)
        {
            _connection = connection;
            LkDataConnection.Connection.Connect();
            LkDataConnection.Connection.ConnectionStr = _connection.GetSqlConnection().ConnectionString;

        }



        

        [HttpGet]

        [Route("GetAllMenu")]
        public IActionResult GetAllMenu()
        {
            string query = $"select * from Menu_Mst ORDER BY Menu_Name ASC";
            Console.WriteLine(query);
            var connection = new LkDataConnection.Connection();

            var result = connection.bindmethod(query);


            DataTable Table = result._DataTable;

            var MenuList = new List<MenusModel>();

            foreach (DataRow row in Table.Rows)
            {
                MenuList.Add(new MenusModel
                {
                    Menu_Id = Convert.ToInt32(row["Menu_Id"]),
                    Menu_Name = row["Menu_Name"].ToString()
                   


                });
            }


            return Ok(MenuList);
        }


        [HttpPost]

        [Route("AddMenu")]
        public IActionResult AddMenu([FromBody] MenusModel menus)
        {
            try
            {
                var duplicacyChecker = new CheckDuplicacy(_connection);
                var duplicacyParameter = new CheckDuplicacyPerameter
                {
                    tableName = "Menu_Mst",
                    fields = new[] { "Menu_Name" },
                    values = new[] { menus.Menu_Name }
                };
                bool isDuplicate = duplicacyChecker.CheckDuplicate(duplicacyParameter);
                if (isDuplicate)
                {
                    return StatusCode(StatusCodes.Status208AlreadyReported, new { message = "Menu already exists.", DUP = true });
                }
                if (String.IsNullOrEmpty(menus.Menu_Name))
                {
                    return StatusCode(StatusCodes.Status200OK, new { message = "Menu  Can't be Blank Or Null", DUP = false });
                }
                _query = _dc.InsertOrUpdateEntity(menus, "Menu_Mst", -1);
                return StatusCode(StatusCodes.Status200OK, new { message = "Menu Added Successfully", DUP = false });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }

        [HttpPut]
        [Route("updateMenu/{Menu_Id}")]

        public IActionResult updateMenu(int Menu_Id, [FromBody] MenusModel menus)
        {
            try
            {

                var roleExists = $"SELECT COUNT(*) FROM Menu_Mst WHERE Menu_Id = {Menu_Id} ";
                int result = Convert.ToInt32(_connection.ExecuteScalar(roleExists));


                if (result == 0)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { message = "Menu ID does not exist.", DUP = false });
                }

                var duplicacyChecker = new CheckDuplicacy(_connection);
                var duplicacyParameter = new CheckDuplicacyPerameter
                {
                    tableName = "Menu_Mst",
                    fields = new[] { "Menu_Name" },
                    values = new[] { menus.Menu_Name },
                    idField = "Menu_Id",
                    idValue = Menu_Id.ToString()
                };

                bool isDuplicate = duplicacyChecker.CheckDuplicate(duplicacyParameter);


                if (isDuplicate)
                {
                    return StatusCode(StatusCodes.Status208AlreadyReported, new { message = "Menu already exists.", DUP = true });

                }
                if (String.IsNullOrEmpty(menus.Menu_Name))
                {
                    return StatusCode(StatusCodes.Status200OK, new { message = "Menu Can't be Blank Or Null", DUP = true });

                }

                _query = _dc.InsertOrUpdateEntity(menus, "Menu_Mst", Menu_Id, "Menu_Id");
                return StatusCode(StatusCodes.Status200OK, new { message = "Menu Updated Successfully", DUP = false });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
            }
        }

        [HttpDelete]
        [Route("deleteMenu/{id}")]
        public IActionResult deleteMenu(int id)
        {


            try
            {
                var roleExists = $"SELECT COUNT(*) FROM Menu_Mst WHERE Menu_Id = {id} ";
                int result = Convert.ToInt32(_connection.ExecuteScalar(roleExists));


                if (result == 0)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { message = "Menu ID does not exist.", DUP = false });
                }

                string deleteRoleQuery = $"Delete from Menu_Mst where Menu_Id='{id}'";

                LkDataConnection.Connection.ExecuteNonQuery(deleteRoleQuery);
                return StatusCode(StatusCodes.Status200OK, new { message = "Menu Deleted successfully" });


            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
            }
        }


    }
}

