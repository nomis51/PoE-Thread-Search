using HtmlAgilityPack;
using PoETS.Data;
using PoETS.Data.Configuration;
using PoETS.Data.Models;
using PoETS.SearchEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HttpClient = PoETS.API.HttpClient;

namespace PoETS.API {
    public class PoEAPI {
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
        //private ForumThreadSearch SearchEngine;
        private readonly IPoETSDatabase _database;

        public PoEAPI() {
            var config = ConfigManager.GetConfig();
            UrlTemplate = config.PoEWebSiteForumUrlTemplate;
            PostsTableRootName = config.PostsHtmlTableXPath;

            //SearchEngine = new ForumThreadSearch();
            _httpClient = new HttpClient();
            _htmlHelper = new HtmlHelper();
            _database = new PoETSDatabase();
        }

        private IEnumerable<CustomWordMatch> TestSearch(List<string> keywords, Post post) {
            //return SearchEngine.Search(keywords, new List<string>() {
            //    post.Content,
            //    post.Author.Name,
            //    post.Time.ToString(),
            //    post.Page.ToString()
            //});

            return new List<CustomWordMatch>();
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

                if (urls.Count() >= bundleSize) {
                    bundle.Add(urls);
                    urls = new List<string>();
                }
            }

            if (urls.Count > 0) {
                bundle.Add(urls);
            }

            return bundle;
        }



        private async void ParsePages(Dictionary<int, HtmlDocument> documents, int threadId) {
            List<Post> posts = new List<Post>();

            foreach (var document in documents) {
                var pagePosts = _htmlHelper.ParsePage(document.Value, document.Key, threadId);

                var existingPage = (await _database.Get<Page>(p => p.No == document.Key && p.ThreadId == threadId)).FirstOrDefault();

                if(existingPage == null) {
                    Page page = new Page() {
                        No = document.Key,
                        ThreadId = threadId
                    };

                    await _database.Insert(page);
                } 

                posts.AddRange(pagePosts);
            }

            SavePosts(posts);
        }

        private async void SavePosts(List<Post> posts) {
            foreach (var post in posts) {
                var existingPost = await _database.Get<Post>(p => p.PostId == post.PostId);

                if (existingPost == null) {
                    await _database.Insert(post);
                } else {
                    await _database.Update(post);
                }
            }
        }

        private async void FetchPages(ForumThread forumThread) {
            var existingPages = await _database.Get<Page>(p => p.ThreadId == forumThread.ThreadId);

            if (existingPages.Count() != forumThread.NbPage) {
                var pages = FindMissingPages(existingPages, forumThread.NbPage);
                var urlsBundles = GenerateUrlsBundles(forumThread.ThreadId, pages);

                foreach (var urls in urlsBundles) {
                    _ = Task.Run(async () => {
                        var response = await _httpClient.Query(urls);
                        var documents = _htmlHelper.ParseHtmlResponses(response);

                        ParsePages(documents, forumThread.ThreadId);
                    });
                }
            } else {
                var url = ToUrl(forumThread.ThreadId, forumThread.NbPage);
                var response = await _httpClient.Query(url);
                var document = _htmlHelper.ParseHtmlResponse(response);
                var posts = _htmlHelper.ParsePage(document, forumThread.NbPage, forumThread.ThreadId);

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

            var existingForumThread = (await _database.Get<ForumThread>(f => f.ThreadId == forumThread.ThreadId)).FirstOrDefault();

            if (existingForumThread == null) {
                await _database.Insert(forumThread);
            } else {
                existingForumThread.Name = forumThread.Name;
                existingForumThread.NbPage = forumThread.NbPage;

                await _database.Update(existingForumThread);
            }

            FetchPages(forumThread);
        }


        private void SearchExistingPosts(List<string> keywords, ForumThread forumThread, List<Post> posts) {
            for (int i = 0; i < posts.Count; ++i) {
                if (posts[i].PageNo < forumThread.NbPage) {
                    //var matches = SearchEngine.Search(keywords, new List<string>() { posts[i].Content });

                    //if (matches.Any()) {
                    //    OnPostsFound(posts[i], matches);
                    //}
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
