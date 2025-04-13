using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
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
        //private readonly string baseUrl = "https://localhost:40443/api/Countries";

        private string countryData;

        public class CityResponse
        {
            public List<City> Data { get; set; }
        }
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
        public async Task<ActionResult> Edit(int id)
        {
            CountryWithCitiesViewModel vm = new CountryWithCitiesViewModel();

            using (HttpClient client = new HttpClient())
            {
                // Get country
                var countryResponse = await client.GetAsync($"{countryUrl}/{id}");
                if (countryResponse.IsSuccessStatusCode)
                {
                    string countryData = await countryResponse.Content.ReadAsStringAsync();
                    vm.Country = JsonConvert.DeserializeObject<Country>(countryData);
                }

                // Get cities of this country
                var citiesResponse = await client.GetAsync($"https://localhost:40443/api/Cities?countryId={id}");
                if (citiesResponse.IsSuccessStatusCode)
                {
                    string citiesData = await citiesResponse.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CityResponse>(citiesData);
                    vm.Cities = result.Data
                        .Where(c => c.CountryId == id)
                        .ToList();
                }
            }

            return View(vm); // Edit.cshtml
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(CountryWithCitiesViewModel vm, string DeletedCityIds)
        {
            using (HttpClient client = new HttpClient())
            {
                // Delete removed cities from DB
                if (!string.IsNullOrWhiteSpace(DeletedCityIds))
                {
                    var cityIds = DeletedCityIds
                        .Split(',')
                        .Select(id => Convert.ToInt32(id))
                        .ToList();

                    foreach (var cityId in cityIds)
                    {
                        await client.DeleteAsync($"https://localhost:40443/api/Cities/{cityId}");
                    }
                }

                // Update Country
                await client.PutAsJsonAsync($"{countryUrl}/{vm.Country.Id}", vm.Country);

                // Update/Add Cities
                foreach (var city in vm.Cities)
                {
                    if (city.Id > 0)
                        await client.PutAsJsonAsync($"https://localhost:40443/api/Cities/{city.Id}", city);
                    else
                        await client.PostAsJsonAsync("https://localhost:40443/api/Cities", city);
                }
            }

            return RedirectToAction("Index");
        }


        // GET: Country/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            CountryWithCitiesViewModel vm = new CountryWithCitiesViewModel();

            using (HttpClient client = new HttpClient())
            {
                // Get the country
                var countryResponse = await client.GetAsync($"{countryUrl}/{id}");
                if (countryResponse.IsSuccessStatusCode)
                {
                    string countryData = await countryResponse.Content.ReadAsStringAsync();
                    vm.Country = JsonConvert.DeserializeObject<Country>(countryData);
                }

                // Get related cities for this country only
                var citiesResponse = await client.GetAsync($"https://localhost:40443/api/Cities?countryId={id}");
                if (citiesResponse.IsSuccessStatusCode)
                {
                    string citiesData = await citiesResponse.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CityResponse>(citiesData);

                    // Filter only cities of this country
                    vm.Cities = result.Data
                        .Where(city => city.CountryId == id)
                        .ToList();
                }
            }

            return View(vm);
        }
        // POST: Country/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            using (HttpClient client = new HttpClient())
            {
                // Get cities of selected country
                var cityResponse = await client.GetAsync($"https://localhost:40443/api/Cities?countryId={id}");
                if (cityResponse.IsSuccessStatusCode)
                {
                    string cityData = await cityResponse.Content.ReadAsStringAsync();
                    var cityResult = JsonConvert.DeserializeObject<CityResponse>(cityData);

                    var citiesToDelete = cityResult.Data
                        .Where(c => c.CountryId == id)
                        .ToList();

                    // Delete only these cities
                    foreach (var city in citiesToDelete)
                    {
                        await client.DeleteAsync($"https://localhost:40443/api/Cities/{city.Id}");
                    }
                }

                // Then delete the country
                var countryDeleteResponse = await client.DeleteAsync($"{countryUrl}/{id}");
                if (countryDeleteResponse.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }

            // If anything fails
            return RedirectToAction("Index");
        }
        public async Task<ActionResult> Details(int id)
        {
            CountryWithCitiesViewModel vm = new CountryWithCitiesViewModel();

            using (HttpClient client = new HttpClient())
            {
                // Get Country by ID
                HttpResponseMessage countryResponse = await client.GetAsync($"{countryUrl}/{id}");
                if (countryResponse.IsSuccessStatusCode)
                {
                    string countryData = await countryResponse.Content.ReadAsStringAsync();
                    vm.Country = JsonConvert.DeserializeObject<Country>(countryData);
                }

                // Get Cities related to the selected Country
                HttpResponseMessage citiesResponse = await client.GetAsync($"https://localhost:40443/api/Cities?countryId={id}");
                if (citiesResponse.IsSuccessStatusCode)
                {
                    string citiesData = await citiesResponse.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CityResponse>(citiesData);
                    vm.Cities = result.Data
                        .Where(city => city.CountryId == id) // Optional safety check
                        .ToList();
                }
            }

            return View(vm);
        }

    }
}