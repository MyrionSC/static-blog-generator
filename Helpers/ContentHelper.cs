using System.Collections.Generic;
using System.Linq;

namespace static_blog_generator;

public static class ContentHelper
{
    public static string CreateFrontPageHtmlContent(List<ParsedFile> parsedFinanceFileList)
    {
        var me = parsedFinanceFileList.Select(f => f.MetaData.UrlPath).StringJoin("\n");
        var financeListItems = parsedFinanceFileList.Select(f => 
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
                    <link rel=preload href="../static/JetBrainsMono-Regular.woff2" as=font type=font/woff2>
                    <link rel="stylesheet" href="../static/style.css">
                </head>
                <body class="max-width mx-auto px3 ltr">
                <div class="content index py4">
                    <section id="about">
                        Hugo is a general-purpose website framework. Technically speaking, Hugo is a static site generator. Unlike
                        systems that dynamically build a page with each visitor request, Hugo builds pages when you create or update
                        your content. Since websites are viewed far more often than they are edited, Hugo is designed to provide an
                        optimal viewing experience for your website’s end users and an ideal writing experience for website authors.
                    </section>
                    <section class="d-me-flex flex-gap-1rem">
                        <div style="flex: 1">
                            <span class="h1">Finance</span>
                            <ul class="post-list">
                                {{financeListItems}}
                            </ul>
                        </div>
                        <div style="flex: 1">
                            <span class="h1">Technical</span>
                            <ul class="post-list">
                                <li class="post-item">
                                    <div class="text-nowrap mr05rem">2021-06-27</div>
                                    <span><a href="https://www.takuzen.me/hugo-theme-cactus/posts/code-block-test/">Code Block Test</a></span>
                                </li>
                                <li class="post-item">
                                    <div class="text-nowrap mr05rem">2021-06-07</div>
                                    <span><a href="https://www.takuzen.me/hugo-theme-cactus/posts/2020-06-06-changes-in-java-string/">Changes to String in java (from 1.7.0_06)</a></span>
                                </li>
                            </ul>
                        </div>
                    </section>
                </div>
                </body>
                </html>
               """"";
    }
}