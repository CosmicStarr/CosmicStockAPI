using System.Security.Claims;
using AutoMapper;
using CosmicStockapi.ErrorHandling;
using CosmicStockapi.Extension;
using Data.Interfaces;
using Data.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs;

namespace CosmicStockapi.Controllers
{
    [Authorize(policy:"AdminCEO")]
    public class AdministrationController:BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICreateEntity _createEntity;
        private readonly IOrderRepository _order;

        public AdministrationController(IMapper mapper, IUnitOfWork unitOfWork, ICreateEntity createEntity,IOrderRepository order)
        {
            _createEntity = createEntity;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _order = order;

        }

        [HttpGet("SingleOrder/{Id}")]
        public async Task<ActionResult<ActualOrderDTO>> GetASingleOrderById(int Id)
        {
            var orders = await _unitOfWork.Repository<ActualOrder>().GetFirstOrDefault(x=>x.Id == Id,"BillingAddress,ShippingAddress,OrderedProducts");
            if(orders is null) return BadRequest(new ApiErrorResponse(401,"This order does not exist!"));
            return Ok(_mapper.Map<ActualOrder,ActualOrderDTO>(orders));
        }

        [HttpPut("UpdateAnOrder")]
        public async Task<ActionResult<ActualOrderDTO>> AddTracking(UpdateTrackingDTO updateTrackingDTO)
        {
            var data = await _createEntity.OrderToAddTracking(updateTrackingDTO);
            return Ok(_mapper.Map<ActualOrder,ActualOrderDTO>(data)); 
        }

        [HttpPut("UpdateOrderReceivedInfo")]
        public async Task<ActionResult<ActualOrderDTO>> UpdateOrderReceivedDate(UpdateTrackingDTO updateTrackingDTO)
        {
            var data = await _createEntity.UpdateOrderReceviedInformation(updateTrackingDTO);
            return Ok(_mapper.Map<ActualOrder,ActualOrderDTO>(data)); 
        }

        [HttpGet]
        public async Task<ActionResult<PagerList<AllOrdersInformationDTO>>> GetAllOrdersAsync([FromQuery]PageParams pageParams)
        {
            //I have to implement Pagination on the client side!
            await _unitOfWork.Repository<OrderedProducts>().GetAll(null,null,"imageUrl");
            var order = await _unitOfWork.Repository<ActualOrder>().GetAllParams(pageParams,null,x=>x.OrderByDescending(x=>x.OrderDate),"BillingAddress,ShippingAddress,OrderedProducts");
            Response.AddPaginationHeader(order.CurrentPage,order.PageSize,order.TotalCount,order.TotalPages);
            return Ok(_mapper.Map<IEnumerable<ActualOrder>,IEnumerable<AllOrdersInformationDTO>>(order));
        }

        [HttpDelete("{Id}")]
        public async Task<ActionResult> DeleteOrder(int Id)
        {
            var info = await _createEntity.DeleteAnOrder(Id);
            if(info == 0) return new BadRequestObjectResult(new ApiValidationResponse{Errors = new [] {"There was a problem deleting the order!"}});
            return Ok("deleted!");
        }

        [HttpPost("inProcess")]
        public async Task OrderInProcess([FromBody]int id)
        {
            await _order.UpdateOrderStatus(id,StaticInfo.NotYet);
            await _unitOfWork.Complete();
        }

        [HttpPost("ready")]
        public async Task OrderReady(int id)
        {
            await _order.UpdateOrderStatus(id,StaticInfo.GoAhead);
            await _unitOfWork.Complete();
        }

    }
}