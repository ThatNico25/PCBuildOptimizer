using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerBuilder.Services
{
    public class FormFactorPrediction
    {
        [ColumnName("PredictedLabel")]
        public string Category { get; set; }
        public float Score { get; set; }
        public double ScorePercentage { get; set; }
    }

}
