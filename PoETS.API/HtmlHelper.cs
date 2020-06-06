using HtmlAgilityPack;
using Newtonsoft.Json;
using PoETS.Data.Configuration;
using PoETS.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoETS.API {
    public class HtmlHelper {

        public HtmlHelper() {

        }

        public int ParsePostId(HtmlNode node) {
            var postAnchor = node.SelectSingleNode(".//div[@class='post_anchor']");
            return Convert.ToInt32(postAnchor.Attributes["id"].Value.Substring(1));
        }

        public List<Post> ParsePage(HtmlDocument doc, int page, int threadId) {
            var postsElement = doc.DocumentNode.SelectSingleNode(ConfigManager.GetConfig().PostsHtmlTableXPath);

            List<Post> posts = new List<Post>();

            foreach (var p in postsElement.ChildNodes) {
                if (p.InnerText.Trim().Replace("\\n", "").Length > 0) {
                    Player author = ParseAuthor(p);
                    int id = ParsePostId(p);
                    Post post = ParsePost(p, page, threadId, author);
                    post.PostId = id;

                    posts.Add(post);

                    #region TODO: Quoted block
                    //var matches = TestSearch(keywords, post);

                    //if (matches.Any()) {
                    //    OnPostsFound(post, matches);
                    //}

                    //var blockQuoteElement = p.SelectSingleNode("//blockquote");

                    //if (blockQuoteElement != null) {
                    //    var blockQuoteAuthor = p.SelectSingleNode("//blockquote/div[@class='top']/cite/span/a").InnerText;
                    //    var blockQuoteContent = p.SelectSingleNode("//blockquote/div[@class='bot']").InnerText;
                    //    var g = 0;

                    //    contentText = "***\n" + blockQuoteAuthor + " wrote: " + blockQuoteContent + "***\n\n" + contentText;
                    //}

                    //Database.SavePost(post);

                    //var matches = SearchEngine.Search(keywords, post.Content);

                    //if (matches.Any()) {
                    //    OnPostsFound(post, matches);
                    //}
                    #endregion
                }
            }

            return posts;
        }

        public int ParseNbPages(HtmlDocument doc) {
            var paginationElm = doc.DocumentNode.SelectSingleNode(ConfigManager.GetConfig().ForumThreadPaginationHtmlXPath);

            int lastPage = 0;
            foreach (var a in paginationElm.ChildNodes) {
                int p = 0;
                try {
                    p = Convert.ToInt32(a.InnerText);
                } catch (Exception) { }

                if (p > lastPage) {
                    lastPage = p;
                }
            }

            return lastPage;
        }

        public Player ParseAuthor(HtmlNode node) {
            var postInfoElement = node.SelectSingleNode(ConfigManager.GetConfig().PostAuthorHtmlXPath);
            var postInfoText = postInfoElement.InnerText;
            int startIndexAuthor = postInfoText.IndexOf("Posted by") + 9;
            int endIndexAuthor = postInfoText.LastIndexOf("on ") - startIndexAuthor;
            var name = postInfoText.Substring(startIndexAuthor, endIndexAuthor);

            var avatarElement = node.SelectSingleNode(ConfigManager.GetConfig().PostAvatarHtmlXPath);
            var avatarSrc = avatarElement.Attributes["src"].Value;

            var author = new Player() {
                Name = name,
                AvatarUri = avatarSrc
            };

            return author;
        }

        public ForumThread ParseForumThread(HtmlDocument doc, int threadId) {
            var titleNode = doc.DocumentNode.SelectSingleNode(ConfigManager.GetConfig().ForumThreadTitleHtmlXPath);
            var title = titleNode != null ? titleNode.InnerText : "";
            var tableElement = doc.DocumentNode.SelectSingleNode(ConfigManager.GetConfig().PostsHtmlTableXPath);
            Player author = ParseAuthor(tableElement.ChildNodes[1]);

            return new ForumThread() {
                ThreadId = threadId,
                Name = title,
                Author = author
            };
        }

        public Post ParsePost(HtmlNode node, int page, int threadId, Player author) {
            var contentElement = node.SelectSingleNode(ConfigManager.GetConfig().PostContentHtmlXPath);
            var contentText = contentElement.InnerText;

            var postInfoElement = node.SelectSingleNode(ConfigManager.GetConfig().PostTimeHtmlXPath);
            var postInfoText = postInfoElement.InnerText;
            int startIndexDateTime = postInfoText.LastIndexOf("on ") + 3;
            int testAM = postInfoText.LastIndexOf("AM") + 2;
            int testPM = postInfoText.LastIndexOf("PM") + 2;
            int endIndexDateTime = testAM <= testPM ? testPM : testAM;
            string date = postInfoText.Substring(startIndexDateTime, endIndexDateTime - startIndexDateTime);
            DateTime time = DateTime.Parse(date);

            var post = new Post() {
                Content = contentText,
                PageNo = page,
                Author = author,
                Time = time,
                ThreadId = threadId
            };

            return post;
        }

        public HtmlDocument ParseHtmlResponse(string serverResponse) {
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(serverResponse);

            if (response.Count == 0) {
                return null;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(response.ElementAt(0).Value);

            return doc;
        }

        public Dictionary<int, HtmlDocument> ParseHtmlResponses(Dictionary<string, string> response) {
            if (response.Count == 0) {
                return new Dictionary<int, HtmlDocument>();
            }

            var docs = new Dictionary<int, HtmlDocument>();

            Mutex _lock = new Mutex();

            foreach (var r in response) {
                var doc = new HtmlDocument();
                doc.LoadHtml(r.Value);
                int page = Convert.ToInt32(r.Key.Substring(r.Key.LastIndexOf('/') + 1));

                _lock.WaitOne();
                docs.Add(page, doc);
                _lock.ReleaseMutex();
            }

            return docs;
        }
    }
}
