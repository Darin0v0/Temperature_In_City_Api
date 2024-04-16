using System;
using System.Net.Http;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

class Program
{
    static async Task Main(string[] args)
    {
        string quitCommand = "quit";

        while (true)
        {
            Console.WriteLine("Podaj nazwe miasta lub 'quit' żeby wyjść :");
            string? cityName = Console.ReadLine();

            if (cityName.ToLower() == "quit")
            {
                break;
            }
            
            if (double.TryParse(cityName, out _))
            {
                Console.WriteLine("Nieprawidłowa nazwa miasta. Proszę podać nazwę miasta, a nie liczbę.");
                continue;
            }



            // Twój klucz API
            string apiKey = "32b8338c9c1fdc43d190e72ea7ce9b5f";

            string geoApiUrl = $"http://api.openweathermap.org/geo/1.0/direct?q={cityName}&limit=1&appid={apiKey}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage geoResponse = await client.GetAsync(geoApiUrl);

                    if (geoResponse.IsSuccessStatusCode)
                    {
                        string geoResponseBody = await geoResponse.Content.ReadAsStringAsync();
                        dynamic locationData = Newtonsoft.Json.JsonConvert.DeserializeObject(geoResponseBody);

                        double cityLatitude = locationData[0].lat;
                        double cityLongitude = locationData[0].lon;
                        Console.WriteLine($"{cityLatitude} {cityLongitude}");   
                        string weatherApiUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={cityLatitude}&lon={cityLongitude}&appid={apiKey}&units=metric";

                        HttpResponseMessage weatherResponse = await client.GetAsync(weatherApiUrl);

                        if (weatherResponse.IsSuccessStatusCode)
                        {
                            string weatherResponseBody = await weatherResponse.Content.ReadAsStringAsync();
                            dynamic weatherData = Newtonsoft.Json.JsonConvert.DeserializeObject(weatherResponseBody);
                            double temperature = weatherData.main.temp;
                            double wisillity = weatherData.visibility;
                            Console.WriteLine($"Aktualna temperatura w {cityName}: {temperature}°C");
                           // string ok = weatherData.weather;
                           // Console.WriteLine($"Pogoda to {ok}");
                            double wisible2 = wisillity / 1000;
                            Console.WriteLine($"Obszar widzialny to {wisible2} km lub {wisillity} m");
                            double wiatr1 = weatherData.wind.speed;
                            double wiatrkont = weatherData.wind.deg;
                            Console.WriteLine($"Prendkość wiatru to {wiatr1} km, od strony {wiatrkont}");       
                            int cisnienie = weatherData.main.pressure;
                            Console.WriteLine($"Jest {cisnienie} hPa");
                            double chmury = weatherData.clouds.all;
                            Console.WriteLine($"Zachurzenie w tym miejscu wynosi {chmury} %");
                            double wilgotość = weatherData.main.humidity;
                            Console.WriteLine($"Wilgotność wynośi {wilgotość} %");
                        }
                        else
                        {
                            Console.WriteLine("Nieudane zapytanie do API pogodowego");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Nieudane zapytanie do Geocoding API.");
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Błąd: {e.Message}");
                }
            }
        }
    }
}
