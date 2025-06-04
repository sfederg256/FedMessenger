using Npgsql;
using System;

namespace Server
{
    public class UserRepository
    {
        private readonly string connectionString;

        public UserRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public bool UserExists(string email)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT COUNT(*) FROM users WHERE user_email = @user_email";
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@user_email", email);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        public void AddUser(User user)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string query = "INSERT INTO users (user_name, user_email, user_password) VALUES (@user_name, @user_email, @user_password)";
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@user_name", user.UserName);
                    cmd.Parameters.AddWithValue("@user_email", user.Email);
                    cmd.Parameters.AddWithValue("@user_password", user.Password);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool ValidateCredentials(string email, string password)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT COUNT(*) FROM users WHERE user_email = @user_email AND user_password = @user_password";
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@user_email", email);
                    cmd.Parameters.AddWithValue("@user_password", password);

                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }
    }
}
