using Api.RequestHelpers;
using System.Text.Json;

namespace Api.Extensions
{
    public static class HttpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse response, MetaData metaData)
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            // Build HTTP response header.
            response.Headers.Add("Pagination", JsonSerializer.Serialize(metaData, options));
            
            // Allow client to have access to header.
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }
    }
}
