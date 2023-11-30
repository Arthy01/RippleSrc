using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ripple
{
    public class Flight
    {
        public int ID { get; private set; }
        public int AirlineID { get; private set; }
        public string Departure { get; private set; }
        public string Arrival { get; private set; }
        public DateTime DepartureTime { get; private set; }
        public DateTime ArrivalTime { get; private set; }
        public int MaxPassengers { get; private set; }
        public float Price { get; private set; }
        public int CurrentPassengers = -1;

        public Flight(int id, int airlineID, string departure, string arrival, DateTime departureTime, DateTime arrivalTime, int maxPassengers, float price)
        {
            ID = id;
            AirlineID = airlineID;
            Departure = departure;
            Arrival = arrival;
            DepartureTime = departureTime;
            ArrivalTime = arrivalTime;
            MaxPassengers = maxPassengers;
            Price = price;
        }

        public Flight(int id)
        {
            string sql = "SELECT * FROM Flights WHERE flight_id = @param1";

            using (MySqlDataReader reader = DatabaseManager.Instance.ExcecuteQuery(sql, new object[] { id }))
            {
                reader.Read();

                ID = id;
                AirlineID = reader.GetInt32("airline_id");
                Departure = reader.GetString("departure");
                Arrival = reader.GetString("arrival");
                DepartureTime = reader.GetDateTime("departure_time");
                ArrivalTime = reader.GetDateTime("arrival_time");
                MaxPassengers = reader.GetInt32("max_passengers");
                Price = reader.GetFloat("price");
            }
        }
    }
}
