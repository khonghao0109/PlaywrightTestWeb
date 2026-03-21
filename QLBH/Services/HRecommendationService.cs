using MyWebApp.Models;

namespace MyWebApp.Services
{
    public interface HRecommendationService
    {
        Task<List<ProductModel>> GetRecommendedProductsAsync(long productId);
        Task<List<ProductModel>> GetRecommendedProductsByUserAsync(string userName);
    }
}