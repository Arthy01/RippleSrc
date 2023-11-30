using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ripple
{
    public class RatingItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text _username_creationDate;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _description;
        [SerializeField] private Transform _starsParent;

        public void Initialize(string username, DateTime creationTime, string title, string description, int stars)
        {
            _username_creationDate.text = username + " - " + creationTime.Date.ToString("dd.MM.yyyy");
            _title.text = title;
            _description.text = description;

            for (int i = 0; i < stars; i++)
                _starsParent.GetChild(i).GetComponent<Image>().color = new Color(255, 255, 255, 1);
        }
    }
}
