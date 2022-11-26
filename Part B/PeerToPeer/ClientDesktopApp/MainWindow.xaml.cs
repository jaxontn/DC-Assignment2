using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ServiceModel; //NECESSARY
using RestSharp;       //NECESSARY
using Newtonsoft.Json; //NECESSARY
using ClientDesktopApp.Properties;
using System.Net;
using IronPython.Hosting;
using IronPython;
using Microsoft.Scripting.Hosting;
using System.IO;
using System.Linq.Expressions;
using System.ComponentModel;

namespace ClientDesktopApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        String URLNoPort = "net.tcp://localhost:";
        String ClientPort;
        String URL;
        int theID = 0;

        Thread networkThread;
        Thread serverThread;

      //  List<String> pythonScriptsJobs; //LIST OF JOBS FOR OTHER CLIENTS
        List<String> OtherClientjobList;
        List<String> OtherpythonScriptsAnswers;



        bool loopDisplayClientListBox = true;
        bool hasStartedClient = false;
        bool runServerThread = false;

        ChannelFactory<ServerInterface> foobFactory;
        ChannelFactory<ServerInterface> foobFactoryHost;


        public MainWindow()
        {
            InitializeComponent();

         //   pythonScriptsJobs = new List<String>(); //Initialize the list
            OtherClientjobList = new List<String>(); //Initialize the list
            OtherpythonScriptsAnswers = new List<String>(); //Initialize the list


            

        }

        private void SetClientWithPort()
        {
            //This is the actual host service system of the client.
            ServiceHost host;

            //This represents a tcp/ip binding in the Windows network stack
            NetTcpBinding tcp = new NetTcpBinding();

            //Bind server to the implementation of the DataServer
            host = new ServiceHost(typeof(ServerImplementation));

            //Present the publicity accessible interface to the client. 0.0.0.0 tells .net
            //to accept on any interface. ClientPort means this will use the port mentioned.
            //ClientService is a name for the actual service, this can be any string.
            URL = URLNoPort + ClientPort;
            host.AddServiceEndpoint(typeof(ServerInterface), tcp, URL);

            //and open the host for business!
            host.Open();


            //do i need to close host ?  host.Close();

            NetTcpBinding tcp2 = new NetTcpBinding();

            //Set the URL and Create the connection
            foobFactoryHost = new ChannelFactory<ServerInterface>(tcp2, URL);
        }


        //Register the client to the web server for other clients to discover
        private void RegisterClient()
        {
            bool duplicateId = false;
            List<Client> clients = getClientList();

            Random rand = new Random();
            int id = rand.Next(10000, 20000);
            theID = id;
            
            if (clients != null) //Client List not empty
            {
                //number of clients
                int numClients = clients.Count;      //ERROR: null?

                


                //loop thorugh the clients
                foreach (Client c in clients)
                {
                    //if c has same ID
                    if (c.Id == id)
                    {
                        duplicateId = true;
                    }
                }
                //-------------------------------------------------------------


                //AVOID CREATING A NEW CLIENT WITH EXISTING ID THAT OTHER
                //CLIENTS ARE REGISTERED ON
                //-------------------------------------------------------------
                while (duplicateId)
                {
                    duplicateId = false;

                    //1. create another random port number
                    id = rand.Next(10000, 20000);

                    //2. Check for list of clients if have same port or not.
                    List<Client> checkNewClients = getClientList();

                    //loop thorugh the clients
                    foreach (Client c in checkNewClients)
                    {
                        //if c has same ID
                        if (c.Id == id)
                        {
                            duplicateId = true;
                        }
                    }
                }

            }



            Client newClient = new Client();

            newClient.Id = id;
            newClient.Name = URLNoPort + ClientPort;
            newClient.IPAddress = URLNoPort;
            newClient.Port = ClientPort;

            theID = id;
            
            RestClient restClient = new RestClient("http://localhost:61569/");
            RestRequest restRequest = new RestRequest("api/Clients", Method.Post);
            restRequest.AddJsonBody(JsonConvert.SerializeObject(newClient));
            RestResponse restResponse = restClient.Execute(restRequest);


            

            try
            {
                Client returnClient = JsonConvert.DeserializeObject<Client>(restResponse.Content);

                if (returnClient != null && returnClient.Id == newClient.Id)
                {
                    MessageBox.Show("Client registered successfully!");
                }
                else
                {
                    MessageBox.Show("Somehting is ODD: line 167");
                }
            }
            catch (ArgumentNullException e)
            {
                MessageBox.Show("Client registration failed!: " + restResponse.Content);
            }

        }


        //GET CLIENTS FROM WEB SERVER
        private List<Client> getClientList()
        {
            RestClient restClient = new RestClient("http://localhost:61569/");
            RestRequest restRequest = new RestRequest("api/Clients", Method.Get);
            RestResponse restResponse = restClient.Execute(restRequest);

            List<Client> clients = null;

            try
            {
                clients = JsonConvert.DeserializeObject<List<Client>>(restResponse.Content);
            }
            catch(ArgumentNullException e)
            {
                MessageBox.Show("Empty List of Clients");
            }
            catch(JsonSerializationException e)
            {
                MessageBox.Show("JSON: Empty List of Clients");
            }
            

            return clients;
        }


        private void displayClientListBox()
        {
            //OtherClientListBox
            List<Client> clients = getClientList();

            //TO AVOID THE THREAD ISSUE FOR GUI-----------------------------------
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    OtherClientListBox.Items.Clear();

                    foreach (Client c in clients)
                    {
                        //if matches URL, dont add
                        if (c.Name != URL)
                        {
                            OtherClientListBox.Items.Add(c.Name);
                        }
                        else
                        {
                            
                        }
                    }
                });
            }
            catch(TaskCanceledException e)
            {
                MessageBox.Show("Task Canceled Exception, ListBox");
            }
            catch (NullReferenceException e)
            {

            }


            //---------------------------------------------------------------------
        }


        private void CheckOtherClientJobs()
        {
            List<String> jobs = new List<String>();
            jobs = OtherClientjobList;
            //if have job, do the job.
            //if otherclientjoblist not null and not empty
            if (OtherClientjobList != null && OtherClientjobList.Count > 0)
            {
                //loop through the list
                foreach (String job in OtherClientjobList)
                {
                    //if job is not null
                    if (job != null)
                    {
                        //do the job
                        //after done with jobs, store answers in a list of answers.
                        OtherpythonScriptsAnswers.Add(RunPythonCode(job));

                        //remove job from OtherClientjobList

                        //OtherClientjobList.Remove(job);
                    }
                }

                OtherClientjobList = null;

                foobFactory.CreateChannel().UploadAnswers(OtherpythonScriptsAnswers, jobs);

                runServerThread = true;
            }
        }




        //TWO THREADS!*******************************************************************************


        //NETWORKING THREAD------------------------------------------------
        private void NetworkThread()
        {
            bool hasDuplicatePort = false;

            //INITIAL------------------------------------------------------
            //1. create a random port number
            Random rand = new Random();
            int port = rand.Next(10000, 20000);

            

            //2. Check for list of clients if have same port or not.
            List<Client> clients = getClientList();

            //loop thorugh the clients
            foreach(Client c in clients ?? Enumerable.Empty<Client>())  //NEW--------------
            {
                //if c has same port
                if(c.Port.Equals(port))
                {
                    hasDuplicatePort = true;
                }
            }
            //-------------------------------------------------------------


            //AVOID CREATING A NEW CLIENT WITH EXISTING PORTS THAT OTHER
            //CLIENTS ARE REGISTERED ON
            //-------------------------------------------------------------
            while(hasDuplicatePort)
            {
                hasDuplicatePort = false;

                //1. create another random port number
                port = rand.Next(10000, 20000);

                //2. Check for list of clients if have same port or not.
                List<Client> checkNewClients = getClientList();

                //loop thorugh the clients
                foreach (Client c in checkNewClients)
                {
                    //if c has same port
                    if (c.Port.Equals(port))
                    {
                        hasDuplicatePort = true;
                    }
                }
            }


            //SET THE CLIENT PORT WITH THE SERVICE NAME (can be any name)
            ClientPort = port + "/ClientService";

            SetClientWithPort();

            RegisterClient(); //Register the client to the web server for other clients to discover



            while(loopDisplayClientListBox)
            {
                displayClientListBox(); //Display the list of clients in the listbox
                Thread.Sleep(5000);
                //  MessageBox.Show("refreshed Client List"); //FOR DEBUG: THE REFRESH LIST of CLIENTS


                CheckOtherClientJobs();                
            }


        }
        //END OF NETWORKING THREAD----------------------------------------


        

        //SERVER THREAD---------------------------------------------------
        private void ServerThread()
        {
            while(loopDisplayClientListBox)
            {
                if (runServerThread)
                {
                    //display result listbox with foobFactoryHost.CreateChannel().getAnwers();
                   // List<String> answers = foobFactoryHost.CreateChannel().getAnwers();
                    List<String> answers = foobFactory.CreateChannel().getAnwers();
                    List<String> answers2 = foobFactoryHost.CreateChannel().getAnwers();

                    //TO AVOID THE THREAD ISSUE FOR GUI-----------------------------------
                    try
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            //ResultListBox.Items.Clear();

                            foreach (String ans in answers)
                            {
                                ResultListBox.Items.Add(ans);
                            }
                        });
                    }
                    catch (TaskCanceledException e)
                    {
                        MessageBox.Show("Task Canceled Exception, ListBox");
                    }
                    catch (NullReferenceException e)
                    {

                    }


                    runServerThread = false;
                }
            } 
        }
        //END OF SERVER THREAD--------------------------------------------



        //END OF TWO THREADS***************************************************************




        private void OpenFileBtn_Click(object sender, RoutedEventArgs e)
        {
            //open file explorer and select a file
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".py";
            dlg.Filter = "All Files|*.*";

            Nullable<bool> result = dlg.ShowDialog();

            //get the selected file path name
            if (result == true)
            {
                string filename = dlg.FileName;
                filePathLabel.Content = "File path: \n" + filename;

                //get the string code from the file.-------------------
                string stringScript = File.ReadAllText(filename);
               // pythonScriptsJobs.Add(stringScript); //---------------------------------
                foobFactoryHost.CreateChannel().UploadJob(stringScript, URL, theID);
                /*
                RunPythonCode(stringScript);
                */
                //-----------------------------------------------------
            }
        }

        private void StartClientBtn_Click(object sender, RoutedEventArgs e)
        {
            if(!hasStartedClient)
            {
                //start a new network thread
                networkThread = new Thread(new ThreadStart(NetworkThread));
                networkThread.Start();

                //start a new server thread
                serverThread = new Thread(new ThreadStart(ServerThread));
                serverThread.Start();

                hasStartedClient = true;
            }
            else 
            {
                MessageBox.Show("Client has started and registered already");
            }
        }


        private void AddJobBtn_Click(object sender, RoutedEventArgs e)
        {
            String pycode = codeBox.Text;
            //pythonScriptsJobs.Add(pycode);//-------------
            foobFactoryHost.CreateChannel().UploadJob(pycode, URL, theID);
            //RunPythonCode(pycode);
        }


        //show a pop-up box when the press the close x button
        protected override void OnClosing(CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to close? This Client will be removed from Server", "Close", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                if (theID == 0)
                {

                }
                else
                {
                    RestClient restClient = new RestClient("http://localhost:61569/");
                    RestRequest restRequest = new RestRequest("api/Clients/" + theID, Method.Delete);

                    RestResponse restResponse = restClient.Execute(restRequest);


                    Client returnClient = JsonConvert.DeserializeObject<Client>(restResponse.Content);


                    if (returnClient != null && returnClient.Id == theID)
                    {
                        MessageBox.Show("Client removed successfully!");
                        loopDisplayClientListBox = false;
                    }
                    else
                    {
                        MessageBox.Show("Client removal failed!: " + restResponse.Content);
                    }

                }
            }
        }




       
        

        private void ConnectOtherClientBtn_Click(object sender, RoutedEventArgs e)
        {
            if(OtherClientListBox.SelectedIndex != -1)
            {
                String selectedClient = OtherClientListBox.SelectedItem.ToString();

                MessageBox.Show("Selected: " + selectedClient);

                NetTcpBinding tcp = new NetTcpBinding();

                //Set the URL and Create the connection
                foobFactory = new ChannelFactory<ServerInterface>(tcp, selectedClient);

                //Also, tell me how many entries are in the DB.
                OtherClientjobList = (foobFactory.CreateChannel()).getJob();
                MessageBox.Show((OtherClientjobList.Count).ToString());

              //  OtherClientjobList = (foobFactoryHost.CreateChannel()).getJob();
               // MessageBox.Show((OtherClientjobList.Count).ToString());

            }
            else
            {
                MessageBox.Show("Please select a client from the list");
            }
        }

        
        //IRON PYTHON--------------------------------------------------------------------------------

        public String RunPythonCode(String script)
        {
            // Create a new engine
            ScriptEngine engine = Python.CreateEngine();
            ScriptScope scope = engine.CreateScope();
            engine.Execute(script, scope);

            //this uses C# Dynamic types. They can be anything
            dynamic testFunction = scope.GetVariable("test_func");
            var result = testFunction();

            MessageBox.Show(result.ToString());

            return result.ToString();
        }
        //END OF IRON PYTHON-------------------------------------------------------------------------

    }
}
