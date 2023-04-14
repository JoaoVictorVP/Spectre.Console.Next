using Nipah.Markdown.Contracts;
using Nipah.Markdown.Models;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectre.Console.Next.Markdown;

public class MarkdownView : JustInTimeRenderable
{
    private readonly MarkdownDocument doc;

    MarkdownView(MarkdownDocument doc)
    {
        this.doc = doc;
    }

    public static MarkdownView? From(string markdown, IMarkdownParser parser)
    {
        var result = parser.Parse(markdown);
        return result.IsSuccess is false
            ? null
            : new(result.Expect(""));
    }

    public Style? SubTitleStyle { get; set; } = Style.Plain.Foreground(Color.NavajoWhite1);
    public Style? TextStyle { get; set; }
    public Style? ListItemDotStyle { get; set; }
    public Style? SeparatorStyle { get; set; }

    IRenderable Build(MarkdownElement element)
    {
        return element switch
        {
            MarkdownTitle x => BuildTitle(x),
            MarkdownText x => BuildText(x),
            MarkdownList x => BuildList(x),
            MarkdownCitation x => BuildCitation(x),
            MarkdownSeparator x => BuildSeparator(x),
            MarkdownListItem x => BuildListItem(x),
            _ => throw new Exception($"Unsupported Markdown Element: {element.GetType().FullName}")
        };
    }

    private IRenderable BuildTitle(MarkdownTitle x)
        => x.Level switch
        {
            1 => new FigletText(x.Title),
            _ => new Text(x.Title, SubTitleStyle)
        };

    private IRenderable BuildText(MarkdownText x)
    {
        return new Text(x.Text, TextStyle);
    }

    private IRenderable BuildCitation(MarkdownCitation x)
    {
        return new Panel(Build(x.Citation));
    }

    private IRenderable BuildList(MarkdownList x)
    {
        var tree = new Tree("");
        tree.AddNodes(x.Elements.Select(item => Build(item)));
        return tree;
    }

    private IRenderable BuildListItem(MarkdownListItem x)
    {
        return Build(x.Item);
    }

    private IRenderable BuildSeparator(MarkdownSeparator x)
    {
        return new Rule();
    }

    protected override IRenderable Build()
    {
        return new Rows(
            doc.Elements.Select(Build)
        );
    }
}
