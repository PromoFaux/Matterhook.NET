using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Matterhook.NET.Code
{
    public class Util
    {
        public static string CalculateSignature(string payload, string signatureWithPrefix, string secret,
            string shaPrefix)
        {
            if (!signatureWithPrefix.StartsWith(shaPrefix, StringComparison.OrdinalIgnoreCase))
                return "Invalid shaPrefix";

            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);


            switch (shaPrefix)
            {
                case "sha1=":
                    using (var hmSha1 = new HMACSHA1(secretBytes))
                    {
                        var hash = hmSha1.ComputeHash(payloadBytes);

                        return $"{shaPrefix}{ToHexString(hash)}";
                    }
                case "sha256=":
                    using (var hmSha256 = new HMACSHA256(secretBytes))
                    {
                        var hash = hmSha256.ComputeHash(payloadBytes);

                        return $"{shaPrefix}{ToHexString(hash)}";
                    }
                default:
                    return "Invalid shaPrefix";
            }
        }

        private static string ToHexString(byte[] bytes)
        {
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                builder.AppendFormat("{0:x2}", b);
            }

            return builder.ToString();
        }
    }
}