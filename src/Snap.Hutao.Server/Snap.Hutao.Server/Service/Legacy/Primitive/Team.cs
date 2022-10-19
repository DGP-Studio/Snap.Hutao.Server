// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Text;

namespace Snap.Hutao.Server.Service.Legacy.Primitive;

/// <summary>
/// 队伍
/// </summary>
public readonly struct Team : IEquatable<Team>
{
    /// <summary>
    /// 位置1
    /// </summary>
    public readonly AvatarId Position1;

    /// <summary>
    /// 位置2
    /// </summary>
    public readonly AvatarId Position2;

    /// <summary>
    /// 位置3
    /// </summary>
    public readonly AvatarId Position3;

    /// <summary>
    /// 位置4
    /// </summary>
    public readonly AvatarId Position4;

    /// <summary>
    /// Initializes a new instance of the <see cref="Team"/> struct.
    /// </summary>
    /// <param name="list">value</param>
    public Team(List<int> list)
    {
        Position1 = list[0];
        Position2 = list[1];
        Position3 = list[2];
        Position4 = list[3];
    }

    public static implicit operator string(Team value)
    {
        StringBuilder teamBuider = new(35);
        teamBuider
            .Append(value.Position1)
            .Append(',')
            .Append(value.Position2)
            .Append(',')
            .Append(value.Position3)
            .Append(',')
            .Append(value.Position4);
        return teamBuider.ToString();
    }

    public static bool operator ==(Team left, Team right)
    {
        return left.Position1 == right.Position1
            && left.Position2 == right.Position2
            && left.Position3 == right.Position3
            && left.Position4 == right.Position4;
    }

    public static bool operator !=(Team left, Team right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public bool Equals(Team other)
    {
        return Position1 == other.Position1
            && Position2 == other.Position2
            && Position3 == other.Position3
            && Position4 == other.Position4;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is Team other && Equals(other);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Position1, Position2, Position3, Position4);
    }
}