﻿using System.Collections.Generic;
using Demo.Domain.Shared.Interfaces;

namespace Demo.Domain.Shared.DomainEntity;

internal class DomainEntityState : IDomainEntityState
{
    private readonly Dictionary<string, object> _dictionary;

    public DomainEntityState()
    {
        _dictionary = new Dictionary<string, object>();
    }

    public T Get<T>(string key)
    {
        return (T)_dictionary[key];
    }

    public void Set<T>(string key, T value)
    {
        _dictionary[key] = value;
    }

    public bool TryGet<T>(string key, out T value)
    {
        if (_dictionary.TryGetValue(key, out var internalValue) && internalValue is T result)
        {
            value = result;
            return true;
        }

        value = default;
        return false;
    }
}
