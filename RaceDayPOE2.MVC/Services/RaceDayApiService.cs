using System.Net.Http.Json;

namespace RaceDayPOE2.MVC.Services
{
    public class RaceDayApiService
    {
        private readonly HttpClient _httpClient;

        //CONSTRUCTOR
        public RaceDayApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        //GET REQUEST
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }

        //POST REQUEST
        public async Task<TResponse?> PostAsync<TRequest, TResponse>(
            string endpoint, TRequest data)
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        //PUT REQUEST
        public async Task PutAsync<TRequest>(
            string endpoint, TRequest data)
        {
            var response = await _httpClient.PutAsJsonAsync(endpoint, data);

            response.EnsureSuccessStatusCode();
        }

        //DELETE REQUEST
        public async Task DeleteAsync(string endpoint)
        {
            var response = await _httpClient.DeleteAsync(endpoint);

            response.EnsureSuccessStatusCode();
        }
    }
}