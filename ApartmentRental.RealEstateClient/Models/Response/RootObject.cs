namespace ApartmentRental.RealEstateClient.Models.Response
{
    public class RootObject
    {
        public int status { get; set; }
        public Data data { get; set; }
    }

    public class Data
    {
        public HomeSearch home_search { get; set; }
    }

    public class HomeSearch
    {
        public int total { get; set; }
        public int count { get; set; }
        public Result[] results { get; set; }
    }

    public class Result
    {
        public string property_id { get; set; }
        public Photo[] photos { get; set; }
        public int? list_price_min { get; set; }
        public string href { get; set; }
        public Location location { get; set; }
        public string list_date { get; set; }
        public Description description { get; set; }
        public int? photo_count { get; set; }
        public object list_price { get; set; }
        public int? list_price_max { get; set; }
    }
    public class Location
    {
        public County county { get; set; }
        public Address address { get; set; }
        public Search_Areas[] search_areas { get; set; }
    }

    public class County
    {
        public string name { get; set; }
        public string state_code { get; set; }
        public string fips_code { get; set; }
    }

    public class Address
    {
        public string street_number { get; set; }
        public string state { get; set; }
        public string street_suffix { get; set; }
        public string state_code { get; set; }
        public string postal_code { get; set; }
        public string street_name { get; set; }
        public string country { get; set; }
        public string line { get; set; }
        public string city { get; set; }
    }

    public class Search_Areas
    {
        public string city { get; set; }
        public string state_code { get; set; }
    }

    public class Description
    {
        public int? sqft_min { get; set; }
        public int? beds_max { get; set; }
        public int? beds_min { get; set; }
        public string name { get; set; }
        public int? sqft { get; set; }
        public int? sqft_max { get; set; }
        public object beds { get; set; }
        public string type { get; set; }
    }

    public class Photo
    {
        public string href { get; set; }
    }
}
