using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ripple
{
    public class BookingViewManager : MonoBehaviour
    {
        [SerializeField] private GameObject _bookingViewItemPrefab;
        [SerializeField] private Transform _bookingViewItemParent;

        private void OnEnable()
        {
            LoadBookings();
        }

        private void LoadBookings()
        {
            string sql = @"SELECT b.status, b.booking_id, b.persons, b.start_date, b.end_date, p.price, p.payment_method, h.name, c.city_name, co.country_name
	                        FROM Bookings b 
                            JOIN PaymentInfo p ON b.booking_id = p.booking_id 
                            JOIN HotelRooms hr ON b.hotel_room_id = hr.hotel_room_id
                            JOIN Hotels h ON hr.hotel_id = h.hotel_id
                            JOIN Cities c ON h.city_id = c.city_id
                            JOIN Countries co ON h.country_id = co.country_id 
                            WHERE user_id = @param1";

            using (MySqlDataReader reader = DatabaseManager.Instance.ExcecuteQuery(sql, new object[] { LoginManager.CurrentUserID }))
            {
                while (reader.Read())
                {
                    BookingViewItem item = Instantiate(_bookingViewItemPrefab, _bookingViewItemParent).GetComponent<BookingViewItem>();
                    item.Initialize(
                        reader.GetString("name"),
                        reader.GetDateTime("start_date"),
                        reader.GetDateTime("end_date"),
                        reader.GetString("country_name"),
                        reader.GetString("city_name"),
                        reader.GetInt32("persons"),
                        reader.GetFloat("price"),
                        reader.GetString("payment_method"),
                        reader.GetInt32("booking_id"),
                        reader.GetString("status"));
                }
            }
        }

        private void OnDisable()
        {
            foreach (BookingViewItem item in _bookingViewItemParent.GetComponentsInChildren<BookingViewItem>(true))
            {
                Destroy(item.gameObject);
            }
        }
    }
}
