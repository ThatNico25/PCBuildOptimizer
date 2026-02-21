using System.Threading.Tasks;

namespace ComputerBuilder.Services
{
    public interface IBestBuildResultApiClient
    {
        Task<BestBuildResult?> PredictAsync(List<PCBuildData> builds, string category, float price);

        Task<FormFactorPrediction[]?> ClassifyAsync(FormFactorInput build);
    }
}
