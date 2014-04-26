using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Common.Extensions;
using Data.EntityFramework;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using ServiceStack.OrmLite;

namespace Data.Utils
{
    public static class DatabaseHelpers
    {
        public static void CreateUser(string loginName, string password, string connStringName = "AdminContext", string databaseName = "master")
        {
            var defaultConnection = ConfigurationManager.ConnectionStrings[connStringName];
            var connString = defaultConnection.ConnectionString;
            SqlConnectionStringBuilder stringBuilder = new SqlConnectionStringBuilder(connString);
            var hostName = stringBuilder.DataSource;
            var dbName = databaseName;

            Server server = new Server(hostName);
            server.ConnectionContext.LoginSecure = true; // Login using windows auth
            server.ConnectionContext.Connect();
            var db = server.Databases[dbName];

            if (server.Logins[loginName] == null)
            {
                // You probably want to create a login and add as a user to your database
                Login login = new Login(server, loginName) { DefaultDatabase = dbName, LoginType = LoginType.SqlLogin };
                login.Create(password, LoginCreateOptions.None); // Enter a suitable password
                login.Enable();
            }

            if (db.Users[loginName] == null)
            {
                User user = new User(db, loginName) { UserType = UserType.SqlLogin, Login = loginName };
                user.Create();
                // add a role
                user.AddToRole("db_owner");
            }
        }

        public static void KillProcesses(string connStringName = "AdminContext", string databaseName = null)
        {
            var defaultConnection = ConfigurationManager.ConnectionStrings[connStringName];
            var connString = defaultConnection.ConnectionString;
            SqlConnectionStringBuilder stringBuilder = new SqlConnectionStringBuilder(connString);
            var hostName = stringBuilder.DataSource;
            var dbName = databaseName ?? stringBuilder.InitialCatalog;

            Server server = new Server(hostName);
            server.ConnectionContext.LoginSecure = true; // Login using windows auth
            server.ConnectionContext.Connect();
            var db = server.Databases[dbName];

            if (db != null)
            {
                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    const string sqlFormat = @"
             USE master; 

             DECLARE @databaseName VARCHAR(50);
             SET @databaseName = '{0}';

             declare @kill varchar(8000) = '';
             select @kill=@kill+'kill '+convert(varchar(5),spid)+';'
             from master..sysprocesses 
             where dbid=db_id(@databaseName);

             exec (@kill);";

                    var sql = string.Format(sqlFormat, dbName);

                    con.ExecuteNonQuery(sql);
                    con.Close();
                }

            }
        }

        public static void RunSqlFromFiles(string filePath = "", string connStringName = "WoggleContext")
        {
            var defaultConnection = ConfigurationManager.ConnectionStrings[connStringName].ConnectionString;

            // Path and location
            string path = filePath;
            if (string.IsNullOrWhiteSpace(filePath))
            {
                string sourcePath = typeof(WoggleDbContext).GetAssemblyPath();
                int intLocation = sourcePath.IndexOf("\\src", StringComparison.InvariantCulture);
                string solutionFolder = sourcePath.Substring(0, intLocation);
                const string databaseFolder = "database";

                path = Path.Combine(solutionFolder, databaseFolder);
            }

            // Get the view files from the folder.
            IEnumerable<string> schemaFiles = from file in Directory.GetFiles(path)
                .Where(p => Regex.Match(p, "Schema.sql").Success)
                                              orderby file ascending
                                              select file;
            foreach (var file in schemaFiles)
            {
                ExecuteFile(file, defaultConnection);
            }

            // Get the view files from the folder.
            string pattern = "View.sql";
            IEnumerable<string> sqlFiles = from file in Directory.GetFiles(path)
                .Where(p => Regex.Match(p, pattern).Success)
                                           orderby file ascending
                                           select file;

            foreach (string sqlFile in sqlFiles)
            {
                ExecuteFile(sqlFile, defaultConnection);
            }
        }

        public static void ExecuteFile(string sqlFile, string connString)
        {
            SqlConnection conn = new SqlConnection(connString);
            Server server = new Server(new ServerConnection(conn));

            FileInfo sqFileInfo = new FileInfo(sqlFile);
            string script = sqFileInfo.OpenText().ReadToEnd();

            try
            {
                server.ConnectionContext.ExecuteNonQuery(script);
            }
            catch (ExecutionFailureException exception)
            {
                Console.WriteLine(exception);
            }
            catch (SqlException exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}