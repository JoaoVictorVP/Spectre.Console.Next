using Spectre.Console.Next.Base;
using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spectre.Console.Next;

public class FileExplorer : IDynamicUI
{
    private readonly string baseDirectory;
    private readonly int showRange;
    private readonly FileSystemLayer fs;
    private string currentDirectory = "";
    private IEnumerable<string> items = Enumerable.Empty<string>();
    private string selected = "";
    private readonly TextBox search = new TextBox(placeholder: "Press F to search");
    bool isDirty = true;

    public static async Task<IEnumerable<string>> Show(string baseDirectory, int showRange = 10, FileSystemLayer? fsLayer = null)
        => await new FileExplorer(baseDirectory, showRange, fsLayer).Show();

    public FileExplorer(string baseDirectory, int showRange = 10, FileSystemLayer? fsLayer = null)
    {
        this.baseDirectory = baseDirectory;
        this.showRange = showRange / 2;
        fs = fsLayer ?? FileSystemLayer.OS;
        
        Enter(baseDirectory);
    }

    string FormatPath(string path)
    {
        return Path.GetFileName(path);
        /*bool isCurDir = path == currentDirectory;

        path = path.Replace(baseDirectory + '/', "");

        if (isCurDir is false)
        {
            var with = currentDirectory.Replace(baseDirectory, "");
            if(with is not "")
                path = Path.GetRelativePath(path, with);
        }

        return path;*/
    }

    public Task<IEnumerable<string>> Show() => Show(AnsiConsole.Console);
    
    void MarkAsDirty() => isDirty = true;

    IEnumerable<string> BuildItems(string directory)
    {
        return fs.EnumerateDirectories((directory, "", false))
            .Concat(fs.EnumerateFiles((directory, "", false)))
            .Select(p => p.Contains('\\') ? p.Replace('\\', '/') : p);
    }

    void Enter(string directory)
    {
        if (fs.DirectoryExists(directory) is false
            || directory == currentDirectory
            || directory.Contains(baseDirectory) is false)
            return;

        currentDirectory = directory;

        items = BuildItems(directory);

        selected = items.FirstOrDefault() ?? "";

        MarkAsDirty();
    }
    void Exit(string directory)
    {
        if (directory.Length is 0 || directory.Contains('/') is false)
            return;
        string baseDir = directory[..(directory.LastIndexOf('/'))];
        Enter(baseDir);
    }
    void MoveUp()
    {
        var old = selected;

        if (selected is "")
        {
            selected = items.FirstOrDefault() ?? "";
            if (selected != old)
                MarkAsDirty();
            return;
        }

        foreach(var item in items.Reverse())
        {
            if (item == selected)
                selected = null!;
            else if (selected is null)
            {
                selected = item;
                break;
            }
        }
        selected ??= old;

        if (selected != old)
            MarkAsDirty();
    }
    void MoveDown()
    {
        var old = selected;

        if (selected is "")
        {
            selected = items.FirstOrDefault() ?? "";
            if (selected != old)
                MarkAsDirty();
            return;
        }

        foreach (var item in items)
        {
            if (item == selected)
                selected = null!;
            else if (selected is null)
            {
                selected = item;
                break;
            }
        }
        selected ??= old;

        if (old != selected)
            MarkAsDirty();
    }

    bool isKilled;
    public void Kill()
    {
        isKilled = true;
    }

    public async Task<IEnumerable<string>> Show(IAnsiConsole console)
    {
        isKilled = false;
        
        var results = new List<string>(32);

        bool searching = false;

        await console.Live(Build())
            .AutoClear(true)
            .StartAsync(async ctx =>
            {
                isDirty = true;
                while (true && isKilled is false)
                {
                    if (searching)
                    {
                        if(search.IsDirty)
                            items = BuildItems(currentDirectory)
                                .Where(p =>
                                {
                                    if (search.Text is { Length: > 1 } and ['/', ..] and [.., '/'])
                                        return Regex.IsMatch(p, search.Text[1..^1]);
                                    else
                                        return p.ToLower().Contains(search.Text.ToLower());
                                });
                    }

                    if (IsDirty)
                        ctx.UpdateTarget(Build());

                    if (searching)
                    {
                        await Task.Delay(10);
                        continue;
                    }

                    switch (await Update(console.Input))
                    {
                        case FileExplorerAction.MoveUp:
                        {
                            MoveUp();
                            break;
                        }
                        case FileExplorerAction.MoveDown:
                        {
                            MoveDown();
                            break;
                        }
                        case FileExplorerAction.EnterDirectory:
                        {
                            Enter(selected);
                            break;
                        }
                        case FileExplorerAction.ExitDirectory:
                        {
                            Exit(currentDirectory);
                            break;
                        }
                        case FileExplorerAction.Select:
                        {
                            results.Add(selected);
                            return;
                        }

                        case FileExplorerAction.Find:
                        {
                            searching = true;
                            search.Text = "";
                            search.StartManager(sent =>
                            {
                                searching = false;
                                search.StopManager();

                                if (items.Count() is 1)
                                    selected = items.First();
                                else if (items.Contains(selected) is false)
                                    selected = "";
                            });
                            break;
                        }
                    }

                    await Task.Delay(10);
                }
            });

        return results;
    }

    bool simpleGraphics = SysConsole.OutputEncoding.Equals(Encoding.UTF8) is false;
    public async Task<FileExplorerAction> Update(IAnsiConsoleInput input, bool blocking = false)
    {
        FileExplorerAction SwitchSimpleGraphics()
        {
            simpleGraphics = !simpleGraphics;
            MarkAsDirty();
            return FileExplorerAction.None;
        }

        if (blocking is false && input.IsKeyAvailable() is false)
            return FileExplorerAction.None;

        var press = blocking
            ? await input.ReadKeyAsync(true, CancellationToken.None)
            : input.ReadKey(true);
        
        return press switch
        {
            { Key: ConsoleKey.UpArrow or ConsoleKey.W } => FileExplorerAction.MoveUp,
            { Key: ConsoleKey.DownArrow or ConsoleKey.S } => FileExplorerAction.MoveDown,
            { Key: ConsoleKey.Spacebar or ConsoleKey.D } => FileExplorerAction.EnterDirectory,
            { Key: ConsoleKey.Backspace or ConsoleKey.A } => FileExplorerAction.ExitDirectory,
            { Key: ConsoleKey.Enter } => FileExplorerAction.Select,
            { Key: ConsoleKey.F } => FileExplorerAction.Find,
            { Key: ConsoleKey.OemMinus } => SwitchSimpleGraphics(),
            _ => FileExplorerAction.None
        };
    }

    public bool IsDirty
        => isDirty
        || search.IsDirty;

    int GetIndexOf(string selected)
    {
        int index = 0;
        foreach (var item in items)
        {
            if (item == selected)
                return index;
            index++;
        }
        return 0;
    }

    public IRenderable Build()
    {
        IRenderable BuildItems()
        {
            string FormatToShow(string path, string item)
            {
                if (simpleGraphics)
                {
                    if (fs.DirectoryExists(path))
                        return $"D - {item}";
                    else if (fs.FileExists(path))
                        return $"F - {item}";
                    else
                        return item;
                }

                if (fs.DirectoryExists(path))
                    return $":file_folder: {item}";
                var ext = Path.GetExtension(path.AsSpan());

                if (ext is { IsEmpty: true })
                    return item;

                return ext[1..] switch
                {
                    "png" or "jpg" or "jpeg" or "bmp" or "gif" => $":framed_picture: {item}",
                    "mp3" or "wav" or "obb" => $":musical_notes: {item}",
                    "mov" or "mp4" or "mpeg" or "avg" or "mkv" => $":videocassette: {item}",

                    _ => $":memo: {item}"
                };
            }

            var entries = new List<IRenderable>(32);

            int index = GetIndexOf(selected);

            var count = items.Count();

            int startAt = Math.Max(index - showRange, 0);
            int endAt = Math.Min(index + showRange, count - 1);
            int showItemsCount = endAt - startAt + 1;

            if (showItemsCount < showRange * 2)
            {
                int diff = showRange * 2 - showItemsCount;

                if (startAt == 0)
                    endAt = Math.Min(endAt + diff, count - 1);
                else if (endAt == count - 1)
                    startAt = Math.Max(startAt - diff, 0);
                else
                {
                    int leftDiff = (diff + 1) / 2;
                    int rightDiff = diff - leftDiff;

                    startAt = Math.Max(startAt - leftDiff, 0);
                    endAt = Math.Min(endAt + rightDiff, count - 1);
                }
            }

            var showItems = items.Skip(startAt).Take(endAt - startAt + 1);

            foreach (var item in showItems)
            {
                var format = FormatPath(item);

                var display = FormatToShow(item, format);

                var entry = Markup.FromInterpolated($"{(item == selected ? ">" : " ")} {display}");

                entries.Add(entry);
            }

            if (showItems.Count() is 0)
                entries.Add(new Markup("<N/A>"));
            else
            {
                // We have any before?
                if (items.FirstOrDefault() != showItems.FirstOrDefault())
                    entries.Insert(0, new Markup("..."));

                // We have any after?
                if (items.LastOrDefault() != showItems.LastOrDefault())
                    entries.Add(new Markup("..."));
            }

            return new Rows(entries);
        }

        var searcher = search.Build();

        var entries = BuildItems();

        var body = new Rows(searcher, entries);

        var fsPanel = new Panel(body)
            .Header(FormatPath(currentDirectory))
            .Expand();

        isDirty = false;

        return fsPanel;
    }
}
public enum FileExplorerAction
{
    None,
    MoveUp,
    MoveDown,
    EnterDirectory,
    ExitDirectory,
    Select,
    Find,
    Finish
}
public class FileSystemLayer
{
    public static readonly FileSystemLayer OS
        = new FileSystemLayer
        {
            EnumerateFiles = x => Directory.EnumerateFiles(x.path, x.searchPattern, x.recursive
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly),
            EnumerateDirectories = x => Directory.EnumerateDirectories(x.path, x.searchPattern, x.recursive
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly),
            FileExists = File.Exists,
            DirectoryExists = Directory.Exists,
            GetFullPath = Path.GetFullPath
        };

    public required Func<(string path, string searchPattern, bool recursive), IEnumerable<string>> EnumerateFiles { get; init; }
    public required Func<(string path, string searchPattern, bool recursive), IEnumerable<string>> EnumerateDirectories { get; init; }
    public required Func<string, bool> FileExists;
    public required Func<string, bool> DirectoryExists;
    public required Func<string, string> GetFullPath { get; init; }
}