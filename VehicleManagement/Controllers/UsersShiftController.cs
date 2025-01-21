using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using VehicleManagement.Classes;
using VehicleManagement.Model;

namespace VehicleManagement.Controllers
{
    [Authorize]
    [Route("/[controller]")]
    [ApiController]
    public class UsersShiftController : ControllerBase
    {
        private readonly ConnectionClass _connection;
        private apiResponse Resp = new apiResponse();
        LkDataConnection.DataAccess _dc = new LkDataConnection.DataAccess();
        LkDataConnection.SqlQueryResult _query = new LkDataConnection.SqlQueryResult();
        public UsersShiftController(ConnectionClass connection)
        {
            _connection = connection;
            LkDataConnection.Connection.Connect();
            LkDataConnection.Connection.ConnectionStr = _connection.GetSqlConnection().ConnectionString;

        }
        [HttpGet]

        [Route("GetAllShifts")]
        public IActionResult GetAllShifts()
        {
            try
            {
                string query = $"select * from User_Shift_Mst  ORDER BY Shift_Name ASC";
                Console.WriteLine(query);
                var connection = new LkDataConnection.Connection();

                var result = connection.bindmethod(query);


                DataTable Table = result._DataTable;

                var ShiftList = new List<UsershiftModel>();

                foreach (DataRow row in Table.Rows)
                {
                    ShiftList.Add(new UsershiftModel
                    {
                        Shift_Id = Convert.ToInt32(row["Shift_Id"]),
                        Shift_Name = row["Shift_Name"].ToString(),
                        End_Time = TimeSpan.Parse(row["End_Time"].ToString()).ToString("hh\\:mm"),
                        Start_Time = TimeSpan.Parse(row["Start_Time"].ToString()).ToString("hh\\:mm"),
                        Grace_Time = float.Parse(row["Grace_Time"].ToString()),
                        Created_At = Convert.ToDateTime(row["Created_At"]),
                        Shift_Status = Convert.ToBoolean(row["Shift_Status"])

                    });
                }

                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = $"Shift List fetched successfully ";
                Resp.ApiResponse = ShiftList;
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

        [Route("AddShift")]
        public IActionResult AddUserShift([FromBody] UsershiftModel Usershift)
        {
            try
            {
                var duplicacyChecker = new CheckDuplicacy(_connection);
                var duplicacyParameter = new CheckDuplicacyPerameter
                {
                    tableName = "User_Shift_Mst",
                    fields = new[] { "Shift_Name" },
                    values = new[] { Usershift.Shift_Name }
                };
                bool isDuplicate = duplicacyChecker.CheckDuplicate(duplicacyParameter);
                if (isDuplicate)
                {
                    Resp.StatusCode = StatusCodes.Status208AlreadyReported;
                    Resp.Message = $"Shift already exists.";
                    Resp.Dup = true;

                    return StatusCode(StatusCodes.Status208AlreadyReported, Resp);
                }
                if (String.IsNullOrEmpty(Usershift.Shift_Name))
                {
                    Resp.StatusCode = StatusCodes.Status404NotFound;
                    Resp.Message = $"Shift Can't be Blank Or Null";

                    return StatusCode(StatusCodes.Status404NotFound, Resp);
                }


                //Usershift.Start_Time = new TimeSpan(Usershift.Start_Time.Value.Hours, Usershift.End_Time.Value.Minutes, 0);

                //Usershift.End_Time = new TimeSpan(Usershift.End_Time.Value.Hours, Usershift.End_Time.Value.Minutes, 0);


                _query = _dc.InsertOrUpdateEntity(Usershift, "User_Shift_Mst", -1);
                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = "Shift Added Successfully";
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
        [Route("updateShift/{Shift_Id}")]

        public IActionResult updateShift(int Shift_Id, [FromBody] UsershiftModel usershift)
        {
            try
            {

                var ShiftExists = $"SELECT COUNT(*) FROM User_Shift_Mst WHERE  Shift_Id = {Shift_Id} ";
                int result = Convert.ToInt32(_connection.ExecuteScalar(ShiftExists));


                if (result == 0)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { message = "Shift ID does not exist.", DUP = false });
                }

                var duplicacyChecker = new CheckDuplicacy(_connection);
                var duplicacyParameter = new CheckDuplicacyPerameter
                {
                    tableName = "User_Shift_Mst",
                    fields = new[] { "Shift_Name" },
                    values = new[] { usershift.Shift_Name },
                    idField = "Shift_Id",
                    idValue = Shift_Id.ToString()
                };

                bool isDuplicate = duplicacyChecker.CheckDuplicate(duplicacyParameter);


                if (isDuplicate)
                {
                    Resp.StatusCode = StatusCodes.Status208AlreadyReported;
                    Resp.Message = $"Shift already exists.";
                    Resp.Dup = true;

                    return StatusCode(StatusCodes.Status208AlreadyReported, Resp);
                }




                //usershift.Start_Time = new TimeSpan(usershift.Start_Time.Value.Hours, usershift.End_Time.Value.Minutes, 0);

                //usershift.End_Time = new TimeSpan(usershift.End_Time.Value.Hours, usershift.End_Time.Value.Minutes, 0);
                _query = _dc.InsertOrUpdateEntity(usershift, "User_Shift_Mst", Shift_Id, "Shift_Id");
                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = "Shift Name Updated Successfully";
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
        [Route("deleteShift/{id}")]
        public IActionResult DeleteRoleName(int id)
        {



            try
            {
                var ShiftExists = $"SELECT COUNT(*) FROM User_Shift_Mst WHERE  Shift_Id = {id} ";
                int result = Convert.ToInt32(_connection.ExecuteScalar(ShiftExists));



                if (result == 0)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { message = "Shift ID does not exist.", DUP = false });
                }
                string checkQuery = $"SELECT COUNT(*) AS recordCount FROM User_mst WHERE Shift_Id = {id}";


                int shift_IdInUser = Convert.ToInt32(_connection.ExecuteScalar(checkQuery));
                if (shift_IdInUser > 0)
                {
                    Resp.StatusCode = StatusCodes.Status404NotFound;
                    Resp.Message = $"Can't delete Exists in another table ";

                    return StatusCode(StatusCodes.Status404NotFound, Resp);
                  


                }

                string deleteRoleQuery = $"Delete from User_Shift_Mst where Shift_Id ='{id}'";

                LkDataConnection.Connection.ExecuteNonQuery(deleteRoleQuery);
                Resp.StatusCode = StatusCodes.Status200OK;
                Resp.Message = "Shift  Deleted successfully";
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
