using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyWebApp.Models;
using MyWebApp.Repository;

namespace MyWebApp.Services
{
    public class HomeRecommendationService : HRecommendationService
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<HomeRecommendationService> _logger;
        private List<AssociationRuleModel> _rules;

        public HomeRecommendationService(DataContext dataContext, IWebHostEnvironment environment, ILogger<HomeRecommendationService> logger)
        {
            _dataContext = dataContext;
            _environment = environment;
            _logger = logger;
        }

        // Method cũ - recommendation dựa vào product ID
        public async Task<List<ProductModel>> GetRecommendedProductsAsync(long productId)
        {
            try
            {
                _logger.LogInformation($"DEBUG: GetRecommendedProductsAsync called with productId: {productId}");

                // Load rules if not already loaded
                if (_rules == null)
                {
                    await LoadRulesAsync();
                }

                if (_rules == null || _rules.Count == 0)
                {
                    _logger.LogWarning("DEBUG: No rules loaded");
                    return new List<ProductModel>();
                }

                _logger.LogInformation($"DEBUG: Rules count: {_rules.Count}");

                // Format product ID as 2-digit string (e.g., 1 -> "01", 10 -> "10")
                string productIdFormatted = productId.ToString("D2");
                _logger.LogInformation($"DEBUG: Formatted product ID: {productIdFormatted}");

                // Find all rules where the consequent contains the current product ID
                var relevantRules = _rules
                    .Where(r => r.Consequent != null && r.Consequent.Contains(productIdFormatted))
                    .OrderByDescending(r => r.Confidence)
                    .ThenByDescending(r => r.Lift)
                    .ToList();

                _logger.LogInformation($"DEBUG: Relevant rules count: {relevantRules.Count}");

                if (relevantRules.Count == 0)
                {
                    _logger.LogWarning($"DEBUG: No relevant rules found for product {productIdFormatted}");
                    return new List<ProductModel>();
                }

                // Collect all antecedent product IDs from relevant rules
                var recommendedProductIds = new HashSet<string>();
                foreach (var rule in relevantRules)
                {
                    if (rule.Antecedent != null)
                    {
                        foreach (var antecedentId in rule.Antecedent)
                        {
                            recommendedProductIds.Add(antecedentId);
                            _logger.LogInformation($"DEBUG: Added antecedent ID: {antecedentId}");
                        }
                    }
                }

                _logger.LogInformation($"DEBUG: Total recommended product IDs: {recommendedProductIds.Count}");

                // Convert string IDs to long and query database
                var productIds = recommendedProductIds
                    .Select(id => long.TryParse(id, out var parsedId) ? parsedId : (long?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .ToList();

                _logger.LogInformation($"DEBUG: Parsed product IDs count: {productIds.Count}");

                if (productIds.Count == 0)
                {
                    _logger.LogWarning("DEBUG: No product IDs to fetch");
                    return new List<ProductModel>();
                }

                // Get products from database, excluding the current product
                var recommendedProducts = await _dataContext.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Where(p => productIds.Contains(p.Id) && p.Id != productId)
                    .Take(4)
                    .ToListAsync();

                _logger.LogInformation($"DEBUG: Final recommended products count: {recommendedProducts.Count}");
                foreach (var product in recommendedProducts)
                {
                    _logger.LogInformation($"DEBUG: Recommended product - ID: {product.Id}, Name: {product.Name}");
                }

                return recommendedProducts;
            }
            catch (Exception ex)
            {
                _logger.LogError($"DEBUG: Exception in GetRecommendedProductsAsync: {ex.Message}");
                return new List<ProductModel>();
            }
        }

        // Method mới - recommendation dựa vào user (từ lịch sử mua hàng)
        public async Task<List<ProductModel>> GetRecommendedProductsByUserAsync(string userName)
        {
            try
            {
                _logger.LogInformation($"DEBUG: GetRecommendedProductsByUserAsync called for user: {userName}");

                if (string.IsNullOrEmpty(userName))
                {
                    _logger.LogWarning("DEBUG: Username is empty");
                    return new List<ProductModel>();
                }

                // Lấy tất cả OrderDetails của user để debug
                var allOrderDetails = await _dataContext.OrderDetails
                    .AsNoTracking()
                    .Where(od => od.UserName == userName)
                    .OrderByDescending(od => od.OrderId)
                    .ToListAsync();

                _logger.LogInformation($"DEBUG: Total OrderDetails found for user {userName}: {allOrderDetails.Count}");
                foreach (var od in allOrderDetails)
                {
                    _logger.LogInformation($"DEBUG: OrderDetail - OrderId: {od.OrderId}, ProductId: {od.ProductId}, UserName: {od.UserName}");
                }

                // Lấy sản phẩm từ order mới nhất (OrderId lớn nhất)
                var lastPurchasedProduct = allOrderDetails
                    .FirstOrDefault()?.ProductId;

                if (lastPurchasedProduct == null || lastPurchasedProduct == 0)
                {
                    _logger.LogWarning($"DEBUG: User {userName} has no purchase history or last purchased product is 0");
                    return new List<ProductModel>();
                }

                _logger.LogInformation($"DEBUG: Last purchased product ID: {lastPurchasedProduct}");

                // Sử dụng logic recommendation dựa vào sản phẩm từ lịch sử mua hàng
                return await GetRecommendedProductsAsync(lastPurchasedProduct.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DEBUG: Exception in GetRecommendedProductsByUserAsync: {ex.Message}, StackTrace: {ex.StackTrace}");
                return new List<ProductModel>();
            }
        }

        private async Task LoadRulesAsync()
        {
            try
            {
                var rulesPath = Path.Combine(_environment.ContentRootPath, "Recomment.json");
                _logger.LogInformation($"DEBUG: Rules path: {rulesPath}");

                if (!File.Exists(rulesPath))
                {
                    _logger.LogWarning("DEBUG: Rules file does not exist");
                    _rules = new List<AssociationRuleModel>();
                    return;
                }

                var jsonContent = await File.ReadAllTextAsync(rulesPath);
                _logger.LogInformation($"DEBUG: Rules file loaded, content length: {jsonContent.Length}");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                _rules = JsonSerializer.Deserialize<List<AssociationRuleModel>>(jsonContent, options);

                if (_rules == null)
                {
                    _logger.LogWarning("DEBUG: Rules deserialization returned null");
                    _rules = new List<AssociationRuleModel>();
                }
                else
                {
                    _logger.LogInformation($"DEBUG: Rules loaded successfully, count: {_rules.Count}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"DEBUG: Exception in LoadRulesAsync: {ex.Message}");
                _rules = new List<AssociationRuleModel>();
            }
        }
    }
}