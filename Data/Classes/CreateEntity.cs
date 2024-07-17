using AutoMapper;
using Microsoft.AspNetCore.Identity.UI.Services;
using Models.DTOs;
using System.Text;

namespace Data.Classes
{
    public class CreateEntity : ICreateEntity
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPhotoService _photoService;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        public CreateEntity(IUnitOfWork unitOfWork,IPhotoService photoService,IMapper mapper,IEmailSender emailSender)
        {
            _mapper = mapper;
            _photoService = photoService;
            _unitOfWork = unitOfWork;
            _emailSender = emailSender; 
        }

        public async Task<Brand> CreateBrand(BrandDTO brand)
        {
            var nuBrand = new Brand
            {
                Name = brand.Name
            };
            _unitOfWork.Repository<Brand>().Add(nuBrand);
            await _unitOfWork.Complete();
            return nuBrand;
        }

        public async Task<Category> CreateCategory(CategoryDTO category)
        {
            var nuCat = new Category
            {
                Name = category.Name
            };
            _unitOfWork.Repository<Category>().Add(nuCat);
            await _unitOfWork.Complete();
            return nuCat;
        }

        public async Task<WishedProducts> AddToWishList(int Id, string user)
        {
            var objToadd = await _unitOfWork.Repository<Products>().GetFirstOrDefault(x=>x.Id == Id,"Details,imageUrl,Brand,Category,Ratings");
            var picInfo = objToadd.imageUrl.FirstOrDefault(x =>x.IsMain);
            var wished = new WishedProducts
            {
                Name = objToadd.Name,
                price = objToadd.price,
                imageUrl = picInfo.PhotoUrl,
                User = user
            };
            var currentUser = await _unitOfWork.Repository<AppUser>().GetFirstOrDefault(x=>x.Email == user,"WishiList"); 
            var checkList = currentUser.WishiList.FirstOrDefault(x=>x.Name == objToadd.Name);
            if(checkList is not null) return null;
            if(objToadd is not null)
            {
               currentUser.WishiList.Add(wished);
            }
            await _unitOfWork.Complete();
            return wished;
        }

        public async Task RemoveItemFromWishList(int Id, string user)
        {
            var currentUser = await _unitOfWork.Repository<AppUser>().GetFirstOrDefault(x=>x.Email == user,"WishiList"); 
            var infoToDelete = currentUser.WishiList.FirstOrDefault(x=>x.Id == Id);
            _unitOfWork.Repository<WishedProducts>().Remove(infoToDelete);
            await _unitOfWork.Complete();
        }

        public async Task<Products> UpdateProduct(int Id, Post product)
        {
            var objToUpdate = await _unitOfWork.Repository<Products>().GetFirstOrDefault(x=>x.Id == Id,"Details");
            objToUpdate.Name = product.Name;
            objToUpdate.FeaturedName = product.FeaturedName;
            objToUpdate.Description = product.Description;
            objToUpdate.Details.ActualDetails = product.ActualDetails;
            objToUpdate.Details.ActualDetails1 = product.ActualDetails1;
            objToUpdate.Details.ActualDetails2 = product.ActualDetails2;
            objToUpdate.Details.ActualDetails3 = product.ActualDetails3;
            objToUpdate.Details.ActualDetails4 = product.ActualDetails4;
            objToUpdate.Details.ActualDetails5 = product.ActualDetails5;
            objToUpdate.Details.ActualDetails6 = product.ActualDetails6;
            objToUpdate.Details.ActualDetails7 = product.ActualDetails7;
            objToUpdate.Details.ActualDetails8 = product.ActualDetails8;
            objToUpdate.Details.ActualDetails9 = product.ActualDetails9;
            objToUpdate.AvailableAmount = product.AvailableAmount;
            objToUpdate.price = product.price;
            objToUpdate.isAvailable = product.isAvailable;
            objToUpdate.isFeatured = product.isFeatured;
            objToUpdate.isOnSale = product.isOnSale;

            if(product.Category is not null)
            {
                objToUpdate.Category = await _unitOfWork.Repository<Category>().GetFirstOrDefault(x=>x.Name == product.Category) ?? new Category {Name = product.Category}; 
            }
            if(product.Brand is not null)
            {
                objToUpdate.Brand = await _unitOfWork.Repository<Brand>().GetFirstOrDefault(x=>x.Name == product.Brand) ?? new Brand {Name = product.Brand}; 
            }
            if(product.file is not null)
            {
                foreach(var item in product.file)
                {
                    var oldpics = await _unitOfWork.Repository<Photo>().GetAll(x=>x.products.Id == Id);
                    foreach(var items in oldpics)
                    {
                        var picsToRemove = await _photoService.DeletePhotoAsync(items.PublicId);
                        _unitOfWork.Repository<Photo>().Remove(items);
                        await _unitOfWork.Complete();
                    }
                    
                    var pics = await _photoService.AddPhotoAsync(item);
                    if(pics is not null)
                    {
                        var nuPhoto = new Photo
                        {
                            PhotoUrl = pics.SecureUrl.AbsoluteUri,
                            PublicId = pics.PublicId,
                            products = objToUpdate
                        };
                        if(objToUpdate.imageUrl.Count == 0)
                        {
                            nuPhoto.IsMain = true;
                        }
                        objToUpdate.imageUrl.Add(nuPhoto);
                    }               
                }
            }
             _unitOfWork.Repository<Products>().Update(objToUpdate);
            await _unitOfWork.Complete();
            return objToUpdate;
        }

        public async Task<Products> CreateProduct(Post product)
        {
            if(!product.isAvailable.HasValue)
            {
                product.isAvailable = false;
            }
            if(!product.isFeatured.HasValue)
            {
                product.isFeatured = false;
            }
            if(!product.isOnSale.HasValue)
            {
                product.isOnSale = false;
            }
            if(product.Category == "undefined")
            {
                product.Category = "CosmicStock";
            }
            if(product.Brand == "undefined")
            {
                product.Brand = "CosmicStock";
            }
            var nuProd = new Products
            {
                Name = product.Name,
                FeaturedName = product.FeaturedName,
                Description = product.Description,
                MSRP = product.MSRP,
                price = product.price,
                isAvailable = product.isAvailable,
                isFeatured = product.isFeatured,
                isOnSale = product.isOnSale,
                imageUrl = new List<Photo>(),
                Category = await _unitOfWork.Repository<Category>().GetFirstOrDefault(x=>x.Name == product.Category) ?? new Category {Name = product.Category},
                Brand = await _unitOfWork.Repository<Brand>().GetFirstOrDefault(x=>x.Name == product.Brand) ?? new Brand {Name = product.Brand},
                TotalRate = 0
            };

            nuProd.Details = new ProductDetails
            {
                ActualDetails= product.ActualDetails,
                ActualDetails1= product.ActualDetails1,
                ActualDetails2= product.ActualDetails2,
                ActualDetails3= product.ActualDetails3,
                ActualDetails4= product.ActualDetails4,
                ActualDetails5= product.ActualDetails5,
                ActualDetails6= product.ActualDetails6,
                ActualDetails7= product.ActualDetails7,
                ActualDetails8= product.ActualDetails8,
                ActualDetails9= product.ActualDetails9
            };
            if(product.file != null )
            {
                foreach(var item in product.file)
                {
                    var pics = await _photoService.AddPhotoAsync(item);
                    if(pics is not null)
                    {
                        var nuPhoto = new Photo
                        {
                            PhotoUrl = pics.SecureUrl.AbsoluteUri,
                            PublicId = pics.PublicId,
                            products = nuProd
                        };
                        if(nuProd.imageUrl.Count == 0)
                        {
                            nuPhoto.IsMain = true;
                        }
                        nuProd.imageUrl.Add(nuPhoto);
                    }            
                }
            }
            _unitOfWork.Repository<Products>().Add(nuProd);
            await _unitOfWork.Complete();
            return nuProd;     
        }

        public async Task<int> DeleteAnAddress(int Id, string user)
        {
            var infoToDelete = await _unitOfWork.Repository<UserAddressToSaveOrNot>().GetFirstOrDefault(x=>x.AppUser == user && x.Id == Id);
            _unitOfWork.Repository<UserAddressToSaveOrNot>().Remove(infoToDelete);
            var saved = await _unitOfWork.Complete();
            return saved;
        }

        public async Task<UserAddressToSaveOrNot> CreateAddressToSaveOrNot(string user, UserAddress userAddress)
        {
            var nuAddressToSave = new UserAddressToSaveOrNot();
            var orderAddress = await _unitOfWork.Repository<UserAddress>().GetFirstOrDefault(x =>x.AppUser == user && x.AddressType == userAddress.AddressType);
            var existingAddToSave = await _unitOfWork.Repository<UserAddressToSaveOrNot>().GetAll(x =>x.AppUser == user);
            if(existingAddToSave.Count() != 0)
            {
                var shippingInfo = existingAddToSave.FirstOrDefault(x=>x.AddressType == userAddress.AddressType);
                if(shippingInfo is not null)
                {
                    shippingInfo.FirstName = userAddress.FirstName;
                    shippingInfo.LastName = userAddress.LastName;
                    shippingInfo.AddressLine1 = userAddress.AddressLine1;
                    shippingInfo.AddressLine2 = userAddress.AddressLine2;
                    shippingInfo.City = userAddress.City;
                    shippingInfo.State = await _unitOfWork.Repository<State>().GetFirstOrDefault(x=>x.Name == userAddress.State.Name);
                    shippingInfo.ZipCode = userAddress.ZipCode;
                    shippingInfo.AppUser = user;
                    shippingInfo.PhoneNumber = userAddress.PhoneNumber;
                    shippingInfo.AddressType = userAddress.AddressType;
                    shippingInfo.SaveAddressInfo = userAddress.SaveAddressInfo;
                    shippingInfo.ShippingIsBilling = userAddress.ShippingIsBilling;
                    _unitOfWork.Repository<UserAddressToSaveOrNot>().Update(shippingInfo);
                    if(orderAddress is not null)
                    {
                        if(orderAddress.SaveAddressInfo == false)
                        {
                            orderAddress.SaveAddressInfo = true;
                        }
                    }
                    await _unitOfWork.Complete();
                    return shippingInfo;
                }
            }
            nuAddressToSave.AddressLine1 = userAddress.AddressLine1;
            nuAddressToSave.AddressLine2 = userAddress.AddressLine2;
            nuAddressToSave.FirstName = userAddress.FirstName;
            nuAddressToSave.LastName = userAddress.LastName;
            nuAddressToSave.City = userAddress.City;
            nuAddressToSave.State = await _unitOfWork.Repository<State>().GetFirstOrDefault(x=>x.Name == userAddress.State.Name);
            nuAddressToSave.ZipCode = userAddress.ZipCode;
            nuAddressToSave.PhoneNumber = userAddress.PhoneNumber;
            nuAddressToSave.AppUser = userAddress.AppUser;
            nuAddressToSave.AddressType = userAddress.AddressType;
            nuAddressToSave.SaveAddressInfo = userAddress.SaveAddressInfo;
            nuAddressToSave.ShippingIsBilling = userAddress.ShippingIsBilling;
            _unitOfWork.Repository<UserAddressToSaveOrNot>().Add(nuAddressToSave);
            if(orderAddress is not null)
            {
                if(orderAddress.SaveAddressInfo == false)
                {
                    orderAddress.SaveAddressInfo = true;
                }
                await _unitOfWork.Complete();
                return nuAddressToSave;
            }
            await _unitOfWork.Complete();
            return nuAddressToSave;
        }
          
        public async Task<UserAddressToSaveOrNot> CreateUserAddress(string user, UserAddressDTO userAddress)
        {
            var existingUserAddress = await _unitOfWork.Repository<UserAddressToSaveOrNot>().GetFirstOrDefault(x =>x.AppUser == user && x.AddressType == userAddress.AddressType);
            
            if(existingUserAddress == null)
            {
                var nuAddress = new UserAddressToSaveOrNot
                {
                    FirstName = userAddress.FirstName,
                    LastName = userAddress.LastName,
                    AddressLine1 = userAddress.AddressLine1,
                    AddressLine2 = userAddress.AddressLine2,
                    City = userAddress.City,
                    State = await _unitOfWork.Repository<State>().GetFirstOrDefault(x=>x.Name == userAddress.State),
                    ZipCode = userAddress.ZipCode,
                    AppUser = user,
                    PhoneNumber = userAddress.PhoneNumber,
                    AddressType = userAddress.AddressType,
                    SaveAddressInfo = userAddress.SaveAddressInfo
                    
                };
                _unitOfWork.Repository<UserAddressToSaveOrNot>().Add(nuAddress);
                await _unitOfWork.Complete();
                return nuAddress;
            } 
            else
            {
                    existingUserAddress.AddressLine1 = userAddress.AddressLine1;
                    existingUserAddress.AddressLine2 = userAddress.AddressLine2;
                    existingUserAddress.FirstName = userAddress.FirstName;
                    existingUserAddress.LastName = userAddress.LastName;
                    existingUserAddress.City = userAddress.City;
                    existingUserAddress.State = await _unitOfWork.Repository<State>().GetFirstOrDefault(x=>x.Name == userAddress.State);
                    existingUserAddress.ZipCode = userAddress.ZipCode;
                    existingUserAddress.PhoneNumber = userAddress.PhoneNumber;
                    existingUserAddress.AppUser = user;
                    existingUserAddress.AddressType = userAddress.AddressType;
                    existingUserAddress.SaveAddressInfo = userAddress.SaveAddressInfo;
                    existingUserAddress.ShippingIsBilling = userAddress.ShippingIsBilling;

                _unitOfWork.Repository<UserAddressToSaveOrNot>().Update(existingUserAddress);
                await _unitOfWork.Complete();
                return existingUserAddress;
            }
        }

        public async Task<UserAddress> UpdateUserOrderAddress(UserAddress userAddress, UserAddress UpdatedInfo)
        {
            userAddress.FirstName = UpdatedInfo.FirstName;
            userAddress.LastName = UpdatedInfo.LastName;
            userAddress.AddressLine1 = UpdatedInfo.AddressLine1;
            userAddress.AddressLine2 = UpdatedInfo.AddressLine2;
            userAddress.City = UpdatedInfo.City;
            userAddress.State = await _unitOfWork.Repository<State>().GetFirstOrDefault(x=>x.Name == UpdatedInfo.State.Name);
            userAddress.ZipCode = UpdatedInfo.ZipCode;
            userAddress.PhoneNumber = UpdatedInfo.PhoneNumber;
            userAddress.AddressType = UpdatedInfo.AddressType;
            userAddress.AppUser = UpdatedInfo.AppUser;
            userAddress.SaveAddressInfo = UpdatedInfo.SaveAddressInfo;
            userAddress.ShippingIsBilling = UpdatedInfo.ShippingIsBilling;
            _unitOfWork.Repository<UserAddress>().Update(userAddress);

            return await Task.FromResult(userAddress);
        }

        public async Task<int> DeleteProduct(int id)
        {
            var objToDelete = await _unitOfWork.Repository<Products>().GetFirstOrDefault(x=>x.Id == id,"Details,imageUrl,Brand,Category,Ratings");
            if(objToDelete is not null)
            {
                if(objToDelete.imageUrl.Count > 0)
                {
                    foreach(var items in objToDelete.imageUrl)
                    {
                        var picsToRemove = await _photoService.DeletePhotoAsync(items.PublicId);
                        _unitOfWork.Repository<Photo>().Remove(items);
                    }
                }  
                _unitOfWork.Repository<Products>().Remove(objToDelete);
            }
            var info = await _unitOfWork.Complete();
            return (info);
        }

        //Send an email to the user with the tracking number!
        public async Task<ActualOrder> OrderToAddTracking(UpdateTrackingDTO updateTrackingDTO)
        {
            var info = await _unitOfWork.Repository<ActualOrder>().GetFirstOrDefault(x=>x.Id == updateTrackingDTO.Id,"OrderedProducts,ShippingAddress,BillingAddress");
            info.TrackingNumber = updateTrackingDTO.TrackingNumber;
            _unitOfWork.Repository<ActualOrder>().Update(info);
            await _unitOfWork.Complete();
            var url = "https://www.stamps.com/tracking/";
            var EmailBody = await UpdatedEmailToSend(info,info.Subtotal,info.Tax,url);
            await _emailSender.SendEmailAsync(info.Email,"Track Your Order",EmailBody);
            return info;
        } 
        //Create a method in the Admin Controller to update the order Received information
        public async Task<ActualOrder> UpdateOrderReceviedInformation(UpdateTrackingDTO updateTrackingDTO)
        {
            var info = await _unitOfWork.Repository<ActualOrder>().GetFirstOrDefault(x=>x.Id == updateTrackingDTO.Id,"OrderedProducts");
            info.OrderReceived = updateTrackingDTO.OrderReceivedInfo;
            foreach(var item in info.OrderedProducts)
            {
                item.TimeOrderWasReceived = updateTrackingDTO.OrderReceivedInfo;
            }
            info.OrderStatus = StaticInfo.Received;
            _unitOfWork.Repository<ActualOrder>().Update(info);
            await _unitOfWork.Complete();
            return info;
        }

        public async Task<String> RefundOrderedItem(int Id,RefundModelDTO refundModelDTO,string user)
        {
            if(!refundModelDTO.CancelRefund.HasValue)
            {
                refundModelDTO.CancelRefund = false;
            }
            var checkRefund = await _unitOfWork.Repository<RefundModel>().GetFirstOrDefault(x=>x.ProductId  == refundModelDTO.ProductId && x.OrderId == Id && x.currentUser == user );
            if(checkRefund is not null)
            {
                return "You already requested a refund for this item!";
            }
            var orderWithitemToBeRefunded = await _unitOfWork.Repository<ActualOrder>().GetFirstOrDefault(x=>x.Id == Id,"OrderedProducts");

            if(orderWithitemToBeRefunded is not null)
            {
                var actualItemsToBeRefunded = orderWithitemToBeRefunded.OrderedProducts.FirstOrDefault(x=>x.ItemId == refundModelDTO.ProductId);

                if(actualItemsToBeRefunded is not null)
                {
                    if(refundModelDTO.amount > actualItemsToBeRefunded.Amount)
                    {
                        refundModelDTO.amount = actualItemsToBeRefunded.Amount;
                    }
                    var nuRefundForm = new RefundModel
                    {
                        FirstName = refundModelDTO.FirstName,
                        LastName = refundModelDTO.LastName,
                        OrderId = Id,
                        ProductId = actualItemsToBeRefunded.ItemId,
                        ProductName = actualItemsToBeRefunded.Name,
                        amount = refundModelDTO.amount,
                        currentUser = user,
                        ReasonForRefund = refundModelDTO.ReasonForRefund,
                        CancelRefund = refundModelDTO.CancelRefund,
                        IsRefundAGo = refundModelDTO.IsRefundAGo,
                        AdditionalNotes = refundModelDTO.AdditionalNotes
                    };
                    var dateInfo = orderWithitemToBeRefunded.OrderReceived.AddDays(30);
                    if(nuRefundForm.RequestDate > dateInfo)
                    {
                        nuRefundForm.IsRefundAGo = false;
                        return "You do not qualify for a refund. See refund policy for details!";
                    } 
                    else
                    {
                        nuRefundForm.IsRefundAGo = true;
                    }
                    _unitOfWork.Repository<RefundModel>().Add(nuRefundForm);
                    actualItemsToBeRefunded.RequestRefund = nuRefundForm.IsRefundAGo;
                    await _unitOfWork.Complete();

                 
                    var orderCount =  orderWithitemToBeRefunded.OrderedProducts.Count();
                    var refundedOrders = orderWithitemToBeRefunded.OrderedProducts.Where(x=>x.RequestRefund == true).Count();
                    if(orderCount == refundedOrders)
                    {
                        orderWithitemToBeRefunded.CancelOrder = true;
                        orderWithitemToBeRefunded.OrderStatus = StaticInfo.Refund;
                        await _unitOfWork.Complete();
                    }
             

                    var amountToRefund = actualItemsToBeRefunded.price * refundModelDTO.amount;                   
                    //Notify the user you got their refund request and you will send them an email in reguards to the item they want to return.
                    string text = $"<div style=\"text-align: center; width:100%; padding:16px;\"><div style=\"font-family: Arial, Helvetica, sans-serif; font-size:16px;text-align: center;border-style:solid; padding:16px; width:60%; margin:auto; border-width:.1px; border-color:rgb(0, 0, 0,0.1);\"><h2 style=\"margin-bottom:16px;\">Refund Request</h2><p style=\"margin-bottom: 16px;\"> Hello {refundModelDTO.FirstName}. A refund amount of {amountToRefund} will be returned to the account you provided once you send back the {actualItemsToBeRefunded.Name}. It can take up to 3 to 7 days for refunds to be processed.</p></div>";
                    text = "<html lang=\"en\"><body>" + Environment.NewLine + text + Environment.NewLine + "</body></html>";
                    await _emailSender.SendEmailAsync(nuRefundForm.currentUser,"Refund Confirmation!",text);
                    return nuRefundForm.IsRefundAGo.ToString();
                }
            }
            //If you reach here, you made a big mistake!
            return null;
        }
        
        public async Task<Object> CancelRefundItemOrOrder(int Id,int orderId)
        {
            var data = await _unitOfWork.Repository<RefundModel>().GetFirstOrDefault(x=>x.Id == Id);
            if(data is not null)
            {
                var orderData = await _unitOfWork.Repository<OrderedProducts>().GetFirstOrDefault(x=>x.Id == orderId);
                if(orderData is not null)
                {
                    orderData.RequestRefund = false;
                    data.IsRefundAGo = false;
                    data.CancelRefund = true;
                    _unitOfWork.Repository<RefundModel>().Remove(data);
                    await _unitOfWork.Complete();
                }
                return orderData;
            }
            return null;
        }

        public async Task ActualCancel(int Id, int ItemId)
        {
            var orderWithItemToCancel = await _unitOfWork.Repository<ActualOrder>().GetFirstOrDefault(x =>x.Id == Id,"OrderedProducts");
            if(orderWithItemToCancel is not null)
            {
                var actualItemToCancel = orderWithItemToCancel.OrderedProducts.FirstOrDefault(x=>x.ActualOrderId == orderWithItemToCancel.Id && x.ItemId == ItemId);
                if(actualItemToCancel is not null)
                {
                    actualItemToCancel.CancelItem = true;
                    actualItemToCancel.TimeToKeepCancelButtonOn = false;
                    await _unitOfWork.Complete();

                }
                if(orderWithItemToCancel.OrderedProducts.Count() > 1)
                {
                    var information = orderWithItemToCancel.OrderedProducts.Count();
                    var canceleditemsInformation = orderWithItemToCancel.OrderedProducts.Where(x=>x.CancelItem == true).Count();
                    if(information == canceleditemsInformation)
                    {
                        orderWithItemToCancel.CancelOrder = true;
                        orderWithItemToCancel.OrderStatus = StaticInfo.Canceled;
                        await _unitOfWork.Complete();
                    }
                }
            }
        }

        public async Task<int> DeleteAnOrder(int Id)
        {
            var orderedItems = await _unitOfWork.Repository<OrderedProducts>().GetAll(x =>x.ActualOrderId == Id,null,"imageUrl");
            
            if(orderedItems is not null)
            {
                foreach(var item in orderedItems)
                {
                    if(item.imageUrl.Count > 0)
                    {
                        _unitOfWork.Repository<PhotosForOrderedProducts>().RemoveRange(item.imageUrl);
                    }
                }
                 _unitOfWork.Repository<OrderedProducts>().RemoveRange(orderedItems);
            }
            var objToDelete = await _unitOfWork.Repository<ActualOrder>().GetFirstOrDefault(x=>x.Id == Id);
            if(objToDelete is not null)
            {
                //Check if this work
                _unitOfWork.Repository<ActualOrder>().Remove(objToDelete);
                var info = await _unitOfWork.Complete();
                var orderAddressInfo = await _unitOfWork.Repository<UserAddress>().GetAll(x=>x.AppUser == objToDelete.Email);
                _unitOfWork.Repository<UserAddress>().RemoveRange(orderAddressInfo);

                await _unitOfWork.Complete();
                return info;
            }
            return 0;
        }

        public async Task<string> UpdatedEmailToSend(ActualOrder actualOrder, Decimal subTotal, Decimal tax,string url)
        {
            var body = new StringBuilder();
            body.Append("<html lang=\"en\">");
            body.Append("<div style=\"width: 100%;\">");
            body.Append("<div style=\"text-align: center;  width: 355px; margin: auto; border: 1px solid #0000001c; border-radius: 5px;\">");
            body.Append("<div style=\"line-height: 24px; background-color: #9ACD32; padding: 16px;\">");
            body.Append("<h4>Your Order Has Been Shipped!</h4>");
            body.Append("<p>Again, thank you for your order!</p>");
            body.Append("<p>You can track your purchase with the tracking number below. Your package is being shipped with usps!</p>");
            body.Append($"<p>{actualOrder.TrackingNumber}</p>");
            body.Append($"<button style=\"margin-top: 16px; cursor: pointer; margin-bottom: 16px; padding: 5px; background-color: aqua; color: #ffea01; border-radius: 5px; border: none; text-shadow: 2px 2px 5px #00000080; -webkit-text-stroke-width: 1px;\" ><a href=\"{url}\" style=\"text-decoration: none;\"> Track your package </a></button>");
            body.Append("</div>");
            body.Append("<div style=\"padding-left: 16px; padding-right: 16px;\">");
            body.Append("<h4 style=\"margin-top: 16px; margin-bottom: 16px;\">Order Summary</h4>");
            body.Append("<div style=\"width: 100%; margin: auto; border: 1px solid #00000080;\">");
            body.Append("<table style=\"width: 100%; background-color: aqua;\">");
            body.Append("<tr><th style=\"width: 40%;\">Product Name</th><th style=\"width: 40%;\">Price</th><th style=\"width: 40%;\">QTY</th></tr>");
            foreach(var item in actualOrder.OrderedProducts)
            body.Append($"<tr><td style=\"border: 1px solid rgba(0, 0, 0, 0.5);\">{item.Name}</td><td style=\"border: 1px solid rgba(0, 0, 0, 0.5); text-align: right;\">${item.price}</td><td style=\"border: 1px solid rgba(0, 0, 0, 0.5); text-align: right;\">{item.Amount}</td></tr>");
            body.Append("</table>");
            body.Append("</div>");
            body.Append("<h4 style=\"margin-top: 16px; margin-bottom: 16px;\">Order Total</h4>");
            body.Append("<div style=\"width: 100%; display: flex;\">");
            body.Append("<div style=\"width: 50%; text-align: left;\">");
            body.Append("<p>Subtotal</p>");
            body.Append("<p>Shipping</p>");
            body.Append("<p>Tax</p>");
            body.Append("</div>");
            body.Append("<div style=\"width: 50%; text-align: right;\">");
            body.Append($"<p>${subTotal}</p>");
            body.Append($"<p>${actualOrder.ShippingHandlingPrice}</p>");
            body.Append($"<p>${tax}</p>");
            body.Append("</div>");
            body.Append("</div>");
            body.Append("<hr style=\"margin-top: 16px; margin-bottom: 16px;\">");
            body.Append("<div style=\"width: 100%; display: flex;\">");
            body.Append("<div style=\"width: 50%; text-align: left;\">");
            body.Append("<p>Total</p>");
            body.Append("</div>");
            body.Append("<div style=\"width: 50%; text-align: right;\">");
            body.Append($"<p><strong>${actualOrder.TrueTotal}</strong></p>");
            body.Append("</div>");
            body.Append("</div>");
            body.Append("<h4 style=\"margin-top: 16px; margin-bottom: 16px;\">Billing and Shipping</h4>");
            body.Append("<div style=\"display: flex; width: 100%; margin: auto;\">");
            body.Append("<div style=\"text-align: left; width: 50%;\">");
            body.Append("<div style=\"text-align: center;\">");
            body.Append("<h4>Billing</h4>");
            body.Append("</div>");
            body.Append($"<p>{actualOrder.BillingAddress.FirstName} {actualOrder.BillingAddress.LastName}</p>");
            body.Append($"<p>{actualOrder.BillingAddress.AddressLine1}</p>");
            body.Append($"<p>{actualOrder.BillingAddress.AddressLine2}</p>");
            body.Append($"<p>{actualOrder.BillingAddress.City} , {actualOrder.BillingAddress.State} </p>");
            body.Append($"<p>{actualOrder.BillingAddress.ZipCode}</p>");
            body.Append($"<p>{actualOrder.BillingAddress.PhoneNumber}</p>");
            body.Append("</div>");
            body.Append("<div style=\"text-align: left; width: 50%;\">");
            body.Append("<div style=\"text-align: center;\">");
            body.Append("<h4>Shipping</h4>");
            body.Append("</div>");
            body.Append($"<p>{actualOrder.ShippingAddress.FirstName} {actualOrder.ShippingAddress.LastName}</p>");
            body.Append($"<p>{actualOrder.ShippingAddress.AddressLine1}</p>");
            body.Append($"<p>{actualOrder.ShippingAddress.AddressLine2}</p>");
            body.Append($"<p>{actualOrder.ShippingAddress.City} , {actualOrder.ShippingAddress.State} </p>");
            body.Append($"<p>{actualOrder.ShippingAddress.ZipCode}</p>");
            body.Append($"<p>{actualOrder.ShippingAddress.PhoneNumber}</p>");
            body.Append("</div>");
            body.Append("</div>");
            body.Append("<button style=\"margin-top: 16px; cursor: pointer; margin-bottom: 16px; padding: 5px; background-color: aqua; color: #ffea01; border-radius: 5px; border: none; text-shadow: 2px 2px 5px #00000080; -webkit-text-stroke-width: 1px;\">View Order</button>");
            body.Append("</div>");
            body.Append("</div>");
            body.Append("</div>");
            body.Append("</html>");

            return await Task.FromResult(body.ToString());
        }
    }
}