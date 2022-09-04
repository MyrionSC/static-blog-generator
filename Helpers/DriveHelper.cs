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
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace static_blog_generator;

public static class DriveHelper
{
    private const string APPLICATION_NAME = "static-blog-generator";
    private static readonly string[] SCOPES = { DocsService.Scope.DocumentsReadonly, DriveService.Scope.Drive };

    public static async Task<IList<DriveFile>> GetFileList(GoogleCredential credential)
    {
        // Create Google Docs API service.
        var driveService = new DriveService(new BaseClientService.Initializer {
            HttpClientInitializer = credential,
            ApplicationName = APPLICATION_NAME
        });

        var listRequest = driveService.Files.List();
        listRequest.PageSize = 100;
        listRequest.Fields = "nextPageToken, files(*)";
        return (await listRequest.ExecuteAsync()).Files;
    }

    public static GoogleCredential GetGoogleCreds()
    {
        using var stream = new FileStream("creds/service-account-creds.json", FileMode.Open, FileAccess.Read);
        return GoogleCredential.FromStream(stream).CreateScoped(SCOPES);
    }

    public static List<DriveFile> GetDirFilesByParentId(IList<DriveFile> fileList, string parentDirId)
    {
        return fileList
            .Where(f => f.Parents is not null && f.Parents.Contains(parentDirId) &&
                        f.MimeType.Contains("application/vnd.google-apps.document"))
            .ToList();
    }

    public static ParsedFile ParseFile(DriveFile file, GoogleCredential credential,
        List<ImageMetadata> imageMetadataList)
    {
        var doc = GetDoc(credential, file.Id);
        var contentList = doc.Body.Content;

        // extract metadata
        var metaDataSeparator = contentList
            .TakeWhile(ele => !GetTextInElement(ele).Contains("==="))
            .ToList();
        var metaDataString = GetTextInElementList(metaDataSeparator);
        var metaData = JsonConvert.DeserializeObject<ArticleMetaData>(metaDataString)!;
        metaData.UrlPath = $"articles/{metaData.Title.Replace(" ", "-").ToLower()}";

        // find and parse article content
        var actualDocumentList = contentList
            .SkipWhile(ele => !GetTextInElement(ele).Contains("==="))
            .Skip(1); // remove "==="

        return new ParsedFile {
            MetaData = metaData,
            HtmlContent = ContentHelper.CreateArticleHtmlContent(actualDocumentList, imageMetadataList)
        };
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

    public static Document GetDoc(GoogleCredential credential, string docsId)
    {
        var service = new DocsService(new BaseClientService.Initializer {
            HttpClientInitializer = credential,
            ApplicationName = APPLICATION_NAME
        });
        return service.Documents.Get(docsId).Execute();
    }
}