using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
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
        [HttpGet("{tourId}")]
        public async Task<IActionResult> GetDefaultTour(Guid tourId) =>
            await GetTourGeneric<Tour>(tourId);


        [HttpGet("{tourId}", Name = "GetTour")]
        [RequestheaderMatchesMediaType("Accept", new[] { "application/vnd.isidore.tour+json" })]
        public async Task<IActionResult> GetTour(Guid tourId) =>
            await GetTourGeneric<Tour>(tourId);

        [HttpGet("{tourId}")]
        [RequestheaderMatchesMediaType("Accept", new[] { "application/vnd.isidore.tourwithestimatedprofits+json" })]
        public async Task<IActionResult> GetTourWithEstimatedProfits(Guid tourId) =>
            await GetTourGeneric<TourWithEstimatedProfits>(tourId);

        [HttpGet("{tourId}")]
        [RequestheaderMatchesMediaType("Accept", new[] { "application/vnd.isidore.tourwithshows+json" })]
        public async Task<IActionResult> GetTourWithShows(Guid tourId) =>
            await GetTourGeneric<TourWithShows>(tourId, true);

        [HttpGet("{tourId}")]
        [RequestheaderMatchesMediaType("Accept", new[] { "application/vnd.isidore.tourwithestimatedprofitsandshows+json" })]
        public async Task<IActionResult> GetTourWithEstimatedProfitsAndShows(Guid tourId) =>
            await GetTourGeneric<TourWithEstimatedProfitsAndShows>(tourId, true);


        public async Task<IActionResult> GetTourGeneric<T>(Guid tourId, bool includeShows = false) where T : class
        {
            var tourFromRepo = await _tourManagementRepository.GetTour(tourId, includeShows);
            if (tourFromRepo == null) return BadRequest();
            var tour = Mapper.Map<T>(tourFromRepo);
            return Ok(tour);
        }
        #endregion HttpGet

        #region HttpPost

        [HttpPost]
        public async Task<IActionResult> AddTourDefault([FromBody]TourForCreation tour) =>
            await AddSpecificTour(tour);


        [HttpPost]
        [RequestheaderMatchesMediaType("Content-Type", new[] { "application/json", "application/vnd.isidore.tourforcreation+json" })]
        public async Task<IActionResult> AddTour([FromBody]TourForCreation tour) =>
            await AddSpecificTour(tour);


        [HttpPost]
        [RequestheaderMatchesMediaType("Content-Type", new[] { "application/vnd.isidore.tourwithmanagerforcreation+json" })]
        public async Task<IActionResult> AddTourWithManager([FromBody]TourWithManagerForCreation tour) =>
            await AddSpecificTour(tour);


        [HttpPost]
        [RequestheaderMatchesMediaType("Content-Type", new[] { "application/vnd.isidore.tourwithshowsforcreation+json" })]
        public async Task<IActionResult> AddTourWithShows([FromBody]TourWithShowsForCreation tour) =>
            await AddSpecificTour(tour);


        [HttpPost]
        [RequestheaderMatchesMediaType("Content-Type", new[] { "application/vnd.isidore.tourwithmanagerandshowsforcreation+json" })]
        public async Task<IActionResult> AddTourWithManagerAndShows([FromBody]TourWithManagerAndShowsForCreation tour) =>
            await AddSpecificTour(tour);


        public async Task<IActionResult> AddSpecificTour<T>(T tour) where T : class
        {
            if (tour == null) return BadRequest();
            if (!ModelState.IsValid) return BadRequest(); // we make sure invalid objects are not passed through
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

        #region HttpPatch

        [HttpPatch("{tourId}")]
        public async Task<IActionResult> PartiallyUpdateTour(Guid tourId,
          [FromBody] JsonPatchDocument<TourForUpdate> jsonPatchDocument)
        {
            if (jsonPatchDocument == null)
            {
                return BadRequest();
            }

            var tourFromRepo = await _tourManagementRepository.GetTour(tourId);

            if (tourFromRepo == null)
            {
                return BadRequest();
            }

            var tourToPatch = Mapper.Map<TourForUpdate>(tourFromRepo);

            jsonPatchDocument.ApplyTo(tourToPatch, ModelState);

            //if (!ModelState.IsValid)
            //{
            //    return new UnprocessableEntityObjectResult(ModelState);
            //}

            if (!TryValidateModel(tourToPatch))
            {
                //return new UnprocessableEntityObjectResult(ModelState);
                return BadRequest();
            }

            Mapper.Map(tourToPatch, tourFromRepo);

            await _tourManagementRepository.UpdateTour(tourFromRepo);

            if (!await _tourManagementRepository.SaveAsync())
            {
                throw new Exception("Updating a tour failed on save.");
            }

            return NoContent();
        }

        #endregion HttpPatch
    }
}
