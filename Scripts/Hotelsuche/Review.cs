using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ripple
{
    public class Review
    {
        public int Stars { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string User { get; private set; }
        public DateTime CreationDate { get; private set; }

        public Review(int stars, string title, string description, string user, DateTime creationDate) 
        {
            Stars = stars;
            Title = title;
            Description = description;
            User = user;
            CreationDate = creationDate;
        }
    }
}
