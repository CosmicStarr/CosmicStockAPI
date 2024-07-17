using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CosmicStockapi.ErrorHandling
{
    //This class will Validate Error Response Users Create when filling out a form!
    public class ApiValidationResponse:ApiErrorResponse
    {
        public ApiValidationResponse():base(400){}
        public ApiValidationResponse(IEnumerable<string> errors):base(400)
        {
            Errors = errors;
        }
        public IEnumerable<string> Errors { get; set; }
    }
}