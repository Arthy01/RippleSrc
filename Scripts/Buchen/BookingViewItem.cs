using Michsky.MUIP;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ripple
{
    public class BookingViewItem : MonoBehaviour
    {
        private int _bookingID = -1;
        private string _status = "";

        [SerializeField] private TMP_Text _hotelName_period;
        [SerializeField] private TMP_Text _country;
        [SerializeField] private TMP_Text _city;
        [SerializeField] private TMP_Text _personCount;
        [SerializeField] private TMP_Text _price;
        [SerializeField] private TMP_Text _paymentMethod;
        [SerializeField] private ButtonManager _stornoButton;
        [SerializeField] private TMP_Text _statusText;

        public void Initialize(string hotelName, DateTime start, DateTime end, string country, string city, int personCount, float price, string paymentMehtod, int bookingID, string status)
        {
            _hotelName_period.text = hotelName + " (" + start.ToString("dd.MM.yyyy") + " - " + end.ToString("dd.MM.yyyy") + ")";
            _country.text = country;
            _city.text = city;
            _personCount.text = personCount.ToString();
            _price.text = string.Format("{0:C}", price);
            _paymentMethod.text = paymentMehtod;
            _statusText.text = status;
            _status = status;
            _bookingID = bookingID;

            if (_status == "gebucht")
            {
                _stornoButton.gameObject.SetActive(true);
                _statusText.gameObject.SetActive(false);
            }
            else 
            {
                _stornoButton.gameObject.SetActive(false);
                _statusText.gameObject.SetActive(true);
            }
        }

        public void OnStornoButtonPressed()
        {
            string sql = "UPDATE Bookings SET status = \"storniert\" WHERE booking_id = @param1";
            DatabaseManager.Instance.ExcecuteCommand(sql, new object[] { _bookingID });

            _status = "storniert";
            _statusText.text = _status;
            _stornoButton.gameObject.SetActive(false);
            _statusText.gameObject.SetActive(true);
        }
    }
}
