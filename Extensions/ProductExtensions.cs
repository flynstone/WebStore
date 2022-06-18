using Api.Models;

namespace Api.Extensions
{
    public static class ProductExtensions
    {
        // Return Sorted IQueryable of type Product.
        public static IQueryable<Product> Sort(this IQueryable<Product> query, string orderBy)
        {
            // If orderBy is null or empty, sort list alphabetically.
            if (string.IsNullOrWhiteSpace(orderBy)) return query.OrderBy(p => p.Name);

            // Switch method to sort products.
            query = orderBy switch
            {
                // Order by price. (Ascending)
                "price" => query.OrderBy(p => p.Price),
                // Order by price. (Descending)
                "priceDesc" => query.OrderByDescending(p => p.Price),
                // Default case. (Alphabetical)
                _ => query.OrderBy(p => p.Name)
            };

            return query;
        }

        // Search IQueryable of type Product.
        public static IQueryable<Product> Search(this IQueryable<Product> query, string searchTerm)
        {
            // If searchTerm is null or empty return query.
            if (string.IsNullOrEmpty(searchTerm)) return query;

            // Store search term in lower case.
            var lowerCaseSearchTerm = searchTerm.Trim().ToLower();

            // Return matching result.
            return query.Where(p => p.Name.ToLower().Contains(lowerCaseSearchTerm));
        }

        // Filter IQueryable of type Product.
        public static IQueryable<Product> Filter(this IQueryable<Product> query, string brands, string types)
        {
            // Store new lists in varables.
            var brandList = new List<string>();
            var typeList = new List<string>();

            // If brandList is not empty, return range to list.
            if (!string.IsNullOrWhiteSpace(brands))
                brandList.AddRange(brands.ToLower().Split(",").ToList());

            // If typesList is not empty, return range to list.
            if (!string.IsNullOrWhiteSpace(types))
                typeList.AddRange(types.ToLower().Split(",").ToList());

            // Query the brand list.
            query = query.Where(p => brandList.Count == 0 || brandList.Contains(p.Brand.ToLower()));

            // Query the type list.
            query = query.Where(p => typeList.Count == 0 || typeList.Contains(p.Type.ToLower()));

            return query;
        }
    }
}
