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

                return StringDistanceCalculator.LevenshteinDistance(s1, s2);
            }
        }
    }
}