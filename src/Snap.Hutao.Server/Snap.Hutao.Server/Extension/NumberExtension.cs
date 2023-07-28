// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Extension;

public static class NumberExtension
{
    public static int StringLength(this in int number)
    {
        return (int)MathF.Log10(number) + 1;
    }
}