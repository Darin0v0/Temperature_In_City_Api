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

                            string weatherApiUrl = $"https://api.openweathermap.org/data/2.5/weather?lat={cityLatitude}&lon={cityLongitude}&appid={apiKey}&units=metric&lang=pl";
                            string airQualityApiUrl = $"http://api.openweathermap.org/data/2.5/air_pollution?lat={cityLatitude}&lon={cityLongitude}&appid={apiKey}&lang=pl";
                            string nextDayApiUrl = $"https://api.openweathermap.org/data/2.5/forecast?lat={cityLatitude}&lon={cityLongitude}&appid={apiKey}&units=metric&cnt=12";

                            HttpResponseMessage weatherResponse = await client.GetAsync(weatherApiUrl);
                            HttpResponseMessage airResponse = await client.GetAsync(airQualityApiUrl);
                            HttpResponseMessage nextDayResponse = await client.GetAsync(nextDayApiUrl);

                            if (weatherResponse.IsSuccessStatusCode && airResponse.IsSuccessStatusCode && nextDayResponse.IsSuccessStatusCode)
                            {
                                string weatherResponseBody = await weatherResponse.Content.ReadAsStringAsync();
                                string airQualityBody = await airResponse.Content.ReadAsStringAsync();
                                JObject airData = JObject.Parse(airQualityBody);
                                JObject weatherData = JObject.Parse(weatherResponseBody);

                                int? quality = (int?)airData["list"][0]["main"]["aqi"];
                                double temperature = (double)weatherData["main"]["temp"];
                                double tempFeels = (double)weatherData["main"]["feels_like"];
                                int visibility = (int)weatherData["visibility"];
                                double visibleKm = visibility / 1000.0;
                                double windSpeed = (double)weatherData["wind"]["speed"];
                                int? windDirection = (int?)weatherData["wind"]["deg"];
                                int? pressure = (int?)weatherData["main"]["pressure"];
                                int? cloudiness = (int?)weatherData["clouds"]["all"];
                                int? humidity = (int?)weatherData["main"]["humidity"];
                                string weatherDescription = (string)weatherData["weather"][0]["description"];
                                string weatherMain = (string)weatherData["weather"][0]["main"];
                                string qualityDescription = "";

                                Console.WriteLine($"Pogoda: {weatherMain}, {weatherDescription}");
                                Console.WriteLine($"W {cityName} jest {temperature} stopni, a odczuwalna temperatura to {tempFeels} stopni.");
                                Console.WriteLine($"Obszar widzialny: {visibleKm} km lub {visibility} m.");
                                Console.WriteLine($"Prędkość wiatru: {Math.Round((windSpeed * 3.6), 2)} km/h ({windSpeed} m/s), kierunek: {windDirection}°.");
                                Console.WriteLine($"Ciśnienie: {pressure} hPa.");
                                Console.WriteLine($"Zachmurzenie: {cloudiness} %.");
                                Console.WriteLine($"Wilgotność: {humidity} %.");

                                switch (quality)
                                {
                                    case 1:
                                        qualityDescription = "Wspaniała";
                                        break;
                                    case 2:
                                        qualityDescription = "Dobra";
                                        break;
                                    case 3:
                                        qualityDescription = "Średnia";
                                        break;
                                    case 4:
                                        qualityDescription = "Słaba";
                                        break;
                                    case 5:
                                        qualityDescription = "Tragiczna";
                                        break;
                                    default:
                                        qualityDescription = "Nieznana";
                                        break;
                                }

                                Console.WriteLine($"Jakość powietrza: {qualityDescription}");

                                Console.WriteLine("Pokazać prognozę pogody następne 36 godzin");
                                Console.WriteLine("1 - tak, 2 - nie");
                                int? input = int.Parse(Console.ReadLine());
                                if (input == 1)
                                {
                                    Console.WriteLine("Prognoza pogody na następne 36 godzin:");
                                    string nextDayResponseBody = await nextDayResponse.Content.ReadAsStringAsync();
                                    JObject nextDayData = JObject.Parse(nextDayResponseBody);

                                    foreach (var item in nextDayData["list"])
                                    {
                                        string dateTime = (string)item["dt_txt"];
                                        double temp = (double)item["main"]["temp"];
                                        string weather = (string)item["weather"][0]["description"];
                                        int pressure1 = (int)item["main"]["pressure"];
                                        int humidity1 = (int)item["main"]["humidity"];
                                        int cloudiness1 = (int)item["clouds"]["all"];
                                        Console.WriteLine($"Data: {dateTime}, Temperatura: {temp}°C, Pogoda: {weather}");
                                        Console.WriteLine($"Ciśnienie: {pressure1} hPa, Wilgotność: {humidity1}%, Zachmurzenie: {cloudiness1}%");
                                        if (dateTime.Contains("00:00:00"))
                                        {
                                            Console.WriteLine("- - - - - - - - - - Następny dzień - - - - - - - - - -");
                                        }

                                    }
                                }

                                Console.WriteLine("Pokazać dokładniejsze dane na temat jakości powietrza?");
                                Console.WriteLine("1 - tak, 2 - nie");
                                input = int.Parse(Console.ReadLine());
                                if (input == 1)
                                {
                                    double coQ = (double)airData["list"][0]["components"]["co"];
                                    Console.WriteLine($"CO: {coQ} μg/m³");
                                    double noQ = (double)airData["list"][0]["components"]["no"];
                                    Console.WriteLine($"NO: {noQ} μg/m³");
                                    double no2Q = (double)airData["list"][0]["components"]["no2"];
                                    Console.WriteLine($"NO2: {no2Q} μg/m³");
                                    double o3Q = (double)airData["list"][0]["components"]["o3"];
                                    Console.WriteLine($"O3: {o3Q} μg/m³");
                                    double so2Q = (double)airData["list"][0]["components"]["so2"];
                                    Console.WriteLine($"SO2: {so2Q} μg/m³");
                                    double pm2_5 = (double)airData["list"][0]["components"]["pm2_5"];
                                    Console.WriteLine($"PM2.5: {pm2_5} μg/m³");
                                    double pm10 = (double)airData["list"][0]["components"]["pm10"];
                                    Console.WriteLine($"PM10: {pm10} μg/m³");
                                    double nh3 = (double)airData["list"][0]["components"]["nh3"];
                                    Console.WriteLine($"NH3: {nh3} μg/m³");
                                    double tempmin = (double)weatherData["main"]["temp_min"];
                                    double tempmax = (double)weatherData["main"]["temp_max"];
                                    Console.WriteLine($"temperatura min: {tempmin}, max: {tempmax}");
                                    Console.WriteLine($"{cityName} znajduje się na: {cityLatitude}  :  {cityLongitude}");
                                    string baza = (string)weatherData["base"];
                                    Console.WriteLine($"Mierzone ze stacji: {baza}");
                                    int czas = (int)weatherData["timezone"];
                                    Console.WriteLine($"jesteś w +{czas / 3600} strefie czasowej");
                                    string sunrise = weatherData["sys"]["sunrise"].ToString();
                                    string sunset = weatherData["sys"]["sunset"].ToString();
                                    string sunsetTime = $"{sunset.Substring(0, 2)}:{sunset.Substring(2, 2)}:{sunset.Substring(4, 2)}";
                                    Console.WriteLine($"Czas zachodu słońca: {sunsetTime}");
                                    if (weatherData["rain"] != null && weatherData["rain"]["1h"] != null)
                                    {
                                        string rainperh = (string)weatherData["rain"]["1h"];
                                        Console.WriteLine($"Przez ostatnią godzinę napadało {rainperh}:metra deszczu.");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Brak danych o opadach deszczu w ciągu ostatniej godziny.");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Nie wybrano opcji.");
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
