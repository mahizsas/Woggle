using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace Web.Host.Controllers
{
    [Authorize]
    public class ValuesController : ApiController
    {
        public List<int> GetVaules()
        {
            return new List<int>() { 2, 6, 9, 33, 99 };
        }
    }
}