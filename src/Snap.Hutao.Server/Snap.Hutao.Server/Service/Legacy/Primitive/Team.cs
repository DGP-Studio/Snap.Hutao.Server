// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Snap.Hutao.Server.Service.Legacy.Primitive;

public readonly struct Team : IEquatable<Team>
{
    public readonly AvatarId Position1;
    public readonly AvatarId Position2;
    public readonly AvatarId Position3;
    public readonly AvatarId Position4;
    public readonly int Count;

    public Team(List<int> list)
    {
        Span<int> source = CollectionsMarshal.AsSpan(list);
        source.CopyTo(MemoryMarshal.CreateSpan(ref Unsafe.As<Team, int>(ref this), 4));
        Count = source.Length;
    }

    public static implicit operator string(Team value)
    {
        StringBuilder teamBuider = new(35);
        teamBuider.Append(value.Position1);

        if (value.Position2 != 0)
        {
            teamBuider.Append(',').Append(value.Position2);
        }

        if (value.Position3 != 0)
        {
            teamBuider.Append(',').Append(value.Position3);
        }

        if (value.Position4 != 0)
        {
            teamBuider.Append(',').Append(value.Position4);
        }

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

    public bool Equals(Team other)
    {
        return Position1 == other.Position1
            && Position2 == other.Position2
            && Position3 == other.Position3
            && Position4 == other.Position4;
    }

    public override bool Equals(object? obj)
    {
        return obj is Team other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Position1, Position2, Position3, Position4);
    }
}