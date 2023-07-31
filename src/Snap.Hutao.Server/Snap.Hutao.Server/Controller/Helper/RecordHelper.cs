// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.Upload;
using Snap.Hutao.Server.Service;
using Snap.Hutao.Server.Service.Ranking;
using System.Runtime.CompilerServices;

namespace Snap.Hutao.Server.Controller.Helper;

/// <summary>
/// 记录帮助类
/// </summary>
internal static class RecordHelper
{
    /// <summary>
    /// 异步保存记录
    /// </summary>
    /// <param name="appDbContext">数据库上下文</param>
    /// <param name="rankService">排行服务</param>
    /// <param name="expireService">续期服务</param>
    /// <param name="record">记录</param>
    /// <returns>是否触发了赠送时长</returns>
    internal static async Task<RecordUploadResult> SaveRecordAsync(AppDbContext appDbContext, IRankService rankService, ExpireService expireService, SimpleRecord record)
    {
        RecordUploadResult result = RecordUploadResult.None;
        EntityRecord? entityRecord = await appDbContext.Records.SingleOrDefaultAsync(r => r.Uid == record.Uid).ConfigureAwait(false);

        bool hasSpiralAbysses = true;
        if (entityRecord == null)
        {
            hasSpiralAbysses = false;
        }
        else
        {
            if (!await appDbContext.SpiralAbysses.AnyAsync(s => s.RecordId == entityRecord.PrimaryId).ConfigureAwait(false))
            {
                hasSpiralAbysses = false;
            }

            await appDbContext.Records.RemoveAndSaveAsync(entityRecord).ConfigureAwait(false);
        }

        if (hasSpiralAbysses)
        {
            result = RecordUploadResult.NotFirstAttempt;
        }
        else
        {
            if (record.Identity != UploaderIdentities.SnapHutao)
            {
                result = RecordUploadResult.NotSnapHutao;
            }
            else
            {
                if (record.ReservedUserName != null)
                {
                    await expireService.ExtendGachaLogTermAsync(record.ReservedUserName, 3).ConfigureAwait(false);
                    result = RecordUploadResult.GachaLogExtented;
                }
                else
                {
                    result = RecordUploadResult.NoUserNamePresented;
                }
            }
        }

        // EntityRecord
        entityRecord = ToEntity(record);
        await appDbContext.Records.AddAndSaveAsync(entityRecord).ConfigureAwait(false);

        long recordId = entityRecord.PrimaryId;

        // EntityAvatars
        List<EntityAvatar> entityAvatars = record.Avatars.SelectList(a => ToEntity(a, recordId));
        await appDbContext.Avatars.AddRangeAndSaveAsync(entityAvatars).ConfigureAwait(false);

        // EntitySpiralAbyss
        EntitySpiralAbyss entitySpiralAbyss = ToEntity(record.SpiralAbyss, entityRecord.PrimaryId);
        await appDbContext.SpiralAbysses.AddAndSaveAsync(entitySpiralAbyss).ConfigureAwait(false);

        long spiralAbyssId = entitySpiralAbyss.PrimaryId;

        // EntityFloors
        List<EntityFloor> entityFloors = record.SpiralAbyss.Floors.Where(f => f.Index >= 9).Select(f => ToEntity(f, spiralAbyssId)).ToList();
        await appDbContext.SpiralAbyssFloors.AddRangeAndSaveAsync(entityFloors).ConfigureAwait(false);

        // EntityDamageRank
        EntityDamageRank entityDamageRank = ToEntityDamageRank(record.SpiralAbyss.Damage, spiralAbyssId, record.Uid);
        await appDbContext.DamageRanks.AddAndSaveAsync(entityDamageRank).ConfigureAwait(false);

        // EntityTakeDamageRank
        if (record.SpiralAbyss.TakeDamage != null)
        {
            EntityTakeDamageRank entityTakeDamageRank = ToEntityTakeDamageRank(record.SpiralAbyss.TakeDamage, spiralAbyssId, record.Uid);
            await appDbContext.TakeDamageRanks.AddAndSaveAsync(entityTakeDamageRank).ConfigureAwait(false);
        }

        // Redis rank sync
        await rankService.SaveRankAsync(record.Uid, record.SpiralAbyss.Damage, record.SpiralAbyss.TakeDamage).ConfigureAwait(false);

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static EntityRecord ToEntity(SimpleRecord simpleRecord)
    {
        return new()
        {
            Uid = simpleRecord.Uid,
            Uploader = simpleRecord.Identity,
            UploadTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static EntityAvatar ToEntity(SimpleAvatar simpleAvatar, long recordId)
    {
        Dictionary<int, int> relicSetCounter = new();
        foreach (int id in simpleAvatar.ReliquarySetIds)
        {
            relicSetCounter.IncreaseOne(id);
        }

        foreach (int id in relicSetCounter.Keys.ToList())
        {
            if (relicSetCounter[id] >= 4)
            {
                relicSetCounter[id] = 4;
            }
            else if (relicSetCounter[id] >= 2)
            {
                relicSetCounter[id] = 2;
            }
            else
            {
                relicSetCounter.Remove(id);
            }
        }

        return new()
        {
            RecordId = recordId,
            AvatarId = simpleAvatar.AvatarId,
            WeaponId = simpleAvatar.WeaponId,
            ReliquarySet = string.Join(',', relicSetCounter.OrderBy(kvp => kvp.Key).Select(x => $"{x.Key}-{x.Value}")),
            ActivedConstellationNumber = simpleAvatar.ActivedConstellationNumber,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static EntitySpiralAbyss ToEntity(SimpleSpiralAbyss simpleSpiralAbyss, long recordId)
    {
        return new()
        {
            RecordId = recordId,
            TotalBattleTimes = simpleSpiralAbyss.TotalBattleTimes,
            TotalWinTimes = simpleSpiralAbyss.TotalWinTimes,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static EntityFloor ToEntity(SimpleFloor simpleFloor, long spiralAbyssId)
    {
        // Sort team avatars.
        foreach (SimpleLevel level in simpleFloor.Levels)
        {
            foreach (SimpleBattle battle in level.Battles)
            {
                battle.Avatars.Sort();
            }
        }

        return new()
        {
            SpiralAbyssId = spiralAbyssId,
            Index = simpleFloor.Index,
            Star = simpleFloor.Star,
            Levels = simpleFloor.Levels,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static EntityDamageRank ToEntityDamageRank(SimpleRank rank, long spiralAbyssId, string uid)
    {
        return new()
        {
            SpiralAbyssId = spiralAbyssId,
            AvatarId = rank.AvatarId,
            Value = rank.Value,
            Uid = uid,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static EntityTakeDamageRank ToEntityTakeDamageRank(SimpleRank rank, long spiralAbyssId, string uid)
    {
        return new()
        {
            SpiralAbyssId = spiralAbyssId,
            AvatarId = rank.AvatarId,
            Value = rank.Value,
            Uid = uid,
        };
    }
}