using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Apis.Docs.v1.Data;
using static_blog_generator.Extensions;
using static_blog_generator.Models;

namespace static_blog_generator.Helpers;

public static class ContentHelper
{
    public static string CreateFrontPageHtmlContent(List<ParsedFile> parsedBusinessFileList,
        List<ParsedFile> parsedTechFileList)
    {
        var businessListItems = parsedBusinessFileList
            .Where(f => f.MetaData.State == ArticleState.Published)
            .Select(f =>
            $"""
            <li class="post-item">
                <div class="text-nowrap mr05rem">{f.MetaData.Date:yyyy-MM-dd}</div>
                <span><a href="{f.MetaData.UrlPath}">{f.MetaData.Title} </a></span>
            </li>
            """
        ).StringJoin("\n");
        var techListItems = parsedTechFileList
            .Where(f => f.MetaData.State == ArticleState.Published)
            .Select(f =>
            $"""
            <li class="post-item">
                <div class="text-nowrap mr05rem">{f.MetaData.Date:yyyy-MM-dd}</div>
                <span><a href="{f.MetaData.UrlPath}">{f.MetaData.Title}</a></span>
            </li>
            """
        ).StringJoin("\n");
        return $"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="UTF-8">
                    <title>Marand's blog</title>
                    <meta name="viewport" content="width=device-width,initial-scale=1">
                    <link rel=preload href="static/JetBrainsMono-Regular.woff2" as=font type=font/woff2>
                    <link rel="stylesheet" href="static/style.css">
                </head>
                <body class="max-width mx-auto px3 ltr">
                <div class="content index py-4 pt-xs-2">
                    <section id="about">
                        Halløjsovs, velkommen til! I am Martin, This is my blog. I write about my experience running a 
                        one-man software consulting business in Denmark, as well as various technical stuff. 
                        I am a born and raised native dane, currently residing in Aalborg, the Paris of the north!
                        This blog is statically generated from google docs.
                        <a href="https://github.com/MyrionSC/static-blog-generator">Source is here.</a>
                    </section>
                    <section class="d-md-flex flex-gap-1rem">
                        <div style="flex: 1">
                            <span class="h1">Business</span>
                            <ul class="post-list">
                                {businessListItems}
                            </ul>
                        </div>
                        <div style="flex: 1">
                            <span class="h1">Technical</span>
                            <ul class="post-list">
                                {techListItems}
                            </ul>
                        </div>
                    </section>
                </div>
                </body>
                </html>
                """ ;
    }

    public static string CreateArticleHtmlContent(ArticleMetaData articleMetaData,
        IEnumerable<StructuralElement> documentContentList,
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
                    <title>{{articleMetaData.Title}}</title>
                    <meta name="viewport" content="width=device-width,initial-scale=1">
                    <link rel=preload href="../../static/JetBrainsMono-Regular.woff2" as=font type=font/woff2>
                    <link rel="stylesheet" href="../../static/style.css">
                </head>
                <body class="max-width mx-auto px3 ltr">
                <div class="content index py-4 pt-xs-2">
                  {{stringBuilder}}
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
        begin:
        var ele = documentIter.Current!;

        // HEADER
        if (ele.Paragraph.ParagraphStyle.NamedStyleType == "HEADING_2") {
            stringBuilder.Append($"<h1>{ele.Paragraph.Elements.First().TextRun.Content}</h1>");
        }

        // LIST
        else if (ele.Paragraph.Bullet is not null) {
            var listId = ele.Paragraph.Bullet.ListId;
            stringBuilder.Append("<ul>");
            do {
                stringBuilder.Append($"<li>{ParseNormalText(ele)}</li>");
                documentIter.MoveNext();
                ele = documentIter.Current!;
                if (ele is null) { // if list is the last thing in the document
                    stringBuilder.Append("</ul>");
                    return;
                }
                if (ele.Paragraph.Bullet is null || listId != ele.Paragraph.Bullet.ListId) {
                    stringBuilder.Append("</ul>");
                    goto begin; // back to spaghetti basics!
                }
            } while (true);
        }

        // IMAGE
        else if (ele.Paragraph.Elements.Any(e => e.InlineObjectElement is not null)) {
            var imageEle = ele.Paragraph.Elements.First(e => e.InlineObjectElement is not null);
            var imageMetadata = imageMetadataList.First(i => i.Id == imageEle.InlineObjectElement.InlineObjectId);
            string imageName = imageEle.InlineObjectElement.InlineObjectId.Replace(".", "_");
            stringBuilder.Append($"""
                    <div class="m-auto mt-2 mb-2">
                      <img class="w-xs-100 h-xs-initial" style="height: {imageMetadata.HeighPx.ToString().Replace(",", ".")}px; width:{imageMetadata.WidthPx.ToString().Replace(",", ".")}px"
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
                stringBuilder.Append($"<p>{ParseNormalText(ele)}</p>");
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

    private static string ParseNormalText(StructuralElement ele)
    {
        return ele.Paragraph.Elements
            .Where(paragraphElement => paragraphElement.TextRun is not null)
            .Select(ParseNormalTextElement).StringJoin("");
    }

    private static string ParseNormalTextElement(ParagraphElement p)
    {
        if (p?.TextRun.TextStyle.Link is not null) {
            return $"""<a href="{p.TextRun.TextStyle.Link.Url }">{p.TextRun.Content}</a>""" ;
        }
        if (p?.TextRun.TextStyle.Bold == true) {
            return $"""<b>{p.TextRun.Content}</b>""" ;
        }
        return p!.TextRun.Content;
    }
}
