// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Text.Json.Serialization;

namespace Snap.Hutao.Server.Model.Afdian;

/// <summary>
/// 订单
/// </summary>
public sealed class Order
{
    /// <summary>
    /// 订单号
    /// </summary>
    [JsonPropertyName("out_trade_no")]
    public string OutTradeNo { get; set; } = default!;

    /// <summary>
    /// 下单用户ID
    /// </summary>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = default!;

    /// <summary>
    /// 方案ID，如自选，则为空
    /// </summary>
    [JsonPropertyName("plan_id")]
    public string PlanId { get; set; } = default!;

    /// <summary>
    /// 赞助月份个数
    /// </summary>
    [JsonPropertyName("month")]
    public int Month { get; set; }

    /// <summary>
    /// 真实付款金额，如有兑换码，则为0.00
    /// </summary>
    [JsonPropertyName("total_amount")]
    public string TotalAmount { get; set; } = default!;

    /// <summary>
    /// 显示金额，如有折扣则为折扣前金额
    /// </summary>
    [JsonPropertyName("show_amount")]
    public string ShowAmount { get; set; } = default!;

    /// <summary>
    /// 2 为交易成功。目前仅会推送此类型
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>
    /// 订单留言
    /// </summary>
    [JsonPropertyName("remark")]
    public string Remark { get; set; } = default!;

    /// <summary>
    /// 兑换码ID
    /// </summary>
    [JsonPropertyName("redeem_id")]
    public string RedeemId { get; set; } = default!;

    /// <summary>
    /// 0表示常规方案 1表示售卖方案
    /// </summary>
    [JsonPropertyName("product_type")]
    public int ProductType { get; set; }

    /// <summary>
    /// 折扣
    /// </summary>
    [JsonPropertyName("discount")]
    public string Discount { get; set; } = default!;

    /// <summary>
    /// 如果为售卖类型，以数组形式表示具体型号
    /// </summary>
    [JsonPropertyName("sku_detail")]
    public List<SkuDetail> SkuDetail { get; set; } = default!;

    /// <summary>
    /// 创建时间
    /// </summary>
    [JsonPropertyName("create_time")]
    public long CreateTime { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [JsonPropertyName("plan_title")]
    public string PlanTitle { get; set; } = default!;

    /// <summary>
    /// 用户UUID
    /// </summary>
    [JsonPropertyName("user_private_id")]
    public string UserPrivateId { get; set; } = default!;

    /// <summary>
    /// 收件人
    /// </summary>
    [JsonPropertyName("address_person")]
    public string AddressPerson { get; set; } = default!;

    /// <summary>
    /// 收件人电话
    /// </summary>
    [JsonPropertyName("address_phone")]
    public string AddressPhone { get; set; } = default!;

    /// <summary>
    /// 收件人地址
    /// </summary>
    [JsonPropertyName("address_address")]
    public string AddressAddress { get; set; } = default!;
}