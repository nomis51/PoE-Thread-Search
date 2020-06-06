using Newtonsoft.Json;
using PoETS.API;
using PoETS.Application.Controls;
using PoETS.Data.Models;
using PoETS.SearchEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

namespace PoETS.Application.Views {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        private PoEAPI api;
        private int nbPageParsed = 0;
        private int nbTotalPages = 0;
        private int nbPostsFound = 0;
        private DateTime startTime;
        private CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

        public MainWindow() {
            InitializeComponent();

            api = new PoEAPI();

            //api.OnPostsFound += Api_OnPostsFound;
            api.OnPageParsed += Api_OnPageParsed;
            api.OnTotalPages += Api_OnTotalPages;
            api.OnPageFromCache += Api_OnPageFromCache;
            api.OnDoneSearching += Api_OnDoneSearching;
            this.KeyUp += MainWindow_KeyUp;
        }

        private void EnableDisableSearchButton(bool state) {
            this.Dispatcher.Invoke(() => {
                btnSearch.IsEnabled = state;
            });
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                GetPosts();
            }
        }

        private void Api_OnDoneSearching() {
            EnableDisableSearchButton(true);
            double seconds = Math.Round((DateTime.Now - startTime).TotalSeconds, 2);

            this.Dispatcher.Invoke(() => {
                pgrLoading.IsActive = false;
                lblSearchTime.Content = $"{seconds} second{(seconds > 0 ? "s" : "")}";
            });
        }

        private void Api_OnPageFromCache(int nbPages) {
            this.Dispatcher.Invoke(() => {
                lblNbPageFromCache.Content = $"({nbPages})";
            });
        }

        private void SetContent(string content, IEnumerable<CustomWordMatch> matches, TextBlock control) {
            int currentIndex = 0;

            for (int i = 0; i < matches.Count(); ++i) {
                var m = matches.ElementAt(i);
                Run normalRun = new Run(content.Substring(currentIndex, m.Index - currentIndex));
                control.Inlines.Add(normalRun);

                Run highlightedrun = new Run(content.Substring(m.Index, m.Word.Length));
                bool hasSynonym = m.SynonymOf != null;
                highlightedrun.Background = hasSynonym ? Brushes.LimeGreen : Brushes.Yellow;
                highlightedrun.Foreground = Brushes.Black;

                if (m.SynonymOf != null) {
                    highlightedrun.ToolTip = $"Synonym of \"{m.SynonymOf}\"";
                }

                control.Inlines.Add(highlightedrun);

                currentIndex = m.Index + m.Word.Length;
            }

            Run run = new Run(content.Substring(currentIndex, content.Length - currentIndex));
            control.Inlines.Add(run);
        }

        private void Api_OnPostsFound(Post post, IEnumerable<CustomWordMatch> matches) {
            this.Dispatcher.Invoke(() => {
                var pControl = new PostControl();
                SetContent(post.Content, matches, pControl.txtContent);
                pControl.lblAuthor.Content = $"{post.Author.Name}";
                pControl.lblTime.Content = $"on {post.Time.ToString()}";
                pControl.lblPage.Content = $"Page {post.PageNo.ToString()}";
                pControl.lblThreadID.Content = $"Thread {post.ThreadId.ToString()}";
                pControl.PostLink = new Uri(api.ToUrl(post.ThreadId, post.PageNo, post.PostId));

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(post.Author.AvatarUri, UriKind.Absolute);
                bitmap.EndInit();

                pControl.imgProfilePicture.Stretch = Stretch.Fill;
                pControl.imgProfilePicture.Source = bitmap;

                pControl.PMLink = new Uri($"https://www.pathofexile.com/private-messages/compose/to/{post.Author.Name}@pc");

                spPosts.Children.Add(pControl);
                ++nbPostsFound;

                lblNbPost.Content = $"{nbPostsFound} post{(nbPostsFound > 1 ? "s" : "")} found";
            });
        }

        private void Api_OnTotalPages(int nbPages) {
            this.Dispatcher.Invoke(() => {
                nbTotalPages = nbPages;
                lblNbPage.Content = $"{nbPageParsed} of {nbTotalPages} page{(nbTotalPages > 0 ? "s" : "")} verified";
            });
        }

        private void Api_OnPageParsed() {
            this.Dispatcher.Invoke(() => {
                ++nbPageParsed;
                lblNbPage.Content = $"{nbPageParsed} of {nbTotalPages} page{(nbTotalPages > 0 ? "s" : "")} verified";
            });
        }

        private List<string> ParseQuery(string query) {
            return (query.IndexOf('"') != -1 ? query.Split('"')
                .Select(k => {
                    if (k.Length > 0 && (k[0] == ' ' || k[k.Length - 1] == ' ')) {
                        return k.Split(' ')
                        .Where(e => e.Length > 0);
                    }

                    return new string[] { k };
                })
                .SelectMany(e => e)
                .Distinct()
                :
                query.Split(' ')
                ).ToList()
                .FindAll(e => e.Length > 0);
        }

        private void GetPosts() {
            pgrLoading.IsActive = true;
            EnableDisableSearchButton(false);

            var keywords = ParseQuery(txtSearchQuery.Text);
            int threadId = 0;


            try {
                threadId = Convert.ToInt32(txtThreadID.Text);
            } catch (FormatException) {
                string url = txtThreadID.Text.Substring(txtThreadID.Text.IndexOf("view-thread") + 12);
                int indexSlash = url.IndexOf("/");
                threadId = Convert.ToInt32(url.Substring(0, indexSlash != -1 ? url.IndexOf("/") : url.Length));
            }
            startTime = DateTime.Now;

            spPosts.Children.Clear();
            nbPageParsed = 0;
            //lblPagesParsed.Content = "0 of 0 page verified";
            nbPostsFound = 0;
            //lblNbPostFound.Content = "0 post found";
            nbTotalPages = 0;
            //lblPageCached.Content = "0 page from cache";
            //lblSearchTime.Content = "";

            CancellationToken ct = cancelTokenSource.Token;

            Task.Run(() => {
                api.FetchForumThread(ct, keywords, threadId);
            });

            //Task.Run(() => {
            //    this.Dispatcher.Invoke(() => lblLoadingMessages.Visibility = Visibility.Visible);

            //    var random = new Random();
            //    var messages = ConfigManager.GetConfig().LoadingMessages;
            //    bool done = false;


            //    while (!done) {
            //        this.Dispatcher.Invoke(() => {
            //            done = !pgrLoading.IsActive;

            //            int index = random.Next(messages.Count);

            //            lblLoadingMessages.Content = $"{messages.ElementAt(index)}...";
            //        });

            //        Thread.Sleep(5000);
            //    }
            //});
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) {
            GetPosts();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e) {
            cancelTokenSource.Cancel();
            EnableDisableSearchButton(true);
        }
    }
}
