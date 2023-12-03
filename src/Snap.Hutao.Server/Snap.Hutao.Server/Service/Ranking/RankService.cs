// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.SpiralAbyss;
using Snap.Hutao.Server.Model.Legacy;
using Snap.Hutao.Server.Model.Upload;
using Snap.Hutao.Server.Option;
using StackExchange.Redis;

namespace Snap.Hutao.Server.Service.Ranking;

// Singleton
internal sealed class RankService : IRankService, IDisposable
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ConnectionMultiplexer redis;

    public RankService(IServiceScopeFactory scopeFactory, AppOptions appOptions, ILogger<RankService> logger)
    {
        this.scopeFactory = scopeFactory;
        string redisAddress = appOptions.RedisAddress;
        logger.LogInformation("Using Redis: {config}", redisAddress);
        redis = ConnectionMultiplexer.Connect(redisAddress);
    }

    /// <summary>
    /// 将排行存入 Redis
    /// </summary>
    /// <param name="uid">uid</param>
    /// <param name="damage">造成伤害</param>
    /// <param name="takeDamage">受到伤害</param>
    /// <returns>任务</returns>
    public async Task SaveRankAsync(string uid, SimpleRank damage, SimpleRank? takeDamage)
    {
        IDatabase db = redis.GetDatabase(12);

        // 保存使用过的键
        await db.SetAddAsync("Hutao.UsedKeys", "Hutao.Damage.Avatar.All").ConfigureAwait(false);
        await db.SetAddAsync("Hutao.UsedKeys", "Hutao.TakeDamage.Avatar.All").ConfigureAwait(false);

        // 造成伤害
        await db.SetAddAsync("Hutao.UsedKeys", $"Hutao.Damage.Avatar.{damage.AvatarId}").ConfigureAwait(false);
        await db.SortedSetAddAsync($"Hutao.Damage.Avatar.{damage.AvatarId}", uid, damage.Value).ConfigureAwait(false);
        await db.SortedSetAddAsync("Hutao.Damage.Avatar.All", uid, damage.Value).ConfigureAwait(false);

        // 受到伤害
        if (takeDamage != null)
        {
            await db.SetAddAsync("Hutao.UsedKeys", $"Hutao.TakeDamage.Avatar.{takeDamage.AvatarId}").ConfigureAwait(false);
            await db.SortedSetAddAsync($"Hutao.TakeDamage.Avatar.{takeDamage.AvatarId}", uid, takeDamage.Value).ConfigureAwait(false);
            await db.SortedSetAddAsync("Hutao.TakeDamage.Avatar.All", uid, takeDamage.Value).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 异步获取排行
    /// </summary>
    /// <param name="uid">uid</param>
    /// <returns>排行数据</returns>
    public async Task<Rank> RetriveRankAsync(string uid)
    {
        EntityDamageRank? damage;
        EntityTakeDamageRank? takeDamage;

        using (IServiceScope scope = scopeFactory.CreateScope())
        {
            AppDbContext appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            damage = appDbContext.DamageRanks.AsNoTracking().SingleOrDefault(r => r.Uid == uid);
            takeDamage = appDbContext.TakeDamageRanks.AsNoTracking().SingleOrDefault(r => r.Uid == uid);
        }

        if (damage == null)
        {
            return new(null, null);
        }

        IDatabase db = redis.GetDatabase(12);

        long damageIndex = await db.SortedSetRankAsync("Hutao.Damage.Avatar.All", uid).ConfigureAwait(false) ?? 0;
        long damageTotal = await db.SortedSetLengthAsync("Hutao.Damage.Avatar.All").ConfigureAwait(false);
        long avatarDamageIndex = await db.SortedSetRankAsync($"Hutao.Damage.Avatar.{damage.AvatarId}", uid).ConfigureAwait(false) ?? 0;
        long avatarDamageTotal = await db.SortedSetLengthAsync($"Hutao.Damage.Avatar.{damage.AvatarId}").ConfigureAwait(false);

        RankValue damageValue = new(
            damage.AvatarId,
            damage.Value,
            GetRankValue(damageIndex, damageTotal),
            GetRankValue(avatarDamageIndex, avatarDamageTotal));

        if (takeDamage == null)
        {
            return new Rank(damageValue, null);
        }

        long takeDamageIndex = await db.SortedSetRankAsync("Hutao.TakeDamage.Avatar.All", uid).ConfigureAwait(false) ?? 0;
        long takeDamageTotal = await db.SortedSetLengthAsync("Hutao.TakeDamage.Avatar.All").ConfigureAwait(false);
        long avatarTakeDamageIndex = await db.SortedSetRankAsync($"Hutao.TakeDamage.Avatar.{damage.AvatarId}", uid).ConfigureAwait(false) ?? 0;
        long avatarTakeDamageTotal = await db.SortedSetLengthAsync($"Hutao.TakeDamage.Avatar.{damage.AvatarId}").ConfigureAwait(false);

        RankValue takeDamageValue = new(
            takeDamage.AvatarId,
            takeDamage.Value,
            GetRankValue(takeDamageIndex, takeDamageTotal),
            GetRankValue(avatarTakeDamageIndex, avatarTakeDamageTotal));

        return new Rank(damageValue, takeDamageValue);
    }

    /// <summary>
    /// 异步清除排行
    /// </summary>
    /// <returns>任务</returns>
    public async Task<long> ClearRanksAsync()
    {
        IDatabase db = redis.GetDatabase(12);

        RedisValue[] keys = await db.SetMembersAsync("Hutao.UsedKeys").ConfigureAwait(false);
        RedisKey[] usedKeys = keys.Select(k => new RedisKey(k)).ToArray();

        return await db.KeyDeleteAsync(usedKeys).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        redis.Dispose();
    }

    private static double GetRankValue(long index, long total)
    {
        return total == 0 ? 0 : (index + 1) / (double)total;
    }
}