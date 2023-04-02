
using Spectre.Console;
using Spectre.Console.Next;

var selected = await FileExplorer.Show("C:/dev", 10);

AnsiConsole.WriteLine(string.Join(", ", selected));
