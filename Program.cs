using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

class Program
{
    static async Task Main(string[] args)
    {
        string quitCommand = "quit";

        while (true)
        {
            Console.WriteLine("Podaj nazwę miasta lub 'quit' żeby wyjść:");
            string? cityName = Console.ReadLine();

            if (cityName?.ToLower() == quitCommand)
            {
                break;
            }

            if (double.TryParse(cityName, out _))
            {
                Console.WriteLine("Nieprawidłowa nazwa miasta. Proszę podać nazwę miasta, a nie liczbę.");
                continue;
            }

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
                        JArray locationData = JArray.Parse(geoResponseBody);

                        if (locationData.Count > 0)
                        {
                            double cityLatitude = (double)locationData[0]["lat"];
                            double cityLongitude = (double)locationData[0]["lon"];

                            string weatherApiUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={cityLatitude}&lon={cityLongitude}&appid={apiKey}&units=metric";
                            string airQualityApiUrl = $"http://api.openweathermap.org/data/2.5/air_pollution?lat={cityLatitude}&lon={cityLongitude}&appid={apiKey}";
                            string NexDayApiUrl = $"https://api.openweathermap.org/data/2.5/forecast?lat={cityLatitude}&lon={cityLongitude}&appid={apiKey}&units=metric";

                            HttpResponseMessage weatherResponse = await client.GetAsync(weatherApiUrl);
                            HttpResponseMessage AirResponse = await client.GetAsync(airQualityApiUrl);
                            HttpResponseMessage NextDay = await client.GetAsync(NexDayApiUrl);

                            if (weatherResponse.IsSuccessStatusCode && AirResponse.IsSuccessStatusCode && NextDay.IsSuccessStatusCode)
                            {
                                string weatherResponseBody = await weatherResponse.Content.ReadAsStringAsync();
                                string AirQualityBody = await AirResponse.Content.ReadAsStringAsync();
                                //string NextDayBody = await NextDay.Content.ReadAsStringAsync();

                                //JObject NextData = JObject.Parse(NextDayBody);
                                JObject AirData = JObject.Parse(AirQualityBody);
                                JObject weatherData = JObject.Parse(weatherResponseBody);

                                int Quality = (int)AirData["list"][0]["main"]["aqi"];
                                double temperature = (double)weatherData["main"]["temp"];
                                int visibility = (int)weatherData["visibility"];
                                double visibleKm = visibility / 1000.0;
                                double windSpeed = (double)weatherData["wind"]["speed"];
                                int windDirection = (int)weatherData["wind"]["deg"];
                                int pressure = (int)weatherData["main"]["pressure"];
                                int cloudiness = (int)weatherData["clouds"]["all"];
                                int humidity = (int)weatherData["main"]["humidity"];
                                string a = "";
                                Console.WriteLine($"Aktualna temperatura w {cityName}: {temperature}°C");
                                Console.WriteLine($"Obszar widzialny to {visibleKm} km lub {visibility} m");
                                Console.WriteLine($"Prędkość wiatru to {windSpeed} km/h, kierunek {windDirection}°");
                                Console.WriteLine($"Ciśnienie wynosi {pressure} hPa");
                                Console.WriteLine($"Zachmurzenie wynosi {cloudiness} %");
                                Console.WriteLine($"Wilgotność wynosi {humidity} %");
                                if(Quality == 1)
                                {
                                    a = "Wspaniała";
                                }
                                else if(Quality == 2)
                                {
                                    a = "Dobra";
                                }
                                else if(Quality == 3)
                                {
                                    a = "Średina";
                                }
                                else if (Quality == 4)
                                {
                                    a = "Słaba";
                                }
                                else if(Quality == 5)
                                {
                                    a = "Tragiczna";
                                }
                                Console.WriteLine($"Jakość powietsza {a}");
                                Console.WriteLine("Pokazać dokładniejsze dane dotyczące powietrza");
                                Console.WriteLine("1-tak, 2-nie");
                                int Input = int.Parse(Console.ReadLine());
                                if(Input == 1)
                                {
                                    int coQ = (int)AirData["list"][0]["components"]["co"];
                                    Console.WriteLine($"co: {coQ}");
                                    int noQ = (int)AirData["list"][0]["components"]["no"];
                                    Console.WriteLine($"no: {noQ}");
                                    int no2Q = (int)AirData["list"][0]["components"]["no2"];
                                    Console.WriteLine($"no2: {no2Q}");
                                    int o3Q = (int)AirData["list"][0]["components"]["o3"];
                                    Console.WriteLine($"o3: {o3Q}");
                                    int so2Q = (int)AirData["list"][0]["components"]["so2"];
                                    Console.WriteLine($"so2: {so2Q}");
                                    int pm2_5 = (int)AirData["list"][0]["components"]["pm2_5"];
                                    Console.WriteLine($"pm2-5: {pm2_5}");
                                    int pm10 = (int)AirData["list"][0]["components"]["pm10"];
                                    Console.WriteLine($"pm10: {pm10}");
                                    int nh3 = (int)AirData["list"][0]["components"]["nh3"];
                                    Console.WriteLine($"nh3: {nh3}");
                                }

                            }
                            else
                            {
                                Console.WriteLine("Nieudane zapytanie do API pogodowego.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Nie znaleziono danych dla podanego miasta.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Nieudane zapytanie do Geocoding API.");
                    }
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Błąd HttpRequest: {e.Message}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Błąd: {e.Message}");
                }
            }
        }
    }
}
