using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiteBoard.Data.Extensions
{
    public static class GuidExtensions
    {
        public static string ToStringFromGuid(this Guid id)
        {
            return Convert.ToBase64String(id.ToByteArray())
                .Replace("/", "-")
                .Replace("+", "_")
                .Replace("=", string.Empty);
        }

        public static Guid ToGuidFromString(this string id)
        {
            var efficientBase64 = Convert.FromBase64String(id
                .Replace("-", "/")
                .Replace("_", "+") + "==");
            return new Guid(efficientBase64);
        }
    }
}
