using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Classes
{
    public class PostRating : IPostRating
    {
        private readonly IUnitOfWork _unitOfWork;
        public PostRating(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork; 
        }
        public async Task<ProductRating> Rating(string user, int id, int rating)
        {
            var prod = await _unitOfWork.Repository<Products>().GetFirstOrDefault(x=>x.Id == id,"Ratings");
            //existing Rate that belongs to the user
            var existingRate = prod.Ratings.FirstOrDefault(x=>x.User == user);
            if(existingRate == null)
            {
                //new Rating
                var rateInfo = await TotalRating(id);
                var nuRatingObj = new ProductRating();
                nuRatingObj.ActualRating = rating;
                nuRatingObj.User = user;
                if(rateInfo == 0)
                {
                    prod.TotalRate = rating;
                }
                else
                {
                    prod.TotalRate = rateInfo + rating;
                }
                prod.Ratings.Add(nuRatingObj);
                _unitOfWork.Repository<ProductRating>().Add(nuRatingObj);
                await _unitOfWork.Complete();
                return nuRatingObj;
            }
            if(existingRate.ActualRating > rating)
            {
                existingRate.ActualRating = rating;
                await _unitOfWork.Complete();
            }
            if(existingRate.ActualRating < rating)
            {
                existingRate.ActualRating = rating;
                await _unitOfWork.Complete();
            }
            prod.TotalRate = await TotalRating(id);
            await _unitOfWork.Complete();
            return existingRate;
        }

        private async Task<int> TotalRating(int Id)
        {
            var info = await _unitOfWork.Repository<ProductRating>().GetAll(x=>x.ProductsId == Id);
            return info.Sum(x =>x.ActualRating);
        }
    }
}