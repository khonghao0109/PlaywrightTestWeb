using MyWebApp.Models;
using System.Text.RegularExpressions;

namespace MyWebApp.Services
{
    public class ProductSearchService
    {
        private readonly List<ProductModel> _products;

        public ProductSearchService(List<ProductModel> products)
        {
            _products = products;
        }

        public List<ProductModel> Search(string keyword, int top = 12)
        {
            if (string.IsNullOrWhiteSpace(keyword) || !_products.Any())
                return new List<ProductModel>();

            // Normalize keyword
            var normalizedKeyword = NormalizeText(keyword).ToLower();
            var keywordTokens = normalizedKeyword.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (!keywordTokens.Any())
                return new List<ProductModel>();

            var results = _products
                .Select(p => new
                {
                    Product = p,
                    Score = CalculateScore(p, normalizedKeyword, keywordTokens)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Take(top)
                .Select(x => x.Product)             
                .ToList();

            return results;
        }

        private double CalculateScore(ProductModel product, string normalizedKeyword, string[] keywordTokens)
        {
            double score = 0;

            var name = NormalizeText(product.Name ?? "").ToLower();
            var description = NormalizeText(product.Description ?? "").ToLower();
            var category = NormalizeText(product.Category?.Name ?? "").ToLower();
            var brand = NormalizeText(product.Brand?.Name ?? "").ToLower();
            
            // Chính xác khớp (cao nhất)
            if (name == normalizedKeyword || category == normalizedKeyword || brand == normalizedKeyword)
                score += 100;

            // Chứa chuỗi
            if (name.Contains(normalizedKeyword))
                score += 50;
            if (category.Contains(normalizedKeyword))
                score += 40;
            if (brand.Contains(normalizedKeyword))
                score += 35;
            if (description.Contains(normalizedKeyword))
                score += 20;

            // Token matching (từng từ)
            foreach (var token in keywordTokens)
            {
                if (name.Contains(token))
                    score += 15;
                if (category.Contains(token))
                    score += 12;
                if (brand.Contains(token))
                    score += 10;
                if (description.Contains(token))
                    score += 5;
            }

            return score;
        }

        private string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            // Loại bỏ dấu đặc biệt, giữ lại chữ cái và số
            return Regex.Replace(text, @"[^\w\s]", " ");
        }
    }
}
