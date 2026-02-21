
using ComputerBuilder.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace ComputerBuilder
{
    public partial class MainPage : ContentPage
    {
        private readonly IBestBuildResultApiClient _apiClient;
        private static readonly Random _rng = new();
        private List<PCBuildData> _allPCBuildDatas;
        private ObservableCollection<PCBuildData> _filteredPCBuildDatas;
        private readonly PCBuildService _buildService;

        private ObservableCollection<PCBuildData> _bestResultBuildDatas;

        public MainPage()
        {
            InitializeComponent();
            _apiClient = IPlatformApplication.Current.Services.GetRequiredService<IBestBuildResultApiClient>();
            _filteredPCBuildDatas = new ObservableCollection<PCBuildData>();
            BuildsCollectionView.ItemsSource = _filteredPCBuildDatas;
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                // Test database connection
                string connectionResult = DatabaseManager.Instance.TestConnection();
                if (!connectionResult.Contains("success"))
                {
                    await DisplayAlert("Connection Error", connectionResult, "OK");
                    return;
                }

                // Load all builds from database
                string result = DatabaseManager.Instance.ExecuteQuery("SELECT * FROM computer_build");
  
                try
                {
                    ParseBuildData(result);
                }
                finally
                {
                    // Display all builds initially
                    //RefreshBuildsList();
                }
        
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to initialize: {ex.Message}", "OK");
            }
        }

        private async void ParseBuildData(string result)
        {
            var builds = new List<PCBuildData>();
            string[] lines = result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string[] parts = line.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 7)
                {
              
                    try
                    {
                        // CPU (Name, Score)
                        string cpuName = DatabaseManager.Instance.ExecuteQuery($"SELECT name FROM cpu WHERE id = '{parts[3].Trim()}'");

                        if(cpuName.Contains("\n"))
                        {
                            cpuName = cpuName.Remove(cpuName.IndexOf("\n"));
                        }

                        string cpuScore = DatabaseManager.Instance.ExecuteQuery($"SELECT cpu_multicore_score FROM cpu WHERE id = '{parts[3].Trim()}'");

                        string cpu_form_factor = DatabaseManager.Instance.ExecuteQuery($"SELECT intended_form_factor FROM cpu WHERE id = '{parts[4].Trim()}'");
                        cpu_form_factor = cpu_form_factor.Remove(cpu_form_factor.IndexOf("\n"));

                        // Motherboard
                        string motherboardName = DatabaseManager.Instance.ExecuteQuery($"SELECT name FROM motherboard WHERE id = '{parts[4].Trim()}'");

                        if (motherboardName.Contains("\n"))
                        {
                            motherboardName = motherboardName.Remove(motherboardName.IndexOf("\n"));
                        }

                        string id_form_factor = DatabaseManager.Instance.ExecuteQuery($"SELECT form_factor_id FROM motherboard WHERE id = '{parts[4].Trim()}'");
                        id_form_factor = id_form_factor.Remove(id_form_factor.IndexOf("\n"));

                        string mother_form_factor = DatabaseManager.Instance.ExecuteQuery($"SELECT name FROM motherboard_form_factor WHERE form_factor_id = '{id_form_factor}'");
                        mother_form_factor = mother_form_factor.Remove(mother_form_factor.IndexOf("\n"));

                        // Ram
                        string ramName = DatabaseManager.Instance.ExecuteQuery($"SELECT model FROM memory WHERE ram_id = '{parts[6].Trim()}'");

                        if (ramName.Contains("\n"))
                        {
                            ramName = ramName.Remove(ramName.IndexOf("\n"));
                        }

                        string ramCapacityScore = DatabaseManager.Instance.ExecuteQuery($"SELECT capacity_gb FROM memory WHERE ram_id = '{parts[6].Trim()}'");

                        string ramModules = DatabaseManager.Instance.ExecuteQuery($"SELECT module_count FROM memory WHERE ram_id = '{parts[6].Trim()}'");

                        int ramPerModule = int.Parse(ramCapacityScore) / int.Parse(ramModules);

                        string moduleCountMessage = $"RAM Modules: [{int.Parse(ramModules)} x {ramPerModule} GB]";

                        // GPU
                        string gpuName = "No GPU";
                        string gpuBenchmark = "";
                        float Gpu_compute_score = 0f;
                        string gpu_compute_score = "";

                        if (parts[11] == "True" && parts[5].Trim() != "NULL")
                        {
                            gpuName = DatabaseManager.Instance.ExecuteQuery($"SELECT model FROM videocard WHERE gpu_id = '{parts[5].Trim()}'");
                            if (gpuName.Contains("\n"))
                            {
                                gpuName = gpuName.Remove(gpuName.IndexOf("\n"));
                            }

                            gpuBenchmark = "Benchmark GPU : " + DatabaseManager.Instance.ExecuteQuery($"SELECT gpu_compute_score FROM videocard WHERE gpu_id = '{parts[5].Trim()}'");

                            if (gpuBenchmark.Contains("\n"))
                            {
                                gpuBenchmark = gpuBenchmark.Remove(gpuBenchmark.IndexOf("\n"));
                            }
                            gpu_compute_score = DatabaseManager.Instance.ExecuteQuery($"SELECT gpu_compute_score FROM videocard WHERE gpu_id = '{parts[5].Trim()}'");
                            Gpu_compute_score = float.Parse(DatabaseManager.Instance.ExecuteQuery($"SELECT gpu_compute_score FROM videocard WHERE gpu_id = '{parts[5].Trim()}'"));
                        }

                        // Internal Storage
                        string storageName = "";
                        string storageCapacity = "0";

                        if(parts[7] != "NULL")
                        {
                            storageName = DatabaseManager.Instance.ExecuteQuery($"SELECT model_name FROM internal_storage WHERE id = '{parts[7].Trim()}'");
                            if (storageName.Contains("\n"))
                            {
                                storageName = storageName.Remove(storageName.IndexOf("\n"));
                            }
                            storageCapacity = DatabaseManager.Instance.ExecuteQuery($"SELECT amount_gb FROM internal_storage WHERE id = '{parts[7].Trim()}'");
                        }

                        // PSU
                        string psuNameModel = "No PSU or externe...";
                        string psuWatt = "0";

                        if (parts[8] != "NULL")
                        {
                            psuNameModel = DatabaseManager.Instance.ExecuteQuery($"SELECT model_name FROM psu WHERE psu_id = '{parts[8].Trim()}'");
                            if (psuNameModel.Contains("\n"))
                            {
                                psuNameModel = psuNameModel.Remove(psuNameModel.IndexOf("\n"));
                            }
                            psuWatt = DatabaseManager.Instance.ExecuteQuery($"SELECT wattage FROM psu WHERE psu_id = '{parts[8].Trim()}'");
                        }

                        // Case
                        string caseName = "";

                        if (parts[9] != "NULL")
                        {
                            caseName = DatabaseManager.Instance.ExecuteQuery($"SELECT case_type FROM pc_case WHERE id = '{parts[9].Trim()}'");

                            if (caseName.Contains("\n"))
                            {
                                caseName = caseName.Remove(caseName.IndexOf("\n"));
                            }
                        }


                        // Title / Category
                        FormFactorInput build = new FormFactorInput();
                        build.Cpu_form_factor = cpu_form_factor;
                        build.GpuComputeScore = 0F;

                        bool has_gpu_build = false;

                        if (gpu_compute_score != "")
                        {
                            build.GpuComputeScore = float.Parse(gpu_compute_score);
                            has_gpu_build = true;
                        } 

                        build.CaseType = caseName;
                        build.MotherboardFormFactor = mother_form_factor;
                       
                        var json = await _apiClient.ClassifyAsync(build);

                        string url_image = "";

                        switch(json[0].Category)
                        {
                            case "Laptop":
                                url_image = "laptop.png";
                                break;
                            case "Gaming Laptop":
                                url_image = "gaming_laptop.png";
                                break;
                            case "Tower":
                                url_image = "tower.png";
                                break;
                            case "Gaming Tower":
                                url_image = "gaming_tower.png";
                                break;
                            case "Mini-PC":
                                url_image = "mini_pc.png";
                                break;
                            case "Workstation":
                                url_image = "workstation.png";
                                break;
                            case "Server":
                                url_image = "server.png";
                                break;

                        }

                        float Cpu_multicore_score_build = float.Parse(DatabaseManager.Instance.ExecuteQuery($"SELECT cpu_multicore_score FROM cpu WHERE id = '{parts[3].Trim()}'"));
                        float Cpu_efficient_score_build = float.Parse(DatabaseManager.Instance.ExecuteQuery($"SELECT cpu_efficiency_score FROM cpu WHERE id = '{parts[3].Trim()}'"));
                        float Ram_bandwidth_score = float.Parse(DatabaseManager.Instance.ExecuteQuery($"SELECT capacity_gb FROM memory WHERE ram_id = '{parts[6].Trim()}'"));

                        PCBuildData buildData = new PCBuildData
                        {
                            // TItle
                            title = json != null ? json[0].Category : "Error",

                            // ID
                            build_id = int.Parse(parts[0]),

                            // Total Price (USD$)
                            total_price_usd = float.Parse(parts[1], CultureInfo.InvariantCulture),

                            // CPU
                            cpu_name = cpuName,
                            cpu_compute_score = float.Parse(cpuScore),

                            // Motherboard
                            motherboard_name = motherboardName,

                            // RAM
                            ram_name = ramName,
                            ram_capacity_score = int.Parse(ramCapacityScore),
                            ram_modules_message = moduleCountMessage,

                            // GPU
                            gpu_name = gpuName,
                            gpu_benchmark = gpuBenchmark,

                            // Storage
                            storage_name = storageName,
                            storage_capacity_gb = int.Parse(storageCapacity),

                            // Power Supply
                            psu_name_model = psuNameModel,
                            psu_wattage = int.Parse(psuWatt),

                            // Case (if not laptop)
                            case_name = caseName,

                            // ADD photo per category
                            image_url = url_image,

                            // Value Prediction
                            has_gpu = has_gpu_build ? 1f : 0f,
                            cpu_efficiency_score = Cpu_efficient_score_build,
                            cpu_multicore_score = Cpu_multicore_score_build,
                            gpu_compute_score = Gpu_compute_score,
                            ram_bandwidth_score = Ram_bandwidth_score

                        };

                        builds.Add(buildData);
                    }
                    catch 
                    {
                        continue;  // Skip malformed rows
                    }
                }
            }

            builds = builds.OrderBy(n => n.build_id).ToList();

            _allPCBuildDatas = builds;
            RefreshBuildsList();
        }

        private void RefreshBuildsList()
        {
            if (_allPCBuildDatas == null) return;
            BuildsCollectionView.ItemsSource = _allPCBuildDatas;
        }

        private async void GetBestPerformanceBudget(object sender, EventArgs e) 
        {

            // 1. Create a list of each type with budjet (Min and max)
            float min = 0;
            if (MinBudgetEntry.Text != "" && MinBudgetEntry.Text != null)
                min = float.Parse(MinBudgetEntry.Text);

            float max = 999999;
            if (MaxBudgetEntry.Text != "" && MaxBudgetEntry.Text != null)
                max = float.Parse(MaxBudgetEntry.Text);

            List<PCBuildData> m_Tower_Build = _allPCBuildDatas.Where(n => n.title == "Tower" && n.total_price_usd >= min && n.total_price_usd <= max).ToList();
            List<PCBuildData> m_Tower_Gaming_Build = _allPCBuildDatas.Where(n => n.title == "Gaming Tower" && n.total_price_usd >= min && n.total_price_usd <= max).ToList();
            List<PCBuildData> m_Laptop_Build = _allPCBuildDatas.Where(n => n.title == "Laptop" && n.total_price_usd >= min && n.total_price_usd <= max).ToList();
            List<PCBuildData> m_Laptop_Gaming_Build = _allPCBuildDatas.Where(n => n.title == "Gaming Laptop" && n.total_price_usd >= min && n.total_price_usd <= max).ToList();
            List<PCBuildData> m_Mini_PC_Build = _allPCBuildDatas.Where(n => n.title == "Mini-PC" && n.total_price_usd >= min && n.total_price_usd <= max).ToList();
            List<PCBuildData> m_Workstation_Build = _allPCBuildDatas.Where(n => n.title == "Workstation" && n.total_price_usd >= min && n.total_price_usd <= max).ToList();
            List<PCBuildData> m_Server_Build = _allPCBuildDatas.Where(n => n.title == "Server" && n.total_price_usd >= min && n.total_price_usd <= max).ToList();

            // 2. Only get the best performance value
            _bestResultBuildDatas = new ObservableCollection<PCBuildData>();
            PCBuildData aBuild;

            try
            {
                var json_tower = await _apiClient.PredictAsync(m_Tower_Build, "Tower", max);
                if(json_tower != null)
                {
                    aBuild = json_tower.Build;
                    aBuild.performance_value = json_tower.PerformancePerDollar;
                    _bestResultBuildDatas.Add(aBuild);
                }
           
                var json_tower_gaming = await _apiClient.PredictAsync(m_Tower_Gaming_Build, "Gaming Tower", max);
                if (json_tower_gaming != null)
                {
                    aBuild = json_tower_gaming.Build;
                    aBuild.performance_value = json_tower_gaming.PerformancePerDollar;
                    _bestResultBuildDatas.Add(json_tower_gaming.Build);
                }

                var json_laptop= await _apiClient.PredictAsync(m_Laptop_Build, "Laptop", max);
                if (json_laptop != null)
                {
                    aBuild = json_laptop.Build;
                    aBuild.performance_value = json_laptop.PerformancePerDollar;
                    _bestResultBuildDatas.Add(json_laptop.Build);
                }

                var json_laptop_gaming = await _apiClient.PredictAsync(m_Laptop_Gaming_Build, "Gaming Laptop", max);
                if (json_laptop_gaming != null)
                {
                    aBuild = json_laptop_gaming.Build;
                    aBuild.performance_value = json_laptop_gaming.PerformancePerDollar;
                    _bestResultBuildDatas.Add(json_laptop_gaming.Build);
                }

                var json_mini_pc = await _apiClient.PredictAsync(m_Mini_PC_Build, "Mini-PC", max);
                if (json_mini_pc != null)
                {
                    aBuild = json_mini_pc.Build;
                    aBuild.performance_value = json_mini_pc.PerformancePerDollar;
                    _bestResultBuildDatas.Add(json_mini_pc.Build);
                }

                var json_workstation = await _apiClient.PredictAsync(m_Workstation_Build, "Workstation", max);
                if (json_workstation != null)
                {
                    aBuild = json_workstation.Build;
                    aBuild.performance_value = json_workstation.PerformancePerDollar;
                    _bestResultBuildDatas.Add(json_workstation.Build);
                }
               
                var json_server = await _apiClient.PredictAsync(m_Server_Build, "Server", max);
                if (json_server != null)
                {
                    aBuild = json_server.Build;
                    aBuild.performance_value = json_server.PerformancePerDollar;
                    _bestResultBuildDatas.Add(json_server.Build);
                }                
            }
            finally
            {
            }

            // 3. Show the result
            BuildsCollectionViewResult.ItemsSource = _bestResultBuildDatas;
        }

        private void OnApplyFiltersClicked(object sender, EventArgs e)
        {
            RefreshBuildsList();
        }

        private void OnResetFiltersClicked(object sender, EventArgs e)
        {
            MinBudgetEntry.Text = string.Empty;
            MaxBudgetEntry.Text = string.Empty;
            RefreshBuildsList();
        }
    }
}
