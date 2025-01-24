using System.Diagnostics;

namespace VehicleManagement.Classes
{
    public class SQLVersionClas
    {


        public void GetSqlVersion()
        {
            string command = "sqlcmd -Q \"SELECT @@VERSION\"";

            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe"; 
            process.StartInfo.Arguments = "/C " + command; 
            process.StartInfo.RedirectStandardOutput = true; 
            process.StartInfo.UseShellExecute = false; 
            process.StartInfo.CreateNoWindow = true; 

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Console.WriteLine("SQL Command Output: " + output);
        }
    }
}
