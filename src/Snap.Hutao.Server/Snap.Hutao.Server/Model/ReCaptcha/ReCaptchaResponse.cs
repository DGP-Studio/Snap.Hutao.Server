// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System.Text.Json.Serialization;

namespace Snap.Hutao.Server.Model.ReCaptcha;

/// <summary>
/// reCAPTCHA 验证响应
/// </summary>
public class ReCaptchaResponse
{
    /// <summary>
    /// Whether this request was a valid reCAPTCHA token for your site
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// The score for this request (0.0 - 1.0)
    /// </summary>
    [JsonPropertyName("score")]
    public float Score { get; set; }

    /// <summary>
    /// The action name for this request (important to verify)
    /// </summary>
    [JsonPropertyName("action")]
    public string Action { get; set; } = default!;

    /// <summary>
    /// timestamp of the challenge load (ISO format yyyy-MM-dd'T'HH:mm:ssZZ)
    /// </summary>
    [JsonPropertyName("challenge_ts")]
    public string ChallengeTimeStamp { get; set; } = default!;

    /// <summary>
    /// The hostname of the site where the reCAPTCHA was solved
    /// </summary>
    [JsonPropertyName("hostname")]
    public string HostName { get; set; } = default!;
}
