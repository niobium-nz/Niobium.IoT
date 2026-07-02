using System.Security.Cryptography;
using System.Text;
using System.IO.Hashing;

namespace Cod.Platform.Devices.Provisioning
{
    public static class PINHelper
    {
        public static Guid GenerateDeviceIDFromDeviceUID(string uid, string secret)
        {
            if (string.IsNullOrWhiteSpace(uid))
            {
                throw new ArgumentException($"'{nameof(uid)}' cannot be null or whitespace.", nameof(uid));
            }

            using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(secret));
            var hash256 = hmac.ComputeHash(Encoding.UTF8.GetBytes(uid));
            var hash128 = hash256.Skip(8).Take(16).ToArray();
            return new Guid(hash128);
        }

        public static bool ValidateDevicePIN(string pin, string secret)
        {
            pin = pin.Trim();
            if (pin.Length != 10)
            {
                return false;
            }

            var year = pin.Substring(2, 2);
            if (!int.TryParse(year, out var y) || y < 0 || y > 99)
            {
                return false;
            }

            var month = pin.Substring(6, 2);
            if (!int.TryParse(month, out var m) || m < 1 || m > 12)
            {
                return false;
            }

            var day = pin.Substring(0, 2);
            if (!int.TryParse(day, out var d) || d < 1 || d > 31)
            {
                return false;
            }

            var twoRandomNumber = pin.Substring(4, 2);
            if (!int.TryParse(twoRandomNumber, out var r) || r < 0 || r > 99)
            {
                return false;
            }

            var basestr = pin.Substring(0, 8);
            var actualCRC = pin.Substring(8, 2);
            var expectedCRC = CalculateCRC(basestr, secret);

            return actualCRC == expectedCRC;
        }

        public static string GenerateDevicePIN(string secret)
        {
            var now = DateTimeOffset.UtcNow;
            var random = new Random(DateTime.UtcNow.Millisecond);
            var sb = new StringBuilder();
            sb.Append(now.ToString("dd"));
            sb.Append(now.ToString("yy"));
            sb.Append(random.Next(9));
            sb.Append(random.Next(9));
            sb.Append(now.ToString("MM"));
            var pin = sb.ToString();
            var crc = CalculateCRC(pin, secret);
            return $"{pin}{crc}";
        }

        private static string CalculateCRC(string input, string secret)
        {
            using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(secret));
            byte[] sha256 = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
            Crc32 crc32 = new();
            crc32.Append(sha256);

            uint hash = crc32.GetCurrentHashAsUInt32();
            string crc = (hash % 100).ToString().PadLeft(2, '0');
            return crc;
        }
    }
}
