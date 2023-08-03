// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Upload;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace Snap.Hutao.Server.Service.Legacy.PizzaHelper;

internal sealed class SubmitDetailModel
{
    [JsonPropertyName("floor")]
    public int Floor { get; set; }

    [JsonPropertyName("room")]
    public int Room { get; set; }

    [JsonPropertyName("half")]
    public int Half { get; set; }

    [JsonPropertyName("usedChars")]
    public List<int> UsedChars { get; set; } = default!;
}
