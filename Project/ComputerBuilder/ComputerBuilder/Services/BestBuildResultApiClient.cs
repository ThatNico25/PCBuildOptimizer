using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ComputerBuilder.Services
{
    public class BestBuildResultApiClient : IBestBuildResultApiClient
    {
        private readonly HttpClient _httpClientValuePrediction;
        private readonly HttpClient _httpClientDataClassification;

        public BestBuildResultApiClient(HttpClient httpClientValuePrediction, HttpClient httpClientDataClassification)
        {
            _httpClientValuePrediction = httpClientValuePrediction;
            _httpClientDataClassification = httpClientDataClassification;
        }

        public async Task<FormFactorPrediction[]?> ClassifyAsync(FormFactorInput build)
        {
            var response = await _httpClientDataClassification.PostAsJsonAsync($"classify", build);

            var test = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<FormFactorPrediction[]>();
        }

        public async Task<BestBuildResult?> PredictAsync(List<PCBuildData> builds, string category, float price)
        {
           var response = await _httpClientValuePrediction.PostAsJsonAsync($"bestBuild?category={category}&price={price}", builds);

            var test = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<BestBuildResult>();
        }
    }
}