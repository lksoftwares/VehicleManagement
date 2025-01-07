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
    public class UsersShiftController : ControllerBase
    {
        private readonly ConnectionClass _connection;
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
                    Start_Time = TimeSpan.Parse(row["Start_Time"].ToString()), 
                    End_Time = TimeSpan.Parse(row["End_Time"].ToString()),
                    Grace_Time = float.Parse(row["Grace_Time"].ToString()),
                    Created_At = Convert.ToDateTime(row["Created_At"]),
                    Shift_Status = Convert.ToBoolean(row["Shift_Status"])

                });
            }


            return Ok(ShiftList);
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
                    return StatusCode(StatusCodes.Status208AlreadyReported, new { message = "Shift already exists.", DUP = true });
                }
                if (String.IsNullOrEmpty(Usershift.Shift_Name))
                {
                    return StatusCode(StatusCodes.Status200OK, new { message = "Shift Can't be Blank Or Null", DUP = false });
                }
                Usershift.Start_Time = new TimeSpan(Usershift.Start_Time.Value.Hours, Usershift.End_Time.Value.Minutes, 0);

                Usershift.End_Time = new TimeSpan(Usershift.End_Time.Value.Hours, Usershift.End_Time.Value.Minutes, 0);

                _query = _dc.InsertOrUpdateEntity(Usershift, "User_Shift_Mst", -1);
                return StatusCode(StatusCodes.Status200OK, new { message = "Shift Added Successfully", DUP = false });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
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
                    return StatusCode(StatusCodes.Status404NotFound, new { message = "Role ID does not exist.", DUP = false });
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
                    return StatusCode(StatusCodes.Status208AlreadyReported, new { message = "Shift Name already exists.", DUP = true });

                }
                usershift.Start_Time = new TimeSpan(usershift.Start_Time.Value.Hours, usershift.End_Time.Value.Minutes, 0);

                usershift.End_Time = new TimeSpan(usershift.End_Time.Value.Hours, usershift.End_Time.Value.Minutes, 0);
                _query = _dc.InsertOrUpdateEntity(usershift, "User_Shift_Mst", Shift_Id, "Shift_Id");
                return StatusCode(StatusCodes.Status200OK, new { message = "Shift Name Updated Successfully", DUP = false });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
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

               
                string deleteRoleQuery = $"Delete from User_Shift_Mst where Shift_Id ='{id}'";

                LkDataConnection.Connection.ExecuteNonQuery(deleteRoleQuery);
                return StatusCode(StatusCodes.Status200OK, new { message = "Shift  Deleted successfully" });


            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
            }
        }



    }
}
