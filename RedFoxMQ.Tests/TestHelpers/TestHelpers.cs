using System;

namespace RedFoxMQ.Tests.TestHelpers
{
    public static class TestHelpers
    {
        public static Random CreateSemiRandomGenerator()
        {
            var now = DateTime.Now;
            return new Random(now.DayOfYear * 365 + now.Hour);
        }

        public static byte[] GetRandomBytes(Random random, int size)
        {
            byte[] result = new byte[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = (byte)random.Next(256);
            }
            return result;
        }
    }
}
