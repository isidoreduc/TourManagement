using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TourManagement.API.Dtos;
using TourManagement.API.Helpers;
using TourManagement.API.Services;

namespace TourManagement.API.Controllers
{
    [Route("api/tours")]
    public class ToursController : Controller
    {
        private readonly ITourManagementRepository _tourManagementRepository;

        public ToursController(ITourManagementRepository tourManagementRepository)
        {
            _tourManagementRepository = tourManagementRepository;
        }

        #region HttpGet
        [HttpGet]
        public async Task<IActionResult> GetTours()
        {
            var toursFromRepo = await _tourManagementRepository.GetTours();
            var tours = Mapper.Map<IEnumerable<Tour>>(toursFromRepo);
            return Ok(tours);
        }

        // to support passing default, unprivilleged data of generic media type application/json for example
        [HttpGet("{tourId}", Name = "GetTour")]
        public async Task<IActionResult> GetDefaultTour(Guid tourId) =>
            await GetTourGeneric<Tour>(tourId);


        [HttpGet("{tourId}", Name = "GetTour")]
        [RequestheaderMatchesMediaType("Accept", new[] {"application/vnd.isidore.tour+json" })]
        public async Task<IActionResult> GetTour(Guid tourId) =>
            await GetTourGeneric<Tour>(tourId);

        [HttpGet("{tourId}")]
        [RequestheaderMatchesMediaType("Accept", new[] { "application/vnd.isidore.tourwithestimatedprofits+json" })]
        public async Task<IActionResult> GetTourWithEstimatedProfits(Guid tourId) =>
            await GetTourGeneric<TourWithEstimatedProfits>(tourId);



        public async Task<IActionResult> GetTourGeneric<T>(Guid tourId) where T : class
        {
            var tourFromRepo = await _tourManagementRepository.GetTour(tourId);
            if (tourFromRepo == null) return BadRequest();
            var tour = Mapper.Map<T>(tourFromRepo);
            return Ok(tour);
        }
        #endregion HttpGet

        #region HttpPost

        [HttpPost]
        [RequestheaderMatchesMediaType("Content-Type", new[] { "application/json", "application/vnd.isidore.tourforcreation+json" })]
        public async Task<IActionResult> AddTour([FromBody]TourForCreation tour)
        {
            if (tour == null) return BadRequest();
            return await AddSpecificTour(tour);
        }

        [HttpPost]
        [RequestheaderMatchesMediaType("Content-Type", new[] { "application/vnd.isidore.tourwithmanagerforcreation+json" })]
        public async Task<IActionResult> AddTourWithManager([FromBody]TourWithManagerForCreation tour)
        {
            if (tour == null) return BadRequest();
            return await AddSpecificTour(tour);
        }

        public async Task<IActionResult> AddSpecificTour<T>(T tour) where T : class
        {
            var tourEntity = Mapper.Map<Entities.Tour>(tour); // map parameter to persistance model
            if (tourEntity.ManagerId == Guid.Empty) // if no managerid, hard code one
            {
                tourEntity.ManagerId = new Guid("g07ba678-b6e0-4307-afd9-e804c23b3cd3");
            }
            await _tourManagementRepository.AddTour(tourEntity); // add to repo
            if (!await _tourManagementRepository.SaveAsync()) // error message if fails on save
            {
                throw new Exception("Failed on save!");
            }
            var tourToReturn = Mapper.Map<Tour>(tourEntity); // need to remap to return to the client
            // 201 status plus creating the access route for the new tour
            return CreatedAtRoute("GetTour", new { tourId = tourToReturn.TourId }, tourToReturn);
        }

        #endregion HttpPost
    }
}
