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

namespace WorldCitiesAPIClientSite.Controllers
{
    public class CountryController : Controller
    {
        // GET: Country
        private readonly string baseUrl = "https://localhost:40443/api/Countries";
        public class CountryResponse
        {
            public List<Country> Data { get; set; }
        }
        // GET: Country
        public async Task<ActionResult> Index(int? page, int? pageSize)
        {
            List<Country> countries = new List<Country>();
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(baseUrl);
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CountryResponse>(data);
                    countries = result.Data;
                }
            }
            // Pagination logic
            int defaultPageSize = 100;
            int finalPageSize = pageSize ?? defaultPageSize;
            ViewBag.CurrentPageSize = finalPageSize;
            int pageNumber = page ?? 1;

            return View(countries.ToPagedList(pageNumber, finalPageSize));
        }
        // GET: Country/Create
        public ActionResult Create() => View();
        // POST: Country/Create
        [HttpPost]
        public async Task<ActionResult> Create(Country country)
        {
            if (!ModelState.IsValid)
            {
                return View(country); 
            }

            using (HttpClient client = new HttpClient())
            {
             
                var content = new StringContent(JsonConvert.SerializeObject(country), System.Text.Encoding.UTF8, "application/json");               
                HttpResponseMessage response = await client.PostAsync(baseUrl, content);            
                string result = await response.Content.ReadAsStringAsync();               
                System.Diagnostics.Debug.WriteLine(result);               
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                   
                    ViewBag.ErrorMessage = "Failed to create the country.";
                    return View(country);
                }
            }
        }
        // GET: Country/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            Country country = null;
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync($"{baseUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    country = JsonConvert.DeserializeObject<Country>(data);
                }
            }
            if (country == null)
            {
                return HttpNotFound();
            }
            return View(country);
        }

        // POST: Country/Edit/5
        [HttpPost]
        public async Task<ActionResult> Edit(Country country)
        {
            if (!ModelState.IsValid)
            {
                return View(country);  // Return the view with the current data if validation fails
            }

            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(country), System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync($"{baseUrl}/{country.Id}", content);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index"); // Redirect to Index if update is successful
                }
                else
                {
                    ViewBag.ErrorMessage = "Failed to update the country."; // Handle error
                    return View(country);
                }
            }
        }

        // GET: Country/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            Country country = null;
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync($"{baseUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    country = JsonConvert.DeserializeObject<Country>(data);
                }
            }
            return View(country);
        }

        // POST: Country/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.DeleteAsync($"{baseUrl}/{id}");
                if (response.IsSuccessStatusCode)
                    return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
        public async Task<ActionResult> Details(int id)
        {
            Country country = null;
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync($"{baseUrl}/{id}");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    country = JsonConvert.DeserializeObject<Country>(data);
                }
            }
            if (country == null)
            {
                return HttpNotFound(); 
            }
            return View(country); 
        }
    }
}