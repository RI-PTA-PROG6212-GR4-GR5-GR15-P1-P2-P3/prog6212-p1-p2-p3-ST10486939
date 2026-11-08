using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace RaceDayPOE2.MVC.Services
{
    public class RaceDayApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //CONSTRUCTOR
        public RaceDayApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        //Attach logged-in user's JWT to the request
        private HttpRequestMessage CreateRequest(
            HttpMethod method, string endpoint)
        {
            var request = new HttpRequestMessage(method, endpoint);

            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWTToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return request;
        }

        //GET REQUEST
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            using var request = CreateRequest(HttpMethod.Get, endpoint);

            using var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }

        //POST REQUEST
        public async Task<TResponse?> PostAsync<TRequest, TResponse>(
            string endpoint, TRequest data)
        {
            using var request = CreateRequest(HttpMethod.Post, endpoint);

            request.Content = JsonContent.Create(data);

            using var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        //PUT REQUEST
        public async Task PutAsync<TRequest>(
            string endpoint, TRequest data)
        {
            using var request = CreateRequest(HttpMethod.Put, endpoint);

            request.Content = JsonContent.Create(data);

            using var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();
        }

        //DELETE REQUEST
        public async Task DeleteAsync(string endpoint)
        {

            using var request = CreateRequest(HttpMethod.Delete, endpoint);

            using var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();
        }
    }
}