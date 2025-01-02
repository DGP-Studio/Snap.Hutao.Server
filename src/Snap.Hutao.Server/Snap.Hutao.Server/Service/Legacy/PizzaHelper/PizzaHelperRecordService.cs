// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Upload;
using Snap.Hutao.Server.Option;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace Snap.Hutao.Server.Service.Legacy.PizzaHelper;

// Scoped
internal sealed class PizzaHelperRecordService
{
    private readonly HttpClient httpClient;
    private readonly GenshinPizzaHelperOptions options;
    private readonly ILogger<PizzaHelperRecordService> logger;

    public PizzaHelperRecordService(HttpClient httpClient, AppOptions appOptions, ILogger<PizzaHelperRecordService> logger)
    {
        this.httpClient = httpClient;
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        options = appOptions.GenshinPizzaHelper;
        this.logger = logger;
    }

    public async ValueTask TryPostRecordAsync(SimpleRecord record)
    {
        if (record.Identity == UploaderIdentities.GenshinPizzaHelper)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(record.Uid);
        ArgumentNullException.ThrowIfNull(record.Avatars);
        ArgumentNullException.ThrowIfNull(record.SpiralAbyss);

        string obfuscatedUid = MD5Hash($"{record.Uid}{MD5Hash(record.Uid)}{options.UidSalt}");
        string region = EvaluateRegion(record.Uid);
        List<int> owningChars = record.Avatars.Select(x => x.AvatarId).ToList();
        AvatarHoldingData holdingData = new()
        {
            Uid = obfuscatedUid,
            ServerId = region,
            UpdateDate = $"{DateTimeOffset.Now:yyyy-MM-dd}",
            OwningChars = owningChars,
        };

        AddSignatureToHttpClientHeader(holdingData);
        using (HttpResponseMessage response = await httpClient.PostAsJsonAsync(options.EndPoints.AvatarHolding, holdingData).ConfigureAwait(false))
        {
            logger.LogInformation("Pizza Helper user holding sync: {Resp}", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        if (record.SpiralAbyss.Floors.Where(f => f.Index >= 9).SelectMany(f => f.Levels).Select(f => f.Star).Sum() != 36)
        {
            // 只同步满星数据
            return;
        }

        DateTimeOffset now = DateTimeOffset.Now;
        AbyssData abyssData = new()
        {
            Uid = obfuscatedUid,
            Server = region,
            AbyssSeason = $"{now:yyyyMM}{(now.Day <= 15 ? 0 : 1)}",
            OwningChars = owningChars,
            AbyssRank = new()
            {
                TopDamageValue = record.SpiralAbyss.Damage.Value,
                TopDamage = record.SpiralAbyss.Damage.AvatarId,
                TopDefeat = record.SpiralAbyss.Defeat?.AvatarId ?? -1,
                TopTakeDamage = record.SpiralAbyss.TakeDamage?.AvatarId ?? -1,
                TopQUsed = record.SpiralAbyss.EnergySkill?.AvatarId ?? -1,
                TopEUsed = record.SpiralAbyss.NormalSkill?.AvatarId ?? -1,
            },
            SubmitDetails = ToSubmitDetailModels(record.SpiralAbyss.Floors).ToList(),
            BattleCount = record.SpiralAbyss.TotalBattleTimes,
            WinCount = record.SpiralAbyss.TotalWinTimes,
        };

        AddSignatureToHttpClientHeader(abyssData);
        using (HttpResponseMessage response = await httpClient.PostAsJsonAsync(options.EndPoints.SpiralAbyss, abyssData).ConfigureAwait(false))
        {
            logger.LogInformation("Pizza Helper abyss sync: {Resp}", await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }
    }

    private static string MD5Hash(string source)
    {
        return Convert.ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(source))).ToLowerInvariant();
    }

    private static string SHA256Hash(string source)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source))).ToLowerInvariant();
    }

    private static string EvaluateRegion(string uid)
    {
        char first = uid.AsSpan()[0];
        return first switch
        {
            // CN
            // >= '1' and <= '4' => "cn_gf01",
            '5' => "cn_qd01",               // 渠道

            // OS
            '6' => "os_usa",                // 美服
            '7' => "os_euro",               // 欧服
            '8' => "os_asia",               // 亚服
            '9' => "os_cht",                // 台服
            _ => "cn_gf01",                 // 国服
        };
    }

    private static IEnumerable<SubmitDetailModel> ToSubmitDetailModels(List<SimpleFloor> floors)
    {
        foreach (SimpleFloor floor in floors.Where(f => f.Index >= 9))
        {
            foreach (SimpleLevel level in floor.Levels)
            {
                foreach (SimpleBattle battle in level.Battles)
                {
                    yield return new()
                    {
                        Floor = floor.Index,
                        Room = level.Index,
                        Half = battle.Index,
                        UsedChars = battle.Avatars,
                    };
                }
            }
        }
    }

    private void AddSignatureToHttpClientHeader<T>(T data)
    {
        HttpRequestHeaders headers = httpClient.DefaultRequestHeaders;
        headers.Remove("dseed");
        headers.Add("dseed", $"{Random.Shared.Next(0, 999999)}");
        headers.Remove("ds");
        headers.Add("ds", $"{SHA256Hash($"{SHA256Hash(JsonSerializer.Serialize(data))}{options.ApiSalt}")}");
    }
}