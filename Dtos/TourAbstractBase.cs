using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TourManagement.API.Dtos
{
    // abstract as we don;t want it used directly
    // very usefull with validation, you only need to annotate here, and children will inherit validations
    public class TourAbstractBase : IValidatableObject
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Max. 200 characters")]
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            if (!(StartDate < EndDate))
            {
                yield return new ValidationResult("The start date should be before the end date", new[] { "Tour" });
            }
        }
    }
}
