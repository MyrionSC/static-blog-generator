﻿namespace static_blog_generator.Models;

public class ParsedFile
{
    public ArticleMetaData MetaData { get; set; }
    public string HtmlContent { get; set; }
}