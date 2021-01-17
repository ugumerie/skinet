using API.Errors;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("errors/{code}")]
    /*ignored by swagger -> won't see this as an endpoint - initially causing error while trying to load 
    swagger - cause we want this controller to handle any type of error post, ger, put, delete for our 
    errors
    */
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : BaseApiController
    {
        public IActionResult Error(int code)
        {
            return new ObjectResult(new ApiResponse(code));
        }
    }
}