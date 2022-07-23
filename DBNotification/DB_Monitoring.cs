using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Transactions;

namespace DBNotification
{
    public class DB_Monitoring
    {
        public DB_Monitoring()
        {

        }

        public static Thread? BackgroundThread;
        public static bool Done = false;
        public const string ConnectionString = "Data Source=server10;Initial Catalog=QueueExample;Integrated Security=True";
        public const string ReceiveStoredProcedureName = "[dbo].[Receive_Message]";
        public static void ReceiveMessages()
        {
            string UserName = "guest";
            string Password = "guest";
            string HostName = "localhost";

            //Main entry point to the RabbitMQ .NET AMQP client
            var connectionFactory = new RabbitMQ.Client.ConnectionFactory()
            {
                UserName = UserName,
                Password = Password,
                HostName = HostName
            };

            var connection = connectionFactory.CreateConnection();
            var model = connection.CreateModel();
            var properties = model.CreateBasicProperties();

            properties.Persistent = false;

            while (!Done)
            {
                using (TransactionScope? scope = new TransactionScope())
                {
                    using (SqlConnection conn = new SqlConnection(ConnectionString))
                    {
                        using (SqlCommand command = new SqlCommand(ReceiveStoredProcedureName, conn))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.Add("text", SqlDbType.NVarChar, -1);
                            command.Parameters["text"].Direction = ParameterDirection.Output;

                            conn.Open();
                            command.ExecuteNonQuery();
                            string? text = command.Parameters["text"].Value as string;
                            conn.Close();
                            scope.Complete();

                            if (text != null)
                            {
                                //Console.WriteLine("Message Received: " + text);
                                byte[] messagebuffer = Encoding.Default.GetBytes(text);
                                model.BasicPublish("PeopleExchange", "RouteKey", true, properties, messagebuffer);
                                //Console.WriteLine("Message Sent");
                            }
                        }
                    }
                }
            }
        }



        public void Start()
        {
            ThreadStart? ts = new ThreadStart(ReceiveMessages);
            BackgroundThread = new Thread(ts);
            BackgroundThread.Start();
        }

        public void Stop()
        {
            Done = true;
        }
    }
}
