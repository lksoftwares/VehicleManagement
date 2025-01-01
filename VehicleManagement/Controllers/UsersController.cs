using LkDataConnection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VehicleManagement.Classes;
using VehicleManagement.Model;

namespace VehicleManagement.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private   IConfiguration _config;

        private ConnectionClass _connectionClass;
        DataAccess _dc = new DataAccess();
        SqlQueryResult _query = new SqlQueryResult();
        public UsersController(ConnectionClass connectionClass,IConfiguration configuration)
        {
            _config = configuration;

            _connectionClass = connectionClass;
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
                EncryptDecrypt encryptPassword = new EncryptDecrypt();
                
              
                string hashedPassword = encryptPassword.Encrypt("ABC", user.User_Password);


                string query = $"SELECT U.*,R.* FROM User_Mst U  Join Role_Mst R on U.Role_Id=R.Role_Id WHERE  U.User_Email = '{user.User_Email}' AND U.User_Password='{hashedPassword}'";

                var connection = new LkDataConnection.Connection();

                var result = connection.bindmethod(query);

                DataTable Table = result._DataTable;
                DataRow userData = Table.Rows.Count > 0 ? Table.Rows[0] : null;



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
                //string token = GenerateToken(new UsersModel
                //{
                //    User_Id = Convert.ToInt32(userData["User_Id"]),
                //    User_Name = userData["User_Name"].ToString(),
                //    Role_Id = Convert.ToInt32(userData["Role_Id"]),

                //    userRole = userData["RoleName"].ToString()
                //});
                response = Ok(new
                {
                    token,
                    User_Id = userData["User_Id"],
                    Role_Id = userData["Role_Id"],
                    Role_Name = userData["Role_Name"]

                });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }

            return response;
        }
        private string GenerateToken(UsersModel users)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var issuedAt = DateTime.UtcNow;
            var localIssuedAt = TimeZoneInfo.ConvertTimeFromUtc(issuedAt, TimeZoneInfo.Local);

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                new List<Claim>
                {
              new Claim("Role_Id", users.Role_Id.ToString()),


            new Claim(ClaimTypes.NameIdentifier, users.User_Id.ToString()),
            new Claim(ClaimTypes.Name, users.User_Name),
            new Claim("iat", new DateTimeOffset(localIssuedAt).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer)
                },
                expires: localIssuedAt.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



    }
}
