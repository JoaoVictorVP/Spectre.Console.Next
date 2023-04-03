using Spectre.Console.Next.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectre.Console.Next;

public class InputSource : IInputSource
{
    private readonly ConcurrentQueue<ConsoleKeyInfo> pressedKeys = new();
    private readonly IAnsiConsoleInput input;

    public InputSource(IAnsiConsoleInput input)
    {
        this.input = input;
    }

    public CallbackListener<ConsoleKeyInfo, bool> OnKeyPressed { get; } = new();

    public bool IsKeyAvailable()
        => pressedKeys.IsEmpty is false;

    public ConsoleKeyInfo ReadKey(bool consume)
    {
        ConsoleKeyInfo keyInfo;
        while (consume
            ? pressedKeys.TryDequeue(out keyInfo) is false
            : pressedKeys.TryPeek(out keyInfo) is false)
        {
            Thread.Sleep(10);
        }
        return keyInfo;
    }

    public async Task<ConsoleKeyInfo> ReadKeyAsync(bool consume)
    {
        ConsoleKeyInfo keyInfo;
        while (consume
            ? pressedKeys.TryDequeue(out keyInfo) is false
            : pressedKeys.TryPeek(out keyInfo) is false)
        {
            await Task.Delay(10);
        }
        return keyInfo;
    }

    bool isRunning;

    public void Run()
    {
        isRunning = true;
        _ = Task.Run(async () =>
        {
            while (isRunning)
            {
                if (input.IsKeyAvailable())
                {
                    var keyInfo = input.ReadKey(true);

                    if (keyInfo is null)
                        continue;

                    bool shouldSkip = false;
                    while (OnKeyPressed.TryInvokeOne(keyInfo.Value, out bool consume))
                    {
                        if(consume)
                        {
                            shouldSkip = true;
                            break;
                        }
                    }
                    if (shouldSkip)
                        continue;

                    pressedKeys.Enqueue(keyInfo.Value);
                }

                await Task.Delay(10);
            }
        });
    }
    public void Finish()
    {
        isRunning = false;
    }
}
