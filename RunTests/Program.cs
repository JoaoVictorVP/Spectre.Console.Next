
using Nipah.Markdown;
using Spectre.Console;
using Spectre.Console.Next;
using Spectre.Console.Next.Markdown;

const string md = """
    # This is a title
    > And this is a citation
    * We can type
    * lists
    * as well

    ## And this is a subtitle

    And this is just plain text
    ---
    With a separator in between.
    """;

var mdParser = new MarkdownParser();

var mdView = MarkdownView.From(md, mdParser);

if (mdView is null)
    return;

AnsiConsole.Write(mdView);

await AnsiConsole.Console.Input.ReadKeyAsync(true, default);
