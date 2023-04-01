using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectre.Console.Next.Base;

public interface IDynamicUI
{
    bool IsDirty { get; }
    IRenderable Build();
}
