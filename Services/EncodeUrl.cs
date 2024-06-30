using Newtonsoft.Json;
using System.Text;
using System.Web;

namespace Web_TracNghiem_HTSV.Services
{
    public static class EncodeUrl
    {
        public static string Encode(string url)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes((url)));
        }

        public static string Decode(string encodedUrl)
        {
            var decodedBytes = Convert.FromBase64String(encodedUrl);
            return Encoding.UTF8.GetString(decodedBytes);
        }
    }
}
