// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.Legacy;
using Snap.Hutao.Server.Model.Upload;
using StackExchange.Redis;

namespace Snap.Hutao.Server.Service;

/// <summary>
/// 排行服务
/// </summary>
public sealed class RankService : IDisposable
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ConnectionMultiplexer redis;

    /// <summary>
    /// 构造一个新的排行服务
    /// </summary>
    /// <param name="scopeFactory">范围工厂</param>
    public RankService(IServiceScopeFactory scopeFactory)
    {
        this.scopeFactory = scopeFactory;
        redis = ConnectionMultiplexer.Connect("172.17.0.1:6379");
    }

    /// <summary>
    /// 将排行存入 Redis
    /// </summary>
    /// <param name="uid">uid</param>
    /// <param name="damage">造成伤害</param>
    /// <param name="takeDamage">受到伤害</param>
    /// <returns>任务</returns>
    public async Task SaveRankAsync(string uid, SimpleRank damage, SimpleRank takeDamage)
    {
        IDatabase db = redis.GetDatabase(12);

        using (IServiceScope scope = scopeFactory.CreateScope())
        {
            AppDbContext appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        }

        // 保存使用过的键
        await db.SetAddAsync("Hutao.UsedKeys", "Hutao.Damage.Avatar.All").ConfigureAwait(false);
        await db.SetAddAsync("Hutao.UsedKeys", "Hutao.TakeDamage.Avatar.All").ConfigureAwait(false);
        await db.SetAddAsync("Hutao.UsedKeys", $"Hutao.Damage.Avatar.{damage.AvatarId}").ConfigureAwait(false);
        await db.SetAddAsync("Hutao.UsedKeys", $"Hutao.TakeDamage.Avatar.{takeDamage.AvatarId}").ConfigureAwait(false);

        // 造成伤害
        await db.SortedSetAddAsync($"Hutao.Damage.Avatar.{damage.AvatarId}", uid, damage.Value).ConfigureAwait(false);
        await db.SortedSetAddAsync("Hutao.Damage.Avatar.All", uid, damage.Value).ConfigureAwait(false);

        // 受到伤害
        await db.SortedSetAddAsync($"Hutao.TakeDamage.Avatar.{takeDamage.AvatarId}", uid, takeDamage.Value).ConfigureAwait(false);
        await db.SortedSetAddAsync("Hutao.TakeDamage.Avatar.All", uid, takeDamage.Value).ConfigureAwait(false);
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

            damage = appDbContext.DamageRanks.SingleOrDefault(r => r.Uid == uid);
            takeDamage = appDbContext.TakeDamageRanks.SingleOrDefault(r => r.Uid == uid);
        }

        if (damage == null || takeDamage == null)
        {
            return new(null, null);
        }

        IDatabase db = redis.GetDatabase(12);

        long damageTotal = await db.SortedSetLengthAsync("Hutao.Damage.Avatar.All").ConfigureAwait(false);
        long takeDamageTotal = await db.SortedSetLengthAsync("Hutao.TakeDamage.Avatar.All").ConfigureAwait(false);

        long avatarDamageTotal = await db.SortedSetLengthAsync($"Hutao.Damage.Avatar.{damage.AvatarId}").ConfigureAwait(false);
        long avatarTakeDamageTotal = await db.SortedSetLengthAsync($"Hutao.TakeDamage.Avatar.{damage.AvatarId}").ConfigureAwait(false);

        long damageIndex = await db.SortedSetRankAsync("Hutao.Damage.Avatar.All", uid).ConfigureAwait(false) ?? 0;
        long takeDamageIndex = await db.SortedSetRankAsync("Hutao.TakeDamage.Avatar.All", uid).ConfigureAwait(false) ?? 0;

        long avatarDamageIndex = await db.SortedSetRankAsync($"Hutao.Damage.Avatar.{damage.AvatarId}", uid).ConfigureAwait(false) ?? 0;
        long avatarTakeDamageIndex = await db.SortedSetRankAsync($"Hutao.TakeDamage.Avatar.{damage.AvatarId}", uid).ConfigureAwait(false) ?? 0;

        RankValue damageValue = new(
            damage.AvatarId,
            damage.Value,
            GetRankValue(damageIndex, damageTotal),
            GetRankValue(avatarDamageIndex, avatarDamageTotal));

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
        return (index + 1) / (double)total;
    }
}
