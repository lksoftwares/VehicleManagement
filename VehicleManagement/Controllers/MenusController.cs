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
        private readonly IWebHostEnvironment _hostingEnvironment;

        public MenusController(ConnectionClass connection, IWebHostEnvironment hostingEnvironment )
        {
            _connection = connection;
            LkDataConnection.Connection.Connect();
            LkDataConnection.Connection.ConnectionStr = _connection.GetSqlConnection().ConnectionString;
            _hostingEnvironment = hostingEnvironment;

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

                //var result = connection.bindmethod(query);


                //DataTable Table = result._DataTable;
                DataTable Table = _connection.ExecuteQueryWithResult(query);

                var MenuList = new List<MenusModel>();
                var MenuImgPath = "http://192.168.1.64:7148/public/Icons/";

                foreach (DataRow row in Table.Rows)
                {
                    MenuList.Add(new MenusModel
                    {
                        Menu_Id = Convert.ToInt32(row["Menu_Id"]),
                        Menu_Name = row["Menu_Name"].ToString(),
                        Parent_Id = row["Parent_Id"] as int?,

                        IconUrl = string.IsNullOrEmpty(row["IconPath"]?.ToString()) ? null : MenuImgPath + row["IconPath"].ToString()

                        // IconPath =  row["IconPath"] ,




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
        public IActionResult AddMenu([FromForm] MenusModel menus)
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

                if (string.IsNullOrEmpty(menus.Menu_Name))
                {
                    Resp.StatusCode = StatusCodes.Status404NotFound;
                    Resp.Message = $"Menu name can't be blank or null.";

                    return StatusCode(StatusCodes.Status404NotFound, Resp);
                }

              
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Public", "Icons");




        _query = _dc.InsertOrUpdateEntity(menus, "Menus_Mst", -1, imgFolderpath:folderPath);


                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = $"Menu added successfully";
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


        //[HttpPost]

        //[Route("AddMenu")]
        //public IActionResult AddMenu([FromForm] MenusModel menus)
        //{
        //    try
        //    {
        //        var duplicacyChecker = new CheckDuplicacy(_connection);
        //        var duplicacyParameter = new CheckDuplicacyPerameter
        //        {
        //            tableName = "Menus_Mst",
        //            fields = new[] { "Menu_Name" },
        //            values = new[] { menus.Menu_Name }
        //        };

        //        //string wwwRootPath = _hostingEnvironment.WebRootPath;


        //      //     string imagePath = Path.Combine(wwwRootPath, "Public/Icons", menus.IconFile.Name.ToString());
        //   //     menus.IconPath  = menus.IconFile.Name.ToString();


        //        bool isDuplicate = duplicacyChecker.CheckDuplicate(duplicacyParameter);
        //        if (isDuplicate)
        //        {
        //            Resp.StatusCode = StatusCodes.Status208AlreadyReported;
        //            Resp.Message = $"Menu already exists";
        //            Resp.Dup = true;

        //            return StatusCode(StatusCodes.Status208AlreadyReported, Resp);

        //        }
        //        if (String.IsNullOrEmpty(menus.Menu_Name))
        //        {
        //            Resp.StatusCode = StatusCodes.Status404NotFound;
        //            Resp.Message = $"Menu  Can't be Blank Or Null";

        //            return StatusCode(StatusCodes.Status404NotFound, Resp);
        //        }
        //        _query = _dc.InsertOrUpdateEntity(menus, "Menus_Mst", -1, "wwwroot/Public/Icons");
        //        Resp.StatusCode = StatusCodes.Status200OK;
        //        Resp.Message = $"Menu Added Successfully";
        //        Resp.IsSuccess = true;

        //        return StatusCode(StatusCodes.Status200OK, Resp);

        //    }
        //    catch (Exception ex)
        //    {
        //        Resp.StatusCode = StatusCodes.Status500InternalServerError;
        //        Resp.Message = ex.Message;

        //        return StatusCode(StatusCodes.Status500InternalServerError, Resp);
        //    }
        //}






        [HttpPut]
        [Route("updateMenu/{Menu_Id}")]

        public IActionResult updateMenu(int Menu_Id, [FromForm] MenusModel menus)
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
                //if (String.IsNullOrEmpty(menus.Menu_Name))
                //{
                //    Resp.StatusCode = StatusCodes.Status404NotFound;
                //    Resp.Message = $"Menu  Can't be Blank Or Null";

                //    return StatusCode(StatusCodes.Status404NotFound, Resp);
                //}
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Public", "Icons");
                _query = _dc.InsertOrUpdateEntity(menus, "Menus_Mst", Menu_Id, "Menu_Id", folderPath);
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

