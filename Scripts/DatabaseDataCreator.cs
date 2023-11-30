using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ripple
{
    public class DatabaseDataCreator : MonoBehaviour
    {
        public static readonly  Dictionary<string, List<string>> citiesByCountry = new Dictionary<string, List<string>>
        {
            { "Spanien", new List<string> { "Barcelona", "Madrid", "Valencia", "Sevilla" } },
            { "Italien", new List<string> { "Rom", "Venedig", "Mailand", "Florenz" } },
            { "Frankreich", new List<string> { "Paris", "Nizza", "Marseille", "Lyon" } },
            { "Griechenland", new List<string> { "Athen", "Thessaloniki", "Heraklion", "Rhodos" } },
            { "Türkei", new List<string> { "Istanbul", "Ankara", "Izmir", "Antalya" } },
            { "Thailand", new List<string> { "Bangkok", "Phuket", "Chiang Mai", "Pattaya" } },
            { "USA", new List<string> { "New York", "Los Angeles", "Miami", "Las Vegas" } },
            { "Mexiko", new List<string> { "Mexiko-Stadt", "Cancún", "Guadalajara", "Monterrey" } },
            { "Brasilien", new List<string> { "Rio de Janeiro", "São Paulo", "Salvador", "Brasília" } },
            { "Ägypten", new List<string> { "Kairo", "Alexandria", "Gizeh", "Luxor" } }
        };

        public static readonly Dictionary<string, (string airportCode, string airportName)> airports = new Dictionary<string, (string, string)>
        {
            {"Barcelona", ("LEBL", "Flughafen Barcelona")},
            {"Madrid", ("LEMD", "Flughafen Madrid")},
            {"Valencia", ("LEVC", "Flughafen Valencia")},
            {"Sevilla", ("LEZL", "Flughafen Sevilla")},
            {"Rom", ("LIRF", "Flughafen Rom-Fiumicino")},
            {"Venedig", ("LIPZ", "Flughafen Venedig Marco Polo")},
            {"Mailand", ("LIMC", "Flughafen Mailand Malpensa")},
            {"Florenz", ("LIRQ", "Flughafen Florenz Peretola")},
            {"Paris", ("LFPG", "Flughafen Paris-Charles-de-Gaulle")},
            {"Nizza", ("LFMN", "Flughafen Nizza Côte d'Azur")},
            {"Marseille", ("LFML", "Flughafen Marseille Provence")},
            {"Lyon", ("LFLL", "Flughafen Lyon-Saint Exupéry")},
            {"Athen", ("LGAV", "Flughafen Athen-Eleftherios Venizelos")},
            {"Thessaloniki", ("LGTS", "Flughafen Thessaloniki Makedonien")},
            {"Heraklion", ("LGIR", "Flughafen Heraklion Nikos Kazantzakis")},
            {"Rhodos", ("LGRP", "Flughafen Rhodos Diagoras")},
            {"Istanbul", ("LTFM", "Flughafen Istanbul")},
            {"Ankara", ("LTAC", "Flughafen Ankara Esenboğa")},
            {"Izmir", ("LTBJ", "Flughafen Izmir Adnan Menderes")},
            {"Antalya", ("LTAI", "Flughafen Antalya")},
            {"Bangkok", ("VTBS", "Flughafen Bangkok-Suvarnabhumi")},
            {"Phuket", ("VTSP", "Flughafen Phuket")},
            {"Chiang Mai", ("VTCC", "Flughafen Chiang Mai")},
            {"Pattaya", ("VTBU", "Flughafen Pattaya U-Tapao")},
            {"New York", ("JFK", "John F. Kennedy International Airport")},
            {"Los Angeles", ("KLAX", "Flughafen Los Angeles")},
            {"Miami", ("KMIA", "Miami International Airport")},
            {"Las Vegas", ("KLAS", "McCarran International Airport")},
            {"Mexiko-Stadt", ("MMMX", "Flughafen Mexiko-Stadt Benito Juárez")},
            {"Cancún", ("MMUN", "Flughafen Cancún")},
            {"Guadalajara", ("MMGL", "Flughafen Guadalajara Miguel Hidalgo y Costilla")},
            {"Monterrey", ("MMMY", "Flughafen Monterrey General Mariano Escobedo")},
            {"Rio de Janeiro", ("SBGL", "Flughafen Rio de Janeiro-Galeão")},
            {"São Paulo", ("SBGR", "Flughafen São Paulo/Guarulhos")},
            {"Salvador", ("SBSV", "Flughafen Salvador-Deputado Luís Eduardo Magalhães")},
            {"Brasília", ("SBBR", "Flughafen Brasília")},
            {"Kairo", ("HECA", "Flughafen Kairo")},
            {"Alexandria", ("HEAX", "Flughafen Alexandria El Nouzha")},
            {"Gizeh", ("SPX", "Flughafen Sphinx International")},
            {"Luxor", ("HELX", "Flughafen Luxor")}
        };

        private Dictionary<int, string> hotelAirportMapper = new();
        private Dictionary<int, List<int>> hotelHotelRoomMapper = new();
        private int totalHotelRooms = 0;

        [Button]
        private void StartCreateData()
        {
            StartCoroutine(CreateData());
        }

        private IEnumerator CreateData()
        {
            if (!DatabaseManager.Instance.IsConnected)
            {
                print("Not Connected, nothing happened");
            }
            else
            {
                #region Truncate

                print("Truncate Tables...");
                yield return null;
                DatabaseManager.Instance.TruncateTables();

                #endregion

                #region Generating Data

                print("Generating Data...");
                yield return null;

                #region Airlines

                print("Generating Airlines...");
                yield return null;

                List<string> airlineSqls = new();
                List<object[]> airlineParams = new();

                foreach (string airlineName in CreateAirlines(10))
                {
                    airlineSqls.Add($"INSERT INTO Airlines (name) VALUES (@param1)");
                    airlineParams.Add(new[] { airlineName });
                }

                print("Adding Airlines: " + DatabaseManager.Instance.ExecuteTransaction(airlineSqls, airlineParams));
                yield return null;

                #endregion

                #region Users

                print("Generating Users...");
                yield return null;

                List<string> userSqls = new();
                List<object[]> userParams = new();

                userSqls.Add($"INSERT INTO Users (username, password) VALUES (@param1, @param2)");
                userParams.Add(new[] { "admin", LoginManager.HashPassword("admin") });

                foreach ((string username, string password) in CreateUsers(100))
                {
                    userSqls.Add($"INSERT INTO Users (username, password) VALUES (@param1, @param2)");
                    userParams.Add(new[] { username, LoginManager.HashPassword(password) });
                }

                print("Adding Users: " + DatabaseManager.Instance.ExecuteTransaction(userSqls, userParams));
                yield return null;

                #endregion

                #region Loyalty Levels

                print("Generating LoyaltyLevels...");
                yield return null;

                List<string> loyaltyLevelsSql = new();
                List<object[]> loyaltyLevelsParams = new()
                {
                    new object[] { 0, "bronze" }, 
                    new object[] { 1000, "silver" },
                    new object[] { 2000, "gold" },
                    new object[] { 3000, "diamond" }
                };

                for (int i = 0; i < loyaltyLevelsParams.Count; i++)
                {
                    loyaltyLevelsSql.Add("INSERT INTO LoyaltyLevels (min_points, level_name) VALUES (@param1, @param2)");
                }

                print("Adding LoyaltyLevels: " + DatabaseManager.Instance.ExecuteTransaction(loyaltyLevelsSql, loyaltyLevelsParams));
                yield return null;

                #endregion

                #region RoomTypes

                print("Generating RoomTypes...");
                yield return null;

                List<string> roomTypesSql = new();
                List<object[]> roomTypesParams = new();

                List<(string name, int maxOccupancy, string description)> predefinedRoomTypes = new List<(string, int, string)>
                {
                    ("Einbettzimmer", 1, "Gemütliches Zimmer mit einem Einzelbett."),
                    ("Zweibettzimmer", 2, "Komfortables Zimmer mit zwei Einzelbetten."),
                    ("Doppelzimmer", 2, "Elegantes Zimmer mit einem großen Doppelbett."),
                    ("Suite", 4, "Luxuriöse Suite mit separatem Wohnbereich und allen Annehmlichkeiten."),
                    ("Familienzimmer", 4, "Geräumiges Zimmer, perfekt für Familien."),
                    ("Studio", 2, "Modernes Studio mit einer kleinen Küchenzeile."),
                    ("Penthouse", 6, "Exklusives Penthouse mit atemberaubender Aussicht."),
                    ("Balkonzimmer", 2, "Zimmer mit privatem Balkon und schöner Aussicht."),
                    ("Loft", 3, "Stilvolles Loft mit hohen Decken und modernem Design."),
                    ("Königssuite", 5, "Prächtige Suite mit extra großen Betten und Premium-Annehmlichkeiten."),
                    ("Queenszimmer", 2, "Elegantes Zimmer mit einem Queensize-Bett."),
                    ("Deluxe Zimmer", 3, "Luxuriöses Zimmer mit zusätzlichem Platz und Premium-Annehmlichkeiten."),
                    ("Dachgeschoss", 2, "Gemütliches Dachgeschosszimmer mit charakteristischen Holzbalken."),
                    ("Bungalow", 4, "Privater Bungalow mit direktem Zugang zum Garten oder Strand."),
                    ("Villa", 8, "Luxusvilla mit mehreren Schlafzimmern und einem privaten Pool.")
                };

                foreach ((string name, int maxOccupancy, string description) in predefinedRoomTypes)
                {
                    roomTypesSql.Add($"INSERT INTO RoomTypes (name, description, max_occupancy) VALUES (@param1, @param2, @param3)");
                    roomTypesParams.Add(new object[] { name, description, maxOccupancy });
                }

                print("Adding RoomTypes: " + DatabaseManager.Instance.ExecuteTransaction(roomTypesSql, roomTypesParams));
                yield return null;

                #endregion

                #region Airports

                print("Generating Airports...");
                yield return null;

                List<string> airportSql = new List<string>();
                List<object[]> airportParams = new List<object[]>();

                foreach (KeyValuePair<string, (string airportCode, string airportName)> kvp in airports)
                {
                    airportSql.Add("INSERT INTO Airports (airport_code, airport_name) VALUES (@param1, @param2)");
                    airportParams.Add(new object[] { kvp.Value.airportCode, kvp.Value.airportName });
                }

                print("Adding Airports: " + DatabaseManager.Instance.ExecuteTransaction(airportSql, airportParams));
                yield return null;

                #endregion

                #region Countries

                print("Generating Countries...");
                yield return null;

                List<string> countriesSql = new();
                List<object[]> countriesParams = new();

                foreach (KeyValuePair<string, List<string>> kvp in citiesByCountry)
                {
                    countriesSql.Add("INSERT INTO Countries (country_name) VALUES (@param1)");
                    countriesParams.Add(new object[] { kvp.Key });
                }

                print("Adding Countries: " + DatabaseManager.Instance.ExecuteTransaction(countriesSql, countriesParams));
                yield return null;

                #endregion

                #region Cities

                print("Generating Cities...");
                yield return null;

                List<string> citiesSql = new();
                List<object[]> citiesParams = new();

                int countryCounter = 1;
                foreach (KeyValuePair<string, List<string>> kvp in citiesByCountry)
                {
                    foreach (string city in kvp.Value)
                    {
                        citiesSql.Add("INSERT INTO Cities (city_name, country_id, nearest_airport) VALUES (@param1, @param2, @param3)");
                        citiesParams.Add(new object[] { city, countryCounter, airports[city].airportCode });
                    }

                    countryCounter++;
                }

                print("Adding Cities: " + DatabaseManager.Instance.ExecuteTransaction(citiesSql, citiesParams));
                yield return null;

                #endregion

                #region Hotels

                print("Generating Hotels...");
                yield return null;

                List<string> hotelsSql = new();
                List<object[]> hotelsParams = new();

                foreach ((string hotelName, string hotelDescription, string address, string zipCode, int cityid, int countryid) in CreateHotels(10, airports))
                {
                    hotelsSql.Add("INSERT INTO Hotels (name, description, address, zip_code, city_id, country_id) VALUES (@param1, @param2, @param3, @param4, @param5, @param6)");
                    hotelsParams.Add(new object[] { hotelName, hotelDescription, address, zipCode, cityid, countryid});
                }

                print("Adding Hotels: " + DatabaseManager.Instance.ExecuteTransaction(hotelsSql, hotelsParams));
                yield return null;

                #endregion

                #region HotelRooms

                print("Generating HotelRooms...");
                yield return null;

                List<string> hotelRoomsSql = new();
                List<object[]> hotelRoomsParams = new();

                for (int i = 0; i < hotelsSql.Count; i++)
                {
                    foreach ((int hotelId, int roomTypeId, int roomNumber, float price) in CreateHotelRooms(i + 1, roomTypesSql.Count, 10, 20))
                    {
                        hotelRoomsSql.Add("INSERT INTO HotelRooms (hotel_id, room_type_id, room_number, price) VALUES (@param1, @param2, @param3, @param4)");
                        hotelRoomsParams.Add(new object[] {hotelId, roomTypeId, roomNumber, price});
                    }
                }

                print("Adding HotelRooms: " + DatabaseManager.Instance.ExecuteTransaction(hotelRoomsSql, hotelRoomsParams));
                yield return null;

                #endregion

                #region Flights

                print("Generating Flights...");
                yield return null;

                List<string> flightsSql = new List<string>();
                List<object[]> flightsParams = new List<object[]>();

                foreach ((int airlineId, string departure, string arrival, System.DateTime departureTime, System.DateTime arrivalTime, int maxPassengers, float price) 
                    in CreateFlights(50, airlineSqls.Count, airports))
                {
                    flightsSql.Add("INSERT INTO Flights (airline_id, departure, arrival, departure_time, arrival_time, max_passengers, price) VALUES (@param1, @param2, @param3, @param4, @param5, @param6, @param7)");
                    flightsParams.Add(new object[] { airlineId, departure, arrival, departureTime, arrivalTime, maxPassengers, price});
                }

                print("Adding Flights: " + DatabaseManager.Instance.ExecuteTransaction(flightsSql, flightsParams));
                yield return null;

                #endregion

                #region Bookings

                print("Generating Bookings...");
                yield return null;

                List<string> bookingsSql = new List<string>();
                List<object[]> bookingsParams = new List<object[]>();

                foreach ((int hotelRoomId, int userId, System.DateTime startDate, System.DateTime endDate, string status, long outboundFlightId, long returnFlightId, int persons) in CreateBookings(10, hotelRoomsSql.Count, userSqls.Count, airlineSqls.Count, airports))
                {
                    bookingsSql.Add("INSERT INTO Bookings (hotel_room_id, user_id, start_date, end_date, status, outbound_flight_id, return_flight_id, persons) VALUES (@param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8)");
                    bookingsParams.Add(new object[] { hotelRoomId, userId, startDate, endDate, status, outboundFlightId, returnFlightId, persons });
                }

                print("Adding Bookings: " + DatabaseManager.Instance.ExecuteTransaction(bookingsSql, bookingsParams));
                yield return null;

                #endregion

                #region HotelReviews

                print("Generating HotelReviews...");
                yield return null;

                List<string> hotelRewievsSql = new List<string>();
                List<object[]> hotelReviewsParams = new List<object[]>();

                foreach ((int hotelId, int userId, int rating, string title, string reviewText) in CreateHotelReviews(30, hotelsSql.Count, userSqls.Count))
                {
                    hotelRewievsSql.Add("INSERT INTO HotelReviews (hotel_id, user_id, rating, title, comment) VALUES (@param1, @param2, @param3, @param4, @param5)");
                    hotelReviewsParams.Add(new object[] { hotelId, userId, rating, title, reviewText });
                }

                print("Adding HotelReviews: " + DatabaseManager.Instance.ExecuteTransaction(hotelRewievsSql, hotelReviewsParams));
                yield return null;

                #endregion

                #region SpecialOffers

                print("Generating SpecialOffers...");
                yield return null;

                List<string> specialOffersSql = new List<string>();
                List<object[]> specialOffersParams = new List<object[]>();

                foreach ((int hotelId, System.DateTime validityStartDate, System.DateTime validityEndDate, double discountPercent, string code) in CreateSpecialOffers(2, hotelsSql.Count, 25, 100))
                {
                    specialOffersSql.Add("INSERT INTO SpecialOffers (hotel_id, validity_start_date, validity_end_date, discount_percent, code) VALUES (@param1, @param2, @param3, @param4, @param5)");
                    specialOffersParams.Add(new object[] { hotelId, validityStartDate, validityEndDate, discountPercent, code });
                }

                print("Adding SpecialOffers: " + DatabaseManager.Instance.ExecuteTransaction(specialOffersSql, specialOffersParams));
                yield return null;

                #endregion

                #region PaymentInfo

                print("Generating PaymentInfo...");
                yield return null;

                List<string> paymentInfoSql = new List<string>();
                List<object[]> paymentInfoParams = new List<object[]>();

                foreach ((int bookingId, string paymentMethod, string paymentStatus, float price, int? discountId) in CreatePaymentInfos(bookingsSql.Count, specialOffersSql.Count))
                {
                    paymentInfoSql.Add("INSERT INTO PaymentInfo (booking_id, payment_method, payment_status, price, used_discount_id) VALUES (@param1, @param2, @param3, @param4, @param5)");
                    paymentInfoParams.Add(new object[] { bookingId, paymentMethod, paymentStatus, price, discountId });
                }

                print("Adding PaymentInfos: " + DatabaseManager.Instance.ExecuteTransaction(paymentInfoSql, paymentInfoParams));
                yield return null;

                #endregion

                #endregion

                print("## FINISHED ##");
            }
        }

        private List<string> CreateAirlines(int count)
        {
            List<string> Prefixes = new List<string>
            {
                "Nord", "Süd", "Ost", "West", "Zentral", "Global", "International", "Sky", "Ocean", "Continental"
            };

            List<string> MainNames = new List<string>
            {
                "Air", "Flug", "Fluglinien", "Airlines", "Airways", "Jet", "Trans", "Fly", "Voyage", "Journey"
            };

            List<string> Suffixes = new List<string>
            {
                "Express", "Elite", "Prime", "Lux", "First", "Choice", "Wings", "Travel", "Journey", "Voyager"
            };

            HashSet<string> generatedNames = new HashSet<string>();

            for (int i = 0; i < count; i++)
            {
                string prefix = Prefixes[Random.Range(0, Prefixes.Count)];
                string main = MainNames[Random.Range(0, MainNames.Count)];
                string suffix = Suffixes[Random.Range(0, Suffixes.Count)];

                string name = $"{prefix} {main} {suffix}";

                generatedNames.Add(name);
            }

            return new List<string>(generatedNames);
        }

        private List<(string username, string password)> CreateUsers(int count)
        {
            string[] prefixes = {
                "Traveler", "Explorer", "Tourist", "Adventurer", "Journeyer",
                "Voyager", "Wanderer", "Nomad", "Jetsetter", "Roamer",
                "Trekker", "Pilgrim", "Wayfarer", "Navigator", "Pathfinder",
                "Rambler", "Drifter", "Commute", "Expeditionist", "Odyssey",
                "Tripper", "Sightseer", "Visitor", "Excursionist", "Cruiser"
            };

            string[] midpart1 = {
                "Sky", "Ocean", "Mountain", "City", "Beach",
                "Forest", "Desert", "River", "Valley", "Island",
                "Hill", "Lake", "Meadow", "Canyon", "Rainforest",
                "Tundra", "Jungle", "Prairie", "Savannah", "Delta",
                "Reef", "Cove", "Harbor", "Peninsula", "Archipelago"
            };

            string[] midpart2 = {
                "Seeker", "Lover", "Fan", "Enthusiast", "Aficionado",
                "Expert", "Connoisseur", "Buff", "Whiz", "Maven",
                "Admirer", "Devotee", "Follower", "Fancier", "Patron",
                "Supporter", "Fanatic", "Addict", "Freak", "Geek",
                "Junkie", "Nut", "Zealot", "Maniac", "Groupie"
            };

            string[] suffixes = { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10" };

            HashSet<string> generatedUsernames = new HashSet<string>();
            List<(string username, string password)> credentialsList = new List<(string username, string password)>();

            while (credentialsList.Count < count)
            {
                string username = prefixes[Random.Range(0, prefixes.Length)] +
                                  midpart1[Random.Range(0, midpart1.Length)] +
                                  midpart2[Random.Range(0, midpart2.Length)] +
                                  suffixes[Random.Range(0, suffixes.Length)];

                if (generatedUsernames.Contains(username))
                    continue;  // Überspringen, wenn der Benutzername bereits generiert wurde

                generatedUsernames.Add(username);

                const string passwordChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
                char[] passwordArray = new char[8];
                for (int i = 0; i < passwordArray.Length; i++)
                {
                    passwordArray[i] = passwordChars[Random.Range(0, passwordChars.Length)];
                }
                string password = new string(passwordArray);

                credentialsList.Add((username, password));
            }

            return credentialsList;
        }

        private List<(string hotelName, string hotelDescription, string address, string zipCode, int cityid, int countryid)> CreateHotels(int count, Dictionary<string, (string airportCode, string airportName)> airports)
        {
            List<string> countries = new List<string>
            {
                "Spanien", "Italien", "Frankreich", "Griechenland", "Türkei", "Thailand", "USA", "Mexiko", "Brasilien", "Ägypten"
            };

            Dictionary<string, List<string>> streetTypesByCountry = new Dictionary<string, List<string>>
            {
                { "Spanien", new List<string> { "Calle", "Avenida", "Paseo" } },
                { "Italien", new List<string> { "Via", "Viale", "Corso" } },
                { "Frankreich", new List<string> { "Rue", "Avenue", "Boulevard" } },
                { "Griechenland", new List<string> { "Odós", "Leóf" } },
                { "Türkei", new List<string> { "Sokak", "Caddesi" } },
                { "Thailand", new List<string> { "Thanon", "Soi" } },
                { "USA", new List<string> { "Street", "Avenue", "Boulevard" } },
                { "Mexiko", new List<string> { "Calle", "Avenida" } },
                { "Brasilien", new List<string> { "Rua", "Avenida" } },
                { "Ägypten", new List<string> { "Share", "Tariq" } }
            };

            List<string> hotelPrefixes = new List<string>
            {
                "Grand", "Royal", "Lux", "Seaside", "Sunrise", "Golden", "Silver", "Historic", "Modern", "Elite"
            };

            List<string> hotelMidParts = new List<string>
            {
                "Central", "Downtown", "Uptown", "Beach", "Garden", "Hillside", "Lakeside", "Riverside", "City", "Harbor"
            };

            List<string> hotelSuffixes = new List<string>
            {
                "Resort", "Hotel", "Lodge", "Inn", "Suites", "Palace", "Retreat", "Villa", "Place", "Stay"
            };

            HashSet<string> hotelNames = new HashSet<string>();

            List<(string hotelName, string hotelDescription, string address, string zipCode, int cityid, int countryid)> hotels = new();

            for (int i = 0; i < count; i++)
            {
                int countryId = Random.Range(1, citiesByCountry.Count + 1);
                int cityId = ((countryId - 1) * 4) + Random.Range(1, 5);

                string country = citiesByCountry.Keys.ElementAt(countryId - 1);
                string city = citiesByCountry[country][cityId - ((countryId - 1) * 4) - 1];
                string streetType = streetTypesByCountry[country][Random.Range(0, streetTypesByCountry[country].Count)];
                string address = $"{Random.Range(1, 200)} {city} {streetType} ";
                string zipCode = Random.Range(10000, 99999).ToString();

                string prefix = hotelPrefixes[Random.Range(0, hotelPrefixes.Count)];
                string midPart = hotelMidParts[Random.Range(0, hotelMidParts.Count)];
                string suffix = hotelSuffixes[Random.Range(0, hotelSuffixes.Count)];
                string hotelName = $"{prefix} {midPart} {city} {suffix}";

                string hotelDescription = CreateHotelDescription(city, country);

                if (hotelNames.Contains(hotelName))
                    continue;

                hotels.Add((hotelName, hotelDescription, address, zipCode, cityId, countryId));
                hotelAirportMapper.Add(i + 1, airports[city].airportCode);
            }

            return hotels;
        }

        private string CreateHotelDescription(string cityName, string countryName)
        {
            string[] adjectives = { "gemütlich", "luxuriös", "atemberaubend", "elegant", "modern", "rustikal", "charmantes", "exklusiv", "idyllisch", "stilvoll" };
            string[] features = { "einem Pool", "einer Sauna", "kostenlosem WLAN", "einem Fitnessstudio", "einem Restaurant", "einer Bar", "einem Spa-Bereich", "einem Kinderspielplatz", "einem Business-Center", "einem Kino" };
            string[] hotelTypes = { "Familienhotel", "Businesshotel", "Boutique-Hotel", "Resort", "Hostel", "Bed & Breakfast", "Aparthotel", "Gästehaus", "Motel", "Landhotel" };

            string adjective = adjectives[Random.Range(0, adjectives.Length)];
            string feature1 = features[Random.Range(0, features.Length)];
            string feature2 = features[Random.Range(0, features.Length)];
            string hotelType = hotelTypes[Random.Range(0, hotelTypes.Length)];

            while (feature1 == feature2)
            {
                feature2 = features[Random.Range(0, features.Length)];
            }

            string[] templates = 
            {
                $"Willkommen in unserem {adjective}en {hotelType} in {cityName}, {countryName}! Wir bieten Zimmer mit {feature1} und {feature2}.",
                $"Entdecken Sie unser {adjective}es {hotelType} in {cityName}! Genießen Sie {feature1} und {feature2} während Ihres Aufenthalts.",
                $"Ihr Aufenthalt in {cityName} wird unvergesslich in unserem {adjective}en {hotelType} mit {feature1} und {feature2}.",
                $"Erleben Sie {cityName} in unserem {adjective}en {hotelType}. Wir bieten {feature1} und {feature2}.",
                $"Unser {adjective}es {hotelType} in {cityName} bietet {feature1} und {feature2} für einen angenehmen Aufenthalt.",
                $"Genießen Sie den Komfort unseres {adjective}en {hotelType}s in {cityName} mit {feature1} und {feature2}.",
                $"In {cityName} erwartet Sie unser {adjective}es {hotelType} mit {feature1} und {feature2}.",
                $"Für einen unvergesslichen Aufenthalt in {cityName}, wählen Sie unser {adjective}es {hotelType} mit {feature1} und {feature2}.",
                $"Unser {adjective}es {hotelType} in {cityName} ist ausgestattet mit {feature1} und {feature2}.",
                $"Erleben Sie den Luxus unseres {adjective}en {hotelType}s in {cityName} mit {feature1} und {feature2}.",
                $"Ihr perfekter Aufenthalt in {cityName} beginnt in unserem {adjective}en {hotelType} mit {feature1} und {feature2}.",
                $"Entspannen Sie in unserem {adjective}en {hotelType} in {cityName}, ausgestattet mit {feature1} und {feature2}.",
                $"Unser {adjective}es {hotelType} in {cityName} bietet Ihnen {feature1} und {feature2}.",
                $"Wählen Sie für Ihren nächsten Aufenthalt in {cityName} unser {adjective}es {hotelType} mit {feature1} und {feature2}.",
                $"Unser {adjective}es {hotelType} in {cityName} ist der ideale Ort für Reisende, die {feature1} und {feature2} schätzen.",
                $"Verbringen Sie erholsame Tage in unserem {adjective}en {hotelType} in {cityName} mit {feature1} und {feature2}.",
                $"Unser {adjective}es {hotelType} in {cityName} ist bekannt für {feature1} und {feature2}.",
                $"Ihre Reise nach {cityName} wird unvergesslich mit einem Aufenthalt in unserem {adjective}en {hotelType}, das {feature1} und {feature2} bietet.",
                $"In unserem {adjective}en {hotelType} in {cityName} genießen Sie {feature1} und {feature2}.",
                $"Unser {adjective}es {hotelType} in {cityName} ist perfekt für Gäste, die {feature1} und {feature2} genießen möchten.",
                $"Für einen komfortablen Aufenthalt in {cityName}, wählen Sie unser {adjective}es {hotelType} mit {feature1} und {feature2}.",
                $"Unser {adjective}es {hotelType} in {cityName} bietet alles, was Sie brauchen: {feature1} und {feature2}.",
                $"In {cityName} erwartet Sie unser {adjective}es {hotelType} mit {feature1} und {feature2} für einen angenehmen Aufenthalt.",
                $"Unser {adjective}es {hotelType} in {cityName} ist die perfekte Wahl für Reisende, die {feature1} und {feature2} schätzen.",
                $"Ihr Aufenthalt in {cityName} wird noch besser in unserem {adjective}en {hotelType} mit {feature1} und {feature2}.",
                $"Unser {adjective}es {hotelType} in {cityName} ist der ideale Ausgangspunkt für Ihre Reise und bietet {feature1} und {feature2}.",
                $"Entdecken Sie die Annehmlichkeiten unseres {adjective}en {hotelType}s in {cityName}, darunter {feature1} und {feature2}.",
                $"Unser {adjective}es {hotelType} in {cityName} ist Ihre erste Wahl für einen komfortablen Aufenthalt mit {feature1} und {feature2}.",
                $"Genießen Sie Ihren Aufenthalt in {cityName} in unserem {adjective}en {hotelType} mit {feature1} und {feature2}.",
                $"Unser {adjective}es {hotelType} in {cityName} bietet eine Vielzahl von Annehmlichkeiten, darunter {feature1} und {feature2}.",
                $"In unserem {adjective}en {hotelType} in {cityName} finden Sie alles, was Sie für einen angenehmen Aufenthalt benötigen, einschließlich {feature1} und {feature2}.",
                $"Unser {adjective}es {hotelType} in {cityName} ist der perfekte Ort zum Entspannen und bietet {feature1} und {feature2}.",
                $"Ihr Traumaufenthalt in {cityName} wartet in unserem {adjective}en {hotelType} mit {feature1} und {feature2}.",
                $"Unser {adjective}es {hotelType} in {cityName} bietet den perfekten Rahmen für einen unvergesslichen Aufenthalt mit {feature1} und {feature2}.",
                $"In {cityName} bietet unser {adjective}es {hotelType} den idealen Rahmen für einen erholsamen Aufenthalt mit {feature1} und {feature2}.",
                $"Unser {adjective}es {hotelType} in {cityName} ist der ideale Ort für einen erholsamen Urlaub und bietet {feature1} und {feature2}.",
                $"Erleben Sie einen unvergesslichen Aufenthalt in unserem {adjective}en {hotelType} in {cityName}, ausgestattet mit {feature1} und {feature2}.",
                $"Unser {adjective}es {hotelType} in {cityName} ist der ideale Ort für einen komfortablen Aufenthalt und bietet {feature1} und {feature2}.",
                $"In {cityName} bietet unser {adjective}es {hotelType} eine komfortable Unterkunft mit {feature1} und {feature2}.",
                $"Unser {adjective}es {hotelType} in {cityName} ist die perfekte Wahl für einen komfortablen und angenehmen Aufenthalt mit {feature1} und {feature2}.",
                $"Für einen unvergesslichen Aufenthalt in {cityName}, bietet unser {adjective}es {hotelType} {feature1} und {feature2}.",
                $"Unser {adjective}es {hotelType} in {cityName} ist der ideale Ort für einen erholsamen und komfortablen Aufenthalt mit {feature1} und {feature2}.",
                $"In {cityName} erwartet Sie ein unvergesslicher Aufenthalt in unserem {adjective}en {hotelType} mit {feature1} und {feature2}.",
                $"Unser {adjective}es {hotelType} in {cityName} bietet den idealen Rahmen für einen komfortablen Aufenthalt mit {feature1} und {feature2}.",
                $"Genießen Sie einen luxuriösen Aufenthalt in unserem {adjective}en {hotelType} in {cityName}, ausgestattet mit {feature1} und {feature2}.",
                $"In {cityName} bietet unser {adjective}es {hotelType} den perfekten Rahmen für einen erholsamen Aufenthalt mit {feature1} und {feature2}.",
                $"Unser {adjective}es {hotelType} in {cityName} ist die erste Wahl für einen komfortablen und angenehmen Aufenthalt mit {feature1} und {feature2}.",
                $"Erleben Sie einen unvergesslichen Aufenthalt in {cityName} in unserem {adjective}en {hotelType} mit {feature1} und {feature2}."
            };

            string description = templates[Random.Range(0, templates.Length)];

            return description;
        }

        private List<(int hotelId, int roomTypeId, int roomNumber, float price)> CreateHotelRooms(int hotelId, int maxRoomTypes, int minRoomsPerHotel, int maxRoomsPerHotel)
        {
            List<(int hotelId, int roomTypeId, int roomNumber, float price)> hotelRooms = new();
            int count = Random.Range(minRoomsPerHotel, maxRoomsPerHotel + 1); // Zieht Performance beim Generieren

            for (int i = 0; i < count; i++)
            {
                int roomTypeId = Random.Range(1, maxRoomTypes + 1);
                int roomNumber = i + 1;

                float price = Random.Range(50f, 500f);

                hotelRooms.Add((hotelId, roomTypeId, roomNumber, price));
                totalHotelRooms++;

                if (hotelHotelRoomMapper.ContainsKey(hotelId))
                    hotelHotelRoomMapper[hotelId].Add(totalHotelRooms);
                else
                    hotelHotelRoomMapper.Add(hotelId, new List<int>() { totalHotelRooms });
            }

            return hotelRooms;
        }

        private List<(int airlineId, string departure, string arrival, System.DateTime departureTime, System.DateTime arrivalTime, int maxPassengers, float price)> CreateFlights(int count, int maxAirlines, Dictionary<string, (string airportCode, string airportName)> airports)
        {
            List<(int airlineId, string departure, string arrival, System.DateTime departureTime, System.DateTime arrivalTime, int maxPassengers, float price)> flights = new();

            for (int i = 0; i < count; i++)
            {
                int airlineId = Random.Range(1, maxAirlines + 1);

                var departureCity = airports.ElementAt(Random.Range(0, airports.Count));
                var arrivalCity = airports.ElementAt(Random.Range(0, airports.Count));

                while (arrivalCity.Key == departureCity.Key) // to ensure departure and arrival are not the same
                {
                    arrivalCity = airports.ElementAt(Random.Range(0, airports.Count));
                }

                System.DateTime departureTime = System.DateTime.Now.AddHours(Random.Range(24, 240)); // Random time between 1 to 10 days from now
                System.DateTime arrivalTime = departureTime.AddHours(Random.Range(1, 12)); // Random duration between 1 to 12 hours

                int maxPassengers = Random.Range(50, 300); // Random number of maximum passengers

                flights.Add((airlineId, departureCity.Value.airportCode, arrivalCity.Value.airportCode, departureTime, arrivalTime, maxPassengers, Random.Range(50f, 400f)));
            }

            return flights;
        }

        public static int AddSingleFlight(int maxAirlines, string departureAirport, System.DateTime departureTime, string arrivalAirport, System.DateTime arrivalTime, int persons, bool forceNew = false)
        {
            int airlineId = Random.Range(1, maxAirlines + 1);

            int maxPassengers = Random.Range(50, 300); // Random number of maximum passengers

            if (!forceNew)
            {

                string sql = "SELECT * FROM Flights " +
                 "WHERE DATE(departure_time) = @param1 " +
                 "AND departure = @param2 " +
                 "AND arrival = @param3";

                int foundId = -1;
                int maxPassengersFoundID = -1;

                using (MySql.Data.MySqlClient.MySqlDataReader reader = DatabaseManager.Instance.ExcecuteQuery(sql, new object[] { departureTime.Date.ToString("yyyy-MM-dd"), departureAirport, arrivalAirport }))
                {
                    if (reader.Read())
                    {
                        foundId = reader.GetInt32("flight_id");
                        maxPassengersFoundID = reader.GetInt32("max_passengers");
                    }
                }

                if (foundId != -1)
                {
                    sql = "SELECT COUNT(*) AS passengers FROM Bookings WHERE outbound_flight_id = @param1 OR return_flight_id = @param1";
                    int foundIdCurrentPassengers = 0;

                    using (MySql.Data.MySqlClient.MySqlDataReader reader = DatabaseManager.Instance.ExcecuteQuery(sql, new object[] { foundId }))
                    {
                        reader.Read();

                        foundIdCurrentPassengers = reader.GetInt32("passengers");
                    }

                    if (foundIdCurrentPassengers + persons <= maxPassengersFoundID)
                        return foundId;
                }
            }

            return DatabaseManager.Instance.ExcecuteCommandAndGetLastID(
                "INSERT INTO Flights (airline_id, departure, arrival, departure_time, arrival_time, max_passengers, price) VALUES (@param1, @param2, @param3, @param4, @param5, @param6, @param7)",
                new object[] { airlineId, departureAirport, arrivalAirport, departureTime, arrivalTime, maxPassengers, Random.Range(50f, 400f) });
        }

        private List<(int hotelRoomId, int userId, System.DateTime startDate, System.DateTime endDate, string status, int outboundFlightId, int returnFlightId, int persons)> CreateBookings(int count, int maxHotelRooms, int maxUsers, int maxAirlines, Dictionary<string, (string airportCode, string airportName)> airports)
        {
            List<(int hotelRoomId, int userId, System.DateTime startDate, System.DateTime endDate, string status, int outboundFlightId, int returnFlightId, int persons)> bookings = new();

            string[] statuses = { "gebucht", "storniert" };

            for (int i = 0; i < count; i++)
            {
                int hotelRoomId = Random.Range(1, maxHotelRooms + 1);
                int userId = Random.Range(2, maxUsers + 1); // kein Admin

                System.DateTime startDate = System.DateTime.Now.AddDays(Random.Range(1, 60)); // Start date between 1 to 60 days from now
                startDate = new System.DateTime(startDate.Year, startDate.Month, startDate.Day, 15, 0, 1);
                System.DateTime endDate = startDate.AddDays(Random.Range(1, 14)); // End date between 1 to 14 days from start date
                endDate = new System.DateTime(endDate.Year, endDate.Month, endDate.Day, 10, 0, 0);

                string status = Random.Range(1, 11) == 1 ? statuses[1] : statuses[0];

                string outboundAirport = airports.ElementAt(Random.Range(0, airports.Count)).Value.airportCode;
                string destinationAirport = hotelAirportMapper[hotelHotelRoomMapper.Where(x => x.Value.Contains(hotelRoomId)).FirstOrDefault().Key];

                System.DateTime depTime = startDate.AddHours(Random.Range(-10, -3));
                System.DateTime depTimeReturn = endDate.AddHours(Random.Range(-10, -3));
                int persons = Random.Range(1, 4);
                int outboundFlightId = AddSingleFlight(maxAirlines, outboundAirport, depTime, destinationAirport, depTime.AddHours(Random.Range(3, 10)), persons);
                int returnFlightId = AddSingleFlight(maxAirlines, destinationAirport, depTimeReturn, outboundAirport, depTimeReturn.AddHours(Random.Range(-5, -1)), persons);

                bookings.Add((hotelRoomId, userId, startDate, endDate, status, outboundFlightId, returnFlightId, persons));
            }

            return bookings;
        }

        private List<(int hotelId, int userId, int rating, string title, string reviewText)> CreateHotelReviews(int count, int maxHotels, int maxUsers)
        {
            List<(int hotelId, int userId, int rating, string title, string reviewText)> hotelReviews = new();

            Dictionary<int, int> userHotelMap = new Dictionary<int, int>();

            #region Review Titel und Texte

            Dictionary<int, List<string>> reviewTitlesByRating = new Dictionary<int, List<string>>
            {
                {1, new List<string> {
                    "Schrecklicher Aufenthalt", "Nie wieder!", "Enttäuschend", "Nicht zu empfehlen", "Das Schlimmste ever!",
                    "Unzumutbar", "Ein Albtraum", "Totaler Reinfall", "Katastrophal", "Eine Zumutung"
                }},
                {2, new List<string> {
                    "Unterdurchschnittlich", "Nicht das Geld wert", "Könnte besser sein", "Nicht beeindruckt", "Mangelhaft",
                    "Erwartungen nicht erfüllt", "Nicht mein Ding", "Zu viele Mängel", "Ausbaufähig", "Ziemlich enttäuschend"
                }},
                {3, new List<string> {
                    "Es war okay", "Durchschnittlich", "Nicht schlecht", "Solider Aufenthalt", "Nichts Besonderes",
                    "Akzeptabel", "Nicht übel", "Ganz okay", "Mittelmäßig", "Könnte schlimmer sein"
                }},
                {4, new List<string> {
                    "Sehr gut!", "Würde wiederkommen", "Angenehmer Aufenthalt", "Fast perfekt", "Gute Wahl",
                    "Zufriedenstellend", "Empfehlenswert", "Beinahe perfekt", "Gelungen", "Erstklassiger Service"
                }},
                {5, new List<string> {
                    "Fantastisch!", "Bestes Hotel ever!", "Außergewöhnlich", "Perfekter Aufenthalt", "Hervorragend!",
                    "Ein Traum!", "Übertrifft alle Erwartungen", "Exzellent", "Weltklasse", "Ein Paradies"
                }}
            };

            Dictionary<int, List<string>> reviewTextsByRating = new Dictionary<int, List<string>>
            {
                {1, new List<string> {
                    "Das Zimmer war schmutzig und der Service war schrecklich. Nie wieder!",
                    "Ich hatte hohe Erwartungen, aber wurde sehr enttäuscht.",
                    "Das Essen war ungenießbar und die Betten unbequem.",
                    "Lärmende Nachbarn und kein hilfreiches Personal.",
                    "Total überteuert für das, was geboten wurde.",
                    "Das Badezimmer war in einem schrecklichen Zustand. Wasser lief nicht ab.",
                    "Das Personal war unhöflich und nicht hilfsbereit.",
                    "Das Internet war extrem langsam und ständig unterbrochen.",
                    "Die Klimaanlage funktionierte nicht und es war sehr heiß im Zimmer.",
                    "Das Frühstück war das schlechteste, das ich je hatte."
                }},
                {2, new List<string> {
                    "Das Zimmer hätte sauberer sein können. Das Frühstück war auch ziemlich einfach.",
                    "Der Poolbereich war dreckig und der Service war langsam.",
                    "Das Internet funktionierte nicht und das Personal schien nicht zu wissen, wie man es behebt.",
                    "Die Einrichtung ist veraltet und könnte eine Auffrischung gebrauchen.",
                    "Zu laut wegen Bauarbeiten direkt vor dem Hotel.",
                    "Das Essen im Restaurant war nicht besonders.",
                    "Die Betten waren ziemlich hart und unbequem.",
                    "Das Zimmer war klein für den Preis, den wir bezahlt haben.",
                    "Der Fitnessraum hatte nur wenige Geräte und diese waren alt.",
                    "Das Personal könnte freundlicher sein."
                }},
                {3, new List<string> {
                    "Ein durchschnittliches Hotel, aber okay für den Preis.",
                    "Das Personal war freundlich, obwohl das Zimmer klein war.",
                    "Das Essen war in Ordnung, aber nichts Besonderes.",
                    "Gute Lage, aber das Hotel könnte eine Renovierung gebrauchen.",
                    "Es war ein solider Aufenthalt, aber ich habe schon bessere Hotels in der Gegend gesehen.",
                    "Das Frühstück war vielfältig, aber der Kaffee könnte besser sein.",
                    "Das Zimmer war sauber, aber die Aussicht war nicht besonders.",
                    "Der Service war in Ordnung, aber nichts herausragendes.",
                    "Das WLAN funktionierte gut, aber das Fernsehangebot war begrenzt.",
                    "Das Bad war sauber, aber die Dusche hatte wenig Druck."
                }},
                {4, new List<string> {
                    "Das Zimmer war geräumig und sauber. Nur das Frühstück hätte besser sein können.",
                    "Toller Service und gute Einrichtungen. Würde definitiv wiederkommen.",
                    "Das Essen im Restaurant war lecker und das Personal war hilfsbereit.",
                    "Hatte einen angenehmen Aufenthalt. Nur der Fitnessbereich war etwas klein.",
                    "Schöne Aussicht vom Zimmer und zentrale Lage.",
                    "Das Personal war immer zur Stelle und sehr freundlich.",
                    "Das Bett war super bequem und ich habe gut geschlafen.",
                    "Das Hotel hat eine tolle Atmosphäre und ist stilvoll eingerichtet.",
                    "Die Dachterrasse war ein Highlight mit tollem Blick über die Stadt.",
                    "Das Spa-Angebot war großartig und sehr entspannend."
                }},
                {5, new List<string> {
                    "Dieses Hotel hat meine Erwartungen übertroffen! Alles war perfekt.",
                    "Das Personal hat alles getan, um sicherzustellen, dass unser Aufenthalt fantastisch war.",
                    "Von der Einrichtung bis zum Essen - alles war erstklassig.",
                    "Ich kann dieses Hotel nicht genug empfehlen. Es war ein unvergesslicher Aufenthalt.",
                    "Dies ist bei weitem das beste Hotel, in dem ich je übernachtet habe. Alles war makellos.",
                    "Das Zimmer war wie aus einem Traum - geräumig, sauber und luxuriös.",
                    "Das Frühstücksbuffet war unglaublich mit einer riesigen Auswahl.",
                    "Jedes Detail in diesem Hotel ist durchdacht und perfektioniert.",
                    "Der Service ist unübertroffen. Das Personal hat sich um jeden Wunsch gekümmert.",
                    "Die Lage, die Ausstattung, der Service - alles war hervorragend."
                }}
            };

            #endregion

            for (int i = 0; i < count; i++)
            {
                int rating = Random.Range(1, 6);
                string title = reviewTitlesByRating[rating][Random.Range(0, reviewTitlesByRating[rating].Count)];
                string reviewText = reviewTextsByRating[rating][Random.Range(0, reviewTextsByRating[rating].Count)];

                int userId = Random.Range(2, maxUsers + 1);
                int hotelId = Random.Range(1, maxHotels + 1);

                if (userHotelMap.ContainsKey(userId))
                {
                    if (userHotelMap[userId] == hotelId)
                        continue;
                }

                hotelReviews.Add((hotelId, userId, rating, title, reviewText));
            }

            return hotelReviews;
        }

        private string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Range(0, s.Length)]).ToArray());
        }

        private List<(int hotelId, System.DateTime validityStartDate, System.DateTime validityEndDate, double discountPercent, string code)> CreateSpecialOffers(int count, int maxHotels, float maxDiscount, int maxDaysInFuture)
        {
            List<(int hotelId, System.DateTime validityStartDate, System.DateTime validityEndDate, double discountPercent, string code)> specialOffers = new();

            for (int i = 0; i < count; i++)
            {
                System.DateTime currentDate = System.DateTime.Now;
                System.DateTime validityStartDate = currentDate.AddDays(Random.Range(1, maxDaysInFuture));
                System.DateTime validityEndDate = validityStartDate.AddDays(Random.Range(1, maxDaysInFuture));

                double discountPercent = System.Math.Round(Random.Range(5f, maxDiscount), 2);
                string code = GenerateRandomCode(10); // 10-stelliger zufälliger Code

                specialOffers.Add((Random.Range(1, maxHotels + 1), validityStartDate, validityEndDate, discountPercent, code));
            }

            return specialOffers;
        }

        private List<(int bookingId, string paymentMethod, string paymentStatus, float price, int? usedDiscountCode)> CreatePaymentInfos(int maxBookings, int maxSpecialOffers)
        {
            List<string> paymentMethods = new() { "Kreditkarte", "Paypal", "Überweisung" };
            List<string> paymentStatuses = new() { "bezahlt", "ausstehend" };

            List<(int bookingId, string paymentMethod, string paymentStatus, float price, int? usedDiscountCode)> paymentInfos = new();

            for (int i = 1; i <= maxBookings; i++)
            {
                string paymentMethod = paymentMethods[Random.Range(0, paymentMethods.Count)];
                string paymentStatus = paymentStatuses[Random.Range(0, paymentStatuses.Count)];

                float price = 0;
                int? used_discount_id = null;

                if (Random.Range(1, 101) > 80)
                {
                    used_discount_id = Random.Range(1, maxSpecialOffers);
                }

                string sql = @"
                    SELECT
                      B.booking_id,
                      B.persons,
                      HR.price * DATEDIFF(B.end_date, B.start_date) AS hotel_cost,
                      OF.price AS outbound_flight_cost,
                      RF.price AS return_flight_cost,
                      ROUND(
                        (
                          HR.price * DATEDIFF(B.end_date, B.start_date) +
                          (OF.price + RF.price) * B.persons
                        ) * (
                          CASE
                            WHEN SO.offer_id IS NOT NULL THEN (1 - (SO.discount_percent / 100))
                            ELSE 1
                          END
                        ),
                        2
                      ) AS total_cost
                    FROM
                      Bookings B
                    JOIN HotelRooms HR ON B.hotel_room_id = HR.hotel_room_id
                    JOIN Flights OF ON B.outbound_flight_id = OF.flight_id
                    JOIN Flights RF ON B.return_flight_id = RF.flight_id
                    LEFT JOIN SpecialOffers SO ON SO.offer_id = @param2
                    WHERE
                      B.booking_id = @param1";

                using (MySql.Data.MySqlClient.MySqlDataReader reader = DatabaseManager.Instance.ExcecuteQuery(sql, new object[] { i, used_discount_id }))
                {
                    reader.Read();
                    price = reader.GetFloat("total_cost");
                }

                paymentInfos.Add((i, paymentMethod, paymentStatus, price, used_discount_id));
            }

            return paymentInfos;
        }
    }
}

