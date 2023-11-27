// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Core;

public static class RandomHelper
{
    public static string GetUpperAndNumberString(int count)
    {
        return new(Random.Shared.GetItems<char>("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890", count));
    }
}