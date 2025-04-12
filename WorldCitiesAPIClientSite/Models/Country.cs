using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorldCitiesAPIClientSite.Models
{
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ISO2 { get; set; }
        public string ISO3 { get; set; }
    }
}