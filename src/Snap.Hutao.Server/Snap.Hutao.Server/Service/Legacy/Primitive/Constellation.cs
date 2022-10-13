// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Legacy.Primitive;

/// <summary>
/// 命座
/// </summary>
public readonly struct Constellation : IEquatable<Constellation>
{
    /// <summary>
    /// value
    /// </summary>
    public readonly int Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Constellation"/> struct.
    /// </summary>
    /// <param name="value">value</param>
    public Constellation(int value)
    {
        Value = value;
    }

    public static implicit operator int(Constellation value)
    {
        return value.Value;
    }

    public static implicit operator Constellation(int value)
    {
        return new(value);
    }

    public static bool operator ==(Constellation left, Constellation right)
    {
        return left.Value == right.Value;
    }

    public static bool operator !=(Constellation left, Constellation right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public bool Equals(Constellation other)
    {
        return Value == other.Value;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is Constellation other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}