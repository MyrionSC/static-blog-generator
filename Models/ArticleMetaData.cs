using System;

namespace static_blog_generator;

public record ArticleMetaData
{
    public string Title { get; set; }
    public ArticleState State { get; set; }
    public ArticleCategory Category { get; set; }
    public DateTime Date { get; set; }
    public string UrlPath { get; set; }
}