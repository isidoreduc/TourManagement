using System;
namespace TourManagement.API.Dtos
{
    public class TourWithManagerForCreation : TourForCreation
    {
        public string ManagerId { get; set; }
    }
}
