using System;
using System.Collections.Generic;
using System.Text;

namespace Ecom.ProductService.Core.Models.Dtos.ProductSummaryService
{
    public class DashboardProductDto
    {
        public DashboardProductDto() { }
        public string Title { get; set; } = string.Empty;
        public List<SummaryMetrics> SummaryMetrics { get; set; } = new List<SummaryMetrics>();
    }
    public class SummaryMetrics
    {
        public SummaryMetrics() { }
        public string Title { get; set; } = string.Empty;
        public int value { get; set; }
        public string Group { get; set; } = string.Empty;
        public string TitleGroup { get; set; } = string.Empty;
    }
}
