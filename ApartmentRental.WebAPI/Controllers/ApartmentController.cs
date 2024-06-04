using ApartmentRental.RealEstateClient.Models.Response;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Text.Json;

namespace ApartmentRental.WebAPI.Controllers
{
    // [Route("api/[controller]")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApartmentController : ControllerBase
    {
        private readonly string _address = Constants.Address;
        private readonly string _apikey = Constants.ApiKey;
        private readonly string _apihost = Constants.ApiHost;

        public ApartmentController()
        {

        }
        [HttpGet]
        public async Task<IActionResult> GetApartmentsByFilter(string city = "Detroit", string state_code = "MI",
            int limit = 42, int price_min = 100, int price_max = 1600,
            int beds_min = 1, int beds_max = 2, string property_type = "apartment")
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-RapidAPI-Key", _apikey);
                client.DefaultRequestHeaders.Add("X-RapidAPI-Host", _apihost);

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(_address + $"city={city}&state_code={state_code}&limit={limit}" +
                    $"&price_min={price_min}&price_max={price_max}&beds_min={beds_min}&beds_max={beds_max}&property_type={property_type}")
                };
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    var obj = JsonSerializer.Deserialize<RootObject>(body);
                    return Ok(obj);
                }
            }
        }

        [HttpGet]
        public List<SavedApartment> SavedApartments()
        {
            Database database = new Database();
            return database.SelectSavedListAsync().Result;
        }

        [HttpPost]
        public void SaveApartment(string property_id = "1", string href = "2", string county_name = "3", string state_code = "4", string city = "5",
            string street_name = "6", string street_suffix = "7", int street_number = 8, int sqft_min = 9, int sqft_max = 10, int beds_min = 11,
            int beds_max = 12, int list_price_min = 13, int list_price_max = 14, string user_id = "0")
        {
            SavedApartment savedApartment = new SavedApartment();

            savedApartment.property_id = property_id;
            savedApartment.href = href;
            savedApartment.county_name = county_name;
            savedApartment.state_code = state_code;
            savedApartment.city = city;
            savedApartment.street_name = street_name;
            savedApartment.street_suffix = street_suffix;
            savedApartment.street_number = street_number;
            savedApartment.sqft_min = sqft_min;
            savedApartment.sqft_max = sqft_max;
            savedApartment.beds_min = beds_min;
            savedApartment.beds_max = beds_max;
            savedApartment.list_price_min = list_price_min;
            savedApartment.list_price_max = list_price_max;
            savedApartment.user_id = user_id;

            Database database = new Database();
            database.InsertApartmentAsync(savedApartment);
        }

        [HttpDelete] public void DeleteAppartmentById(string property_id, string user_id)
        {
            Database database = new Database();
            database.DeleteApartmentByIdAsync(property_id, user_id);
        }

        [HttpGet]
        public List<Search> GetSearchesList()
        {
            Database database = new Database();
            return database.SelectSearchesListAsync().Result;
        }

        [HttpPut]
        public void SaveSearch(string city = "Detroit", string state_code = "MI",
            int limit = 42, int price_min = 100, int price_max = 1600,
            int beds_min = 1, int beds_max = 2, string property_type = "apartment", string user_id = "0")
        {
            Search search = new Search();

            search.city = city;
            search.state_code = state_code;
            search.limit = limit;
            search.price_min = price_min;
            search.price_max = price_max;
            search.beds_min = beds_min;
            search.beds_max = beds_max;
            search.property_type = property_type;
            search.user_id = user_id;

            Database database = new Database();
            database.InsertSearchAsync(search);
        }
    }
}
