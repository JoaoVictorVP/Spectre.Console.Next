using Spectre.Console.Next.Base;
using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectre.Console.Next;

public class DynamicPrompt : IDynamicUI
{
    string? previousText = null;
    string text = "";
    readonly string placeholder = "";

    public string Text
    {
        get => text;
        set => text = value;
    }

    public DynamicPrompt(string text, string placeholder)
    {
        this.text = text;
        this.placeholder = placeholder;
    }
    public DynamicPrompt()
    {
        
    }

    bool isRunning;
    public async Task Manage(Action<string> onSend)
    {
        isRunning = true;
        string Send(string what)
        {
            onSend(what);
            return what;
        }
        string Make(char from)
            => from switch
            {
                '\\' => "\\",
                _ => from.ToString()
            };

        while (isRunning)
        {
            if (SysConsole.KeyAvailable)
            {
                var press = SysConsole.ReadKey(true);

                text = (press, text) switch
                {
                    ({ Key: ConsoleKey.Backspace }, { Length: > 0 }) => text = text[..^1],
                    ({ Key: ConsoleKey.Enter }, _) => Send(text),
                    ({ Key: ConsoleKey.Escape }, _) => Send(""),
                    _ => char.IsLetterOrDigit(press.KeyChar) || char.IsPunctuation(press.KeyChar)
                        ? text + Make(press.KeyChar)
                        : text
                };
            }
            await Task.Delay(10);
        }
    }

    public void Stop() => isRunning = false;

    public bool IsDirty => previousText != text;

    public IRenderable Build()
    {
        previousText = text;

        return new Markup(text is "" ? $"[grey]{placeholder}[/]" : text);
    }
}
