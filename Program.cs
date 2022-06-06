using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Docs.v1;
using Google.Apis.Docs.v1.Data;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Newtonsoft.Json;
using File = Google.Apis.Drive.v3.Data.File;

namespace static_blog_generator
{
    class ListDict
    {
        public Dictionary<string, List<string>> d = new();

        public void Add(string key, string str)
        {
            if (d.ContainsKey(key)) {
                d[key].Add(str);
            }
            else {
                d[key] = new List<string> { str };
            }
        }

        public List<string> Get(string key)
        {
            return d[key];
        }
    }

    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/docs.googleapis.com-dotnet-quickstart.json
        static string[] Scopes = { DocsService.Scope.DocumentsReadonly, DriveService.Scope.Drive };
        static string ApplicationName = "static-blog-generator";

        static async Task Main(string[] args)
        {
            GoogleCredential credential = GetGoogleCreds();
            var fileList = await GetFileList(credential);

            string articlesDirId = "1VK3BqjbfhOlbyTPoLtw_ZzgXfotz-qV0";
            var publishedArticlesList = fileList
                .Where(f => f.Parents is not null && f.Parents.Contains(articlesDirId) &&
                            f.Name.Contains("Company Structure"))
                .Take(1)
                .ToList();

            // foreach published article,
            //   extract meta
            //   download images
            //   generate html

            foreach (File articleMeta in publishedArticlesList) {
                var doc = GetDoc(credential, articleMeta.Id);
                var contentList = doc.Body.Content;

                // extract metadata
                var metaDataSeparator = contentList
                    .TakeWhile(ele => !GetTextInElement(ele).Contains("==="))
                    .ToList();
                var metaDataString = GetTextInElementList(metaDataSeparator);
                var metaData = JsonConvert.DeserializeObject<ArticleMetaData>(metaDataString);


                Console.WriteLine(metaDataString);
            }
        }

        private static string GetTextInElement(StructuralElement ele)
        {
            if (ele.Paragraph is null) return "";
            var strWriter = new StringBuilder();
            foreach (var paraEle in ele.Paragraph.Elements) {
                if (paraEle.TextRun is not null) {
                    strWriter.Append(paraEle.TextRun.Content);
                }
                else {
                    Console.WriteLine("element.TextRun is null");
                    Console.WriteLine(JsonConvert.SerializeObject(paraEle));
                }
            }
            return strWriter.Replace("“", "\"").Replace("”", "\"").ToString();
        }

        private static string GetTextInElementList(IEnumerable<StructuralElement> eleList)
        {
            var strWriter = new StringBuilder();
            foreach (var content in eleList) {
                if (content.Paragraph is null) continue;
                foreach (var element in content.Paragraph.Elements) {
                    try {
                        if (element.TextRun is not null) {
                            strWriter.Append(element.TextRun.Content);
                        }
                        else {
                            Console.WriteLine("element.TextRun is null");
                            Console.WriteLine(JsonConvert.SerializeObject(element));
                        }
                    }
                    catch (Exception e) {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }

            return strWriter.Replace("“", "\"").Replace("”", "\"").ToString();
        }

        private static GoogleCredential GetGoogleCreds()
        {
            using var stream = new FileStream("creds/service-account-creds.json", FileMode.Open, FileAccess.Read);
            return GoogleCredential.FromStream(stream).CreateScoped(Scopes);
        }

        private static Document GetDoc(GoogleCredential credential, string docsId)
        {
            var service = new DocsService(new BaseClientService.Initializer {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });
            return service.Documents.Get(docsId).Execute();
        }

        private static async Task<IList<File>> GetFileList(GoogleCredential credential)
        {
            // Create Google Docs API service.
            var driveService = new DriveService(new BaseClientService.Initializer {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            var listRequest = driveService.Files.List();
            listRequest.PageSize = 100;
            listRequest.Fields = "nextPageToken, files(*)";
            return (await listRequest.ExecuteAsync()).Files;
        }
    }

    internal record ArticleMetaData
    {
        public string Title { get; set; }
        public ArticleState State { get; set; }
        public ArticleCategory Category { get; set; }
        public DateTime Date { get; set; }
    }

    internal enum ArticleState
    {
        Draft, Published
    }
    
    internal enum ArticleCategory
    {
        Finance, Tech
    }

}