namespace MyWebApp.Services
{
    public static class VectorMath
    {
        public static double CosineSimilarity(double[] a, double[] b)
        {
            // Kiểm tra null và độ dài
            if (a == null || b == null)
                return 0;

            if (a.Length != b.Length)
                return 0;

            if (a.Length == 0)
                return 0;

            double dot = 0, magA = 0, magB = 0;

            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                magA += a[i] * a[i];
                magB += b[i] * b[i];
            }

            double denominator = Math.Sqrt(magA) * Math.Sqrt(magB);
            
            return denominator == 0 ? 0 : dot / denominator;
        }
    }
}
