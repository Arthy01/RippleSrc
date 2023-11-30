using Michsky.MUIP;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Ripple
{
    public class HotelsucheItemManager : MonoBehaviour
    {
        [SerializeField] private ButtonManager _detailsButton;
        [SerializeField] private HorizontalSelector _expandModeSelector;
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _roomviewItemParent;

        [SerializeField] private TMP_Text _hotelName;
        [SerializeField] private TMP_Text _hotelDescription;
        [SerializeField] private TMP_Text _hotelAddress;
        [SerializeField] private PexelsImage _pexelsImage;

        [SerializeField] private GameObject _roomsOverviewWindow;
        [SerializeField] private GameObject _ratingsWindow;

        [SerializeField] private GameObject _reviewLoadMoreButton;
        [SerializeField] private Transform _reviewsParent;
        [SerializeField] private GameObject _reviewsNoRieviewsText;

        private Hotel _hotel;
        private List<Review> _reviews;
        private int _currentLoadedReviews;
        private int _reviewLoadingChunk = 4;

        private DateTime _startDate;
        private DateTime _endDate;

        public void Initialize(Hotel hotel, DateTime startDate, DateTime endDate)
        {
            _hotel = hotel;
            _expandModeSelector.onValueChanged.AddListener((newMode) => OnExpandModeChanged(newMode));

            _startDate = startDate;
            _endDate = endDate;

            FillBasicInformations();
            CreateRoomViews();
        }

        private void GetReviews()
        {
            string sql = "SELECT hr.*, u.username FROM HotelReviews hr JOIN Users u on hr.user_id = u.user_id WHERE hotel_id = @param1";
            using (MySqlDataReader reader = DatabaseManager.Instance.ExcecuteQuery(sql, _hotel.ID))
            {
                while (reader.Read())
                {
                    _reviews.Add(new Review(
                        reader.GetInt32("rating"),
                        reader.GetString("title"),
                        reader.GetString("comment"),
                        reader.GetString("username"),
                        reader.GetDateTime("created_at")));
                }
            }
        }

        private void FillBasicInformations()
        {
            _hotelName.text = _hotel.Name;
            _hotelDescription.text = _hotel.Description;

            _hotelAddress.text = _hotel.FormattedAddress;

            _pexelsImage.GenerateImage("Hotel in " + _hotel.City, _hotel.ID);

        }

        public void Expand()
        {
            _animator.Play("HotelsucheItemExpand");
        }

        public void Retract()
        {
            _animator.Play("HotelsucheItemRetract");
        }

        public void OnExpandModeChanged(int newMode)
        {
            print("New Expand Mode: " + newMode);

            if (newMode == 1)
            {
                _roomsOverviewWindow.SetActive(false);
                _ratingsWindow.SetActive(true);

                if (_reviews == null)
                {
                    _reviews = new List<Review>();
                    GetReviews();
                }

                if (_reviews.Count == 0)
                    _reviewsNoRieviewsText.SetActive(true);
                else
                    _reviewsNoRieviewsText.SetActive(false);

                if (_currentLoadedReviews == 0 && _reviews.Count > 0)
                    StartCoroutine(CreateItems());
            }
            else
            {
                _ratingsWindow.SetActive(false);
                _roomsOverviewWindow.SetActive(true);
            }
        }

        public void OnLoadMoreReviews()
        {
            StartCoroutine(CreateItems());
        }

        private IEnumerator CreateItems()
        {
            foreach (Review review in _reviews.GetRange(_currentLoadedReviews, Mathf.Min(_reviewLoadingChunk, _reviews.Count - _currentLoadedReviews)))
            {
                Instantiate(Resources.Load<RatingItem>("Hotelsuche/RatingItem"), _reviewsParent).Initialize(review.User, review.CreationDate, review.Title, review.Description, review.Stars);
                _reviewLoadMoreButton.transform.SetAsLastSibling();
                _currentLoadedReviews++;
                yield return null;
            }

            if (_currentLoadedReviews == _reviews.Count)
                _reviewLoadMoreButton.SetActive(false);
            else
                _reviewLoadMoreButton.SetActive(true);
        }

        private void CreateRoomViews()
        {
            string sql = @"
                SELECT 
                    hr.hotel_id, 
                    hr.room_type_id, 
                    rt.name, 
                    rt.description, 
                    rt.max_occupancy, 
                    MIN(hr.price) as MinPreis 
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
                GROUP BY 
                    hr.hotel_id, 
                    hr.room_type_id, 
                    rt.name, 
                    rt.description, 
                    rt.max_occupancy";

            using (MySqlDataReader reader = DatabaseManager.Instance.ExcecuteQuery(sql, new object[] { _hotel.ID, _startDate, _endDate }))
            {
                while (reader.Read()) 
                {
                    Instantiate(Resources.Load<RoomviewItemManager>("Hotelsuche/RoomviewItem"), _roomviewItemParent).Initialize(
                        _hotel,
                        reader.GetInt32("room_type_id"),
                        reader.GetString("name"),
                        reader.GetInt32("max_occupancy"),
                        reader.GetString("description"),
                        reader.GetFloat("MinPreis"),
                        _startDate,
                        _endDate
                        );                                                                             
                }
            }
        }
    }
}
