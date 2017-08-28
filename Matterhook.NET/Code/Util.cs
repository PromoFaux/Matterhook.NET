using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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

    public class UnixDateTimeConverter : DateTimeConverterBase
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Integer)
            {
                return reader.Value;
                throw new Exception(
                    String.Format("Unexpected token parsing date. Expected Integer, got {0}.",
                        reader.TokenType));
            }

            var ticks = (long)reader.Value;

            var date = new DateTime(1970, 1, 1);
            date = date.AddSeconds(ticks);

            return date;
        }

        public override void WriteJson(JsonWriter writer, object value,
            JsonSerializer serializer)
        {
            long ticks;
            if (value is DateTime)
            {
                var epoc = new DateTime(1970, 1, 1);
                var delta = ((DateTime)value) - epoc;
                if (delta.TotalSeconds < 0)
                {
                    throw new ArgumentOutOfRangeException(
                        "Unix epoc starts January 1st, 1970");
                }
                ticks = (long)delta.TotalSeconds;
            }
            else
            {
                throw new Exception("Expected date object value.");
            }
            writer.WriteValue(ticks);
        }
    }
}