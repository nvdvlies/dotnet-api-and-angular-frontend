﻿using System.Diagnostics;

namespace Demo.Domain.Shared.DomainEntity;

internal class PerformanceMeasurement
{
    private readonly string _name;

    private readonly Stopwatch _stopwatch;

    public PerformanceMeasurement(string name)
    {
        _name = name;
        _stopwatch = new Stopwatch();
    }

    public void Start()
    {
        _stopwatch.Start();
    }

    public void Stop()
    {
        _stopwatch.Stop();
    }

    public override string ToString()
    {
        return $"{_name}: {_stopwatch.Elapsed.TotalMilliseconds}ms";
    }
}
