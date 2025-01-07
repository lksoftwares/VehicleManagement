using LkDataConnection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using System.Data;
using VehicleManagement.Classes;
using VehicleManagement.Model;

namespace VehicleManagement.Controllers
{
    [Authorize]
    [Route("/[controller]")]
    [ApiController]
    public class VehicleController : ControllerBase
    {

      
            private readonly ConnectionClass _connection;
            LkDataConnection.DataAccess _dc = new LkDataConnection.DataAccess();
            LkDataConnection.SqlQueryResult _query = new LkDataConnection.SqlQueryResult();
            public VehicleController(ConnectionClass connection)
            {
                _connection = connection;
                LkDataConnection.Connection.Connect();
                LkDataConnection.Connection.ConnectionStr = _connection.GetSqlConnection().ConnectionString;

            }

       

            [HttpGet]

            [Route("GetAllVehicle")]
            public IActionResult GetAllVehicle()
            {
                string query = $"select * from Vehicle_Mst ";
                Console.WriteLine(query);
                var connection = new LkDataConnection.Connection();

                var result = connection.bindmethod(query);


                DataTable Table = result._DataTable;

                var VehicleList = new List<VehiclesModel>();

                foreach (DataRow row in Table.Rows)
                {
                VehicleList.Add(new VehiclesModel
                    {
                        Vehicle_Id = Convert.ToInt32(row["Vehicle_Id"]),
                        Vehicle_No = row["Vehicle_No"].ToString(),
                    Owner_Name = row["Owner_Name"].ToString(),
                    Contact_Number = row["Contact_Number"].ToString(),
                    Vehicle_Status = Convert.ToBoolean(row["Vehicle_Status"]),
                    Created_At = Convert.ToDateTime(row["Created_At"])



                });
                }


                return Ok(VehicleList);
            }

            [HttpPost]

            [Route("AddVehicle")]

            public IActionResult AddVehicle([FromBody] VehiclesModel vehicle)
            {
                try
                {
                    var duplicacyChecker = new CheckDuplicacy(_connection);
                    var duplicacyParameter = new CheckDuplicacyPerameter
                    {
                        tableName = "Vehicle_Mst",
                        fields = new[] { "Vehicle_No" },
                        values = new[] { vehicle.Vehicle_No}
                    };
                    bool isDuplicate = duplicacyChecker.CheckDuplicate(duplicacyParameter);
                    if (isDuplicate)
                    {
                        return StatusCode(StatusCodes.Status208AlreadyReported, new { message = "Vehicle Number already exists.", DUP = true });
                    }
                    if (String.IsNullOrEmpty(vehicle.Vehicle_No))
                    {
                        return StatusCode(StatusCodes.Status200OK, new { message = "Vehicle No Can't be Blank Or Null", DUP = false });
                    }
           
                    vehicle.Created_At = DateTime.Now;
               
                _query = _dc.InsertOrUpdateEntity(vehicle, "Vehicle_Mst", -1);
                    return StatusCode(StatusCodes.Status200OK, new { message = "Vehicle Added Successfully", DUP = false });

                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
                }
            }

            [HttpPut]
            [Route("updateVehicle/{Vehicle_Id}")]

            public IActionResult UpdateVehicle(int Vehicle_Id, [FromBody] VehiclesModel Vehicle)
            {
                try
                {
                var roleExists = $"SELECT COUNT(*) FROM Vehicle_Mst WHERE Vehicle_Id = {Vehicle_Id} ";
                    int result = Convert.ToInt32(_connection.ExecuteScalar(roleExists));


                    if (result == 0)
                    {
                        return StatusCode(StatusCodes.Status404NotFound, new { message = "Vehicle ID does not exist.", DUP = false });
                    }

                    var duplicacyChecker = new CheckDuplicacy(_connection);
                    var duplicacyParameter = new CheckDuplicacyPerameter
                    {
                        tableName = "Vehicle_Mst",
                        fields = new[] { "Vehicle_No" },
                        values = new[] { Vehicle.Vehicle_No },
                        idField = "Vehicle_Id",
                        idValue = Vehicle_Id.ToString()
                    };

                    bool isDuplicate = duplicacyChecker.CheckDuplicate(duplicacyParameter);


                    if (isDuplicate)
                    {
                        return StatusCode(StatusCodes.Status208AlreadyReported, new { message = "Vehicle Number already exists.", DUP = true });

                    }
                   
                Vehicle.Created_At = DateTime.Now;
                _query = _dc.InsertOrUpdateEntity(Vehicle, "Vehicle_Mst", Vehicle_Id, "Vehicle_Id");
                    return StatusCode(StatusCodes.Status200OK, new { message = "Vehicle Updated Successfully", DUP = false });

                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
                }
            }

            [HttpDelete]
            [Route("deleteVehicle/{Vehicle_Id}")]
            public IActionResult DeleteVehicle(int Vehicle_Id)
            {



                try
                {
                    var roleExists = $"SELECT COUNT(*) FROM Vehicle_Mst WHERE Vehicle_Id = {Vehicle_Id} ";
                    int result = Convert.ToInt32(_connection.ExecuteScalar(roleExists));


                    if (result == 0)
                    {
                        return StatusCode(StatusCodes.Status404NotFound, new { message = " Vehicle ID does not exist.", DUP = false });
                    }

                   
                    string deleteRoleQuery = $"Delete from Vehicle_Mst where Vehicle_Id='{Vehicle_Id}'";

                    LkDataConnection.Connection.ExecuteNonQuery(deleteRoleQuery);
                return StatusCode(StatusCodes.Status200OK, new { message = "Vehicle Number Deleted successfully" });

            }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
                }
            }


        }
    }

