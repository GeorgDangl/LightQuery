using System.Net.Http;

namespace LightQuery.Client
{
    internal class HttpResponseMessageData
    {
        public HttpResponseMessage Response { get; set; }
        public int RequestedPage { get; set; }
    }
}
