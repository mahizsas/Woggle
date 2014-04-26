using Data.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Data.EntityFramework
{
    public class WoggleDbContext : IdentityDbContext<ApplicationUser>
    {
        public WoggleDbContext()
            : base("AdminContext", throwIfV1Schema: false)
        {
        }

        public static WoggleDbContext Create()
        {
            return new WoggleDbContext();
        }
    }
}