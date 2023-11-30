using Michsky.MUIP;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using TMPro;
using UnityEngine;

namespace Ripple
{
    public class BookingManager : MonoBehaviour
    {
        public static BookingManager Instance {  get; private set; }

        [SerializeField] private WindowManager _windowManager;

        [SerializeField] private TMP_Text _hotelname;
        [SerializeField] private TMP_Text _bookingPeriod;
        [SerializeField] private TMP_Text _address;
        [SerializeField] private TMP_Text _roomType;
        [SerializeField] private CustomDropdown _personsDropdown;
        [SerializeField] private HorizontalSelector _outboundSelector;
        [SerializeField] private TMP_Text _outboundDeparture;
        [SerializeField] private TMP_Text _outboundArrival;
        [SerializeField] private TMP_Text _outboundPrice;
        [SerializeField] private HorizontalSelector _returnSelector;
        [SerializeField] private TMP_Text _returnDeparture;
        [SerializeField] private TMP_Text _returnArrival;
        [SerializeField] private TMP_Text _returnPrice;
        [SerializeField] private TMP_Text _totalPrice;
        [SerializeField] private TMP_InputField _promoCodeInput;
        [SerializeField] private NotificationManager _promoCodeSuccess;
        [SerializeField] private NotificationManager _promoCodeFail;
        [SerializeField] private NotificationManager _bookingSuccess;

        private Hotel _hotel;
        private DateTime _startTime;
        private DateTime _endTime;
        private int _roomTypeId;
        private int _maxOccupancy;
        private string _roomTypeName;
        private float _hotelRoomPrice;
        private int _hotelRoomID;

        private int _choosedPersonCount = 1;

        private string _outboundDepartureAirport;
        private string _outboundArrivalAirport;

        private List<Flight> _outboundFlights;
        private List<Flight> _returnFlights;

        private Flight _selectedOutboudFlight;
        private Flight _selectedReturnFlight;

        private string _currentPromoCode = "";
        private int _currentPromoID = -1;
        private float _currentDiscoutPercentage = 0;

        private float _currentTotalPrice = -1;

        private void Awake()
        {
            Instance = this;
        }

        public void Initialize(Hotel hotel, DateTime startTime, DateTime endTime, int roomType, string roomTypeName, int maxOccupancy, float price)
        {
            _hotel = hotel;
            _startTime = startTime;
            _endTime = endTime;
            _roomTypeId = roomType;
            _roomTypeName = roomTypeName;
            _maxOccupancy = maxOccupancy;
            this._hotelRoomPrice = price;
            _hotelRoomID = GetHotelRoomIDWhereMinPrice();
            FillBasicInformation();
            SetupPersonsDropdown();
            FillFlights();
            CalculateTotalPrice();
        }

        private void FillBasicInformation()
        {
            _hotelname.text = _hotel.Name;
            _bookingPeriod.text = $"{_startTime.ToString("dd.MM.yyyy")} - {_endTime.ToString("dd.MM.yyyy")}";
            _address.text = _hotel.FormattedAddress;
            _roomType.text = _roomTypeName + " (" + string.Format("{0:C}", _hotelRoomPrice) + " pro Nacht)";
        }

        private void SetupPersonsDropdown()
        {
            _personsDropdown.items.Clear();
            _personsDropdown.selectedItemIndex = 0;

            for (int i = 0; i < _maxOccupancy; i++)
            {
                CustomDropdown.Item item = new CustomDropdown.Item();
                item.itemIndex = i;
                item.itemName = ((i + 1) + " Person" + (i > 0 ? "en" : "")).ToString();
                int local = i;
                item.OnItemSelection.AddListener(() => { _choosedPersonCount = local + 1; CalculateTotalPrice(); });
                _personsDropdown.items.Add(item);
            }

            _personsDropdown.SetupDropdown();
        }

        private void FillFlights()
        {
            _outboundDepartureAirport = DatabaseDataCreator.airports.Where(x => x.Key != _hotel.City).OrderBy(x => Guid.NewGuid()).FirstOrDefault().Value.airportCode;
            _outboundArrivalAirport = DatabaseDataCreator.airports.Where(x => x.Key == _hotel.City).FirstOrDefault().Value.airportCode;

            _outboundFlights = GetFlights(_startTime, _outboundDepartureAirport, _outboundArrivalAirport);
            _returnFlights = GetFlights(_endTime, _outboundArrivalAirport, _outboundDepartureAirport);

            _outboundSelector.items.Clear();
            
            for (int i = 0; i < _outboundFlights.Count; i++)
            {
                HorizontalSelector.Item item = new HorizontalSelector.Item();
                item.itemTitle = "Flug " + (i + 1);
                int local = i;
                item.onItemSelect.AddListener(() => { FillOutboundFlightInformation(_outboundFlights[local]); _selectedOutboudFlight = _outboundFlights[local]; CalculateTotalPrice(); });
                _selectedOutboudFlight = _outboundFlights[0];
                _outboundSelector.items.Add(item);
            }

            _outboundSelector.SetupSelector();
            FillOutboundFlightInformation(_outboundFlights[0]);

            _returnSelector.items.Clear();

            for (int i = 0; i < _returnFlights.Count; i++)
            {
                HorizontalSelector.Item item = new HorizontalSelector.Item();
                item.itemTitle = "Flug " + (i + 1);
                int local = i;
                item.onItemSelect.AddListener(() => { FillReturnFlightInformation(_returnFlights[local]); _selectedReturnFlight = _returnFlights[local]; CalculateTotalPrice(); });
                _selectedReturnFlight = _returnFlights[0];
                _returnSelector.items.Add(item);
            }

            _returnSelector.SetupSelector();
            FillReturnFlightInformation(_returnFlights[0]);
        }

        private void FillOutboundFlightInformation(Flight flight)
        {
            _outboundDeparture.text = $"{flight.Departure} {flight.DepartureTime.TimeOfDay}";
            _outboundArrival.text = $"{flight.Arrival} {flight.ArrivalTime.TimeOfDay}";
            _outboundPrice.text = string.Format("{0:C}", flight.Price);
        }

        private void FillReturnFlightInformation(Flight flight)
        {
            _returnDeparture.text = $"{flight.Departure} {flight.DepartureTime.TimeOfDay}";
            _returnArrival.text = $"{flight.Arrival} {flight.ArrivalTime.TimeOfDay}";
            _returnPrice.text = string.Format("{0:C}", flight.Price);
        }

        private List<Flight> GetFlights(DateTime departureTime, string departureAirport, string arrivalAirport)
        {
            string sql = @"
                SELECT F.*, 
                       (SELECT COUNT(*)
                        FROM Bookings B
                        WHERE B.outbound_flight_id = F.flight_id
                           OR B.return_flight_id = F.flight_id) AS passenger_count
                FROM Flights F
                WHERE DATE(F.departure_time) = @param1 
                  AND F.departure = @param2 
                  AND F.arrival = @param3";

            List<Flight> flights = new List<Flight>();

            int minFlightsCount = UnityEngine.Random.Range(2, 5);

            using (MySqlDataReader reader = DatabaseManager.Instance.ExcecuteQuery(sql, new object[] { departureTime, departureAirport, arrivalAirport }))
            {
                while (reader.Read())
                {
                    Flight flight = new Flight(
                        reader.GetInt32("flight_id"),
                        reader.GetInt32("airline_id"),
                        departureAirport,
                        arrivalAirport,
                        reader.GetDateTime("departure_time"),
                        reader.GetDateTime("arrival_time"),
                        reader.GetInt32("max_passengers"),
                        reader.GetFloat("price")
                        );

                    flight.CurrentPassengers = reader.GetInt32("passenger_count");

                    flights.Add(flight);
                }
            }

            int airlines = -1;
            int addingTime = UnityEngine.Random.Range(1, 12);
            while (flights.Count < minFlightsCount)
            {
                if (airlines == -1)
                {
                    sql = "SELECT COUNT(*) as airlines FROM Airlines";
                    using (MySqlDataReader reader = DatabaseManager.Instance.ExcecuteQuery(sql))
                    {
                        reader.Read();
                        airlines = reader.GetInt32("airlines");
                    }
                }

                DateTime depTime = new DateTime(departureTime.Year, departureTime.Month, departureTime.Day, UnityEngine.Random.Range(6, 17), UnityEngine.Random.Range(0, 60), UnityEngine.Random.Range(0, 60));
                DateTime arrTime = depTime.AddHours(addingTime);
                int newFlightId = DatabaseDataCreator.AddSingleFlight(airlines, departureAirport, depTime, arrivalAirport, arrTime, UnityEngine.Random.Range(50, 300), true);

                flights.Add(new Flight(newFlightId));
            }

            return flights;
        }

        public void OnBookingButtonPressed()
        {
            string sqlBooking = "INSERT INTO Bookings (hotel_room_id, user_id, start_date, end_date, status, outbound_flight_id, return_flight_id, persons) VALUES (@param1, @param2, @param3, @param4, @param5, @param6, @param7, @param8)";
            object[] bookingParams = new object[] {_hotelRoomID , LoginManager.CurrentUserID, _startTime, _endTime, "gebucht", _selectedOutboudFlight.ID, _selectedReturnFlight.ID, _choosedPersonCount };
            
            CreatePaymentInfo(DatabaseManager.Instance.ExcecuteCommandAndGetLastID(sqlBooking, bookingParams));
            _bookingSuccess.OpenNotification();
            _windowManager.OpenWindow("Buchungen");
        }

        private void CreatePaymentInfo(int bookingID)
        {
            List<string> paymentMethods = new() { "Kreditkarte", "Paypal", "Überweisung" };

            string sql = "INSERT INTO PaymentInfo (booking_id, payment_method, payment_status, price, used_discount_id) VALUES (@param1, @param2, @param3, @param4, @param5)";
            
            DatabaseManager.Instance.ExcecuteCommand(sql, new object[] { bookingID, paymentMethods[UnityEngine.Random.Range(0, paymentMethods.Count)], "bezahlt", _currentTotalPrice, _currentPromoID == -1 ? null : _currentPromoID});
        }

        private int GetHotelRoomIDWhereMinPrice()
        {
            string sqlHotelRoom = @"
                SELECT 
                    hr.hotel_id, 
                    hr.room_type_id, 
                    rt.name, 
                    rt.description, 
                    rt.max_occupancy, 
                    MIN(hr.price) as MinPreis,
                    hr.hotel_room_id
                FROM 
                    HotelRooms hr 
                JOIN 
                    RoomTypes rt ON hr.room_type_id = rt.room_type_id 
                WHERE 
                    hr.hotel_id = @param1 
                    AND NOT EXISTS (
                        SELECT 1 
                        FROM Bookings b 
                        WHERE 
                            b.hotel_room_id = hr.hotel_room_id 
                            AND b.status = 'gebucht' 
                            AND (
                                (b.end_date >= DATE_SUB(@param2, INTERVAL 5 HOUR) AND b.end_date <= @param3) 
                                OR 
                                (b.start_date <= DATE_ADD(@param3, INTERVAL 5 HOUR) AND b.start_date >= @param2)
                            )
                    )
                    AND hr.room_type_id = @param4
                GROUP BY 
                    hr.hotel_id, 
                    hr.room_type_id, 
                    rt.name, 
                    rt.description, 
                    rt.max_occupancy,
                    hr.hotel_room_id";

            using (MySqlDataReader reader = DatabaseManager.Instance.ExcecuteQuery(sqlHotelRoom, new object[] { _hotel.ID, _startTime, _endTime, _roomTypeId }))
            {
                if (reader.Read())
                {
                    return reader.GetInt32("hotel_room_id");
                }
                return -1;
            }
        }

        public void OnPromoCodeButtonPressed()
        {
            if (string.IsNullOrWhiteSpace(_promoCodeInput.text))
                return;

            string sql = @"
                SELECT discount_percent, code, offer_id FROM SpecialOffers
                WHERE code = @param1 AND 
                CURRENT_TIMESTAMP BETWEEN validity_start_date AND validity_end_date";

            using (MySqlDataReader reader = DatabaseManager.Instance.ExcecuteQuery(sql, _promoCodeInput.text))
            {
                if (reader.Read())
                {
                    _currentDiscoutPercentage = reader.GetFloat("discount_percent");
                    _currentPromoCode = reader.GetString("code");
                    _currentPromoID = reader.GetInt32("offer_id");
                    _promoCodeSuccess.Open();
                    CalculateTotalPrice();
                    return;
                }
            }

            _promoCodeFail.Open();
        }

        private void CalculateTotalPrice()
        {
            float totalPrice = 0;
            float outboudFlightPrice = _selectedOutboudFlight.Price * _choosedPersonCount;
            float returnFlightPrice = _selectedReturnFlight.Price * _choosedPersonCount;

            DateTime startDateOnly = _startTime.Date;
            DateTime endDateOnly = _endTime.Date;

            TimeSpan timeSpan = endDateOnly - startDateOnly;
            int days = timeSpan.Days;

            totalPrice = outboudFlightPrice + returnFlightPrice + (_hotelRoomPrice * days);
            totalPrice *= 1 - (_currentDiscoutPercentage / 100f);
            totalPrice = (float)Math.Round(totalPrice, 2);
            _totalPrice.text = string.Format("{0:C}", totalPrice);
            _currentTotalPrice = totalPrice;
        }

        private void OnDisable()
        {
            _promoCodeInput.text = "";
            _currentPromoID = -1;
            _currentDiscoutPercentage = 0;
            _currentPromoCode = "";
            _choosedPersonCount = 1;
            _currentTotalPrice = -1;
        }
    }
}
