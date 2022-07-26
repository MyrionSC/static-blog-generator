using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using DriveFile = Google.Apis.Drive.v3.Data.File;
// ReSharper disable InconsistentNaming

namespace static_blog_generator
{
    internal static class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/docs.googleapis.com-dotnet-quickstart.json
        private const string BUSINESS_DIR_ID = "1hfnLBQfz0QRRNag-HdL0noOA0T6Xnl_9";
        private const string TECH_DIR_ID = "1F6IsmhCroPVLZh0rp6Gn_DsGO06GLhFz";

        private static async Task Main()
        {
            GoogleCredential credential = DriveHelper.GetGoogleCreds();
            var fileList = await DriveHelper.GetFileList(credential);
            
            var businessFileList = DriveHelper.GetDirFilesByParentId(fileList, BUSINESS_DIR_ID);
            var parsedBusinessFileList = businessFileList.Select(f => DriveHelper.ParseFile(f, credential)).ToList();
            parsedBusinessFileList.Sort((a, b) => a.MetaData.Date < b.MetaData.Date ? 1 : -1 );
            
            var techFileList = DriveHelper.GetDirFilesByParentId(fileList, TECH_DIR_ID);
            var parsedTechFileList = techFileList.Select(f => DriveHelper.ParseFile(f, credential)).ToList();
            parsedTechFileList.Sort((a, b) => a.MetaData.Date < b.MetaData.Date ? 1 : -1 );

            string frontpageHtmlContent = ContentHelper.CreateFrontPageHtmlContent(parsedBusinessFileList, parsedTechFileList);

            await File.WriteAllTextAsync("index.html", frontpageHtmlContent);
            foreach (ParsedFile parsedFile in parsedBusinessFileList) {
                Directory.CreateDirectory($"{parsedFile.MetaData.UrlPath}");
                await File.WriteAllTextAsync($"{parsedFile.MetaData.UrlPath}/index.html",
                    parsedFile.HtmlContent);
            }
            foreach (ParsedFile parsedFile in parsedTechFileList) {
                Directory.CreateDirectory($"{parsedFile.MetaData.UrlPath}");
                await File.WriteAllTextAsync($"{parsedFile.MetaData.UrlPath}/index.html",
                    parsedFile.HtmlContent);
            }
        }
    }
}