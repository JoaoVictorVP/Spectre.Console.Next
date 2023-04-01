using Spectre.Console.Next.Base;
using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectre.Console.Next;

public class TextBox : IDynamicUI
{
    private readonly string title;
    private readonly DynamicPrompt prompt;
    private bool isDirty;

    void MarkAsDirty() => isDirty = true;

    public string Text
    {
        get => prompt.Text;
        set => prompt.Text = value;
    }

    public TextBox(string title = "", string text = "", string placeholder = "")
    {
        this.title = title;
        prompt = new DynamicPrompt(text, placeholder);
    }

    bool isShowing;
    public void StartManager(Action<string> onSend)
        => Task.Run(async () =>
        {
            isShowing = true;
            MarkAsDirty();
            await prompt.Manage(onSend);
            isShowing = false;
            MarkAsDirty();
        });
    
    public void StopManager() => prompt.Stop();

    public bool IsDirty => prompt.IsDirty
        || isDirty;

    public IRenderable Build()
    {
        isDirty = false;

        var panel = new Panel(prompt.Build())
            .BorderColor(isShowing ? Color.Yellow : Color.White)
            .Expand();
        
        if(title is not "")
            panel = panel.Header(title);

        return panel;
    }
}
