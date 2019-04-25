using System;
namespace TourManagement.API.Dtos
{
    // abstract as we don;t want it used directly
    public class TourAbstractBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
    }
}
