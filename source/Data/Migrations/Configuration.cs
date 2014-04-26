using System.Configuration;
using System.Data.SqlClient;
using Data.EntityFramework;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using Data.Infrastructure;
using Data.Models;
using Data.Utils;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Data.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<WoggleDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = true;

            bool dropDatabase;
            bool.TryParse(ConfigurationManager.AppSettings["DropDatabase"] ?? "false", out dropDatabase);
            
            if (dropDatabase)
            {
                var conn = ConfigurationManager.ConnectionStrings["WoggleContext"];
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(conn.ConnectionString);
                // Lets Drop the WHole thing and start from Scratch... Could be dangerous 
                DatabaseHelpers.KillProcesses(databaseName: builder.InitialCatalog);

                if (Database.Exists("WoggleContext"))
                    Database.Delete("WoggleContext");

                DatabaseHelpers.CreateUser(builder.UserID, builder.Password);

              //  DatabaseHelpers.CreateUser("WoggleUser", "%$Gr48P455W0Rd");
            }
        }

        protected override void Seed(WoggleDbContext context)
        {
            ApplicationUserManager applicationUserManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));

            applicationUserManager.Create(new ApplicationUser
            {
                DisplayName = "Andrew Allison",
                Email = "andrew.allison0411@gmail.com",
                UserName = "AndrewAllison"

            }, "Pa$$word123");
        }
    }
}
