using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SparklingHome.Models;
using System.Net.Http;
using Newtonsoft.Json;

namespace SparklingHome.Controllers
{
    public class NewsController : Controller
    {
        // retrieve news articles from api gateway endpoint
        private const string sparklinghomeNewsEndpoint = "https://15xpego1h2.execute-api.us-east-1.amazonaws.com/sparklinghome/news";

        private async Task<List<News>> GetNewsArticles(string resource)
        {
            HttpClient client = new HttpClient();
            try
            {
                HttpResponseMessage response = await client.GetAsync(resource);

                if (response.IsSuccessStatusCode)
                {
                    string news = await response.Content.ReadAsStringAsync();
                    List<News> newsDeserialised = JsonConvert.DeserializeObject<List<News>>(news);

                    return newsDeserialised;
                }
                else
                {
                    return new List<News>();
                }
            }
            catch (Exception exception)
            {
                return new List<News>();
            }
        }

        public async Task<IActionResult> Index(string? SortOption)
        {
            Tuple<List<News>, bool> newsSortTuple;
            if (SortOption != null)
            {
                newsSortTuple = SortOption == "latest"
                    ? Tuple.Create(await GetNewsArticles(sparklinghomeNewsEndpoint + "?sort=latest"), true)
                    : Tuple.Create(await GetNewsArticles(sparklinghomeNewsEndpoint), false);
            }
            else
            {
                newsSortTuple = Tuple.Create(await GetNewsArticles(sparklinghomeNewsEndpoint), true);
            }

            return View(newsSortTuple);
        }

        public async Task<IActionResult> NewsArticle(string newsId)
        {
            List<News> newsArticle = await GetNewsArticles(sparklinghomeNewsEndpoint + "/" + newsId);
            return View(newsArticle.First());
        }

    }
}
