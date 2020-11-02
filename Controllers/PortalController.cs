using Microsoft.AspNetCore.Mvc;


namespace portal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortalController : ControllerBase
    {
        Database db = new Database();

        [HttpGet]
        public string consultaBD()
        {
            return db.get();
        }
        
        [HttpGet("tag/{tag}")]
        public string consultaBD(string tag)
        {
            return db.get(tag);
        }

    }
}
