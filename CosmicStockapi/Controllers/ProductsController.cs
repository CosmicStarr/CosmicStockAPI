using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CosmicStockapi.ErrorHandling;
using CosmicStockapi.helper;
using Data.Interfaces;
using Data.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.DTOs;

namespace CosmicStockapi.Controllers
{
    public class ProductsController:BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPostRating _rating;
        public ProductsController(IUnitOfWork unitOfWork,IMapper mapper,IPostRating rating)
        {
            _mapper = mapper;
            _rating = rating;
            _unitOfWork = unitOfWork;   
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAllProducts()
        {
            var Data = await _unitOfWork.Repository<Products>().GetAll(null,x=>x.OrderBy(x=>x.Name),"Details,imageUrl,Brand,Category,Ratings");
            return Ok(_mapper.Map<IEnumerable<Products>,IEnumerable<ProductDTO>>(Data)); 
        }

        

        [CachedInformation(60)]
        [HttpGet("{Id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int Id)
        {
            var Data = await _unitOfWork.Repository<Products>().GetFirstOrDefault(x=>x.Id == Id,"Details,imageUrl,Brand,Category,Ratings"); 
            return Ok(_mapper.Map<Products,ProductDTO>(Data));
        }

        [CachedInformation(60)]
        [HttpGet("Brands")]
        public async Task<ActionResult<IEnumerable<BrandDTO>>> GetAllBrands()
        {
            var Data = await _unitOfWork.Repository<Brand>().GetAll(null,x=>x.OrderBy(x=>x.Name));
            return Ok(_mapper.Map<IEnumerable<Brand>,IEnumerable<BrandDTO>>(Data));
        }

        [CachedInformation(60)]
        [HttpGet("Category")]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAllCategory()
        {
            var Data = await _unitOfWork.Repository<Category>().GetAll(null,x=>x.OrderBy(x=>x.Name));
            return Ok(_mapper.Map<IEnumerable<Category>,IEnumerable<CategoryDTO>>(Data));
        }
        
        [HttpPost("Rating/{Id}")]
        [Authorize]
        public async Task<ActionResult<ProductRatingDTO>> PostRating(int Id, [FromBody]int rate)
        {
            var currentUser = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            if(currentUser == null)
            {
                return BadRequest(new ApiErrorResponse(401));
            }
            var info = await _rating.Rating(currentUser,Id,rate);
            var mappedInfo = _mapper.Map<ProductRating,ProductRatingDTO>(info);
            return Ok(mappedInfo);
        }
        
        [HttpGet("GetRatings/{Id}")]
        public async Task<ActionResult<ProductRatingDTO>> GetRatingForProduct(int Id)
        {
            var currentUser = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            if(currentUser is not null)
            {
                var userInfo = await _unitOfWork.Repository<ProductRating>().GetFirstOrDefault(x=>x.ProductsId == Id && x.User == currentUser);
                return Ok(_mapper.Map<ProductRating,ProductRatingDTO>(userInfo));
            }
            return Ok();
        }
    }
}