using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using VehicleManagement.Model;

namespace VehicleManagement.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {

        [HttpGet]

        [Route("GetAllPermission")]
        public IActionResult GetAllRole()
        {
            string query = $"select * from Permission_Mst ";
            Console.WriteLine(query);
            var connection = new LkDataConnection.Connection();

            var result = connection.bindmethod(query);


            DataTable Table = result._DataTable;

            var PermissionList = new List<PermissionModel>();

            foreach (DataRow row in Table.Rows)
            {
                PermissionList.Add(new PermissionModel
                {
                    Permission_Id = Convert.ToInt32(row["Permission_Id"]),
                    Permission_Type = row["Permission_Type"].ToString()


                });
            }


            return Ok(PermissionList);
        }


    }
}
