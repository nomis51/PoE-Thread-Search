using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using PoETS.API;
using PoETS.Data;
using PoETS.Data.Configuration;
using PoETS.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HttpClient = PoETS.API.HttpClient;

namespace PoE_Thread_Search.Core {
    public class API {
        #region Configuration
        private string UrlTemplate;
        private string PostsTableRootName;
        private float PostPerPage = (float)ConfigManager.GetConfig().NbPostPerPage;
        #endregion

        #region Events
        public delegate void PostFound(Post post, IEnumerable<CustomWordMatch> matches);
        public event PostFound OnPostsFound;

        public delegate void PageParsed();
        public event PageParsed OnPageParsed;

        public delegate void TotalPages(int nbPages);
        public event TotalPages OnTotalPages;

        public delegate void PageFromCache(int nbPages);
        public event PageFromCache OnPageFromCache;

        public delegate void DoneSearching();
        public event DoneSearching OnDoneSearching;
        #endregion

        private HttpClient _httpClient;
        private HtmlHelper _htmlHelper;
        private ForumThreadSearch SearchEngine;
        private readonly IPoETSDatabase _database;

        public API() {
            var config = ConfigManager.GetConfig();
            UrlTemplate = config.PoEWebSiteForumUrlTemplate;
            PostsTableRootName = config.PostsHtmlTableXPath;

            SearchEngine = new ForumThreadSearch();
            _httpClient = new HttpClient();
            _htmlHelper = new HtmlHelper();
            _database = new PoETSDatabase();
        }

        private IEnumerable<CustomWordMatch> TestSearch(List<string> keywords, Post post) {
            return SearchEngine.Search(keywords, new List<string>() {
                post.Content,
                post.Author.Name,
                post.Time.ToString(),
                post.Page.ToString()
            });
        }

        private async Task<HtmlDocument> GetRawHtmlDocumentAsync(string url) {
            var response = await _httpClient.Query(url);
            return _htmlHelper.ParseHtmlResponse(response);
        }

        private List<List<string>> GenerateUrlsBundles(int threadId, List<int> pagesNo, int bundleSize = 100) {
            List<List<string>> bundle = new List<List<string>>();
            List<string> urls = new List<string>();

            foreach (var page in pagesNo) {
                string url = ToUrl(threadId, page);

                urls.Add(url);

                if (urls.Count() >= 100) {
                    bundle.Add(urls);
                    urls.Clear();
                }
            }

            if (urls.Count > 0) {
                bundle.Add(urls);
            }

            return bundle;
        }



        private void ParsePages(Dictionary<int, HtmlDocument> documents) {
            List<Post> posts = new List<Post>();

            foreach (var document in documents) {
                var pagePosts = _htmlHelper.ParsePage(document.Value, document.Key);
                posts.AddRange(pagePosts);
            }

            SavePosts(posts);
        }

        private async void SavePosts(List<Post> posts) {
            foreach (var post in posts) {
                var existingPost = _database.Get<Post>(p => p.Page == post.Page && p.Time == post.Time);

                if (existingPost == null) {
                    await _database.Insert(post);
                } else {
                    await _database.Update(post);
                }
            }
        }

        private async void FetchPages(ForumThread forumThread) {
            var existingPages = await _database.Get<Page>(p => p.ThreadId == forumThread.Id);

            if (existingPages.Count() != forumThread.NbPage) {
                var pages = FindMissingPages(existingPages, forumThread.NbPage);
                var urlsBundles = GenerateUrlsBundles(forumThread.Id, pages);

                foreach (var urls in urlsBundles) {
                    var response = await _httpClient.Query(urls);
                    var documents = _htmlHelper.ParseHtmlResponses(response);

                    ParsePages(documents);
                }
            } else {
                var url = ToUrl(forumThread.Id, forumThread.NbPage);
                var response = await _httpClient.Query(url);
                var document = _htmlHelper.ParseHtmlResponse(response);
                var posts = _htmlHelper.ParsePage(document, forumThread.NbPage);

                SavePosts(posts);
            }
        }

        private List<int> FindMissingPages(List<Page> pages, int totalNbPages) {
            List<int> missingPagesNo = new List<int>();

            for (int i = 1; i <= totalNbPages; ++i) {
                if (pages.Find(p => p.No == i) == null) {
                    missingPagesNo.Add(i);
                }
            }

            return missingPagesNo;
        }

        public async void FetchForumThread(CancellationToken ct, List<string> keywords, int threadId) {
            ForumThread forumThread = await GetForumThread(threadId);
            FetchPages(forumThread);

        }


        private void SearchExistingPosts(List<string> keywords, ForumThread forumThread, List<Post> posts) {
            for (int i = 0; i < posts.Count; ++i) {
                if (posts[i].Page < forumThread.NbPage) {
                    var matches = SearchEngine.Search(keywords, new List<string>() { posts[i].Content });

                    if (matches.Any()) {
                        OnPostsFound(posts[i], matches);
                    }
                }
            }
        }

        private async Task<Dictionary<int, HtmlDocument>> GetRawHtmlDocumentAsync(List<string> urls) {
            var responses = await _httpClient.Query(urls);
            return _htmlHelper.ParseHtmlResponses(responses);
        }

        private async Task<ForumThread> GetForumThread(int threadId) {
            ForumThread forumThread = await _database.Get<ForumThread>(threadId);

            string url = ToUrl(threadId, 1);

            HtmlDocument document = await GetRawHtmlDocumentAsync(url);

            if (forumThread == null) {
                forumThread = _htmlHelper.ParseForumThread(document, threadId);
            }

            int nbPages = _htmlHelper.ParseNbPages(document);
            forumThread.NbPage = nbPages;

            return forumThread;
        }

        public string ToUrl(int threadId, int page, int id = -1) {
            var url = UrlTemplate
             .Replace("$threadId", threadId.ToString())
             .Replace("$page", page.ToString());
            return id != -1 ? url.Replace("$id", $"#p{id.ToString()}") : url.Replace("$id", "");
        }
    }
}
