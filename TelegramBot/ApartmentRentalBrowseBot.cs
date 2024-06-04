using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Exceptions;
using ApartmentRental.RealEstateClient.Models.Response;
using ApartmentRental.WebAPI.Controllers;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.OpenApi.Services;

namespace TelegramBot
{
    public class ApartmentRentalBrowseBot
    {
        TelegramBotClient botClient = new TelegramBotClient("7168064259:AAENCQcW7-SiqqCXT7B0o5Nxg8oi7eqEicw");
        CancellationToken cancellationToken = new CancellationToken();
        ReceiverOptions receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
        public async Task Start()
        {
            botClient.StartReceiving(HandlerUpdateAsync, HandlerError, receiverOptions, cancellationToken);
            var botMe = await botClient.GetMeAsync();
            Console.WriteLine($"Бот {botMe.Username} почав працювати");
        }

        private Task HandlerError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMesaage = exception switch
            {
                ApiRequestException apiRequestException => $"Помилка в телеграм бот АПІ:\n {
                    apiRequestException.ErrorCode} \n {apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMesaage);
            return Task.CompletedTask;
        }

        string expecting;
        string city;
        string state_code;
        string limit;
        string price_min;
        string price_max;
        string beds_min;
        string beds_max;
        string property_type;
        int slider = 0;
        int fav_slider = 0;
        RootObject rootObject;
        Result card;
        List<SavedApartment> savedApartments;
        List<SavedApartment> mySavedApartments;
        SavedApartment fav_card;
        List<Search> searches;
        Search? mysearch;

        async Task ShowCardAsync(RootObject searchResults, int i, int chatid)
        {
            card = searchResults.data.home_search.results[i];
            var cardKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("⬅️", "left"),
                    InlineKeyboardButton.WithCallbackData("➡️", "right"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Save to favorites💚", "save favorite"),
                }
            });

            string livingArea = card.description.sqft_min != card.description.sqft_max ?
                $"Living area: {card.description.sqft_min}-{card.description.sqft_max} sq.ft. \n" :
                $"Living area: {card.description.sqft_min} sq.ft. \n";

            string beds = card.description.beds_min != card.description.beds_max ?
                $"Beds: {card.description.beds_min}-{card.description.beds_max} \n" :
                $"Beds: {card.description.beds_min} \n";

            string price = card.list_price_min != card.list_price_max ?
                $"Price: ${card.list_price_min}-{card.list_price_max}" :
                $"Price: ${card.list_price_min}";

            string cardText = $"{card.href} \n" + 
                $"Address: {card.location.county.name}, {card.location.county.state_code}," +
                $" {card.location.address.city}, {card.location.address.street_name}" +
                $" {card.location.address.street_suffix} {card.location.address.street_number} \n" +
                livingArea + beds + price;

            await botClient.SendTextMessageAsync(chatid, cardText, replyMarkup: cardKeyboard);
        }

        async Task ShowFavCardAsync(int i, int chatid)
        {
            fav_card = mySavedApartments[i];
            var cardKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("⬅️", "left favorite"),
                    InlineKeyboardButton.WithCallbackData("➡️", "right favorite"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Delete from favorites❌", "delete favorite"),
                }
            });

            string livingArea = fav_card.sqft_min != fav_card.sqft_max ?
                $"Living area: {fav_card.sqft_min}-{fav_card.sqft_max} sq.ft. \n" :
                $"Living area: {fav_card.sqft_min} sq.ft. \n";

            string beds = fav_card.beds_min != fav_card.beds_max ?
                $"Beds: {fav_card.beds_min}-{fav_card.beds_max} \n" :
                $"Beds: {fav_card.beds_min} \n";

            string price = fav_card.list_price_min != fav_card.list_price_max ?
                $"Price: ${fav_card.list_price_min}-{fav_card.list_price_max}" :
                $"Price: ${fav_card.list_price_min}";

            string cardText = $"{fav_card.href} \n" +
                $"Address: {fav_card.county_name}, {fav_card.state_code}," +
                $" {fav_card.city}, {fav_card.street_name}" +
                $" {fav_card.street_suffix} {fav_card.street_number} \n" +
                livingArea + beds + price;
            
            await botClient.SendTextMessageAsync(chatid, cardText, replyMarkup: cardKeyboard);
        }
        private async Task HandlerUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null) 
            {
                await HandlerMessageAsync(botClient, update.Message);
            }
            if (update.Type == UpdateType.CallbackQuery)
            {
                var callbackQuery = update.CallbackQuery;
                if (callbackQuery.Data == "save search")
                {
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            var request = new HttpRequestMessage
                            {
                                Method = HttpMethod.Put,
                                RequestUri = new Uri("https://localhost:7110/api/Apartment/SaveSearch?" +
                                $"city={city}&state_code={state_code}&limit={Int32.Parse(limit)}" +
                                $"&price_min={Int32.Parse(price_min)}&price_max={Int32.Parse(price_max)}" +
                                $"&beds_min={Int32.Parse(beds_min)}&beds_max={Int32.Parse(beds_max)}" +
                                $"&property_type={property_type}&user_id={callbackQuery.Message.Chat.Id}")
                            };
                            using (var response = await client.SendAsync(request))
                            {
                                response.EnsureSuccessStatusCode();
                            }
                        }
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Search saved");
                    }
                    catch
                    {

                    }
                }
                if (callbackQuery.Data == "continue")
                {
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            var request = new HttpRequestMessage
                            {
                                Method = HttpMethod.Get,
                                RequestUri = new Uri("https://localhost:7110/api/Apartment/GetApartmentsByFilter?" +
                                $"city={city}&state_code={state_code}&limit={Int32.Parse(limit)}" +
                                $"&price_min={Int32.Parse(price_min)}&price_max={Int32.Parse(price_max)}" +
                                $"&beds_min={Int32.Parse(beds_min)}&beds_max={Int32.Parse(beds_max)}" +
                                $"&property_type={property_type}")
                            };
                            using (var response = await client.SendAsync(request))
                            {
                                response.EnsureSuccessStatusCode();
                                var body = await response.Content.ReadAsStringAsync();
                                rootObject = JsonSerializer.Deserialize<RootObject>(body);
                            }
                        }
                        slider = 0;
                        await ShowCardAsync(rootObject, slider, (int)callbackQuery.Message.Chat.Id);
                    }
                    catch
                    {

                    }
                }
                if (callbackQuery.Data == "left")
                {
                    if (slider != 0) 
                    { 
                        try 
                        {
                            slider -= 1;
                            await botClient.DeleteMessageAsync((int)callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                            ShowCardAsync(rootObject, slider, (int)callbackQuery.Message.Chat.Id);
                        }
                        catch
                        {

                        }
                    }
                }
                if (callbackQuery.Data == "right")
                {
                    if (slider != rootObject.data.home_search.results.Length - 1)
                    {
                        try
                        {
                            slider += 1;
                            await botClient.DeleteMessageAsync((int)callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                            ShowCardAsync(rootObject, slider, (int)callbackQuery.Message.Chat.Id);
                        }
                        catch
                        {

                        }
                    }
                }
                if (callbackQuery.Data == "save favorite")
                {
                    try
                    {
                        List<SavedApartment> saved;
                        using (var client = new HttpClient())
                        {
                            var request = new HttpRequestMessage
                            {
                                Method = HttpMethod.Get,
                                RequestUri = new Uri("https://localhost:7110/api/Apartment/SavedApartments")
                            };
                            using (var response = await client.SendAsync(request))
                            {
                                response.EnsureSuccessStatusCode();
                                var body = await response.Content.ReadAsStringAsync();
                                saved = JsonSerializer.Deserialize<List<SavedApartment>>(body);
                            }
                        }

                        bool includes = false;
                        foreach (SavedApartment apt in saved)
                        {
                            if (apt.property_id == card.property_id && apt.user_id == callbackQuery.Message.Chat.Id.ToString())
                            {
                                includes = true;
                                break;
                            }
                        }

                        if (!includes)
                        {
                            using (var client = new HttpClient())
                            {

                                var request = new HttpRequestMessage
                                {
                                    Method = HttpMethod.Post,
                                    RequestUri = new Uri("https://localhost:7110/api/Apartment/SaveApartment?" +
                                    $"property_id={card.property_id}&href={card.href}" +
                                    $"&county_name={card.location.county.name}&state_code={card.location.county.state_code}" +
                                    $"&city={card.location.address.city}&street_name={card.location.address.street_name}" +
                                    $"&street_suffix={card.location.address.street_suffix}" +
                                    $"&street_number={card.location.address.street_number}" +
                                    $"&sqft_min={card.description.sqft_min}&sqft_max={card.description.sqft_max}" +
                                    $"&beds_min={card.description.beds_min}&beds_max={card.description.beds_max}" +
                                    $"&list_price_min={card.list_price_min}&list_price_max={card.list_price_max}" +
                                    $"&user_id={callbackQuery.Message.Chat.Id}")
                                };
                                using (var response = await client.SendAsync(request))
                                {
                                    response.EnsureSuccessStatusCode();
                                }
                            }
                        }
                    }
                    catch
                    {

                    }
                }
                if (callbackQuery.Data == "left favorite")
                {
                    if (fav_slider != 0)
                    {
                        try
                        {
                            fav_slider -= 1;
                            await botClient.DeleteMessageAsync((int)callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                            ShowFavCardAsync(fav_slider, (int)callbackQuery.Message.Chat.Id);
                        }
                        catch
                        {

                        }
                    }
                }
                if (callbackQuery.Data == "right favorite")
                {
                    if (fav_slider != mySavedApartments.Count - 1)
                    {
                        try
                        {
                            fav_slider += 1;
                            await botClient.DeleteMessageAsync((int)callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                            ShowFavCardAsync(fav_slider, (int)callbackQuery.Message.Chat.Id);
                        }
                        catch
                        {

                        }
                    }
                }
                if (callbackQuery.Data == "delete favorite")
                {
                    try
                    {
                        using (var client = new HttpClient())
                        {
                            var request = new HttpRequestMessage
                            {
                                Method = HttpMethod.Delete,
                                RequestUri = new Uri($"https://localhost:7110/api/Apartment/DeleteAppartmentById?" +
                                $"property_id={fav_card.property_id}&user_id={callbackQuery.Message.Chat.Id}")
                            };
                            using (var response = await client.SendAsync(request))
                            {
                                response.EnsureSuccessStatusCode();
                            }
                        }
                    }
                    catch
                    {

                    }
                    if (fav_slider != 0)
                    {
                        fav_slider -= 1;
                    }
                    try
                    {
                        for (int s = mySavedApartments.Count - 1; s >= 0; s--)
                        {
                            if (mySavedApartments[s].property_id == fav_card.property_id)
                            {
                                mySavedApartments.RemoveAt(s);
                            }
                        }
                    }
                    catch
                    {

                    }
                    await botClient.DeleteMessageAsync((int)callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);
                    
                    if (savedApartments != null && savedApartments.Count > 0)
                    {
                        fav_card = savedApartments[fav_slider];
                        ShowFavCardAsync(fav_slider, (int)callbackQuery.Message.Chat.Id);
                    }
                }
            }
        }

        private async Task HandlerMessageAsync(ITelegramBotClient botClient, Message message)
        {
            if (message != null && expecting == "city")
            {
                city = message.Text;
                await botClient.SendTextMessageAsync(message.Chat.Id, "State code:");
                expecting = "state_code";
                message = null;            
            }
            if (message != null && expecting == "state_code")
            {
                state_code = message.Text;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Limit:");
                expecting = "limit";
                message = null;            
            }
            if (message != null && expecting == "limit")
            {
                limit = message.Text;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Min price:");
                expecting = "price_min";
                message = null;            
            }
            if (message != null && expecting == "price_min")
            {
                price_min = message.Text;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Max price:");
                expecting = "price_max";
                message = null;            
            }
            if (message != null && expecting == "price_max")
            {
                price_max = message.Text;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Min beds count:");
                expecting = "beds_min";
                message = null;            
            }
            if (message != null && expecting == "beds_min")
            {
                beds_min = message.Text;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Max beds count:");
                expecting = "beds_max";
                message = null;            
            }
            if (message != null && expecting == "beds_max")
            {
                beds_max = message.Text;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Property type:");
                expecting = "property_type";
                message = null;            
            }
            if (message != null && expecting == "property_type")
            {
                property_type = message.Text;
                expecting = "";

                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Save search", "save search"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Continue", "continue"),
                    }
                });

                await botClient.SendTextMessageAsync(message.Chat.Id,
                    "Search parameters:\n" +
                    "City: " + city + "\n" +
                    "State code: " + state_code + "\n" +
                    "Limit: " + limit + "\n" +
                    "Min price: " + price_min + "\n" +
                    "Max price: " + price_max + "\n" +
                    "Min beds count: " + beds_min + "\n" +
                    "Max beds count: " + beds_max + "\n" +
                    "Property type: " + property_type + "\n",
                    replyMarkup: keyboard);

                message = null;

            }


            if (message != null && message.Text == "/start")
            {

                await botClient.SendTextMessageAsync(message.Chat.Id,
                    "Hello! 😉 \n" +
                    "This Telegram bot was created to search for apartments.\n" +
                    "Type /search to perform a search or \n/last_search to go to the last saved search. \n" +
                    "You can save apartments to favorites and then view them using the /favorites command.");
                return;
            }

            if (message != null && message.Text == "/search")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Input search parameters:");
                await botClient.SendTextMessageAsync(message.Chat.Id, "City:");
                expecting = "city";
                return;
            }

            if (message != null && message.Text == "/favorites")
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        var request = new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri("https://localhost:7110/api/Apartment/SavedApartments")
                        };
                        using (var response = await client.SendAsync(request))
                        {
                            response.EnsureSuccessStatusCode();
                            var body = await response.Content.ReadAsStringAsync();
                            savedApartments = JsonSerializer.Deserialize<List<SavedApartment>>(body);
                        }
                    }

                    mySavedApartments = new List<SavedApartment> { };
                    for (int s = 0; s < savedApartments.Count; s++)
                    {
                        if (savedApartments[s].user_id == message.Chat.Id.ToString())
                        {
                            mySavedApartments.Add(savedApartments[s]);
                        }
                    }

                    if (mySavedApartments.Count > 0)
                    {
                        ShowFavCardAsync(fav_slider, (int)message.Chat.Id);
                    }
                }
                catch
                {

                }
            }
            if (message != null && message.Text == "/last_search")
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        var request = new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri("https://localhost:7110/api/Apartment/GetSearchesList")
                        };
                        using (var response = await client.SendAsync(request))
                        {
                            response.EnsureSuccessStatusCode();
                            var body = await response.Content.ReadAsStringAsync();
                            searches = JsonSerializer.Deserialize<List<Search>>(body);
                        }
                    }

                    mysearch = null;
                    for (int s = 0; s < searches.Count; s++)
                    {
                        if (searches[s].user_id == message.Chat.Id.ToString())
                        {
                            mysearch = searches[s];
                            break;
                        }
                    }

                    if (mysearch != null)
                    {
                        expecting = "";
                        city = mysearch.city;
                        state_code = mysearch.state_code;
                        limit = mysearch.limit.ToString();
                        price_min = mysearch.price_min.ToString();
                        price_max = mysearch.price_max.ToString();
                        beds_min = mysearch.beds_min.ToString();
                        beds_max = mysearch.beds_max.ToString();
                        property_type = mysearch.property_type;

                        using (var client = new HttpClient())
                        {
                            var request = new HttpRequestMessage
                            {
                                Method = HttpMethod.Get,
                                RequestUri = new Uri("https://localhost:7110/api/Apartment/GetApartmentsByFilter?" +
                                $"city={city}&state_code={state_code}&limit={Int32.Parse(limit)}" +
                                $"&price_min={Int32.Parse(price_min)}&price_max={Int32.Parse(price_max)}" +
                                $"&beds_min={Int32.Parse(beds_min)}&beds_max={Int32.Parse(beds_max)}" +
                                $"&property_type={property_type}")
                            };
                            using (var response = await client.SendAsync(request))
                            {
                                response.EnsureSuccessStatusCode();
                                var body = await response.Content.ReadAsStringAsync();
                                rootObject = JsonSerializer.Deserialize<RootObject>(body);
                            }
                        }
                        slider = 0;
                        await ShowCardAsync(rootObject, slider, (int)message.Chat.Id);
                    }
                }
                catch
                {

                }
            }
        }
    }
}
