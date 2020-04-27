using System;
using System.Security.Cryptography;

namespace Requestr.Services
{
    public static class RSG
    {
        public static string Generate(int length)
        {
            byte[] bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            return Convert.ToBase64String(bytes).Substring(0, length);
        }
    }
}
