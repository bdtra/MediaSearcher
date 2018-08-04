using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MediaSearcher.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Redirect("~/Search/Create/");
        }

        public ActionResult Searches()
        {
            ViewBag.Message = "Your application description page.";
            var UserId = User.Identity.GetUserId();
            Dictionary<string, int> Searches = new Dictionary<string, int>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                conn.Open();
                String sql = "SELECT SearchId, Keyword FROM AspNetSearch WHERE OwnerId = '" + UserId.ToString() + "' ORDER BY Value ASC;";
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    if (!Searches.ContainsKey(rdr["Keyword"].ToString()))
                    {
                        Searches.Add(rdr["Keyword"].ToString(), Convert.ToInt32(rdr["SearchId"]));
                    }
                }

            }
            ViewBag.Searches = Searches;
            return View(Searches);
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}