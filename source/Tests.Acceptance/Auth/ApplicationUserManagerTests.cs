using System.Data.Entity;
using System.Security.Claims;
using Data.EntityFramework;
using Data.Infrastructure;
using Data.Migrations;
using Data.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Should;
using Xunit;

namespace Tests.Acceptance.Auth
{
    public class ApplicationUserManagerTests
    {
        public ApplicationUserManagerTests()
        {
            WoggleDbContext context = new WoggleDbContext();
            IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            ApplicationUserManager manager = new ApplicationUserManager(store);

            MigrateDatabaseToLatestVersion<WoggleDbContext, Configuration> confgi = new MigrateDatabaseToLatestVersion<WoggleDbContext, Configuration>();
            confgi.InitializeDatabase(context);
            
            context.Dispose();
            store.Dispose();
            manager.Dispose();
        }

        [Fact]
        public async void ValidateBasicCredentialsReturnsValidIdentity()
        {
            WoggleDbContext context = new WoggleDbContext();
            IUserStore<ApplicationUser> store = new UserStore<ApplicationUser>(context);
            ApplicationUserManager manager = new ApplicationUserManager(store);

            ClaimsPrincipal validIdent = await manager.ValidateBasiccredentials("AndrewAllison", "Pa$$word123");
            validIdent.ShouldNotBeNull();
            validIdent.Identity.IsAuthenticated.ShouldBeTrue();
            validIdent.Identity.AuthenticationType.ShouldEqual("Basic");
            validIdent.Identity.GetUserName().ShouldEqual("AndrewAllison");
        }
    }
}