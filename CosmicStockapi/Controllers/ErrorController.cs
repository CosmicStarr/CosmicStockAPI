using CosmicStockapi.ErrorHandling;
using Microsoft.AspNetCore.Mvc;

namespace CosmicStockapi.Controllers
{
    [Route("/errors/{code}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController:BaseController
    {
        public IActionResult Error(int code)
        {
            return new OkObjectResult(new ApiErrorResponse(code));
        }
    }
}