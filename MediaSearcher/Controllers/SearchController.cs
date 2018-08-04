using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using MediaSearcher.ViewModels;
using Microsoft.AspNet.Identity;
using MediaSearcher.Models;

namespace MediaSearcher.Controllers
{
    public class SearchController : Controller
    {
        // GET: Search
        public ActionResult Index()
        {
            return View();
        }

        // GET: Search/Details/5
        public ActionResult Details(int id)
        {
            Dictionary<string, int> pairs = new Dictionary<string, int>();
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                conn.Open();
                String sql = "SELECT * FROM AspNetSearch WHERE SearchId = '" + id + "' ORDER BY Value DESC;" ;
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    ViewBag.Title = rdr["Keyword"];
                    if (!pairs.ContainsKey(rdr["Word"].ToString()))
                    {
                        pairs.Add(rdr["Word"].ToString(), Convert.ToInt32(rdr["Value"]));
                    }
                }

            }
            
            return View(pairs);
        }

        // GET: Search/Create
        [HttpGet]
        public ActionResult Create()
        {
            Auth.SetUserCredentials("7lYht3bDy2P9ji0SXLl66feKi",
                                    "k9hITdO7MmzeT57xTg75GrtneYjgwglXvoAawQ98XkPE21KMPB",
                                    "963967640753655809-ZMrjjJP4F7lxCR94DUetJkROY6f1gpx",
                                    "ILZMtk7b6lk4dBdekuJz5AvN9NPCghBw4IkqOVCTwuBfT");

            SearchViewModel newSearchViewModel = new SearchViewModel();
            return View(newSearchViewModel);
        }

        // POST: Search/Create
        [HttpPost]
        public ActionResult Create(SearchViewModel newSearchViewModel)
        {
                // TODO: Add insert logic here
            if (ModelState.IsValid)
            {
                string Keyword = "";
                if (newSearchViewModel.Keyword.Contains("'"))
                {
                    Keyword = string.Join(" ", newSearchViewModel.Keyword.Split(' ').Select(x => x.Trim('\'')));
                }
                else
                {
                    Keyword = newSearchViewModel.Keyword;
                }
                //Counter holds all accounted values.
                Dictionary<string, int> counter = new Dictionary<string, int>();

                //Content holds string arrays of all incoming tweets.
                List<string[]> content = new List<string[]>();

                /***Create a tweetinvi searchparameter in english, 
                * that pulls both popular and recent posts and 
                * sets the number of results to the user's sample size.
                ***/
                var searchParameter = new SearchTweetsParameters(Keyword)
                {
                   Lang = LanguageFilter.English,
                   SearchType = SearchResultType.Mixed,
                   MaximumNumberOfResults = newSearchViewModel.SampleSize,
                };

                //All posts are pulled here and added to the content list.
                var tweets = Search.SearchTweets(searchParameter);
                foreach (var tweet in tweets)
                {
                    content.Add(tweet.Text.Split());
                }

                //Each word and it's frequency is calculated and added to the Counter Dictionary.
                foreach (string[] text in content)
                {
                    foreach (string word in text)
                    {
                        if (counter.ContainsKey(word.ToLower()))
                        {
                            counter[word.ToLower()]++;
                        }
                        else
                        {
                            counter.Add(word.ToLower(), 1);
                        }
                    }
                }

                //checks if word occurs more than 1/10 of the sample size and removes if it does - eliminiting one-off words.
                Dictionary<string, int> ccounter = new Dictionary<string, int>();
                foreach (KeyValuePair<string, int> word in counter)
                {
                    if (word.Value <= (newSearchViewModel.SampleSize / 100))
                    {
                        ccounter.Add(word.Key, word.Value);
                    }
                  
                }

                //Removes keywords with apostrophe's
                foreach (KeyValuePair<string, int> word in counter.ToList())
                {
                    if (word.Key.Contains("'"))
                    {
                        counter.Remove(word.Key);
                    }
                    if (word.Key.Contains('"'))
                    {
                        counter.Remove(word.Key);
                    }
                }
                //checks if word in list of 'fluff' words and removes them
                string[] fluff = new string[] { newSearchViewModel.Keyword, "the", "and", "rt", " ", "", "a", "i", "in", "to", "of", "it", "you", "this", "'", "''" };
                foreach (string word in fluff)
                {
                    if (counter.ContainsKey(word))
                    {
                        counter.Remove(word);
                    }
                }
                //TODO saves search if user is logged in
                var UserId = User.Identity.GetUserId();
                if (UserId != null)
                {
                    int lastSearchId = 0;
                    //Create SQL connection
                    using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                    {
                    //Obtain last search Id
                        conn.Open();
                        
                        string lastSearch = "SELECT MAX(SearchId) FROM AspNetSearch WHERE OwnerId='" + UserId.ToString() + "';";
                        SqlCommand lsI = new SqlCommand(lastSearch, conn);
                        var lastSearchTry = lsI.ExecuteScalar();
                        if(lastSearchTry == DBNull.Value)
                        {
                            lastSearchId = 0;
                        }
                        else
                        {
                            lastSearchId = Convert.ToInt32(lastSearchTry) + 1;
                        }
                        //Create new Row and insert data using foreach() loop
                        foreach (var i in counter)
                        {
                            //INSERT INTO AspNetSearch (Word, Value, SearchId, OwnerId) VALUES ('Taco', 1, 1, 1);
                            string sql = "INSERT INTO AspNetSearch (Word, Value, SearchId, OwnerId, Keyword) VALUES " + "(" + "'" + i.Key.ToString() + "', " + i.Value.ToString() + ", " + lastSearchId + ", " + "'" + UserId.ToString() + "', " + "'" + Keyword.ToString() + "');";
                            SqlCommand cmd = new SqlCommand(sql, conn);
                            cmd.ExecuteNonQuery();
                        }
                        conn.Close();
                        
                    }
                    return RedirectToAction("Details", new { id = lastSearchId });
                }
                //TODO user sort-by
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                newSearchViewModel.Errors = errors;
                return View(newSearchViewModel);
            }
        return RedirectToAction("Create");
        }

        // GET: Search/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Search/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
