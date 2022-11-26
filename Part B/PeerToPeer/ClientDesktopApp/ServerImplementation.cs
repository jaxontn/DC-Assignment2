using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using System.Data.SqlClient; //SQL Server local DB
using System.Data;

namespace ClientDesktopApp
{
    //Describing our service behaviour
    //makes the service multi-threaded. Manage our own thread synchronization
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]

    internal class ServerImplementation : ServerInterface
    {

        public ServerImplementation()
        {
        }

        
        public List<String> getJob() //FOR OTHER CLIENT
        {

            String cn_string = Properties.Settings.Default.ClientAppDatabaseConnectionString;

            SqlConnection cn_connection = new SqlConnection(cn_string);

            if (cn_connection.State != ConnectionState.Open) cn_connection.Open();

            String sql_Text = "SELECT * FROM JobNAnswer";

            
            DataTable tbl = new DataTable(); //Create a table
            SqlDataAdapter adapter = new SqlDataAdapter(sql_Text, cn_connection); //the adapter will fill the data into a table

            adapter.Fill(tbl); //fill the table

            List<String> pythonScriptsJobs = new List<String>();

            foreach (DataRow row in tbl.Rows)
            {
                pythonScriptsJobs.Add(row["Job"].ToString());
            }

            return pythonScriptsJobs;

        }

        
        public List<String> getAnwers() //FOR HOST
        {

            String cn_string = Properties.Settings.Default.ClientAppDatabaseConnectionString;

            SqlConnection cn_connection = new SqlConnection(cn_string);

            if (cn_connection.State != ConnectionState.Open) cn_connection.Open();

            String sql_Text = "SELECT * FROM JobNAnswer";


            DataTable tbl = new DataTable(); //Create a table
            SqlDataAdapter adapter = new SqlDataAdapter(sql_Text, cn_connection); //the adapter will fill the data into a table

            adapter.Fill(tbl); //fill the table

            List<String> pythonScriptsAnswers = new List<String>();

            //loop through and get the answer that are not null or empty
            foreach (DataRow row in tbl.Rows)
            {
                if (!String.IsNullOrEmpty(row["Answer"].ToString()))
                {
                    pythonScriptsAnswers.Add(row["Answer"].ToString());
                }
            }

            return pythonScriptsAnswers;

        }




        public void UploadAnswers(List<String> answerList, List<String> jobs) //FOR OTHER CLIENT
        {
            String cn_string = Properties.Settings.Default.ClientAppDatabaseConnectionString;

            SqlConnection cn_connection = new SqlConnection(cn_string);

            if (cn_connection.State != ConnectionState.Open) cn_connection.Open();

            String sql_Text = "SELECT * FROM JobNAnswer";

            DataTable tbl = new DataTable(); //Create a table
            SqlDataAdapter adapter = new SqlDataAdapter(sql_Text, cn_connection); //the adapter will fill the data into a table

            adapter.Fill(tbl); //fill the table

            //loop through and get the answer that are not null or empty
            foreach (DataRow row in tbl.Rows)
            {
                if (!String.IsNullOrEmpty(row["Answer"].ToString()))
                {
                    //do nothing
                }
                else
                {
                    //update the answer
                    row["Answer"] = answerList[0];
                    answerList.RemoveAt(0);
                }
            }
            //update the whoe database


            String sql_Text2 = "UPDATE JobNAnswer SET Answer = @Answer WHERE JOB = @JOB";

            SqlCommand cmd = new SqlCommand(sql_Text2, cn_connection);

            cmd.Parameters.Add("@Answer", SqlDbType.NVarChar);
            cmd.Parameters.Add("@JOB", SqlDbType.NVarChar);

            foreach (DataRow row in tbl.Rows)
            {
                if (!String.IsNullOrEmpty(row["Answer"].ToString()))
                {
                    cmd.Parameters["@Answer"].Value = row["Answer"].ToString();
                    cmd.Parameters["@JOB"].Value = row["Job"].ToString();
                    cmd.ExecuteNonQuery();
                }
            }
        }


        

        public void UploadJob(String job, String URL, int ID) //FOR HOST
        {
            //make sure the Primary Key of the database doesnt match ID
            String cn_string = Properties.Settings.Default.ClientAppDatabaseConnectionString;

            SqlConnection cn_connection = new SqlConnection(cn_string);

            if (cn_connection.State != ConnectionState.Open) cn_connection.Open();

            String sql_Text = "SELECT * FROM JobNAnswer";

            DataTable tbl = new DataTable(); //Create a table

            SqlDataAdapter adapter = new SqlDataAdapter(sql_Text, cn_connection); //the adapter will fill the data into a table

            adapter.Fill(tbl); //fill the table


            Random rand = new Random();
            int newID = 0;


            //loop through and get the answer that are not null or empty
            foreach (DataRow row in tbl.Rows)
            {
                if (row["ID"].ToString() == ID.ToString())
                {
                    newID = rand.Next(10000, 20000);
                    ID = newID;
                    
                    while(row["ID"].ToString() == ID.ToString())
                    {
                        newID = rand.Next(10000, 20000);
                        ID = newID;
                    }
                }
                else
                {
                    //update the answer
                    row["Job"] = job;
                    row["URL"] = URL;
                    row["ID"] = ID;
                }
            }
            //update the whoe database
            
            

            //----------------------------------------------------------------




            //insert new data to the database, Id, URL, Job

           // String cn_string = Properties.Settings.Default.ClientAppDatabaseConnectionString;

           // SqlConnection cn_connection = new SqlConnection(cn_string);

            if (cn_connection.State != ConnectionState.Open) cn_connection.Open();

            String sql_Text2 = "INSERT INTO JobNAnswer (Id, OwnerURL, Job) VALUES (@Id, @URL, @Job)";

            SqlCommand cmd = new SqlCommand(sql_Text2, cn_connection);

            cmd.Parameters.AddWithValue("@Id", ID);
            cmd.Parameters.AddWithValue("@URL", URL);
            cmd.Parameters.AddWithValue("@Job", job);

            cmd.ExecuteNonQuery();

        }
        
    }
}
