using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectre.Console.Next.Contracts;

public interface IInputSource
{
    bool IsKeyAvailable();
    Task<ConsoleKeyInfo> ReadKeyAsync(bool consume);
    ConsoleKeyInfo ReadKey(bool consume);
    CallbackListener<ConsoleKeyInfo, bool> OnKeyPressed { get; }
}
