using System;
using System.Data.SQLite;
namespace AAE2023_P22083_M3
{
    public static class LevenshteinDistanceExtension
    {
        [SQLiteFunction(FuncType = FunctionType.Scalar, Name = "LevenshteinDistance")]
        public class LevenshteinDistanceFunction : SQLiteFunction
        {
            public override object Invoke(object[] args)
            {
                if (args.Length != 2)
                    throw new ArgumentException("Η απόσταση Levenshtein απαιτεί δύο και μόνο 2 ορίσματα.");

                string s1 = Convert.ToString(args[0]);
                string s2 = Convert.ToString(args[1]);

                return LevenshteinDistance(s1, s2);
            }

            private static int LevenshteinDistance(string s1, string s2)
            {
                int[,] d = new int[s1.Length + 1, s2.Length + 1];

                for (int i = 0; i <= s1.Length; i++)
                    d[i, 0] = i;

                for (int j = 0; j <= s2.Length; j++)
                    d[0, j] = j;

                for (int i = 1; i <= s1.Length; i++)
                {
                    for (int j = 1; j <= s2.Length; j++)
                    {
                        int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                        d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                    }
                }

                return d[s1.Length, s2.Length];
            }
        }
    }
}