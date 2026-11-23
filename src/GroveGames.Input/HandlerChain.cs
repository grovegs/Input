using System;

namespace GroveGames.Input;

internal sealed class HandlerChain<T> where T : class
{
    private HandlerChainEntry<T>[] _entries;

    public HandlerChainEntry<T>[] Entries => _entries;
    public int Count => _entries.Length;

    public HandlerChain()
    {
        _entries = Array.Empty<HandlerChainEntry<T>>();
    }

    public void Add(T handler, int priority)
    {
        var entries = _entries;

        for (var i = 0; i < entries.Length; i++)
        {
            if (ReferenceEquals(entries[i].Handler, handler))
            {
                return;
            }
        }

        var index = entries.Length;

        for (var i = 0; i < entries.Length; i++)
        {
            if (entries[i].Priority < priority)
            {
                index = i;
                break;
            }
        }

        var updated = new HandlerChainEntry<T>[entries.Length + 1];
        Array.Copy(entries, 0, updated, 0, index);
        updated[index] = new HandlerChainEntry<T>(handler, priority);
        Array.Copy(entries, index, updated, index + 1, entries.Length - index);
        _entries = updated;
    }

    public void Remove(T handler)
    {
        var entries = _entries;

        for (var i = 0; i < entries.Length; i++)
        {
            if (!ReferenceEquals(entries[i].Handler, handler))
            {
                continue;
            }

            var updated = new HandlerChainEntry<T>[entries.Length - 1];
            Array.Copy(entries, 0, updated, 0, i);
            Array.Copy(entries, i + 1, updated, i, entries.Length - i - 1);
            _entries = updated;
            return;
        }
    }
}
