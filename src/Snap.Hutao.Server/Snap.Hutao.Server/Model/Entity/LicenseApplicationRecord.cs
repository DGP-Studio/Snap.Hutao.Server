// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Snap.Hutao.Server.Model.Entity;

/// <summary>
/// 开发者许可申请记录
/// </summary>
[Table("license_application_records")]
public sealed class LicenseApplicationRecord
{
    /// <summary>
    /// 主键
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long PrimaryId { get; set; }

    /// <summary>
    /// 外键用户
    /// </summary>
    public int UserId { get; set; } = default!;

    /// <summary>
    /// 用户
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public HutaoUser User { get; set; } = default!;

    /// <summary>
    /// 项目地址
    /// </summary>
    public string ProjectUrl { get; set; } = default!;

    /// <summary>
    /// 随机验证码
    /// </summary>
    public string ApprovalCode { get; set; } = default!;

    /// <summary>
    /// 是否通过
    /// </summary>
    public bool IsApproved { get; set; }
}