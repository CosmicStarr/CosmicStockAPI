using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Interfaces
{
    public interface IPostRating
    {
        Task<ProductRating> Rating(string user, int id, int rating);
    }
}