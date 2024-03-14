// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Afdian;

public class AfdianResponse
{
    [JsonPropertyName("ec")]
    public int ErrorCode { get; set; }

    [JsonPropertyName("em")]
    public string ErrorMessage { get; set; } = default!;
}

[SuppressMessage("", "SA1402")]
public sealed class AfdianResponse<T> : AfdianResponse
{
    [JsonPropertyName("data")]
    public T Data { get; set; } = default!;
}