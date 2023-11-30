// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Afdian;

public sealed class Order
{
    [JsonPropertyName("out_trade_no")]
    public string OutTradeNo { get; set; } = default!;

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = default!;

    [JsonPropertyName("plan_id")]
    public string PlanId { get; set; } = default!;

    [JsonPropertyName("month")]
    public int Month { get; set; }

    [JsonPropertyName("total_amount")]
    public string TotalAmount { get; set; } = default!;

    [JsonPropertyName("show_amount")]
    public string ShowAmount { get; set; } = default!;

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("remark")]
    public string Remark { get; set; } = default!;

    [JsonPropertyName("redeem_id")]
    public string RedeemId { get; set; } = default!;

    [JsonPropertyName("product_type")]
    public int ProductType { get; set; }

    [JsonPropertyName("discount")]
    public string Discount { get; set; } = default!;

    [JsonPropertyName("sku_detail")]
    public List<SkuDetail> SkuDetail { get; set; } = default!;

    [JsonPropertyName("create_time")]
    public long CreateTime { get; set; }

    [JsonPropertyName("plan_title")]
    public string PlanTitle { get; set; } = default!;

    [JsonPropertyName("user_private_id")]
    public string UserPrivateId { get; set; } = default!;

    [JsonPropertyName("address_person")]
    public string AddressPerson { get; set; } = default!;

    [JsonPropertyName("address_phone")]
    public string AddressPhone { get; set; } = default!;

    [JsonPropertyName("address_address")]
    public string AddressAddress { get; set; } = default!;
}