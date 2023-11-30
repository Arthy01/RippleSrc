using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ripple
{
    public class Hotel
    {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Address { get; private set; }
        public string FormattedAddress { get; private set; }
        public string ZipCode { get; private set; }
        public string City { get; private set; }
        public string Country { get; private set; }

        public Hotel(int id)
        {
            string sql = "select HO.*, CI.city_name, CO.country_name from Hotels HO join Cities CI on HO.city_id = CI.city_id join Countries CO on HO.country_id = CO.country_id where hotel_id = @param1";
            using (MySql.Data.MySqlClient.MySqlDataReader reader = DatabaseManager.Instance.ExcecuteQuery(sql, new object[] { id }))
            {
                reader.Read();

                ID = id;

                Name = reader.GetString("name");
                Description = reader.GetString("description");
                Address = reader.GetString("address");
                ZipCode = reader.GetString("zip_code");
                City = reader.GetString("city_name");
                Country = reader.GetString("country_name");

                FormattedAddress =
                    Name + "\n" +
                    Address + "\n" +
                    ZipCode + ", " + City + "\n" +
                    Country;
            }
        }

        public Hotel(int id, string name, string description, string address, string zipcode, string city, string country)
        {
            ID = id;

            Name = name;
            Description = description;
            Address = address;
            ZipCode = zipcode; 
            City = city; 
            Country = country;

            FormattedAddress =
                Name + "\n" +
                Address + "\n" +
                ZipCode + ", " + City + "\n" +
                Country;
        }
    }
}
