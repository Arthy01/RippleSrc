using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ripple
{
    public class HotelRoom
    {
        public int ID { get; private set; }
        public int RoomNumber { get; private set; }
        public float Price { get; private set; }
        public int RoomTypeID { get; private set; }
        public string RoomTypeName { get; private set; }
        public string RoomTypeDescription { get; private set; }
        public int MaxOccipancy { get; private set; }

        public HotelRoom(int hotelRoomID)
        {
            string sql = "select HR.*, RT.* from HotelRooms HR join RoomTypes RT on HR.room_type_id = RT.room_type_id where hotel_id = @param1";

            using (MySql.Data.MySqlClient.MySqlDataReader reader = DatabaseManager.Instance.ExcecuteQuery(sql, new object[] { hotelRoomID }))
            {
                reader.Read();

                ID = hotelRoomID;

                RoomNumber = reader.GetInt32("room_number");
                Price = reader.GetFloat("price");
                RoomTypeID = reader.GetInt32("room_type_id");
                RoomTypeName = reader.GetString("name");
                RoomTypeDescription = reader.GetString("description");
                MaxOccipancy = reader.GetInt32("max_occupancy");
            }
        }
    }
}
