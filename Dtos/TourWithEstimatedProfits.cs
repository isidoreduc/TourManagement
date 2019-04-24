using System;
namespace TourManagement.API.Dtos
{
    public class TourWithEstimatedProfits : Tour
    {
        public decimal EstimatedProfits { get; set; }
    }
}
