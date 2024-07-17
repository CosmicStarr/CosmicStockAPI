using AutoMapper;
using CosmicStockapi.ErrorHandling;
using Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs;

namespace CosmicStockapi.Controllers
{
    [Authorize(policy:"AdminCEO")]
    public class CreateController:BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICreateEntity _createEntity;
        private readonly IMapper _mapper;
        public CreateController(IUnitOfWork unitOfWork,ICreateEntity createEntity,IMapper mapper)
        {
            _mapper = mapper;
            _createEntity = createEntity;
            _unitOfWork = unitOfWork;
        }

        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)] 
        [DisableRequestSizeLimit] 
        [Consumes("multipart/form-data")]
        [HttpPost]
        public async Task<ActionResult<ProductDTO>> CreateProduct([FromForm]Post productDTO)
        {
            var info = await _createEntity.CreateProduct(productDTO);
            var mappedObj = _mapper.Map<Products,ProductDTO>(info);
            return Created("api/Products", mappedObj);
        }

        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)] 
        [DisableRequestSizeLimit] 
        [Consumes("multipart/form-data")]
        [HttpPut("UpdateProduct/{Id}")]
        public async Task<ActionResult<ProductDTO>> UpdateProduct(int id,[FromForm] Post productDTO)
        {
            var info = await _createEntity.UpdateProduct(id, productDTO);
            var mappedObj = _mapper.Map<Products,ProductDTO>(info);
            return Ok(mappedObj);
        }

        [HttpDelete("Delete/{Id}")]
        public async Task<ActionResult> DeleteProduct(int Id)
        {
            var info = await _createEntity.DeleteProduct(Id);
            if(info is 0) return BadRequest(new ApiErrorResponse(400,"Could not delete product!"));
            return Ok("Deleted!");
        }

    }
}