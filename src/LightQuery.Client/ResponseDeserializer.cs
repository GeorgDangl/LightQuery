using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LightQuery.Client
{
    internal static class ResponseDeserializer<T>
    {
        public static async Task<ResponseDeserializationResult<PaginationResult<T>>> DeserializeResponse(HttpResponseMessageData responseData)
        {
            if (responseData?.Response?.Content == null || !responseData.Response.IsSuccessStatusCode)
            {
                return null;
            }
            var responseContent = await responseData.Response.Content.ReadAsStringAsync();
            try
            {
                var deserializedResponse = JsonConvert.DeserializeObject<PaginationResult<T>>(responseContent);
                var newPageSuggestion = CheckPaginationResponseIfPageOutOfRange(deserializedResponse);
                var result = new ResponseDeserializationResult<PaginationResult<T>>
                {
                    DeserializedValue = deserializedResponse,
                    NewPageSuggestion = newPageSuggestion == responseData.RequestedPage ? 0 : newPageSuggestion
                };
                return result;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// If the result of the request is out of range, a new page is suggested. Otherwise, 0 is returned.
        /// </summary>
        /// <param name="paginationResult"></param>
        /// <returns></returns>
        private static int CheckPaginationResponseIfPageOutOfRange(PaginationResult<T> paginationResult)
        {
            if (paginationResult.Data.Count > 0)
            {
                return 0;
            }
            if (paginationResult.Page == 1)
            {
                return 0;
            }
            if (paginationResult.TotalCount == 0)
            {
                return 0;
            }
            var lastPageWithItems = paginationResult.TotalCount / paginationResult.PageSize;
            if (paginationResult.TotalCount % paginationResult.PageSize != 0)
            {
                lastPageWithItems++;
            }
            return lastPageWithItems;
        }
    }
}
