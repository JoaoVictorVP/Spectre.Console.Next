
using Spectre.Console;
using Spectre.Console.Next;

var input = new InputSource(AnsiConsole.Console.Input);

input.Run();

var selected = await FileExplorer.Show(input, "C:/dev", 10);

AnsiConsole.WriteLine(string.Join(", ", selected));
