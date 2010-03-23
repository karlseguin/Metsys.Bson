using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Metsys.Bson
{
    internal static class ObjectIdGenerator
    {        
        private static readonly object _inclock = new object();

        private static int _counter;
        private static readonly byte[] _machineHash = GenerateHostHash();
        private static readonly byte[] _processId = BitConverter.GetBytes(GenerateProcId());

        public static byte[] Generate()
        {
            var oid = new byte[12];
            var copyidx = 0;

            Array.Copy(BitConverter.GetBytes(GenerateTime()), 0, oid, copyidx, 4);
            copyidx += 4;

            Array.Copy(_machineHash, 0, oid, copyidx, 3);
            copyidx += 3;

            Array.Copy(_processId, 0, oid, copyidx, 2);
            copyidx += 2;

            Array.Copy(BitConverter.GetBytes(GenerateInc()), 0, oid, copyidx, 3);
            return oid;
        }

        private static int GenerateTime()
        {
            var now = DateTime.Now.ToUniversalTime();

            var nowtime = new DateTime(Helper.Epoch.Year, Helper.Epoch.Month, Helper.Epoch.Day, now.Hour, now.Minute, now.Second, now.Millisecond);
            var diff = nowtime - Helper.Epoch;
            return Convert.ToInt32(Math.Floor(diff.TotalMilliseconds));
        }

        private static int GenerateInc()
        {         
            lock (_inclock)
            {
                return _counter++;
            }
        }
        
        private static byte[] GenerateHostHash()
        {
            using(var md5 = MD5.Create())
            {
                var host = Dns.GetHostName();
                return md5.ComputeHash(Encoding.Default.GetBytes(host));
            }
        }
        private static int GenerateProcId()
        {
            var proc = Process.GetCurrentProcess();
            return proc.Id;
        }
    }    
}