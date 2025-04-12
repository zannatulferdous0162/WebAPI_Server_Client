using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WorldCitiesAPIClientSite.Models;
using static WorldCitiesAPIClientSite.Controllers.CountryController;

namespace WorldCitiesAPIClientSite.Controllers
{
    public class MasterDetailController : Controller
    {
        private readonly string countryUrl = "https://localhost:40443/api/Countries";

        //public async Task<ActionResult> Index(int? page, int? pageSize)
        //{
        //    List<Country> countries = new List<Country>();
        //    using (HttpClient client = new HttpClient())
        //    {
        //        HttpResponseMessage response = await client.GetAsync(countryUrl);
        //        if (response.IsSuccessStatusCode)
        //        {
        //            string data = await response.Content.ReadAsStringAsync();
        //            var result = JsonConvert.DeserializeObject<CountryResponse>(data);
        //            countries = result.Data;
        //        }
        //    }
        //    // Pagination logic
        //    int defaultPageSize = 100;
        //    int finalPageSize = pageSize ?? defaultPageSize;
        //    ViewBag.CurrentPageSize = finalPageSize;
        //    int pageNumber = page ?? 1;

        //    return View(countries.ToPagedList(pageNumber, finalPageSize));
        //}

        public async Task<ActionResult> Index(int? page, int? pageSize)
        {
            List<Country> countries = new List<Country>();
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(countryUrl);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CountryResponse>(data);
                    countries = result.Data;
                }
            }

            // Convert to ViewModel (CountryWithCitiesViewModel)
            var viewModelList = countries.Select(c => new CountryWithCitiesViewModel
            {
                Id = c.Id,
                Name = c.Name,
                ISO2 = c.ISO2,
                ISO3 = c.ISO3,
                // If you need cities, you must fetch them here (additional API call)
                Cities = new List<City>() // Or fetch cities from API if needed
            }).ToList();

            // Pagination logic
            int defaultPageSize = 100;
            int finalPageSize = pageSize ?? defaultPageSize;
            ViewBag.CurrentPageSize = finalPageSize;
            int pageNumber = page ?? 1;

            return View(viewModelList.ToPagedList(pageNumber, finalPageSize));
        }

        // GET: MasterDetail/Create
        public ActionResult Create()
        {
            var model = new CountryWithCitiesViewModel
            {
                Country = new Country(),
                Cities = new List<City> { new City() } 
            };
            return View(model);
        }

        // POST: MasterDetail/Create
        [HttpPost]
        public async Task<ActionResult> Create(CountryWithCitiesViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (HttpClient client = new HttpClient())
                {
                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                    };

                    // 1. Serialize only the Country (not including Cities)
                    var countryContent = new StringContent(JsonConvert.SerializeObject(model.Country, settings), Encoding.UTF8, "application/json");

                    HttpResponseMessage countryResponse = await client.PostAsync("https://localhost:40443/api/Countries", countryContent);

                    if (countryResponse.IsSuccessStatusCode)
                    {
                        // Read the new Country ID from the response
                        var responseString = await countryResponse.Content.ReadAsStringAsync();
                        var createdCountry = JsonConvert.DeserializeObject<Country>(responseString);
                        int newCountryId = createdCountry.Id;

                        // 2. Post each City with the new CountryId
                        foreach (var city in model.Cities)
                        {
                            city.CountryId = newCountryId;

                            var cityContent = new StringContent(JsonConvert.SerializeObject(city, settings), Encoding.UTF8, "application/json");
                            await client.PostAsync("https://localhost:40443/api/Cities", cityContent);
                        }

                        return RedirectToAction("Index");
                    }
                }
            }

            return View(model);
        }
    }
 
}