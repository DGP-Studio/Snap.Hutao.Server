// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity.Redeem;
using Snap.Hutao.Server.Model.Redeem;
using Snap.Hutao.Server.Service.Authorization;
using Snap.Hutao.Server.Service.Expire;

namespace Snap.Hutao.Server.Service.Redeem;

// Scoped
public sealed class RedeemService
{
    private readonly GachaLogExpireService gachaLogExpireService;
    private readonly CdnExpireService cdnExpireService;
    private readonly PassportService passportService;
    private readonly AppDbContext appDbContext;

    public RedeemService(IServiceProvider serviceProvider)
    {
        gachaLogExpireService = serviceProvider.GetRequiredService<GachaLogExpireService>();
        cdnExpireService = serviceProvider.GetRequiredService<CdnExpireService>();
        passportService = serviceProvider.GetRequiredService<PassportService>();
        appDbContext = serviceProvider.GetRequiredService<AppDbContext>();
    }

    public async Task<RedeemUseResponse> UseRedeemCodeAsync(RedeemUseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request.Username);
        if (request.Code.Length is not 18)
        {
            return new(RedeemStatus.Invalid);
        }

        if (appDbContext.RedeemCodes.SingleOrDefault(c => c.Code == request.Code) is not { } code)
        {
            return new(RedeemStatus.NotExists);
        }

        if (code.IsUsed)
        {
            return new(RedeemStatus.AlreadyUsed);
        }

        IExpireService expireService = code.Type switch
        {
            1 => gachaLogExpireService,
            2 => cdnExpireService,
            _ => throw new ArgumentOutOfRangeException(),
        };

        TermExtendResult result = await expireService.ExtendTermForUserNameAsync(request.Username, code.Value);

        if (result.Kind is TermExtendResultKind.Ok)
        {
            code.IsUsed = true;
            code.UseBy = request.Username;
            code.UseTime = DateTimeOffset.UtcNow;
            await appDbContext.RedeemCodes.UpdateAndSaveAsync(code);
            return new(RedeemStatus.Ok, code.Type, result.ExpiredAt);
        }

        return new(RedeemStatus.DbError);
    }

    public async Task<RedeemGenerateResponse> GenerateRedeemCodesAsync(RedeemGenerateRequest req)
    {
        List<string> codes = [];
        for (uint i = 0; i < req.Count; i++)
        {
            RedeemCode code = new()
            {
                Code = Guid.NewGuid().ToString().ToUpperInvariant()[..18],
                Type = req.Type,
                Value = req.Value,
                Description = req.Description,
                Creator = req.Creator,
                CreateTime = DateTimeOffset.UtcNow,
            };

            codes.Add(code.Code);
            await appDbContext.RedeemCodes.AddAndSaveAsync(code).ConfigureAwait(false);
        }

        return new(codes);
    }
}