using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApartmentRental.RealEstateClient.Models.Response
{
    public class Search
    {
        public string city { get; set; }
        public string state_code { get; set; }
        public int? limit { get; set; }
        public int? price_min { get; set; }
        public int? price_max { get; set; }
        public int? beds_min { get; set; }
        public int? beds_max { get; set; }
        public string? property_type { get; set; }
        public string user_id { get; set; }
    }
}
