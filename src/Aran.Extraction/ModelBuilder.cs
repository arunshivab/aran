using System;
using System.Collections.Generic;
using Aran.Model;

namespace Aran.Extraction;

/// <summary>Accumulates extracted elements and produces the final geometry model.</summary>
public sealed class ModelBuilder
{
    private readonly List<Room> _rooms = new List<Room>();
    private readonly List<Barrier> _barriers = new List<Barrier>();
    private readonly List<RadiationSource> _sources = new List<RadiationSource>();
    private int _counter;

    /// <summary>The scale calibration to embed in the built model.</summary>
    public ScaleCalibration? Scale { get; set; }

    /// <summary>Generates the next identifier with the supplied prefix.</summary>
    /// <param name="prefix">A short prefix such as "B" for barrier.</param>
    /// <returns>A model-unique identifier.</returns>
    public string NextId(string prefix)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        _counter++;
        return prefix + _counter.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>Adds a room to the model.</summary>
    /// <param name="room">The room to add.</param>
    public void AddRoom(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        _rooms.Add(room);
    }

    /// <summary>Adds a barrier to the model.</summary>
    /// <param name="barrier">The barrier to add.</param>
    public void AddBarrier(Barrier barrier)
    {
        ArgumentNullException.ThrowIfNull(barrier);
        _barriers.Add(barrier);
    }

    /// <summary>Adds a radiation source to the model.</summary>
    /// <param name="source">The source to add.</param>
    public void AddSource(RadiationSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        _sources.Add(source);
    }

    /// <summary>Builds the immutable geometry model from the accumulated elements.</summary>
    /// <returns>The assembled <see cref="ShieldingGeometryModel"/>.</returns>
    public ShieldingGeometryModel Build()
    {
        return new ShieldingGeometryModel(Scale, _rooms.ToArray(), _barriers.ToArray(), _sources.ToArray());
    }
}
