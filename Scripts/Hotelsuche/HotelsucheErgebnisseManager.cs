using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ripple
{
    public class HotelsucheErgebnisseManager : MonoBehaviour
    {
        [SerializeField] private Transform _hotelsucheItemParent;
        [SerializeField] private Transform _loadMoreButton;

        private int _loadingHotelsChunk = 3;
        private int _currentLoadedHotels = 0;

        private List<Hotel> _hotels;

        private DateTime _start;
        private DateTime _end;

        public void Initialize(List<Hotel> results, DateTime startDate, DateTime endDate)
        {
            _hotels = results;
            _start = startDate;
            _end = endDate;

            StartCoroutine(CreateItems());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            foreach (HotelsucheItemManager item in _hotelsucheItemParent.GetComponentsInChildren<HotelsucheItemManager>(true))
            {
                Destroy(item.gameObject);
            }

            _currentLoadedHotels = 0;
        }

        private IEnumerator CreateItems()
        {
            foreach (Hotel hotel in _hotels.GetRange(_currentLoadedHotels, Mathf.Min(_loadingHotelsChunk, _hotels.Count - _currentLoadedHotels)))
            {
                Instantiate(Resources.Load<HotelsucheItemManager>("Hotelsuche/HotelsucheItem"), _hotelsucheItemParent).Initialize(hotel, _start, _end);
                _loadMoreButton.SetAsLastSibling();
                _currentLoadedHotels++;
                yield return hotel;
            }

            if (_currentLoadedHotels == _hotels.Count)
                _loadMoreButton.gameObject.SetActive(false);
            else
                _loadMoreButton.gameObject.SetActive(true);
        }

        public void LoadMore()
        {
            StartCoroutine(CreateItems());
        }
    }
}
