// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Legacy;
using Snap.Hutao.Server.Model.Upload;

namespace Snap.Hutao.Server.Service.Ranking;

public interface IRankService
{
    Task<long> ClearRanksAsync();

    Task<Rank> RetriveRankAsync(string uid);

    Task SaveRankAsync(string uid, SimpleRank damage, SimpleRank? takeDamage);
}