﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Legacy.Primitive;

/// <summary>
/// 武器Id
/// </summary>
public readonly struct WeaponId : IEquatable<WeaponId>
{
    /// <summary>
    /// 值
    /// </summary>
    public readonly int Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaponId"/> struct.
    /// </summary>
    /// <param name="value">value</param>
    public WeaponId(int value)
    {
        Value = value;
    }

    public static implicit operator int(WeaponId value)
    {
        return value.Value;
    }

    public static implicit operator WeaponId(int value)
    {
        return new(value);
    }

    public static bool operator ==(WeaponId left, WeaponId right)
    {
        return left.Value == right.Value;
    }

    public static bool operator !=(WeaponId left, WeaponId right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public bool Equals(WeaponId other)
    {
        return Value == other.Value;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is WeaponId other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}