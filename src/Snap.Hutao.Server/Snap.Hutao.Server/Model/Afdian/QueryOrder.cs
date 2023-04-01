// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Snap.Hutao.Server.Model.Afdian;

/// <summary>
/// 查询订单
/// </summary>
public class QueryOrder
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [JsonPropertyName("user_id")]
    public string? UserId { get; set; }

    /// <summary>
    /// 参数
    /// </summary>
    [JsonPropertyName("params")]
    public string? Params { get; set; }

    /// <summary>
    /// 时间
    /// </summary>
    [JsonPropertyName("ts")]
    public long Ts { get; set; }

    /// <summary>
    /// 签名
    /// </summary>
    [JsonPropertyName("sign")]
    public string? Sign { get; set; }

    /// <summary>
    /// 根据订单号创建
    /// </summary>
    /// <param name="userId">用户Id</param>
    /// <param name="tradeNumber">订单号</param>
    /// <param name="token">api token</param>
    /// <returns>查询 POST 数据</returns>
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
        /// <summary>
        /// 订单编号
        /// </summary>
        [JsonPropertyName("out_trade_no")]
        public string OutTradeNo { get; set; }
    }
}