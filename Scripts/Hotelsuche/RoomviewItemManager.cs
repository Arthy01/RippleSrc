using Michsky.MUIP;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Ripple
{
    public class RoomviewItemManager : MonoBehaviour
    {
        private int _roomTypeId = -1;

        private Hotel _hotel;
        private DateTime _startTime;
        private DateTime _endTime;
        private int _maxOccupancy;
        private string _roomTypeName;
        float price;

        [SerializeField] private TMP_Text _name_MaxOccupancyText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _priceText;

        public void Initialize(Hotel hotel, int roomTypeId, string roomTypeName, int maxOccupancy, string roomTypeDescription, float price, DateTime startTime, DateTime endTime)
        {
            _hotel = hotel;
            _roomTypeId = roomTypeId;
            _roomTypeName = roomTypeName;
            _name_MaxOccupancyText.text = $"{roomTypeName} (Maximal {maxOccupancy} Personen)";
            _descriptionText.text = roomTypeDescription;
            _priceText.text = string.Format("{0:C}", price);
            this.price = price;
            _startTime = startTime;
            _endTime = endTime;
            _maxOccupancy = maxOccupancy;
        }

        public void OnBookButtonPressed()
        {
            WindowManager.Instance.OpenWindow("Hotel buchen");
            BookingManager.Instance.Initialize(_hotel, _startTime, _endTime, _roomTypeId, _roomTypeName, _maxOccupancy, price);
        }
    }
}
