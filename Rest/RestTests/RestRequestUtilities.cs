using System.Net.Http;
using Rest.Model;

namespace RestTests
{
    internal static class RestRequestUtilities
    {
        public static void ExecuteAndFail(this RestRequest restRequest, HttpMethod httpMethod)
        {
            try
            {
                restRequest.Execute(httpMethod);
            }
            catch
            {
                // ignore
            }
        }
    }
}
