using Michsky.MUIP;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using MySql.Data.MySqlClient;
using System;
using Google.Protobuf;
using System.Globalization;

namespace Ripple
{
    public class HotelsucheManager : MonoBehaviour
    {
        public static HotelsucheManager Instance { get; private set; }

        [SerializeField] private WindowManager _windowManager;
        [SerializeField] private HotelsucheErgebnisseManager _ergebnisseManager;

        [SerializeField] private NotificationManager _wrongInput;

        [SerializeField] private TMP_InputField _startDate;
        [SerializeField] private TMP_InputField _endDate;

        [SerializeField] private CustomDropdown _countrySelector;
        [SerializeField] private CustomDropdown _citySelector;
        [SerializeField] private CustomDropdown _roomTypeSelector;
        [SerializeField] private TMP_InputField _hotelNameInput;
        [SerializeField] private TMP_InputField _maxPriceInput;

        private int _selectedCountryID = -1;
        private int _selectedCityID = -1;
        private int _selectedRoomID = -1;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            StartCoroutine(SetupDropdowns());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator SetupDropdowns()
        {
            string sql;
            int itemIndex = 0;
            CustomDropdown.Item item;

            #region Country

            _countrySelector.items.Clear();

            item = new CustomDropdown.Item();
            item.itemName = "/";
            item.itemIndex = itemIndex++;
            item.OnItemSelection.AddListener(() => { _selectedCountryID = -1; OnCountrySelected(); }) ;

            _countrySelector.items.Add(item);

            sql = "SELECT * FROM Countries";
            using (MySqlDataReader reader = DatabaseManager.Instance.ExcecuteQuery(sql))
            {
                while (reader.Read())
                {
                    item = new CustomDropdown.Item();
                    item.itemName = reader.GetString("country_name");
                    item.itemIndex = itemIndex++;
                    int countryId = reader.GetInt32("country_id");
                    item.OnItemSelection.AddListener(() => { _selectedCountryID = countryId; OnCountrySelected(); });

                    _countrySelector.items.Add(item);
                }
            }

            _countrySelector.SetupDropdown();

            itemIndex = 0;

            #endregion

            yield return null;

            #region City

            _citySelector.items.Clear();

            item = new CustomDropdown.Item();
            item.itemName = "/";
            item.itemIndex = itemIndex++;
            item.OnItemSelection.AddListener(() => _selectedCityID = -1);

            _citySelector.items.Add(item);

            sql = "SELECT * FROM Cities";
            using (MySqlDataReader reader = DatabaseManager.Instance.ExcecuteQuery(sql))
            {
                while (reader.Read())
                {
                    item = new CustomDropdown.Item();
                    item.itemName = reader.GetString("city_name");
                    item.itemIndex = itemIndex++;
                    int cityId = reader.GetInt32("city_id");
                    item.OnItemSelection.AddListener(() => _selectedCityID = cityId);

                    _citySelector.items.Add(item);
                }
            }

            _citySelector.SetupDropdown();

            itemIndex = 0;

            #endregion

            yield return null;

            #region RoomType

            _roomTypeSelector.items.Clear();

            item = new CustomDropdown.Item();
            item.itemName = "/";
            item.itemIndex = itemIndex++;
            item.OnItemSelection.AddListener(() => _selectedRoomID = -1);

            _roomTypeSelector.items.Add(item);

            sql = "SELECT * FROM RoomTypes";
            using (MySqlDataReader reader = DatabaseManager.Instance.ExcecuteQuery(sql))
            {
                while (reader.Read())
                {
                    item = new CustomDropdown.Item();
                    item.itemName = reader.GetString("name") + " (Maximal " + reader.GetString("max_occupancy") + " Personen)";
                    item.itemIndex = itemIndex++;
                    int roomId = reader.GetInt32("room_type_id");
                    item.OnItemSelection.AddListener(() => _selectedRoomID = roomId);

                    _roomTypeSelector.items.Add(item);
                }
            }

            _roomTypeSelector.SetupDropdown();

            itemIndex = 0;

            #endregion

        }

        private void OnCountrySelected()
        {
            string sql;
            int itemIndex = 0;
            CustomDropdown.Item item;

            _citySelector.items.Clear();

            item = new CustomDropdown.Item();
            item.itemName = "/";
            item.itemIndex = itemIndex++;
            item.OnItemSelection.AddListener(() => _selectedCityID = -1);

            _citySelector.items.Add(item);

            if (_selectedCountryID == -1)
                sql = "SELECT * FROM Cities";
            else
                sql = "SELECT * FROM Cities WHERE country_id = @param1";

            using (MySqlDataReader reader = DatabaseManager.Instance.ExcecuteQuery(sql, new object[] { _selectedCountryID }))
            {
                while (reader.Read())
                {
                    item = new CustomDropdown.Item();
                    item.itemName = reader.GetString("city_name");
                    item.itemIndex = itemIndex++;
                    int cityId = reader.GetInt32("city_id");
                    item.OnItemSelection.AddListener(() => _selectedCityID = cityId);

                    _citySelector.items.Add(item);
                }
            }

            _citySelector.selectedItemIndex = 0;
            _citySelector.SetupDropdown();
        }

        public void SearchHotel()
        {
            string baseQuery = "SELECT h.*, co.country_name, ci.city_name FROM Hotels h JOIN HotelRooms hr ON h.hotel_id = hr.hotel_id JOIN Cities ci ON h.city_id = ci.city_id JOIN Countries co ON h.country_id = co.country_id";
            List<string> conditions = new List<string>();
            List<object> parameters = new List<object>();
            int paramCounter = 1;

            if (_selectedCountryID != -1)
            {
                conditions.Add($"h.country_id = @param{paramCounter}");
                parameters.Add(_selectedCountryID);
                paramCounter++;
            }

            if (_selectedCityID != -1)
            {
                conditions.Add($"h.city_id = @param{paramCounter}");
                parameters.Add(_selectedCityID);
                paramCounter++;
            }

            if (!string.IsNullOrEmpty(_hotelNameInput.text))
            {
                conditions.Add($"h.name LIKE @param{paramCounter}");
                parameters.Add($"%{_hotelNameInput.text}%");
                paramCounter++;
            }

            if (_selectedRoomID != -1)
            {
                conditions.Add($"hr.room_type_id = @param{paramCounter}");
                parameters.Add(_selectedRoomID);
                paramCounter++;
            }

            if (!string.IsNullOrEmpty(_maxPriceInput.text))
            {
                conditions.Add($"hr.price <= @param{paramCounter}");
                parameters.Add(float.Parse(_maxPriceInput.text));
                paramCounter++;
            }

            DateTime dateValue1;
            DateTime dateValue2;
            
            bool startDateValid = DateTime.TryParseExact(_startDate.text, "dd.MM.yyyy", CultureInfo.InvariantCulture,DateTimeStyles.None, out dateValue1);
            bool endDateValid = DateTime.TryParseExact(_endDate.text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue2);

            DateTime startDate;
            DateTime endDate;

            if (startDateValid && endDateValid && dateValue1 < dateValue2)
            {
                startDate = new DateTime(dateValue1.Year, dateValue1.Month, dateValue1.Day, 15, 0, 1);
                endDate = new DateTime(dateValue2.Year, dateValue2.Month, dateValue2.Day, 10, 0, 0);
            }
            else
            {
                Debug.LogWarning("Reisedaten nicht im korrekten Format!");
                _wrongInput.Open();
                return;
            }

            parameters.Add(startDate);
            parameters.Add(endDate);

            baseQuery += $@" WHERE 
                NOT EXISTS (
                SELECT 1 
                FROM Bookings b 
                WHERE 
                    b.hotel_room_id = hr.hotel_room_id 
                    AND b.status = 'gebucht' 
                    AND (
                    (b.end_date >= DATE_SUB(@param{paramCounter}, INTERVAL 5 HOUR) AND b.end_date <= @param{paramCounter + 1}) 
                    OR 
                    (b.start_date <= DATE_ADD(@param{paramCounter + 1}, INTERVAL 5 HOUR) AND b.start_date >= @param{paramCounter})
                    )
                )";

            if (conditions.Count > 0)
            {
                baseQuery += " AND " + string.Join(" AND ", conditions);
            }

            baseQuery += " GROUP BY h.hotel_id ORDER BY h.hotel_id ASC";
            
            List<Hotel> foundHotels = new List<Hotel>();

            using (MySqlDataReader reader = DatabaseManager.Instance.ExcecuteQuery(baseQuery, parameters.ToArray()))
            {
                while (reader.Read())
                {
                    foundHotels.Add(new Hotel(
                        reader.GetInt32("hotel_id"),
                        reader.GetString("name"),
                        reader.GetString("description"),
                        reader.GetString("address"),
                        reader.GetString("zip_code"),
                        reader.GetString("city_name"),
                        reader.GetString("country_name")
                        ));
                }
            }

            _windowManager.OpenWindow("Hotel suche - Ergebnisse");
            _ergebnisseManager.Initialize(foundHotels, startDate, endDate);
        }
    }
}
