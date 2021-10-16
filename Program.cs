using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Discovery.v1;
using Google.Apis.Discovery.v1.Data;
using Google.Apis.Docs.v1;
using Google.Apis.Docs.v1.Data;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

namespace static_blog_generator
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/docs.googleapis.com-dotnet-quickstart.json
        static string[] Scopes = { DocsService.Scope.DocumentsReadonly, DriveService.Scope.Drive };
        static string ApplicationName = "static-blog-generator";

        static async Task Main(string[] args)
        {
            GoogleCredential credential;
            using (var stream =
                new FileStream("creds/service-account-creds.json", FileMode.Open, FileAccess.Read)) {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }

            Docs(credential);
        }

        private static void Docs(GoogleCredential credential)
        {
            var service = new DocsService(new BaseClientService.Initializer {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            // Define request parameters.
            String documentId = "1jFz2btSYzy6OmUXSm8qQihwkdTev_02habrZKqsnLZ8";
            DocumentsResource.GetRequest request = service.Documents.Get(documentId);
            //
            // // Prints the title of the requested doc:
            // // https://docs.google.com/document/d/195j9eDD3ccgjQRttHhJPymLJUCOUjs-jmwTrekvdjFE/edit
            Document doc = request.Execute();
            Console.WriteLine("The title of the doc is: {0}", doc.Title);
            Console.WriteLine("la");
        }

        private static async Task Drive(GoogleCredential credential)
        {
            // Create Google Docs API service.
            var driveService = new DriveService(new BaseClientService.Initializer {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });

            var listRequest = driveService.Files.List();
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";

            // List files.
            IList<Google.Apis.Drive.v3.Data.File> files = (await listRequest.ExecuteAsync()).Files;

            if (files != null && files.Count > 0) {
                foreach (var file in files) {
                    Console.WriteLine("{0} ({1})", file.Name, file.Id);
                }
            }
            else {
                Console.WriteLine("No files found.");
            }
        }

        private static async Task Discover()
        {
            var discoverService = new DiscoveryService(new BaseClientService.Initializer {
                ApplicationName = "static-blog-generator",
                ApiKey = "AIzaSyA54gitmtL-MfW5NwOv-9kNr6AkH0HVHGs",
            });

            // Run the request.
            Console.WriteLine("Executing a list request...");
            var result = await discoverService.Apis.List().ExecuteAsync();

            // Display the results.
            if (result.Items != null) {
                foreach (DirectoryList.ItemsData api in result.Items) {
                    Console.WriteLine(api.Id + " - " + api.Title);
                }
            }
        }
    }
}