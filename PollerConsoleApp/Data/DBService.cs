using System;
using MySql.Data.MySqlClient;

namespace PollerConsoleApp.Data
{
    public class DBService
    {
        //example: please change parameters to relevant your MySQL server
        private string connectionString = "server=ip_server;user=db_username;database=db_name;password=db_password";
        
        MySqlConnection connection;

        public DBService()
        {
            initialConnection();
        }

        private void initialConnection()
        {
           connection = new MySqlConnection(connectionString);
        }

        public string readLastRecord()
        {
            string answer = "";

            string readSQLString = "SELECT `keys` FROM `values` ORDER BY id DESC LIMIT 1";

            try
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(readSQLString, connection);
                answer = command.ExecuteScalar().ToString();
                Console.WriteLine("Last record:\n" + answer + "\n");

            } catch (Exception e)
            {
                Console.WriteLine("Error connection or DB is empty");
            }
            finally
            {
                connection.Close();
            }
            return answer;

        } 

        public void insertNewValue(string keyValue)
        {
            
            string insertSQLString = "INSERT INTO `values` (`id`, `keys`) VALUES(NULL, '" + keyValue + "')";
            try
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(insertSQLString, connection);
                command.ExecuteScalar();
                Console.WriteLine("Recorded!\n" + keyValue + "\n");

            }
            catch (Exception e)
            {
                Console.WriteLine("Error connection or DB is empty");
            }
            finally
            {
                connection.Close();
            }

        }

    }
}
