// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Security.Cryptography;

namespace Snap.Hutao.Server.Model.Afdian;

public class QueryOrder
{
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    [JsonPropertyName("params")]
    public string? Params { get; set; }

    [JsonPropertyName("ts")]
    public long Ts { get; set; }

    [JsonPropertyName("sign")]
    public string? Sign { get; set; }

    public static QueryOrder Create(string userId, string tradeNumber, string token)
    {
        string param = JsonSerializer.Serialize(new Param() { OutTradeNo = tradeNumber });
        return new QueryOrder
        {
            UserId = userId,
            Params = param,
            Ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Sign = Signature(userId, token, param),
        };
    }

    private static string Signature(string userId, string token, string param)
    {
        long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes($"{token}params{param}ts{ts}user_id{userId}"));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private sealed class Param
    {
        [JsonPropertyName("out_trade_no")]
        public string OutTradeNo { get; set; } = default!;
    }
}