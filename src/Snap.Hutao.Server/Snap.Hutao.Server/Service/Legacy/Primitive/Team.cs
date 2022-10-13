// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Legacy.Primitive;

/// <summary>
/// 队伍
/// </summary>
public readonly struct Team : IEquatable<Team>
{
    /// <summary>
    /// 值
    /// </summary>
    public readonly string Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Team"/> struct.
    /// </summary>
    /// <param name="value">value</param>
    public Team(string value)
    {
        Value = value;
    }

    public static implicit operator string(Team value)
    {
        return value.Value;
    }

    public static implicit operator Team(string value)
    {
        return new(value);
    }

    public static bool operator ==(Team left, Team right)
    {
        return left.Value == right.Value;
    }

    public static bool operator !=(Team left, Team right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public bool Equals(Team other)
    {
        return Value == other.Value;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is Team other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}