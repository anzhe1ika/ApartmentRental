using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApartmentRental.RealEstateClient.Models.Response
{
    public class SavedApartment
    {
        public string property_id { get; set; }
        public string href { get; set; }
        public string county_name { get; set; }
        public string state_code { get; set; }
        public string city { get; set; }
        public string street_name { get; set; }
        public string street_suffix { get; set; }
        public int street_number { get; set; }
        public int? sqft_min { get; set; }
        public int? sqft_max { get; set; }
        public int? beds_min { get; set; }
        public int? beds_max { get; set; }
        public int? list_price_min { get; set; }
        public int? list_price_max { get; set; }
        public string user_id { get; set; }
    }
}
