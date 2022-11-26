using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using WebGUI.Models; //NEW

namespace WebGUI.Controllers
{
    public class AccessController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Title = "Admin";

            return View();
        }


        [HttpPost]
        public IActionResult Login(String username, String password)
        {
            /*NEW-------------------------------------------------------------------*/
            RestClient restClient = new RestClient("http://localhost:56447/");
            RestRequest restRequest = new RestRequest("api/Admins", Method.Get);
            RestResponse restResponse = restClient.Execute(restRequest);

            List<Admin> admins = JsonConvert.DeserializeObject<List<Admin>>(restResponse.Content);
            

            //loop through admins to find the admin
            foreach (Admin a in admins)
            {
                //if username and password are not empty or null
                if (username != null && password != null)
                {
                    if (a.Username == username && a.Password == password)
                    {
                        //return RedirectToAction("Index", "Admin");
                        return Ok(admins);
                    }
                }
                
            }
            return NotFound();
            /*----------------------------------------------------------------------*/
        }
        

        [HttpGet]
        public List<Centre> ShowAllCentres()
        {//I changed the return type to List<Centre> because have "500" internal error
            /*NEW-------------------------------------------------------------------*/
            RestClient restClient = new RestClient("http://localhost:56447/");
            RestRequest restRequest = new RestRequest("api/Centres", Method.Get);
            RestResponse restResponse = restClient.Execute(restRequest);
            /*----------------------------------------------------------------------*/

            List<Centre> centres = JsonConvert.DeserializeObject<List<Centre>>(restResponse.Content);

            //return View(centres);
            return centres;
        }


        [HttpGet]
        public List<Booking> ShowCentreBooking(String centreName)
        {
            /*NEW-------------------------------------------------------------------*/
            RestClient restClient = new RestClient("http://localhost:56447/");
            RestRequest restRequest = new RestRequest("api/Bookings", Method.Get);
            RestResponse restResponse = restClient.Execute(restRequest);
            /*----------------------------------------------------------------------*/

            List<Booking> bookings = JsonConvert.DeserializeObject<List<Booking>>(restResponse.Content);

            List<Booking> centreBookings = new List<Booking>();

            foreach (Booking b in bookings)
            {
                //if centreName is not emptyor null
                if (centreName != null)
                {
                    String name = (b.CenterName).ToLower();
                    String lowerCaseInput = centreName.ToLower();
                    if (name.Contains(lowerCaseInput))
                    {
                        centreBookings.Add(b);
                    }
                }
                
            }

            return centreBookings;
        }



        [HttpPost]
        public List<Centre> AddCentre(String centreName)
        {
            /*Find number of centres**************************************************/
            RestClient restClient = new RestClient("http://localhost:56447/");
            RestRequest restRequest = new RestRequest("api/Centres", Method.Get);
            RestResponse restResponse = restClient.Execute(restRequest);

            List<Centre> centres = JsonConvert.DeserializeObject<List<Centre>>(restResponse.Content);

            int total = centres.Count;
            /************************************************************************/

            bool foundMatch = false;
            //loop through centres if found centreName
            foreach (Centre c in centres)
            {
                //if centreName is not empty or null
                if (centreName != null)
                {
                    String name = (c.Name).ToLower();
                    String lowerCaseInput = centreName.ToLower();


                    if (name.Equals(lowerCaseInput))
                    {
                        foundMatch = true;
                    }
                }
                
            }

            if(!foundMatch)
            {
                /*NEW-------------------------------------------------------------------*/
                RestClient restClient2 = new RestClient("http://localhost:56447/");
                RestRequest restRequest2 = new RestRequest("api/Centres", Method.Post);
                restRequest2.AddJsonBody(new Centre { Id = total + 1, Name = centreName });
                RestResponse restResponse2 = restClient2.Execute(restRequest2);
                /*----------------------------------------------------------------------*/
            }

            //get the updated centres
            /*NEW-------------------------------------------------------------------*/
            RestClient restClient3 = new RestClient("http://localhost:56447/");
            RestRequest restRequest3 = new RestRequest("api/Centres", Method.Get);
            RestResponse restResponse3 = restClient3.Execute(restRequest3);
            /*----------------------------------------------------------------------*/

            List<Centre> updatedCentres = JsonConvert.DeserializeObject<List<Centre>>(restResponse3.Content);

            return updatedCentres;
        }


        [HttpPost]
        public IActionResult AddBooking(String centreName, String bookingPersonName, DateTime startDate, DateTime endDate)
        {
            /*Find number of bookings**************************************************/
            RestClient restClient = new RestClient("http://localhost:56447/");
            RestRequest restRequest = new RestRequest("api/Bookings", Method.Get);
            RestResponse restResponse = restClient.Execute(restRequest);

            List<Booking> bookings = JsonConvert.DeserializeObject<List<Booking>>(restResponse.Content);

            int total = bookings.Count;
            /************************************************************************/

            //check if the centreName, bookingPersonName, startDate, and endDate are not repeat each row
            bool foundMatch = false;


            //#######################################################################
            DateTime defaultTime = new DateTime(0001, 01, 01, 00, 00, 00);
            int res1 = DateTime.Compare(startDate, defaultTime);
            int res2 = DateTime.Compare(endDate, defaultTime);
            //MAKE SURE ITS NOT DEFAULT TIME, USER NEED TO CHOOSE THE TIME.
            //#######################################################################


            //if centreName, bookingPersonName, startDate, and endDate are not empty or null
            if (centreName != null && bookingPersonName != null && res1 != 0 && res2 != 0)
            {
                bool foundCentreName = false;

                //check if the centre name exist in first
                foreach (Centre c in ShowAllCentres())
                {
                    String cName = (c.Name).ToLower();
                    String cNameLowerCaseInput = centreName.ToLower();

                    if (cName.Equals(cNameLowerCaseInput))
                    {
                        foundCentreName = true;
                    }
                }


                if(foundCentreName)
                {
                    //loop through bookings if found centreName, bookingPersonName, startDate, and endDate
                    foreach (Booking b in bookings)
                    {


                        String name = (b.CenterName).ToLower();
                        String lowerCaseInput = centreName.ToLower();

                        String personName = (b.BookingPersonName).ToLower();
                        String lowerCaseInput2 = bookingPersonName.ToLower();


                        if (name.Equals(lowerCaseInput) && personName.Equals(lowerCaseInput2) && (b.StartDate).Equals(startDate) && (b.EndDate).Equals(endDate))
                        {
                            foundMatch = true;
                        }


                    }

                    if (!foundMatch) //AAAAAAAAAAAAAAAA
                    {
                        /*NEW-------------------------------------------------------------------*/
                        RestClient restClient2 = new RestClient("http://localhost:56447/");
                        RestRequest restRequest2 = new RestRequest("api/Bookings", Method.Post);
                        restRequest2.AddJsonBody(new Booking { Id = total + 1, CenterName = centreName, BookingPersonName = bookingPersonName, StartDate = startDate, EndDate = endDate });
                        RestResponse restResponse2 = restClient2.Execute(restRequest2);
                        /*----------------------------------------------------------------------*/

                        //get the updated bookings
                        /*NEW-------------------------------------------------------------------*/
                        RestClient restClient3 = new RestClient("http://localhost:56447/");
                        RestRequest restRequest3 = new RestRequest("api/Bookings", Method.Get);
                        RestResponse restResponse3 = restClient3.Execute(restRequest3);
                        /*----------------------------------------------------------------------*/

                        List<Booking> updatedBookings = JsonConvert.DeserializeObject<List<Booking>>(restResponse3.Content);

                        return Ok(updatedBookings);
                    }
                    else //duplicated booking
                    {
                        //get the updated bookings
                        /*NEW-------------------------------------------------------------------*/
                        RestClient restClient3 = new RestClient("http://localhost:56447/");
                        RestRequest restRequest3 = new RestRequest("api/Bookings", Method.Get);
                        RestResponse restResponse3 = restClient3.Execute(restRequest3);
                        /*----------------------------------------------------------------------*/

                        List<Booking> updatedBookings = JsonConvert.DeserializeObject<List<Booking>>(restResponse3.Content);
                        //return NotFound(updatedBookings); //means added already

                        return BadRequest("ERROR: Duplicated Booking"); //means added already
                    }
                }
                else 
                {
                    return BadRequest("ERROR: Invalid Centre Name"); //invalid centre name
                }
                
            }
            else 
            {
                return BadRequest("ERROR: Empty Field(s)"); //empty fields
            }
        }




        [HttpGet]
        public IActionResult SearchCentre(String centreName)
        {
            /*NEW-------------------------------------------------------------------*/
            RestClient restClient = new RestClient("http://localhost:56447/");
            RestRequest restRequest = new RestRequest("api/Centres", Method.Get);
            RestResponse restResponse = restClient.Execute(restRequest);
            /*----------------------------------------------------------------------*/

            List<Centre> centres = JsonConvert.DeserializeObject<List<Centre>>(restResponse.Content);

            
            foreach (Centre c in centres)
            {
                //if centreName is not empty or null
                if (centreName != null)
                {
                    String name = (c.Name).ToLower();
                    String lowerCaseInput = centreName.ToLower();
                    if (name.Contains(lowerCaseInput))
                    {
                        return Ok(centres);
                    }
                }
            }

            return NotFound();
        }
    }
}
