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
        private apiResponse Resp = new apiResponse();
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
            try
            {
                string query = $"select * from Menus_Mst ORDER BY Menu_Name ASC";
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
                        Menu_Name = row["Menu_Name"].ToString(),
                        Parent_Id = row["Parent_Id"] as int?
                        // Parent_Id = Convert.ToInt32(row["Menu_Id"])




                    });
                }
                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = $"Data fetched successfully ";
                Resp.ApiResponse = MenuList;
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

        [Route("AddMenu")]
        public IActionResult AddMenu([FromBody] MenusModel menus)
        {
            try
            {
                var duplicacyChecker = new CheckDuplicacy(_connection);
                var duplicacyParameter = new CheckDuplicacyPerameter
                {
                    tableName = "Menus_Mst",
                    fields = new[] { "Menu_Name" },
                    values = new[] { menus.Menu_Name }
                };
                bool isDuplicate = duplicacyChecker.CheckDuplicate(duplicacyParameter);
                if (isDuplicate)
                {
                    Resp.StatusCode = StatusCodes.Status208AlreadyReported;
                    Resp.Message = $"Menu already exists";
                    Resp.Dup = true;

                    return StatusCode(StatusCodes.Status208AlreadyReported, Resp);

                }
                if (String.IsNullOrEmpty(menus.Menu_Name))
                {
                    Resp.StatusCode = StatusCodes.Status404NotFound;
                    Resp.Message = $"Menu  Can't be Blank Or Null";

                    return StatusCode(StatusCodes.Status404NotFound, Resp);
                }
                _query = _dc.InsertOrUpdateEntity(menus, "Menus_Mst", -1);
                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = $"Menu Added Successfully";
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
        [Route("updateMenu/{Menu_Id}")]

        public IActionResult updateMenu(int Menu_Id, [FromBody] MenusModel menus)
        {
            try
            {

                var MenuExists = $"SELECT COUNT(*) FROM Menus_Mst WHERE Menu_Id = {Menu_Id} ";
                int result = Convert.ToInt32(_connection.ExecuteScalar(MenuExists));


                if (result == 0)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { message = "Menu ID does not exist.", DUP = false });
                }

                var duplicacyChecker = new CheckDuplicacy(_connection);
                var duplicacyParameter = new CheckDuplicacyPerameter
                {
                    tableName = "Menus_Mst",
                    fields = new[] { "Menu_Name" },
                    values = new[] { menus.Menu_Name },
                    idField = "Menu_Id",
                    idValue = Menu_Id.ToString()
                };

                bool isDuplicate = duplicacyChecker.CheckDuplicate(duplicacyParameter);


                if (isDuplicate)
                {
                    Resp.StatusCode = StatusCodes.Status208AlreadyReported;
                    Resp.Message = $"Menu already exists";
                    Resp.Dup = true;

                    return StatusCode(StatusCodes.Status208AlreadyReported, Resp);
                }
                if (String.IsNullOrEmpty(menus.Menu_Name))
                {
                    Resp.StatusCode = StatusCodes.Status404NotFound;
                    Resp.Message = $"Menu  Can't be Blank Or Null";

                    return StatusCode(StatusCodes.Status404NotFound, Resp);
                }

                _query = _dc.InsertOrUpdateEntity(menus, "Menus_Mst", Menu_Id, "Menu_Id");
                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = $"Menu Updated Successfully";
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
        [Route("deleteMenu/{id}")]
        public IActionResult deleteMenu(int id)
        {


            try
            {
                var MenuExists = $"SELECT COUNT(*) FROM Menus_Mst WHERE Menu_Id = {id} ";
                int result = Convert.ToInt32(_connection.ExecuteScalar(MenuExists));


                if (result == 0)
                {
                    Resp.StatusCode = StatusCodes.Status404NotFound;
                    Resp.Message = "Menu ID does not exist.";

                    return StatusCode(StatusCodes.Status404NotFound, Resp);
                }

                string deleteRoleQuery = $"Delete from Menus_Mst where Menu_Id='{id}'";

                LkDataConnection.Connection.ExecuteNonQuery(deleteRoleQuery);
                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = "Menu Deleted successfully";
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

