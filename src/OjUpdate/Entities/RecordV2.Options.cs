using System;
using System.Security.Cryptography;
using System.Threading;

namespace SatelliteSite.OjUpdateModule.Services
{
    public class RecordV2Options
    {
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
            int first = (int)DateTimeOffset.Now.ToUnixTimeSeconds();
            Span<byte> res = stackalloc byte[16];
            BitConverter.TryWriteBytes(res[4..8], second);
            BitConverter.TryWriteBytes(res[0..4], first);
            if (BitConverter.IsLittleEndian)
            {
                res[0..4].Reverse();
                res[4..8].Reverse();
            }

            res[1..4].CopyTo(res[9..12]);
            res[5..8].CopyTo(res[12..15]);
            return Convert.ToBase64String(res[9..15]).Replace('+', '-').Replace('/', '|');
        }
    }
}
