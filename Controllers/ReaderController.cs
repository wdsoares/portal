using Microsoft.AspNetCore.Mvc;


namespace portal.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReaderController : ControllerBase
    {
        /* public List<string> Get ()
        {
            var service = new ReaderService();

            var tags = service.Get();

            return tags;
        } */
        public string Get()
        {
           return "kkk";
        }
    }
}
