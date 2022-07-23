using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace static_blog_generator
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/docs.googleapis.com-dotnet-quickstart.json
        private const string FINANCE_DIR_ID = "1hfnLBQfz0QRRNag-HdL0noOA0T6Xnl_9";
        private const string TECH_DIR_ID = "1F6IsmhCroPVLZh0rp6Gn_DsGO06GLhFz";

        static async Task Main(string[] args)
        {
            GoogleCredential credential = DriveHelper.GetGoogleCreds();
            var fileList = await DriveHelper.GetFileList(credential);
            
            var financeFileList = DriveHelper.GetDirFilesByParentId(fileList, FINANCE_DIR_ID);
            var parsedFinanceFileList = financeFileList.Select(f => DriveHelper.ParseFile(f, credential)).ToList();
            parsedFinanceFileList.Sort((a, b) => a.MetaData.Date < b.MetaData.Date ? 1 : -1 );
            
            var techFileList = DriveHelper.GetDirFilesByParentId(fileList, TECH_DIR_ID);
            var parsedTechFileList = techFileList.Select(f => DriveHelper.ParseFile(f, credential)).ToList();
            parsedTechFileList.Sort((a, b) => a.MetaData.Date < b.MetaData.Date ? 1 : -1 );

            string frontpageHtmlContent = ContentHelper.CreateFrontPageHtmlContent(parsedFinanceFileList, parsedTechFileList);

            Directory.CreateDirectory("publish");
            await File.WriteAllTextAsync("publish/index.html", frontpageHtmlContent);
            foreach (ParsedFile parsedFile in parsedFinanceFileList) {
                Directory.CreateDirectory($"publish/{parsedFile.MetaData.UrlPath}");
                await File.WriteAllTextAsync($"publish/{parsedFile.MetaData.UrlPath}/index.html",
                    parsedFile.HtmlContent);
            }
            foreach (ParsedFile parsedFile in parsedTechFileList) {
                Directory.CreateDirectory($"publish/{parsedFile.MetaData.UrlPath}");
                await File.WriteAllTextAsync($"publish/{parsedFile.MetaData.UrlPath}/index.html",
                    parsedFile.HtmlContent);
            }
        }
    }
}