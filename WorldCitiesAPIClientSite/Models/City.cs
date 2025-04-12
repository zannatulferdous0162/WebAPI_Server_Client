using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorldCitiesAPIClientSite.Models
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public decimal Lat { get; set; }
        public decimal Lon { get; set; }
        public Country Country { get; set; }
    }
}