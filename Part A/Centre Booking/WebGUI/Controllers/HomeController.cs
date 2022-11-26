using Microsoft.AspNetCore.Mvc;
using RestSharp;
using WebGUI.Models; //NEW
using Newtonsoft.Json; //NEW
using Newtonsoft.Json.Linq; //NEW, for JObject

namespace WebGUI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Title = "Home";

            /*NEW-------------------------------------------------------------------*/
            RestClient restClient = new RestClient("http://localhost:56447/");
            RestRequest restRequest = new RestRequest("api/Centres", Method.Get);
            RestResponse restResponse = restClient.Execute(restRequest);
            /*----------------------------------------------------------------------*/

            List<Centre> centres = JsonConvert.DeserializeObject<List<Centre>>(restResponse.Content);
           /* String result = JsonConvert.DeserializeObject<String>(restResponse.Content); //in JSON String

            dynamic data = JObject.Parse(result); //Parsing the JSON String data to get the data.
            */
            return View(centres);
        }
    }
}
