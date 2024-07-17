using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Models;
using Models.DTOs;

namespace CosmicStockapi.Auto
{
    public class AutoMapperProfiles:Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<ActualOrder,ActualOrderDTO>().ForPath(x=>x.OrderDate,s=>s.MapFrom(x=>x.DateInformation()))
                                                   .ForPath(x=>x.OrderReceived,s=>s.MapFrom(x=>x.OrderReceivedInformation()));                                 
            CreateMap<ActualOrder,AllOrdersInformationDTO>().ForPath(x=>x.OrderDate,s=>s.MapFrom(x=>x.DateInformation()))
                                                            .ForPath(x=>x.OrderReceived,s=>s.MapFrom(x=>x.OrderReceivedInformation()));
            CreateMap<WishedProducts,WishedProductDTO>();
            CreateMap<RegisterModel,RegisterDTO>();
            CreateMap<ForgotPasswordModel,ForgotPasswordDTO>();
            CreateMap<ResetPassword,ResetPasswordDTO>();
            CreateMap<UserAddressToSaveOrNot,UserAddressDTO>().ForPath(x=>x.State,s=>s.MapFrom(x=>x.State.Name)).ReverseMap();
            CreateMap<UserAddressToSaveOrNot,UserAddress>().ForPath(x=>x.State.Name,s=>s.MapFrom(x=>x.State.Name));
            CreateMap<UserAddressDTO,UserAddress>().ForPath(x=>x.State.Name,s=>s.MapFrom(x=>x.State));
            CreateMap<UserAddress,UserAddressDTO>().ForPath(x=>x.State,s=>s.MapFrom(x=>x.State.Name));
            CreateMap<OrderedProducts,OrderedProductsDTO>().ForPath(x=>x.imageUrl,s=>s.MapFrom(x=>x.picUrl()))
                                                           .ForPath(x=>x.TimeToKeepCancelButtonOn,s=>s.MapFrom(x=>x.FifteenMinutesPassed()))
                                                           .ForPath(x=>x.TimeToKeepRefundButtonOn,s=>s.MapFrom(x=>x.OverThirtyDaysPassed()));
            CreateMap<OrderedProductsDTO,OrderedProducts>().ForPath(x=>x.imageUrl,s=>s.Ignore());
            CreateMap<Category,CategoryDTO>();
            CreateMap<Brand,BrandDTO>();
            CreateMap<LoginModel,LoginDTO>();
            CreateMap<Photo,PhotoDTO>().ReverseMap();
            CreateMap<Photo,PhotoDTO>();
            CreateMap<ProductRating,ProductRatingDTO>();
            CreateMap<ProductDetails,ProductDetailsDTO>();
            CreateMap<ProductDetails,ProductDetailsDTO>().ReverseMap();
            CreateMap<Products,ProductDTO>().ForPath(x=>x.Brand,o=>o.MapFrom(x=>x.Brand.Name))
                                            .ForPath(x=>x.Category,o=>o.MapFrom(x=>x.Category.Name))
                                            .ForPath(x=>x.Ratings,o=>o.MapFrom(x=>x.Ratings))
                                            .ForPath(x=>x.Savings,o=>o.MapFrom(s=>s.savings()))
                                            .ForPath(x=>x.isAvailable,o=>o.MapFrom(s=>s.isAvailableToBuy()));
            CreateMap<RefundModel,RefundModelDTO>();
            CreateMap<RefundModel,RefundModelDTO>().ReverseMap();
                                            
            
        }
    }
}