using System;
using System.Security.Cryptography;
using System.Threading;

namespace SatelliteSite.OjUpdateModule.Services
{
    public class RecordV2Options
    {
        internal const string Base32Chars = "0123456789abcdefhjkmnpqrstuvwxyz";
        private const int Int24Mask = (1 << 24) - 1;
        private static int _seed;

        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; } = "Bricks";

        public string ContainerName { get; set; } = "ExternalRanklist";

        static RecordV2Options()
        {
            try
            {
                int machineName = Environment.MachineName.GetHashCode();
                int pid = Environment.ProcessId;
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                int enrolled = RandomNumberGenerator.GetInt32(1 << 24);
                enrolled ^= (int)((uint)machineName & Int24Mask);
                enrolled ^= (int)(timestamp & Int24Mask);
                enrolled ^= (int)((timestamp >> 24) & Int24Mask);
                enrolled ^= (int)(((ulong)pid << 16) & Int24Mask);
                _seed = enrolled;
            }
            catch
            {
                _seed = 0;
            }
        }

        internal static string CreateUniqueIdentifier()
        {
            int second = Interlocked.Increment(ref _seed);
            int first = (int)(DateTimeOffset.Now.ToUnixTimeSeconds() / 2);
            Span<byte> res = stackalloc byte[8];
            BitConverter.TryWriteBytes(res[4..8], second);
            BitConverter.TryWriteBytes(res[0..4], first);
            if (BitConverter.IsLittleEndian)
            {
                res[0..4].Reverse();
                res[4..8].Reverse();
            }

            Span<char> result = stackalloc char[11];
            result[0] = Base32Chars[(res[0] & 124) >> 2];
            result[1] = Base32Chars[((res[0] & 3) << 3) | (res[1] >> 5)];
            result[2] = Base32Chars[res[1] & 31];
            result[3] = Base32Chars[res[2] >> 3];
            result[4] = Base32Chars[((res[2] & 7) << 2) | (res[3] >> 6)];
            result[5] = Base32Chars[(res[3] & 62) >> 1];
            result[6] = Base32Chars[((res[3] & 1) << 4) | (res[5] >> 4)];
            result[7] = Base32Chars[((res[5] & 15) << 1) | (res[6] >> 7)];
            result[8] = Base32Chars[(res[6] & 124) >> 2];
            result[9] = Base32Chars[((res[6] & 3) << 3) | (res[7] >> 5)];
            result[10] = Base32Chars[res[7] & 31];

            return new string(result);
        }
    }
}
