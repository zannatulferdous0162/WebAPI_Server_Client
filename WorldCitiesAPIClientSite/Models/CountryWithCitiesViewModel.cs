using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorldCitiesAPIClientSite.Models
{
    public class CountryWithCitiesViewModel
    {
        public int Id { get; set; }
        public int CountryId { get; set; }
        public string Name { get; set; }
        public string ISO2 { get; set; }
        public string ISO3 { get; set; }
        public Country Country { get; set; }
        public List<City> Cities { get; set; } = new List<City>();
        public CountryWithCitiesViewModel()
        {
            Country = new Country();
            Cities = new List<City>(); 
        }
    }
}