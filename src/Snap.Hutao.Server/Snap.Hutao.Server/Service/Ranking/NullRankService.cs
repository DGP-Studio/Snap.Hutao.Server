// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Legacy;
using Snap.Hutao.Server.Model.Upload;

namespace Snap.Hutao.Server.Service.Ranking;

internal sealed class NullRankService : IRankService
{
    public Task<long> ClearRanksAsync()
    {
        return Task.FromResult(0L);
    }

    public Task<Rank> RetriveRankAsync(string uid)
    {
        return Task.FromResult(default(Rank)!);
    }

    public Task SaveRankAsync(string uid, SimpleRank damage, SimpleRank? takeDamage)
    {
        return Task.CompletedTask;
    }
}