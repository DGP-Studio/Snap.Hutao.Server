// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Service.Distribution;

public sealed class HutaoPackageMirror
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = default!;

    [JsonPropertyName("mirror_name")]
    public string MirrorName { get; set; } = default!;

    [JsonPropertyName("mirror_type")]
    public string MirrorType { get; set; } = default!;
}
