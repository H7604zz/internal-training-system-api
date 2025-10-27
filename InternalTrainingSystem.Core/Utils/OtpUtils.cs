using System.Security.Cryptography;

namespace InternalTrainingSystem.Core.Utils
{
    public static class OtpUtils
    {
        public static string GenerateOtp()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var randomNumber = BitConverter.ToUInt32(bytes, 0);
            return (randomNumber % 900000 + 100000).ToString();
        }
    }
}
