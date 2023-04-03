using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectre.Console.Next;

public class CallbackListener<TInput, TOutput>
{
    private readonly List<(bool oneShot, Func<TInput, TOutput> callback)> callbacks = new(32);

    public void Add(Func<TInput, TOutput> callback, bool oneShot = true)
    {
        lock (callbacks)
        {
            callbacks.Add((oneShot, callback));
        }
    }

    public void Remove(Func<TInput, TOutput> callback)
    {
        lock (callbacks)
        {
            callbacks.RemoveAll(x => x.callback == callback);
        }
    }

    public bool TryInvokeOne(TInput input, out TOutput? output)
    {
        lock (callbacks)
        {
            if (callbacks.Count is not > 0)
            {
                output = default;
                return false;
            }
            var first = callbacks[0];
            output = first.callback(input);
            if (first.oneShot)
                callbacks.RemoveAt(0);
            return true;
        }
    }

    public IEnumerable<TOutput> InvokeAll(TInput input)
    {
        lock (callbacks)
        {
            var results = new List<TOutput>(callbacks.Count);
            foreach (var (_, callback) in callbacks)
                results.Add(callback(input));
            callbacks.RemoveAll(c => c.oneShot);
            return results;
        }
    }
}
