﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.EntityFrameworkCore;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.Upload;

namespace Snap.Hutao.Server.Controller.Helper;

/// <summary>
/// 记录帮助类
/// </summary>
public static class RecordHelper
{
    /// <summary>
    /// 异步保存记录
    /// </summary>
    /// <param name="appDbContext">数据库上下文</param>
    /// <param name="record">记录</param>
    /// <returns>任务</returns>
    public static async Task SaveRecordAsync(AppDbContext appDbContext, SimpleRecord record)
    {
        EntityRecord? entityRecord = await appDbContext.Records.SingleOrDefaultAsync(r => r.Uid == record.Uid).ConfigureAwait(false);

        if (entityRecord != null)
        {
            appDbContext.Records.Remove(entityRecord);
            await appDbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        entityRecord = new();
        appDbContext.Records.Add(entityRecord);

        // EntityRecord
        entityRecord.Uid = record.Uid;
        entityRecord.Uploader = record.Identity;
        entityRecord.UploadTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        await appDbContext.SaveChangesAsync().ConfigureAwait(false);

        long recordId = entityRecord.PrimaryId;

        // EntityAvatars
        List<EntityAvatar> entityAvatars = record.Avatars.Select(a => ToEntity(a, recordId)).ToList();
        await appDbContext.Avatars.AddRangeAsync(entityAvatars).ConfigureAwait(false);
        await appDbContext.SaveChangesAsync().ConfigureAwait(false);

        // EntitySpiralAbyss
        EntitySpiralAbyss entitySpiralAbyss = new() { RecordId = entityRecord.PrimaryId };
        await appDbContext.SpiralAbysses.AddAsync(entitySpiralAbyss).ConfigureAwait(false);
        await appDbContext.SaveChangesAsync().ConfigureAwait(false);

        long spiralAbyssId = entitySpiralAbyss.PrimaryId;

        // EntityFloors
        List<EntityFloor> entityFloors = record.SpiralAbyss.Floors.Where(f => f.Index >= 9).Select(f => ToEntity(f, spiralAbyssId)).ToList();
        await appDbContext.SpiralAbyssFloors.AddRangeAsync(entityFloors).ConfigureAwait(false);
        await appDbContext.SaveChangesAsync().ConfigureAwait(false);

        // EntityDamageRank
        EntityDamageRank entityDamageRank = ToEntityDamageRank(record.SpiralAbyss.Damage, spiralAbyssId, record.Uid);
        await appDbContext.DamageRanks.AddAsync(entityDamageRank).ConfigureAwait(false);
        await appDbContext.SaveChangesAsync().ConfigureAwait(false);

        // EntityTakeDamageRank
        EntityTakeDamageRank entityTakeDamageRank = ToEntityTakeDamageRank(record.SpiralAbyss.TakeDamage, spiralAbyssId, record.Uid);
        await appDbContext.TakeDamageRanks.AddAsync(entityTakeDamageRank).ConfigureAwait(false);
        await appDbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    private static EntityAvatar ToEntity(SimpleAvatar simpleAvatar, long recordId)
    {
        Dictionary<int, int> relicSetCounter = new();
        foreach (int id in simpleAvatar.ReliquarySetIds)
        {
            relicSetCounter.Increase(id);
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