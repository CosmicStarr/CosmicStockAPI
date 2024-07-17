using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Data.Util;

namespace CosmicStockapi.Extension
{
    public static class HttpExtension
    {
        public static void AddPaginationHeader(this HttpResponse response,int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            var pagingHeader = new PagerHeader(currentPage,itemsPerPage,totalItems,totalPages);
            // var options = new JsonSerializerOptions
            // {
            //     PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            // };
            response.Headers.Add("Pagination", JsonSerializer.Serialize(pagingHeader));
            response.Headers.Add("Access-Control-Expose-Headers","Pagination");
        }
    }
}