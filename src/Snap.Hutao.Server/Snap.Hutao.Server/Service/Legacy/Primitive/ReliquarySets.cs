// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Legacy.Primitive;

/// <summary>
/// 圣遗物套装
/// </summary>
public readonly struct ReliquarySets : IEquatable<ReliquarySets>
{
    /// <summary>
    /// 值
    /// </summary>
    public readonly string Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReliquarySets"/> struct.
    /// </summary>
    /// <param name="value">value</param>
    public ReliquarySets(string value)
    {
        Value = value;
    }

    public static implicit operator string(ReliquarySets value)
    {
        return value.Value;
    }

    public static implicit operator ReliquarySets(string value)
    {
        return new(value);
    }

    public static bool operator ==(ReliquarySets left, ReliquarySets right)
    {
        return left.Value == right.Value;
    }

    public static bool operator !=(ReliquarySets left, ReliquarySets right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public bool Equals(ReliquarySets other)
    {
        return Value == other.Value;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is ReliquarySets other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}