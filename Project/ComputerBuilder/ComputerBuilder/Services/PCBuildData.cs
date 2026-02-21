using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerBuilder
{
    public class PCBuildData
    {
        public string title {  get; set; }

        public int build_id { get; set; }

        public float total_price_usd { get; set; }

        public float total_performance_score { get; set; }

        public int cpu_id { get; set; }

        public string cpu_name { get; set; }

        public float cpu_compute_score { get; set; }

        public int motherboard_id { get; set; }

        public string motherboard_name { get; set; }

        public int ram_id { get; set; }

        public string ram_name { get; set; }    

        public int ram_capacity_score { get; set; }

        public int ram_nb_modules { get; set; }

        public string ram_modules_message { get; set; }

        public int gpu_id { get; set; }

        public string gpu_name { get; set; }

        public string gpu_benchmark { get; set; }

        public int storage_id { get; set; }

        public string storage_name { get; set; }

        public int storage_capacity_gb { get; set; }

        public int psu_id { get; set; }

        public string psu_name_model { get; set; }

        public int psu_wattage { get; set; }

        public int case_id { get; set; }

        public string case_name { get; set; }

        public int nic_id { get; set; }

        public string image_url { get; set; }

        public float cpu_multicore_score { get; set; }

        public float cpu_efficiency_score { get; set; }

        public float gpu_compute_score { get; set; }

        public float gpu_power_efficiency { get; set; }

        public float ram_bandwidth_score { get; set; }

        public float has_gpu { get; set; }

        public string computer_type { get; set; }

        public float performance_value { get; set; }

        public override string ToString()
        {
            return $"Computer Specifications";
        }
    }
}
