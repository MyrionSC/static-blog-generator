using System.Collections.Generic;
using System.Linq;

namespace static_blog_generator;

public static class ContentHelper
{
    public static string CreateFrontPageHtmlContent(List<ParsedFile> parsedBusinessFileList, List<ParsedFile> parsedTechFileList)
    {
        var businessListItems = parsedBusinessFileList.Select(f => 
            $$"""
            <li class="post-item">
                <div class="text-nowrap mr05rem">{{f.MetaData.Date:yyyy-MM-dd}}</div>
                <span><a href="{{f.MetaData.UrlPath}}">{{f.MetaData.Title}}</a></span>
            </li>
            """
        ).StringJoin("\n");
        var techListItems = parsedTechFileList.Select(f => 
            $$"""
            <li class="post-item">
                <div class="text-nowrap mr05rem">{{f.MetaData.Date:yyyy-MM-dd}}</div>
                <span><a href="{{f.MetaData.UrlPath}}">{{f.MetaData.Title}}</a></span>
            </li>
            """
        ).StringJoin("\n");
        return $$"""""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="UTF-8">
                    <title>Title</title>
                    <meta name="viewport" content="width=device-width,initial-scale=1">
                    <link rel=preload href="static/JetBrainsMono-Regular.woff2" as=font type=font/woff2>
                    <link rel="stylesheet" href="static/style.css">
                </head>
                <body class="max-width mx-auto px3 ltr">
                <div class="content index py4">
                    <section id="about">
                        Velkommen til. This is my blog. There are many like it, but this one is mine. I write about my 
                        experience running a one-man software consulting business in Denmark, as a born and raised 
                        native dane, as well as whatever technical subject that catches my fancy.
                    </section>
                    <section class="d-me-flex flex-gap-1rem">
                        <div style="flex: 1">
                            <span class="h1">Business</span>
                            <ul class="post-list">
                                {{businessListItems}}
                            </ul>
                        </div>
                        <div style="flex: 1">
                            <span class="h1">Technical</span>
                            <ul class="post-list">
                                {{techListItems}}
                            </ul>
                        </div>
                    </section>
                </div>
                </body>
                </html>
               """"";
    }
}