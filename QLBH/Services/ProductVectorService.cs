using Accord.MachineLearning;
using MyWebApp.Models;

namespace MyWebApp.Services
{
    public class ProductVectorService
    {
        private readonly TFIDF _tfidf;
        private readonly List<ProductModel> _products;
        private int _vectorSize;

        public ProductVectorService(List<ProductModel> products)
        {
            _products = products;

            var corpus = products
                .Select(p =>
                    new[]
                    {
                        p.Name ?? "",
                        p.Description ?? "",
                        p.Category?.Name ?? "",
                        p.Brand?.Name ?? ""
                    })
                .Where(x => x.Any(s => !string.IsNullOrWhiteSpace(s))) // Lọc empty corpus
                .ToArray();

            if (corpus.Length == 0)
            {
                _vectorSize = 0;
                _tfidf = null;
                return;
            }

            _tfidf = new TFIDF();
            _tfidf.Learn(corpus);

            // Xác định kích thước vector cố định
            var sampleVector = _tfidf.Transform(corpus[0]);
            _vectorSize = sampleVector?.Length ?? 0;
        }

        public double[] Vectorize(string text)
        {
            if (_tfidf == null || string.IsNullOrWhiteSpace(text))
                return new double[_vectorSize];

            try
            {
                var vector = _tfidf.Transform(new[] { text.Trim() });

                // Đảm bảo vector có kích thước cố định
                if (vector == null || vector.Length == 0)
                    return new double[_vectorSize];

                if (vector.Length != _vectorSize)
                {
                    var resizedVector = new double[_vectorSize];
                    Array.Copy(vector, resizedVector, Math.Min(vector.Length, _vectorSize));
                    return resizedVector;
                }

                return vector;
            }
            catch
            {
                return new double[_vectorSize];
            }
        }

        public double[] VectorizeProduct(ProductModel p)
        {
            var text = $"{p.Name ?? ""} {p.Description ?? ""} {p.Category?.Name ?? ""} {p.Brand?.Name ?? ""}".Trim();
            return Vectorize(text);
        }
    }
}
