using ApartmentRental.RealEstateClient.Models.Response;
using Npgsql;
using System.Collections;
using System.ComponentModel;
using System.Data;

namespace ApartmentRental.WebAPI
{
    public class Database
    {
        NpgsqlConnection npgsqlConnection = new NpgsqlConnection(Constants.Connect);
        public async Task InsertApartmentAsync(SavedApartment apartment)
        {
            var sql = "insert into public.\"ApartmentRental\"(\"property_id\", \"href\", \"county_name\", " +
                "\"state_code\", \"city\", \"street_name\", \"street_suffix\", \"street_number\", " +
                "\"sqft_min\", \"sqft_max\", \"beds_min\", \"beds_max\", \"list_price_min\"," +
                " \"list_price_max\", \"user_id\") " +
                $"values(@property_id, @href, @county_name, @state_code, @city, @street_name, @street_suffix," +
                $" @street_number, @sqft_min, @sqft_max, @beds_min, @beds_max, @list_price_min, @list_price_max, @user_id);";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, npgsqlConnection);
            cmd.Parameters.AddWithValue("property_id", apartment.property_id);
            cmd.Parameters.AddWithValue("href", apartment.href);
            cmd.Parameters.AddWithValue("county_name", apartment.county_name);
            cmd.Parameters.AddWithValue("state_code", apartment.state_code);
            cmd.Parameters.AddWithValue("city", apartment.city);
            cmd.Parameters.AddWithValue("street_name", apartment.street_name);
            cmd.Parameters.AddWithValue("street_suffix", apartment.street_suffix);
            cmd.Parameters.AddWithValue("street_number", apartment.street_number);
            cmd.Parameters.AddWithValue("sqft_min", apartment.sqft_min);
            cmd.Parameters.AddWithValue("sqft_max", apartment.sqft_max);
            cmd.Parameters.AddWithValue("beds_min", apartment.beds_min);
            cmd.Parameters.AddWithValue("beds_max", apartment.beds_max);
            cmd.Parameters.AddWithValue("list_price_min", apartment.list_price_min);
            cmd.Parameters.AddWithValue("list_price_max", apartment.list_price_max);
            cmd.Parameters.AddWithValue("user_id", apartment.user_id);

            await npgsqlConnection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await npgsqlConnection.CloseAsync();
        }

        public async Task<List<SavedApartment>> SelectSavedListAsync()
        {
            List<SavedApartment> savedApartments = new List<SavedApartment>();
            await npgsqlConnection.OpenAsync();
            var sql = "select * from public.\"ApartmentRental\"";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, npgsqlConnection);
            NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string _property_id = reader.GetString(0);
                string _county_name = reader.GetString(1);
                string _state_code = reader.GetString(2);
                string _city = reader.GetString(3);
                string _street_name = reader.GetString(4);
                string _street_suffix = reader.GetString(5);
                int _street_number = reader.GetInt32(6);
                int _sqft_min = reader.GetInt32(7);
                int _sqft_max = reader.GetInt32(8);
                int _beds_min = reader.GetInt32(9);
                int _beds_max = reader.GetInt32(10);
                int _list_price_min = reader.GetInt32(11);
                int _list_price_max = reader.GetInt32(12);
                string _href = reader.GetString(13);
                string _user_id = reader.GetString(14);

                savedApartments.Add(new SavedApartment
                {
                    property_id = _property_id,
                    href = _href,
                    county_name = _county_name,
                    state_code = _state_code,
                    city = _city,
                    street_name = _street_name,
                    street_suffix = _street_suffix,
                    street_number = _street_number,
                    sqft_min = _sqft_min,
                    sqft_max = _sqft_max,
                    beds_min = _beds_min,
                    beds_max = _beds_max,
                    list_price_min = _list_price_min,
                    list_price_max = _list_price_max,
                    user_id = _user_id,
                });
            }
            await npgsqlConnection.CloseAsync();
            return savedApartments;
        }

        public async Task DeleteApartmentByIdAsync(string property_id, string user_id)
        {
            var sql = "delete from public.\"ApartmentRental\" where property_id = \'" + property_id + "\'" +
                "and user_id = \'" + user_id + "\';";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, npgsqlConnection);
            await npgsqlConnection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await npgsqlConnection.CloseAsync();
        }

        public async Task InsertSearchAsync(Search search)
        {
            var sql = $"delete from public.\"Searches\" where \"user_id\" = @user_id; \n insert into public.\"Searches\"(\"city\", \"state_code\"," +
                "\"limit\", \"price_min\", \"price_max\"," +
                "\"beds_min\", \"beds_max\", \"property_type\", \"user_id\")"
                + $"values (@city, @state_code, @limit, @price_min," +
                $" @price_max, @beds_min, @beds_max, @property_type, @user_id)";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, npgsqlConnection);
            cmd.Parameters.AddWithValue("city", search.city);
            cmd.Parameters.AddWithValue("state_code", search.state_code);
            cmd.Parameters.AddWithValue("limit", search.limit);
            cmd.Parameters.AddWithValue("price_min", search.price_min);
            cmd.Parameters.AddWithValue("price_max", search.price_max);
            cmd.Parameters.AddWithValue("beds_min", search.beds_min);
            cmd.Parameters.AddWithValue("beds_max", search.beds_max);
            cmd.Parameters.AddWithValue("property_type", search.property_type);
            cmd.Parameters.AddWithValue("user_id", search.user_id);

            await npgsqlConnection.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            await npgsqlConnection.CloseAsync();
        }

        public async Task<List<Search>> SelectSearchesListAsync()
        {
            List<Search> savedSearches = new List<Search>();
            await npgsqlConnection.OpenAsync();
            var sql = "select * from public.\"Searches\"";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, npgsqlConnection);
            NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string _city = reader.GetString(0);
                string _state_code = reader.GetString(1);
                int _limit = reader.GetInt32(2);
                int _price_min = reader.GetInt32(3);
                int _price_max = reader.GetInt32(4);
                int _beds_min = reader.GetInt32(5);
                int _beds_max = reader.GetInt32(6);
                string _property_type = reader.GetString(7);
                string _user_id = reader.GetString(8);

                savedSearches.Add(new Search
                {
                    city = _city,
                    state_code = _state_code,
                    limit = _limit,
                    price_min = _price_min,
                    price_max = _price_max,
                    beds_min = _beds_min,
                    beds_max = _beds_max,
                    property_type = _property_type,
                    user_id = _user_id
                });
            }
            await npgsqlConnection.CloseAsync();
            return savedSearches;
        }
    }
}
