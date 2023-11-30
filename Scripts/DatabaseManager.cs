using Michsky.MUIP;
using MySql.Data.MySqlClient;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Ripple
{
    public class DatabaseManager : MonoBehaviour
    {
        public static DatabaseManager Instance { get; private set; }

        public bool IsConnected => CheckConnection();
        public bool IsConnecting { get; private set; } = false;

        [Header("References")]
        [SerializeField] private NotificationManager _databaseConnectionError;
        [SerializeField] private NotificationManager _databaseConnectionRestored;

        private MySqlConnection _activeConnection = null;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            Connect(null);
        }

        private async void Connect(Action successCallback)
        {
            if (IsConnecting) return;

            IsConnecting = true;

            MySqlConnectionStringBuilder builder = GetBuilder();

            try
            {
                _activeConnection = new MySqlConnection(builder.ToString());
                await _activeConnection.OpenAsync();
                print("<b>[MySQL]</b> Opened Connection");

                IsConnecting = false;

                successCallback?.Invoke();
            }
            catch (Exception exception)
            {
                IsConnecting = false;

                print($"<b>[MySQL]</b> Connection couldn't be opened!\nTrying again...");
                print(exception.Message);

                Connect(successCallback);
            }
        }

        private MySqlConnectionStringBuilder GetBuilder()
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();

            ES3Settings settings = new ES3Settings(ES3.Location.Cache);
            string fileName = Path.Combine(Application.dataPath, "config.rpl");

            ES3.CacheFile(fileName);

            builder.Server = ES3.Load<string>("host", fileName, settings);
            builder.UserID = ES3.Load<string>("user", fileName, settings);
            builder.Password = ES3.Load<string>("password", fileName, settings);
            builder.Database = ES3.Load<string>("database", fileName, settings);
            builder.CharacterSet = "utf8mb4";

            return builder;
        }

        private bool CheckConnection()
        {
            if (_activeConnection.Ping())
                return true;
            else
            {
                _databaseConnectionError.Open();
                Connect(() => _databaseConnectionRestored.Open());
                return false;
            }
        }

        public MySqlDataReader ExcecuteQuery(string sql, params object[] parameters)
        {
            MySqlCommand cmd = new MySqlCommand(sql, _activeConnection);

            for (int i = 0; i < parameters.Length; i++)
            {
                cmd.Parameters.AddWithValue($"@param{i + 1}", parameters[i]);
            }

            return cmd.ExecuteReader();
        }

        public int ExcecuteCommand(string sql, params object[] parameters)
        {
            using (MySqlCommand cmd = new MySqlCommand(sql, _activeConnection))
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    cmd.Parameters.AddWithValue($"@param{i + 1}", parameters[i]);
                }

                return cmd.ExecuteNonQuery(); // Gibt die Anzahl der betroffenen Zeilen zurück
            }
        }

        public int ExcecuteCommandAndGetLastID(string command, object[] parameters)
        {
            using MySqlCommand cmd = new MySqlCommand(command + "; SELECT LAST_INSERT_ID();", _activeConnection);
            for (int i = 0; i < parameters.Length; i++)
            {
                cmd.Parameters.AddWithValue("@param" + (i + 1), parameters[i]);
            }

            int lastInsertedId = Convert.ToInt32(cmd.ExecuteScalar());

            return lastInsertedId;
        }


        public bool ExecuteTransaction(List<string> sqlCommands, List<object[]> parametersList)
        {
            using (MySqlCommand cmd = _activeConnection.CreateCommand())
            {
                MySqlTransaction transaction = _activeConnection.BeginTransaction();

                cmd.Transaction = transaction;

                try
                {
                    for (int i = 0; i < sqlCommands.Count; i++)
                    {
                        cmd.CommandText = sqlCommands[i];
                        cmd.Parameters.Clear();

                        object[] parameters = parametersList[i];
                        for (int j = 0; j < parameters.Length; j++)
                        {
                            cmd.Parameters.AddWithValue($"@param{j + 1}", parameters[j]);
                        }

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return true; // Alle Befehle wurden erfolgreich ausgeführt
                }
                catch (Exception e)
                {
                    transaction.Rollback(); // Bei einem Fehler werden alle Befehle zurückgesetzt
                    print(e.Message);
                    return false;
                }
            }
        }

        [Button]
        public void TruncateTables()
        {
            using (MySqlCommand cmd = _activeConnection.CreateCommand())
            {
                MySqlTransaction transaction = _activeConnection.BeginTransaction();
                cmd.Transaction = transaction;

                try
                {
                    // Deaktiviere Fremdschlüssenüberprüfung
                    cmd.CommandText = "SET foreign_key_checks = 0;";
                    cmd.ExecuteNonQuery();

                    // 1. Tabellen ohne Foreign Keys und die von anderen Tabellen referenziert werden
                    cmd.CommandText = "TRUNCATE TABLE Airlines;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "TRUNCATE TABLE Hotels;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "TRUNCATE TABLE RoomTypes;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "TRUNCATE TABLE Users;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "TRUNCATE TABLE LoyaltyLevels;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "TRUNCATE TABLE Flights;";
                    cmd.ExecuteNonQuery();

                    // 2. Tabellen mit Foreign Keys und die von anderen Tabellen referenziert werden
                    cmd.CommandText = "TRUNCATE TABLE HotelRooms;";
                    cmd.ExecuteNonQuery();

                    // 3. Tabellen, die nur Foreign Keys haben
                    cmd.CommandText = "TRUNCATE TABLE Bookings;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "TRUNCATE TABLE HotelReviews;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "TRUNCATE TABLE PaymentInfo;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "TRUNCATE TABLE SpecialOffers;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "TRUNCATE TABLE Airports;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "TRUNCATE TABLE Countries;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "TRUNCATE TABLE Cities;";
                    cmd.ExecuteNonQuery();

                    // Aktiviere Fremdschlüssenüberprüfung
                    cmd.CommandText = "SET foreign_key_checks = 1;";
                    cmd.ExecuteNonQuery();

                    transaction.Commit();  // Änderungen bestätigen
                }
                catch
                {
                    transaction.Rollback();  // Bei einem Fehler alle Änderungen zurücksetzen
                    throw;  // Fehler weitergeben
                }
            }
        }

        private void OnApplicationQuit()
        {
            if (_activeConnection != null)
            {
                _activeConnection.Close();
                print("<b>[MySQL]</b> Closed Connection");
            }
        }
    }
}
