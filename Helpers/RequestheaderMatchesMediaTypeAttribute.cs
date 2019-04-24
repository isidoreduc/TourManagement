using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace TourManagement.API.Helpers
{
    // action constraint attribute class; 
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public class RequestheaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
    {
        private readonly string _requestHeaderToMatch;
        private readonly string[] _mediaTypes;

        public RequestheaderMatchesMediaTypeAttribute(string requestHeaderToMatch, string[] mediaTypes)
        {
            _requestHeaderToMatch = requestHeaderToMatch;
            _mediaTypes = mediaTypes;
        }

        // specifies the stage value in which all http attributes run. our attribute runs in the same one, which is 0
        public int Order
        {
            get { return 0; }
        }

        // acccept routing to API resource or not based on the request header (example of
        // request header: 'content-type: application/json' = a dictionary, first the key, then the value)
        public bool Accept(ActionConstraintContext context)
        {
            // access request headers
            var requestHeaders = context.RouteContext.HttpContext.Request.Headers;
            // if request header does not match one of the accepted, return false
            // framework does not route to the action decorated with this attribute
            if (!requestHeaders.ContainsKey(_requestHeaderToMatch)) return false;
            // if it does, we check for media type to see if it is accepted
            foreach (var mediaType in _mediaTypes)
            {
                // we check for media types in the accepted header
                var headerValues = requestHeaders[_requestHeaderToMatch].ToString().Split(',').ToList();
                foreach (var headerValue in headerValues)
                {
                    // if the media type(fx. xml, json, custom) is in the list of allowed, return true
                    if (string.Equals(headerValue, mediaType, StringComparison.OrdinalIgnoreCase)) return true;
                }
            }

            return false;
        }
    }
}
