// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Redeem;

public sealed class RedeemGenerateResponse
{
    public RedeemGenerateResponse(List<string> codes)
    {
        Codes = codes;
    }

    public List<string> Codes { get; set; }
}