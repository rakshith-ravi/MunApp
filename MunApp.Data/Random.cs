using System;

namespace MunApp.Common
{
    public class Random
    {
        private static System.Random random = new System.Random();

        public static int Next()
        {
            return random.Next();
        }
        public static int Next(int maxValue)
        {
            return random.Next(maxValue);
        }
        public static int Next(int lowerLimit, int upperLimit)
        {
            return random.Next(lowerLimit, upperLimit);
        }

        public static void NextBytes(byte[] buffer)
        {
            random.NextBytes(buffer);
        }

        public static double NextDouble()
        {
            return random.NextDouble();
        }
    }
}

