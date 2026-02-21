using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http.Json;
using ComputerBuilder.Services;

namespace ComputerBuilder
{
    public class PCBuildService
    {
        private readonly HttpClient _httpClient;

        public PCBuildService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<BestBuildResult?> GetBestBuildAsync(List<PCBuildData> builds, string category = null)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("predict", builds);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<BestBuildResult>();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {response.ReasonPhrase}");
                    var content = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Response: {content}");
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP Request Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected Error: {ex.GetType().Name}: {ex.Message}");
            }

            return null;
        }
    }
}
