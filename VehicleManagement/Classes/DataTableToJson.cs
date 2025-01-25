using Newtonsoft.Json;
using System.Data;
using static VehicleManagement.Classes.CreateQueryWithPermissions;

namespace VehicleManagement.Classes
{
    public class DataTableToJson
    {
        private apiResponse Resp = new apiResponse();

        public string DataTableToJsonMethod(DataTable dataTable)
        {
            string prettyJson = JsonConvert.SerializeObject(dataTable, Formatting.Indented);
            return prettyJson;

        }
        public object QueryToDataTable(DatatblePerameters datatblePerameters)
        {

         
            var connection = new LkDataConnection.Connection();

        
          int maxLevel = GetMaxLevel();
           
            var queryFields = new PerameteFeilds
            {
                Levels = maxLevel,
                RoleId = datatblePerameters.Role_Id ,
                ImagePath = "http://192.168.1.64:7148/public/Icons/"
            };
            CreateQueryWithPermissions createMenuQuery = new CreateQueryWithPermissions();
            string query = createMenuQuery.CreateMenus_Mst(queryFields);

            var result = connection.bindmethod(query);

            if (result == null || result._DataTable == null || result._DataTable.Rows.Count == 0)
            {
                Resp.StatusCode = StatusCodes.Status404NotFound;
                Resp.Message = "No Menus Found";
                return Resp;
            }

            DataTable dataTable = result._DataTable;
            return dataTable;
        }



        public int GetMaxLevel()
        {
            var countQuery = @"WITH Menu AS
    (
        SELECT 
            Menu_Id,
            Parent_Id,
            1 AS Level
        FROM 
            Menus_Mst
        WHERE 
            Parent_Id IS NULL

        UNION ALL

        SELECT 
            m.Menu_Id,
            m.Parent_Id,
            mh.Level + 1 AS Level
        FROM 
            Menus_Mst m
        INNER JOIN 
            Menu mh
        ON 
            m.Parent_Id = mh.Menu_Id
    )
    SELECT MAX(Level) AS Mx
    FROM Menu;";
            var connection = new LkDataConnection.Connection();
            var levelCount = connection.bindmethod(countQuery);

            DataTable levelTable = levelCount._DataTable;
            if (levelTable == null || levelTable.Rows.Count == 0)
            {
                return 0; 
            }

            return Convert.ToInt32(levelTable.Rows[0]["Mx"]);
        }


    }
    public class DatatblePerameters()
    {
        public string query { get; set; }
        public int Role_Id { get; set; }
    }
}
