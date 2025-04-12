using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WorldCitiesAPIClientSite.Models;
using static WorldCitiesAPIClientSite.Controllers.CountryController;

namespace WorldCitiesAPIClientSite.Controllers
{
    public class CityController : Controller
    {
        private readonly string cityUrl = "https://localhost:40443/api/Cities";
        private readonly string baseUrl = "https://localhost:40443/api/Countries";

        public class CityResponse
        {
            public List<City> Data { get; set; }
        }

        // GET: City
        public async Task<ActionResult> Index(int? page, int? pageSize)
        {
            List<City> cities = new List<City>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:40443/api/");
                var response = await client.GetAsync("Cities");

                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CityResponse>(data);
                    cities = result.Data;
                }
            }

            // Pagination logic
            int defaultPageSize = 100;
            int finalPageSize = pageSize ?? defaultPageSize;
            ViewBag.CurrentPageSize = finalPageSize;
            int pageNumber = page ?? 1;

            return View(cities.ToPagedList(pageNumber, finalPageSize));
        }

        // GET: City/Create
        public async Task<ActionResult> Create()
        {
            await LoadCountries();
            return View();
        }

        // POST: City/Create
        [HttpPost]
        public async Task<ActionResult> Create(City city)
        {
            if (ModelState.IsValid)
            {
                using (HttpClient client = new HttpClient())
                {
                    var content = new StringContent(JsonConvert.SerializeObject(city), System.Text.Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(cityUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                }
            }

            await LoadCountries();
            return View(city);
        }

        // GET: City/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            City city = null;
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync($"{cityUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    city = JsonConvert.DeserializeObject<City>(data);
                }
            }
            await LoadCountries();
            return View(city);
        }

        // POST: City/Edit/5
        [HttpPost]
        public async Task<ActionResult> Edit(City city)
        {
            if (ModelState.IsValid)
            {
                using (HttpClient client = new HttpClient())
                {
                    var content = new StringContent(JsonConvert.SerializeObject(city), System.Text.Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PutAsync($"{cityUrl}/{city.Id}", content);
                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            await LoadCountries();
            return View(city);
        }

        // GET: City/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            City city = null;
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync($"{cityUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    city = JsonConvert.DeserializeObject<City>(data);
                }
            }
            return View(city);
        }

        // POST: City/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.DeleteAsync($"{cityUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
        }

        // GET: City/Details/5
        public async Task<ActionResult> Details(int id)
        {
            City city = null;
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync($"{cityUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    city = JsonConvert.DeserializeObject<City>(data);
                }
            }

            if (city == null)
            {
                return HttpNotFound();
            }

            return View(city);
        }

        // Helper method to load countries and populate ViewBag
        private async Task LoadCountries()
        {
            List<Country> countries = new List<Country>();

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(baseUrl);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CountryController.CountryResponse>(data);
                    countries = result.Data;
                }
            }

            ViewBag.Countries = new SelectList(countries, "Id", "Name");
        }
    }
}
