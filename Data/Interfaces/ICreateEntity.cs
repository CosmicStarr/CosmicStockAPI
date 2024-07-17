using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models.DTOs;

namespace Data.Interfaces
{
    public interface ICreateEntity
    {
        Task<Products> CreateProduct(Post product);
        Task<Brand> CreateBrand(BrandDTO brand);
        Task<Category> CreateCategory(CategoryDTO categoryDTO);
        Task<WishedProducts> AddToWishList(int Id, string user);
        Task RemoveItemFromWishList(int Id, string user);
        Task<int> DeleteAnAddress(int Id, string user);
        Task<UserAddressToSaveOrNot> CreateUserAddress(string user,UserAddressDTO userAddress);
        Task<UserAddressToSaveOrNot> CreateAddressToSaveOrNot(string user, UserAddress userAddress);
        Task<UserAddress> UpdateUserOrderAddress(UserAddress userAddress, UserAddress UpdatedInfo);
        Task<Products> UpdateProduct(int Id, Post product);
        Task<ActualOrder> OrderToAddTracking(UpdateTrackingDTO updateTrackingDTO);
        Task<ActualOrder> UpdateOrderReceviedInformation(UpdateTrackingDTO updateTrackingDTO);
        Task<String> RefundOrderedItem(int Id,RefundModelDTO refundModelDTO,string user);
        Task<Object> CancelRefundItemOrOrder(int Id,int orderId);
        Task ActualCancel(int Id, int ItemId);
        Task<int> DeleteAnOrder(int Id);
        Task<int> DeleteProduct(int id);
    }
}