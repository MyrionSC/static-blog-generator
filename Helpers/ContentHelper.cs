using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Docs.v1.Data;

namespace static_blog_generator;

public static class ContentHelper
{
    public static string CreateFrontPageHtmlContent(List<ParsedFile> parsedBusinessFileList,
        List<ParsedFile> parsedTechFileList)
    {
        var businessListItems = parsedBusinessFileList.Select(f =>
            $"""
            <li class="post-item">
                <div class="text-nowrap mr05rem">{f.MetaData.Date:yyyy-MM-dd} </div>
                <span><a href="{ f.MetaData.UrlPath} ">{ f.MetaData.Title} </a></span>
            </li>
            """
        ).StringJoin("\n");
        var techListItems = parsedTechFileList.Select(f =>
            $"""
            <li class="post-item">
                <div class="text-nowrap mr05rem">{f.MetaData.Date:yyyy-MM-dd} </div>
                <span><a href="{ f.MetaData.UrlPath} ">{ f.MetaData.Title} </a></span>
            </li>
            """
        ).StringJoin("\n");
        return $"""
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
                        Halløjsovs, velkommen til! I am Martin, This is my blog. I write about my experience running a 
                        one-man software consulting business in Denmark, as well as various technical stuff. 
                        I am a born and raised native dane, currently residing in Aalborg, Paris of the North, where we
                        are supposedly on average very happy. This blog is statically generated from google docs.
                        <a href="https://github.com/MyrionSC/static-blog-generator">Source is here.</a>
                    </section>
                    <section class="d-me-flex flex-gap-1rem">
                        <div style="flex: 1">
                            <span class="h1">Business</span>
                            <ul class="post-list">
                                { businessListItems} 
                            </ul>
                        </div>
                        <div style="flex: 1">
                            <span class="h1">Technical</span>
                            <ul class="post-list">
                                { techListItems} 
                            </ul>
                        </div>
                    </section>
                </div>
                </body>
                </html>
                """ ;
    }

    public static string CreateArticleHtmlContent(IEnumerable<StructuralElement> documentContentList,
        List<ImageMetadata> imageMetadataList)
    {
        var stringBuilder = new StringBuilder();
        using var documentIter = documentContentList.GetEnumerator();
        BuildHtmlFromDocumentEnumerator(stringBuilder, documentIter, imageMetadataList);

        return $$"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="UTF-8">
                    <title>Title</title>
                    <meta name="viewport" content="width=device-width,initial-scale=1">
                    <link rel=preload href="../../static/JetBrainsMono-Regular.woff2" as=font type=font/woff2>
                    <link rel="stylesheet" href="../../static/style.css">
                </head>
                <body class="max-width mx-auto px3 ltr">
                <div class="content index py4">
                  {{ stringBuilder}} 
                </div>
                </body>
                </html>
                """ ;
    }

    private static void BuildHtmlFromDocumentEnumerator(StringBuilder stringBuilder,
        IEnumerator<StructuralElement> documentIter,
        List<ImageMetadata> imageMetadataList)
    {
        if (!documentIter.MoveNext()) return;
        var ele = documentIter.Current!;

        // HEADER
        if (ele.Paragraph.ParagraphStyle.NamedStyleType == "HEADING_2") {
            stringBuilder.Append($"<h1>{ele.Paragraph.Elements.First().TextRun.Content}</h1>");
        }
        
        // LIST
        if (ele.Paragraph.Bullet is not null) {
            Console.WriteLine("Bullet!");
        }

        // IMAGE
        if (ele.Paragraph.Elements.Any(e => e.InlineObjectElement is not null)) {
            var imageEle = ele.Paragraph.Elements.First(e => e.InlineObjectElement is not null);
            var imageMetadata = imageMetadataList.First(i => i.Id == imageEle.InlineObjectElement.InlineObjectId);
            string imageName = imageEle.InlineObjectElement.InlineObjectId.Replace(".", "_");
            // TODO: mobile friendly
            stringBuilder.Append($"""
                    <div class="m-auto mt-2 mb-2">
                      <img style="height: {imageMetadata.HeighPx.ToString().Replace(",", ".")}px; width:{imageMetadata.WidthPx.ToString().Replace(",", ".")}px"
                       src="../../images/{imageName}.png"/>
                    </div>
                    """ );
        }

        // NORMAL
        else if (ele.Paragraph.ParagraphStyle.NamedStyleType == "NORMAL_TEXT") {
            if (IsLineBreak(ele.Paragraph.Elements)) {
                stringBuilder.Append("""<div class="separator"></div>""");
            }
            else {
                var parsedNormalText = ele.Paragraph.Elements
                    .Where(paragraphElement => paragraphElement.TextRun is not null)
                    .Select(p => ParseNormalText(p)).ToArray();
                stringBuilder.Append($"<p>{parsedNormalText.StringJoin("")}</p>");
            }
        }
        
        BuildHtmlFromDocumentEnumerator(stringBuilder, documentIter, imageMetadataList);
    }

    private static bool IsLineBreak(IList<ParagraphElement> paragraphElements)
    {
        var pureText = paragraphElements
            .Where(paragraphElement => paragraphElement.TextRun is not null)
            .Select(p => p.TextRun.Content)
            .StringJoin("");
        return String.IsNullOrWhiteSpace(pureText);
    }

    private static string ParseNormalText(ParagraphElement p)
    {
        if (p?.TextRun.TextStyle.Link is not null) {
            return $"""<a href="{ p.TextRun.TextStyle.Link.Url} ">{ p.TextRun.Content} </a>""" ;
        }
        return p!.TextRun.Content;
    }
}