using ChartJs.Blazor.ChartJS.Common.Handlers;
using ChartJs.Blazor.ChartJS.Common.Properties;
using ChartJs.Blazor.ChartJS.PieChart;
using ChartJs.Blazor.Charts;
using ChartJs.Blazor.Util;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Client.Shared.Components
{
    public partial class PieChartComponent
    {
        [Parameter]
        public string Title { get; set; }
        [Parameter]
        public List<string> Labels { get; set; }
        [Parameter]
        public List<double> Data { get; set; }
        [Parameter]
        public int Width { get; set; }
        [Parameter]
        public int Height { get; set; }

        public PieConfig _config;
        public ChartJsPieChart _pieChartJs;

        protected override void OnParametersSet()
        {
            _config = new PieConfig
            {
                Options = new PieOptions
                {
                    Title = new OptionsTitle
                    {
                        Text = Title,
                        Display = true,
                        FontColor = ColorUtil.FromDrawingColor(Color.White)
                    },
                    Responsive = true,
                    Animation = new ArcAnimation
                    {
                        AnimateRotate = true,
                        AnimateScale = true
                    },
                    Legend = new Legend()
                    {
                        Display = false
                    }
                }
            };

            var pieSet = new PieDataset
            {
                BorderWidth = 0,
                HoverBackgroundColor = ColorUtil.RandomColorString(),
                HoverBorderColor = ColorUtil.RandomColorString(),
                HoverBorderWidth = 1,
                BorderColor = "#ffffff"
            };

            List<string> colors = new List<string>();
            for (int i = 0; i < Labels.Count; i++)
            {
                colors.Add(ColorUtil.RandomColorString());
            }
            pieSet.BackgroundColor = colors.ToArray();

            _config.Data.Labels.AddRange(Labels);
            pieSet.Data.AddRange(Data);

            _config.Data.Datasets.Add(pieSet);
        }
    }
}
