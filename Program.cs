using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Docs.v1.Data;
using DriveFile = Google.Apis.Drive.v3.Data.File;

// ReSharper disable InconsistentNaming

namespace static_blog_generator;

internal static class Program
{
    // If modifying these scopes, delete your previously saved credentials
    // at ~/.credentials/docs.googleapis.com-dotnet-quickstart.json
    private const string BUSINESS_DIR_ID = "1hfnLBQfz0QRRNag-HdL0noOA0T6Xnl_9";
    private const string TECH_DIR_ID = "1F6IsmhCroPVLZh0rp6Gn_DsGO06GLhFz";

    private static async Task Main()
    {
        Console.WriteLine("Gettings files... ");
        GoogleCredential credential = DriveHelper.GetGoogleCreds();
        var fileList = await DriveHelper.GetFileList(credential);

        Console.WriteLine("Saving images...");
        var imageMetadataList = await SaveArticleImages(fileList, credential);

        Console.WriteLine("Generating html...");
        var businessFileList = DriveHelper.GetDirFilesByParentId(fileList, BUSINESS_DIR_ID);
        var parsedBusinessFileList =
            businessFileList.Select(f => DriveHelper.ParseFile(f, credential, imageMetadataList)).ToList();
        parsedBusinessFileList.Sort((a, b) => a.MetaData.Date < b.MetaData.Date ? -1 : 1);

        var techFileList = DriveHelper.GetDirFilesByParentId(fileList, TECH_DIR_ID);
        var parsedTechFileList = techFileList.Select(f => DriveHelper.ParseFile(f, credential, imageMetadataList)).ToList();
        parsedTechFileList.Sort((a, b) => a.MetaData.Date < b.MetaData.Date ? -1 : 1);

        string frontpageHtmlContent =
            ContentHelper.CreateFrontPageHtmlContent(parsedBusinessFileList, parsedTechFileList);

        Console.WriteLine("Writing to files...");
        await File.WriteAllTextAsync("index.html", frontpageHtmlContent);
        
        // TODO: next / prev buttons
        
        foreach (ParsedFile parsedFile in parsedBusinessFileList) {
            if (parsedFile.MetaData.State == ArticleState.Draft) continue;
            Directory.CreateDirectory($"{parsedFile.MetaData.UrlPath}");
            await File.WriteAllTextAsync($"{parsedFile.MetaData.UrlPath}/index.html",
                parsedFile.HtmlContent);
        }

        foreach (ParsedFile parsedFile in parsedTechFileList) {
            if (parsedFile.MetaData.State == ArticleState.Draft) continue;
            Directory.CreateDirectory($"{parsedFile.MetaData.UrlPath}");
            await File.WriteAllTextAsync($"{parsedFile.MetaData.UrlPath}/index.html",
                parsedFile.HtmlContent);
        }
        Console.WriteLine("Done");
    }

    private static async Task<List<ImageMetadata>> SaveArticleImages(IList<DriveFile> fileList, GoogleCredential credential)
    {
        Directory.CreateDirectory("images");

        List<ImageMetadata> imageMetadataList = new List<ImageMetadata>();

        var businessFileList = DriveHelper.GetDirFilesByParentId(fileList, BUSINESS_DIR_ID);
        var techFileList = DriveHelper.GetDirFilesByParentId(fileList, TECH_DIR_ID);
        foreach (var file in techFileList.Concat(businessFileList)) {
            var doc = DriveHelper.GetDoc(credential, file.Id);

            // save article images for later reuse
            if (doc.InlineObjects is null) continue;
            foreach (KeyValuePair<string, InlineObject> o in doc.InlineObjects) {
                var imageContentUri =
                    o.Value.InlineObjectProperties.EmbeddedObject.ImageProperties.ContentUri;
                var imageBytes = await new HttpClient().GetByteArrayAsync(imageContentUri);
                await File.WriteAllBytesAsync($"images/{o.Key.Replace(".", "_")}.png", imageBytes);
                imageMetadataList.Add(new ImageMetadata {
                    Id = o.Key,
                    WidthPx = o.Value.InlineObjectProperties.EmbeddedObject.Size.Width.Magnitude * 2,
                    HeighPx = o.Value.InlineObjectProperties.EmbeddedObject.Size.Height.Magnitude * 2
                });
            }
        }

        return imageMetadataList;
    }
}