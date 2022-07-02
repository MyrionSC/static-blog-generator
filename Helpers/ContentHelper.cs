using System.Collections.Generic;
using System.Linq;

namespace static_blog_generator;

public static class ContentHelper
{
    public static string CreateFrontPageHtmlContent(List<ParsedFile> parsedFinanceFileList)
    {
        return $@"
<h1>Here we go</h1>
{parsedFinanceFileList.Select(f => $"<p><a href=\"{f.MetaData.UrlPath}\">{f.MetaData.Title}</a></p>").StringJoin("\n")}\n
aa";
    }
}