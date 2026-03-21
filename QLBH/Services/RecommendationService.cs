using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using MyWebApp.Models;
using MyWebApp.Repository;

namespace MyWebApp.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _environment;
        private List<AssociationRuleModel> _rules;

        public RecommendationService(DataContext dataContext, IWebHostEnvironment environment)
        {
            _dataContext = dataContext;
            _environment = environment;
        }

        public async Task<List<ProductModel>> GetRecommendedProductsAsync(long productId)
        {
            try
            {
                // Load rules if not already loaded
                if (_rules == null)
                {
                    await LoadRulesAsync();
                }

                if (_rules == null || _rules.Count == 0)
                {
                    return new List<ProductModel>();
                }

                // Format product ID as 2-digit string (e.g., 1 -> "01", 10 -> "10")
                string productIdFormatted = productId.ToString("D2");

                // Find all rules where the consequent contains the current product ID
                var relevantRules = _rules
                    .Where(r => r.Consequent != null && r.Consequent.Contains(productIdFormatted))
                    .OrderByDescending(r => r.Confidence)
                    .ThenByDescending(r => r.Lift)
                    .ToList();

                if (relevantRules.Count == 0)
                {
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
                        }
                    }
                }

                // Convert string IDs to long and query database
                var productIds = recommendedProductIds
                    .Select(id => long.TryParse(id, out var parsedId) ? parsedId : (long?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .ToList();

                if (productIds.Count == 0)
                {
                    return new List<ProductModel>();
                }

                // Get products from database, excluding the current product
                var recommendedProducts = await _dataContext.Products
                    .Include(p => p.Category)
                    .Include(p => p.Brand)
                    .Where(p => productIds.Contains(p.Id) && p.Id != productId)
                    .Take(4) // Limit to 4 recommended products
                    .ToListAsync();

                return recommendedProducts;
            }
            catch (Exception)
            {
                // Return empty list on any error
                return new List<ProductModel>();
            }
        }

        public Task<IEnumerable<ProductModel>> GetRecommendedProductsAsync(object value)
        {
            throw new NotImplementedException();
        }

        private async Task LoadRulesAsync()
        {
            try
            {
                var rulesPath = Path.Combine(_environment.ContentRootPath, "Recomment.json");

                if (!File.Exists(rulesPath))
                {
                    _rules = new List<AssociationRuleModel>();
                    return;
                }

                var jsonContent = await File.ReadAllTextAsync(rulesPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                _rules = JsonSerializer.Deserialize<List<AssociationRuleModel>>(jsonContent, options);

                if (_rules == null)
                {
                    _rules = new List<AssociationRuleModel>();
                }
            }
            catch (Exception)
            {
                // Log exception if needed - for now just set empty list
                _rules = new List<AssociationRuleModel>();
            }
        }
    }
}

