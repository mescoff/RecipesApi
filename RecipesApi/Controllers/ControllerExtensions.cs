using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RecipesApi.Controllers
{
    /// <summary>
    /// Controller Extensions
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Extension method to return a generic response for Getting multiple results from DB
        /// </summary>
        /// <param name="controller">The controller</param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static void AddCountToHeader(this ControllerBase controller, IEnumerable<object> result)
        {
            var resultCount = result.ToList().Count;          
            controller.Response.Headers.Add("x-items-Count", resultCount.ToString());
        }
    }
}
