using Mango.Web.Models;
using Mango.Web.Service.IService;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using static Mango.Web.Utility.SD;

namespace Mango.Web.Service
{


    public class BaseService(IHttpClientFactory httpClientFactory, ITokenProvider tokenProvider) : IBaseService
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly ITokenProvider _tokenProvider = tokenProvider;

        public async Task<ResponseDTO?> SendAsync(RequestDTO requestDto, bool withBearer = true)
        {
            try
            {
                HttpRequestMessage message = new();
                message.Headers.Add("Accept", "application/json");

                if (withBearer)
                {
                    var token = _tokenProvider.GetToken();
                    message.Headers.Add("Authorization", $"Bearer {token}");
                }


                if (string.IsNullOrEmpty(requestDto.Url))
                    throw new ArgumentNullException(nameof(requestDto.Url), "Request URL cannot be null or empty.");
                message.RequestUri = new Uri(requestDto.Url);



                // Setze Body, falls vorhanden
                if (requestDto.Data != null)
                {
                    message.Content = new StringContent(
                        JsonConvert.SerializeObject(requestDto.Data),
                        Encoding.UTF8,
                        "application/json");
                }

                // Setze AccessToken, falls vorhanden
                if (!string.IsNullOrEmpty(requestDto.AccessToken))
                {
                    message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", requestDto.AccessToken);
                }

                // Setze HTTP-Methode
                switch (requestDto.ApiType)
                {
                    case ApiType.POST:
                        message.Method = HttpMethod.Post;
                        break;
                    case ApiType.PUT:
                        message.Method = HttpMethod.Put;
                        break;
                    case ApiType.DELETE:
                        message.Method = HttpMethod.Delete;
                        break;
                    case ApiType.GET:
                        message.Method = HttpMethod.Get;
                        break;
                    default:
                        message.Method = HttpMethod.Get;
                        break;
                }

                // Sende Anfrage
                using var client = _httpClientFactory.CreateClient("MangoAPI");
                using var apiResponse = await client.SendAsync(message);

                // Fehlerbehandlung anhand Statuscode
                switch (apiResponse.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return new() { IsSuccess = false, Message = "Not Found" };
                    case HttpStatusCode.Forbidden:
                        return new() { IsSuccess = false, Message = "Access Denied" };
                    case HttpStatusCode.Unauthorized:
                        return new() { IsSuccess = false, Message = "Unauthorized" };
                    case HttpStatusCode.InternalServerError:
                        return new() { IsSuccess = false, Message = "Internal Server Error" };
                    default:
                        var apiContent = await apiResponse.Content.ReadAsStringAsync();
                        var responseDto = JsonConvert.DeserializeObject<ResponseDTO>(apiContent);
                        return responseDto;
                }
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}
