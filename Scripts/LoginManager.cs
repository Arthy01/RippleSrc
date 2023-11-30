using Michsky.MUIP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;
using TMPro;

namespace Ripple
{
    public class LoginManager : MonoBehaviour
    {
        public static int CurrentUserID { get; private set; } = -1;

        [SerializeField] private Animator _canvasAnimator;
        [SerializeField] private NotificationManager _wrongCredentialsNotification;
        [SerializeField] private NotificationManager _registerSuccess;
        [SerializeField] private NotificationManager _registerFailure;

        [SerializeField] private TMP_InputField _usernameInput;
        [SerializeField] private TMP_InputField _passwordInput;

        private void Start()
        {
            Application.targetFrameRate = 120;
        }

        public void OnLoginSubmit()
        {
            print("Login...");

            if (DatabaseManager.Instance.IsConnected)
            {
                if (AreLoginCredentialsValid(_usernameInput.text, _passwordInput.text))
                    _canvasAnimator.Play("LoginFinished");
                else
                    _wrongCredentialsNotification.Open();
            }
        }

        public void OnRegisterSubmit()
        {
            print("Register...");

            if (string.IsNullOrEmpty(_usernameInput.text) || string.IsNullOrEmpty(_passwordInput.text))
                return;

            if (DatabaseManager.Instance.IsConnected)
            {
                string sql = "SELECT password FROM Users WHERE username = @param1";
                bool validUsername = false;

                using (MySqlDataReader reader = DatabaseManager.Instance.ExcecuteQuery(sql, _usernameInput.text))
                {
                    validUsername = !reader.HasRows;
                }

                if (validUsername)
                {
                    sql = "INSERT INTO Users (username, password) VALUES (@param1, @param2)";

                    DatabaseManager.Instance.ExcecuteCommand(sql, _usernameInput.text, HashPassword(_passwordInput.text));

                    _registerSuccess.Open();
                }
                else
                {
                    _registerFailure.Open();
                }
            }
        }

        private bool AreLoginCredentialsValid(string username, string enteredPassword)
        {
            string hashedEnteredPassword = HashPassword(enteredPassword);

            string sql = "SELECT * FROM Users WHERE username = @param1";

            using (MySqlDataReader reader = DatabaseManager.Instance.ExcecuteQuery(sql, username))
            {
                if (reader.Read())
                {
                    string storedPassword = reader.GetString("password");

                    if (storedPassword == hashedEnteredPassword)
                    {
                        CurrentUserID = reader.GetInt32("user_id");
                        return true;
                    }

                    return false;
                }
                else
                {
                    // Benutzername nicht gefunden
                    return false;
                }
            }
        }

        public static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public void Logout()
        {
            CurrentUserID = -1;
            _canvasAnimator.Play("Logout");
        }
    }
}
