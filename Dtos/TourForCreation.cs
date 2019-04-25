using System;
namespace TourManagement.API.Dtos
{
    public class TourForCreation : TourAbstractBase
    {
        public Guid BandId { get; set; }
    }
}
