using LkDataConnection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VehicleManagement.Classes;
using VehicleManagement.Model;

namespace VehicleManagement.Controllers
{
    [Authorize]
    [Route("/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private   IConfiguration _config;
        EncryptDecrypt encryptPassword = new EncryptDecrypt();
        private readonly  IWebHostEnvironment _hostingEnvironment;

        private ConnectionClass _connectionClass;
        DataAccess _dc = new DataAccess();
        SqlQueryResult _query = new SqlQueryResult();
        public UsersController(ConnectionClass connectionClass,IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            _config = configuration;

            _connectionClass = connectionClass;
            _hostingEnvironment = hostingEnvironment;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]

        public IActionResult Login(UsersModel user
            )
        {

         IActionResult response = Unauthorized();
            try
            {

                string hashedPassword = encryptPassword.Encrypt("ABC", user.User_Password);


                //string query = $"SELECT U.*,R.* FROM User_Mst U  Join Role_Mst R on U.Role_Id=R.Role_Id WHERE  U.User_Email = '{user.User_Email}' AND U.User_Password='{hashedPassword}'";
              



                string query = $"SELECT U.*, R.Role_Id,R.Role_Name, M.Menu_Name, P.Permission_Type  FROM User_Mst U   JOIN Role_Mst R ON U.Role_Id = R.Role_Id   JOIN Menu_Role_Permission_Mst RMP ON R.Role_Id = RMP.Role_Id   JOIN Menu_Mst M ON RMP.Menu_Id = M.Menu_Id  JOIN Permission_Mst P ON RMP.Permission_Id = P.Permission_Id  WHERE U.User_Email = '{user.User_Email}' AND U.User_Password = '{hashedPassword}'";

                var connection = new LkDataConnection.Connection();

                var result = connection.bindmethod(query);

                DataTable Table = result._DataTable;
                DataRow userData = Table.Rows.Count > 0 ? Table.Rows[0] : null;

                Console.WriteLine($" Here is MEnu  {userData["Menu_Name"]}");


                if (userData == null)
                {
                    return Unauthorized(new { message = "User not found" });
                }


              



                if (hashedPassword != userData["User_Password"].ToString())
                {
                    return Unauthorized(new { message = "Password not matched" });
                }


                if (userData["Role_Id"].ToString() != user.Role_Id.ToString())
                {
                    return Unauthorized(new { message = "Role not matched" });
                }

                if (!(bool)userData["User_Status"])
                {
                    return Unauthorized(new { message = "User is not active. Please contact the administrator." });
                }
                //            var menupermission = Table.AsEnumerable()
                //.GroupBy(row => row["Menu_Name"].ToString()) 
                //.Select(group => new
                //{
                //    MenuName = group.Key,
                //    Permissions = group.Select(row => row["Permission_Type"].ToString()).ToList()
                //})
                //.ToArray();
                //        var menupermission = Table.AsEnumerable().GroupBy(row => row["Menu_Name"].ToString()).ToArray(group => group.Key,
                //group => group.Select(row => row["Permission_Type"].ToString()).ToList());

                var menuPermissions = Table.AsEnumerable()
    .GroupBy(row => row["Menu_Name"].ToString())
    .ToDictionary(
        MenuName => MenuName.Key,
        Permissions => Permissions.Select(row => row["Permission_Type"].ToString()).ToList()
    );




                //var menuPermissions = new Dictionary<string, List<string>>();
                //foreach (DataRow row in Table.Rows)
                //{
                //    string menuName = row["Menu_Name"].ToString();
                //    string permissionType = row["Permission_Type"].ToString();

                //    if (!menuPermissions.ContainsKey(menuName))
                //    {
                //        menuPermissions[menuName] = new List<string>();
                //    }

                //    menuPermissions[menuName].Add(permissionType);
                //}

             


                WebToken _web = new WebToken();
                UserDetails _userdetails = new UserDetails();
                List<KeyDetails> lst = new List<KeyDetails>();
                List<KeyDetails> lst1 = new List<KeyDetails>
                                {
                                    new KeyDetails
                                { KeyName = "User_Id", KeyValue = userData["User_Id"].ToString() },

                                                        new KeyDetails
                { KeyName = "Role_Id", KeyValue = userData["Role_Id"].ToString() },


                                    new KeyDetails
                                { KeyName = "User_Name", KeyValue = userData["User_Name"].ToString() }
                                };

                lst.Add(new KeyDetails
                { KeyName = "User_Id", KeyValue = userData["User_Id"].ToString() });
                lst.Add(new KeyDetails
                { KeyName = "User_Name", KeyValue = userData["User_Name"].ToString() });

                lst.Add(new KeyDetails
                { KeyName = "Role_Id", KeyValue = userData["Role_Id"].ToString() });
                _userdetails.ListKeydetails = lst1;

                string token = _web.GenerateToken(new LkDataConnection.WebTokenValidationParameters
                {
                    ValidIssuer = "http://localhost:7148/",
                    ValidAudience = "http://localhost:7148/",
                    IssuerSigningKey = "2Fsk5LBU5j1DrPldtFmLWeO8uZ8skUzwhe3ktVimUE8l=",

                }, new LkDataConnection.UserDetails
                {

                    ListKeydetails = _userdetails.ListKeydetails 

                });
                WebTokenDetails _tokendetails = new WebTokenDetails();
                _tokendetails.Token = token;
                _tokendetails.TokenKeyName = "Role_Id";
                _tokendetails = _web.ExtractTokenInformation(_tokendetails);

                Console.WriteLine($"Here is the token {token}");
               
                response = Ok(new
                {
                    token,
                    User_Id = userData["User_Id"],
                    Role_Id = userData["Role_Id"],
                    Role_Name = userData["Role_Name"],
                //    Menus = userData["Menu_Name"],
                      Menus= menuPermissions
                    //  Permission_type = userData["Permission_Type"],



                });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }

            return response;
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public IActionResult Register([FromForm] UsersModel user)
        {

            if (user.User_Password != null)
            {
                
                string hashedPassword = encryptPassword.Encrypt("ABC", user.User_Password);

                user.User_Password = hashedPassword;
            }
            user.User_Status = 1;
            user.Role_Id = 6;
            try
            {
                var duplicacyChecker = new CheckDuplicacy(_connectionClass);

                
                var duplicacyParameter = new CheckDuplicacyPerameter
                {
                    tableName = "User_Mst",
                    fields = new[] { "User_Email" , "User_Name" },
                    values = new[] { user.User_Email,user.User_Name },
                  
                };

                bool isDuplicate = duplicacyChecker.CheckDuplicate(duplicacyParameter);

                if (isDuplicate)
                {
                    return StatusCode(StatusCodes.Status208AlreadyReported, new { message = "Duplicate ! Users exists.", DUP = true });
                }
                if (String.IsNullOrEmpty(user.User_Email) || String.IsNullOrEmpty(user.User_Name) || String.IsNullOrEmpty(user.User_Password))
                {
                    return StatusCode(StatusCodes.Status208AlreadyReported, new { message = "User Email ,Name,Password can't be blank " });

                }
                if (user.User_Name != null || !string.IsNullOrEmpty(user.User_Name))
                {
                    var username = new System.Globalization.CultureInfo("en-US", false).TextInfo.ToTitleCase(user.User_Name.ToLower());
                    user.User_Name = username;

                }

            
                _query = _dc.InsertOrUpdateEntity(user, "User_Mst", -1);
                return StatusCode(StatusCodes.Status200OK, new { message = "USer Register successfully" });




            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error:{ex.Message}");
            }

        }





        [HttpGet]
        [Route("ProfileImage/{User_ID}")]
        public IActionResult GetProfileImage(int User_ID)
        {
            string query = $"SELECT Image FROM User_mst WHERE User_Id = {User_ID}";
            var connection = new LkDataConnection.Connection();
            var result = connection.bindmethod(query);
            DataTable Table = result._DataTable;


            if (Table.Rows.Count > 0)
            {
                string imageName = Table.Rows[0]["Image"]?.ToString();

                if (!string.IsNullOrEmpty(imageName))
                {
                    var imageUrl = $"http://192.168.1.66:7148/public/images/{imageName}";

                    return Ok(new { ImageUrl = imageUrl });
                }
                else
                {
                    return NotFound(new { Message = "No image found for this user." });
                }
            }
            else
            {
                return NotFound(new { Message = "User not found." });
            }
        }


        [HttpPut]
        [Route("updateUser/{User_ID}")]

        public IActionResult updateUsers(int User_ID, [FromForm] UsersModel user)
        {
            try
            {
                string wwwRootPath = _hostingEnvironment.WebRootPath;

                string oldImagePath = _connectionClass.GetOldImagePathFromDatabase(User_ID);

                if (!string.IsNullOrEmpty(oldImagePath))
                {
                    string imagePath = Path.Combine(wwwRootPath, "Public/Images", oldImagePath);
                }
                var duplicacyChecker = new CheckDuplicacy(_connectionClass);

                var duplicacyParameter = new CheckDuplicacyPerameter
                {
                    tableName = "User_Mst",
                    fields = new[] { "User_Email", "User_Name" },
                    values = new[] { user.User_Email, user.User_Name },
                    idField = "User_Id",
                    idValue = User_ID.ToString()

                };

                bool isDuplicate = duplicacyChecker.CheckDuplicate(duplicacyParameter);

                if (isDuplicate)
                {
                    return StatusCode(StatusCodes.Status208AlreadyReported, new { message = "Duplicate ! Users exists.", DUP = true });

                }
                if (user.User_Password != null)
                {
                    string hashedPassword = encryptPassword.Encrypt("ABC", user.User_Password);



                    user.User_Password = hashedPassword;
                }
                if (user.User_Name != null || !string.IsNullOrEmpty(user.User_Name))
                {
                    var username = new System.Globalization.CultureInfo("en-US", false).TextInfo.ToTitleCase(user.User_Name.ToLower());
                    user.User_Name = username;

                }

                _query = _dc.InsertOrUpdateEntity(user, "User_mst", User_ID, "User_Id", "wwwroot/Public/Images");
                return StatusCode(StatusCodes.Status200OK, new { message = "Users Updated Successfully", DUP = false });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
            }
        }






        [HttpGet]
        [Route("AllUsers")]

        public IActionResult GetAllUSers([FromQuery] IDictionary<string, string> param)
        {
            var query = $"select U.*,R.Role_Name From User_Mst U join Role_Mst R ON R.Role_Id=U.Role_Id";

            List<string> filter = new List<string>();
            Dictionary<string, object> sqlparams = new Dictionary<string, object>();
            if (param.TryGetValue("User_Id", out string User_ID))
            {
                filter.Add("  U.User_Id = @User_ID");
                sqlparams.Add("@User_ID", User_ID);
            }
            if (param.TryGetValue("Role_Id", out string Role_ID))
            {
                filter.Add("  U.Role_Id = @Role_ID");
                sqlparams.Add("@Role_ID", Role_ID);
            }

            if (filter.Count > 0)
            {
                query += " WHERE " + string.Join(" AND ", filter);
            }


            DataTable UserTable = _connectionClass.ExecuteQueryWithResult(query, sqlparams);
            var UsersList = new List<UsersModel>();

            foreach (DataRow row in UserTable.Rows)
            {


                UsersList.Add(new UsersModel
                {

                    User_Id = Convert.ToInt32(row["User_Id"]),
                    Role_Id = Convert.ToInt32(row["Role_Id"]),

                    User_Name = row["User_Name"].ToString(),
                    User_Email = row["User_Email"].ToString(),
                    User_Password = encryptPassword.Decrypt("ABC",row["User_Password"].ToString()),


                    userRole = row["Role_Name"].ToString(),

                    User_Status = Convert.ToInt32(row["User_Status"])


                }); ;



            }
            string TotalUsers = $"select Count(*) as totalUsers from User_Mst where Role_Id=6";
            DataTable Table = _connectionClass.ExecuteQueryWithResult(TotalUsers);
            int TotalUser = 0;
            if (Table.Rows.Count > 0)
            {
                TotalUser = Table.Rows[0]["totalUsers"] != DBNull.Value
                 ? Convert.ToInt32(Table.Rows[0]["totalUsers"])
                 : 0;
            }
            string TotalAdmin = $"select Count(*) as totalAdmin from User_Mst where Role_Id=1";
            DataTable AdminTable = _connectionClass.ExecuteQueryWithResult(TotalAdmin);
            int Admins = 0;
            if (AdminTable.Rows.Count > 0)
            {
                Admins = AdminTable.Rows[0]["totalAdmin"] != DBNull.Value
                 ? Convert.ToInt32(AdminTable.Rows[0]["totalAdmin"])
                 : 0;
            }

            return Ok(new
            {
                TotalAdmin = Admins,
                UsersLists = UsersList,
                TotalUsers = TotalUser
            });


        }



        [HttpDelete]
        [Route("deleteUser/{User_ID}")]
        public IActionResult DeleteUser(int User_ID)
        {
            try
            {
               
                var roleExists = $"SELECT COUNT(*) FROM User_mst WHERE User_Id = {User_ID} ";
                int result = Convert.ToInt32(_connectionClass.ExecuteScalar(roleExists));


                if (result == 0)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new { message = "User ID does not exist.", DUP = false });
                }

                _connectionClass.GetSqlConnection().Close();

                string roleCheckQuery = $"SELECT COUNT(*) AS roleCount FROM User_Mst WHERE Role_Id = 1";
                int roleCount = Convert.ToInt32(_connectionClass.ExecuteScalar(roleCheckQuery));
                _connectionClass.GetSqlConnection().Close();

                string currentUserRoleQuery = $"SELECT COUNT(*) AS currentUserRoleCount FROM User_mst WHERE User_Id = {User_ID} AND Role_Id = 1";
                int currentUserRoleCount = Convert.ToInt32(_connectionClass.ExecuteScalar(currentUserRoleQuery));

                if (roleCount == 1 && currentUserRoleCount == 1)
                {
                    return StatusCode(StatusCodes.Status200OK, new { message = "Can't delete. This is the only Admin in the Table" });
                }
                _connectionClass.GetSqlConnection().Close();


                string deleteUserQuery = $"DELETE FROM User_Mst WHERE User_Id = {User_ID}";
                LkDataConnection.Connection.ExecuteNonQuery(deleteUserQuery);
                return StatusCode(StatusCodes.Status200OK, new { message = "User deleted successfully" });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }







    }
}
