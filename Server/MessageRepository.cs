using Npgsql;
using System.Collections.Generic;
using System.Linq;

namespace Server
{
    public class MessageRepository
    {
        private readonly string connectionString;

        public MessageRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void SaveMessage(Message message)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO messages (sender_email, receiver_email, text, timestamp) VALUES (@from, @to, @text, @time)";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@from", message.From);
                    cmd.Parameters.AddWithValue("@to", message.To);
                    cmd.Parameters.AddWithValue("@text", message.Text);
                    cmd.Parameters.AddWithValue("@time", message.Timestamp);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Message> GetMessagesForUser(string userEmail)
        {
            var messages = new List<Message>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT sender_email, text, timestamp FROM messages WHERE receiver_email = @to ORDER BY timestamp DESC";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@to", userEmail);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add(new Message
                            {
                                From = reader.GetString(0),
                                To = userEmail,
                                Text = reader.GetString(1),
                                Timestamp = reader.GetDateTime(2)
                            });
                        }
                    }
                }
            }

            return messages;
        }
    }

}
