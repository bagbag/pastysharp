using System.Collections.Generic;
using System.Net.Sockets;

namespace PastySharp
{
    public static class PastyUtils
    {
        private static readonly Dictionary<int, string> _statusCodeTextDictionary = new Dictionary<int, string>()
        {
            {0, "ok"}
        };

        public static Dictionary<int, string> StatusCodeTextDictionary
        {
            get { return _statusCodeTextDictionary; }
        }
    }

    public enum PastyFileType
    {
        Link = 0,
        File = 1
    }

    public class PatyRequestUrlResponse
    {
        private readonly string _url;
        private readonly int _statusCode;
        internal TcpClient _tcpClient;
        internal NetworkStream _tcpStream;

        public string Url
        {
            get { return _url; }
        }

        public int StatusCode
        {
            get { return _statusCode; }
        }

        public string StatusText
        {
            get
            {
                string text;
                bool success = PastyUtils.StatusCodeTextDictionary.TryGetValue(StatusCode, out text);

                return success ? text : null;
            }
        }

        internal PatyRequestUrlResponse(TcpClient tcpClient,NetworkStream tcpStream, string url, int statusCode)
        {
            _tcpClient = tcpClient;
            _tcpStream = tcpStream;
            _url = url;
            _statusCode = statusCode;
        }
    }
}
