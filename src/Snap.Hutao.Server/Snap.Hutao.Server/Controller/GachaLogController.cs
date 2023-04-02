// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Snap.Hutao.Server.Controller.Filter;
using Snap.Hutao.Server.Extension;
using Snap.Hutao.Server.Model.Context;
using Snap.Hutao.Server.Model.Entity;
using Snap.Hutao.Server.Model.GachaLog;
using Snap.Hutao.Server.Model.Response;
using Snap.Hutao.Server.Model.Upload;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// 祈愿记录控制器
/// </summary>
[Authorize]
[Route("[controller]")]
[ApiController]
[ServiceFilter(typeof(RequestFilter))]
public class GachaLogController : ControllerBase
{
    private readonly AppDbContext appDbContext;

    /// <summary>
    /// 构造一个新的祈愿记录控制器
    /// </summary>
    /// <param name="appDbContext">数据库上下文</param>
    public GachaLogController(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    /// <summary>
    /// 获取各个卡池对应的最后Id
    /// </summary>
    /// <param name="uid">uid</param>
    /// <returns>各个卡池对应的最后Id</returns>
    [HttpGet("EndIds")]
    public IActionResult GetEndIds([FromQuery(Name = "Uid")] string uid)
    {
        int userId = this.GetUserId();
        EndIds endIds = new();
        foreach (GachaConfigType type in EndIds.QueryTypes)
        {
            EntityGachaItem? item = appDbContext.GachaItems
                .AsNoTracking()
                .Where(i => i.UserId == userId)
                .Where(i => i.Uid == uid)
                .Where(i => i.QueryType == type)
                .OrderByDescending(i => i.Id)
                .FirstOrDefault();

            endIds.Add(type, item?.Id ?? 0L);
        }

        return Response<EndIds>.Success(userId.ToString(), endIds);
    }

    /// <summary>
    /// 获取小于 End Id 的祈愿记录
    /// </summary>
    /// <param name="uidAndEndIds">数据</param>
    /// <returns>祈愿记录</returns>
    [HttpGet("Retrieve")]
    public async Task<IActionResult> RetrieveAsync([FromBody] UidAndEndIds uidAndEndIds)
    {
        int userId = this.GetUserId();
        string uid = uidAndEndIds.Uid;
        EndIds endIds = uidAndEndIds.EndIds;
        List<SimpleGachaItem> gachaItems = new();

        foreach ((string type, long endId) in endIds)
        {
            GachaConfigType configType = Enum.Parse<GachaConfigType>(type);
            long exactEndId = endId == 0 ? long.MaxValue : endId;

            List<EntityGachaItem> items = await appDbContext.GachaItems
                .AsNoTracking()
                .OrderByDescending(i => i.Id)
                .Where(i => i.UserId == userId)
                .Where(i => i.Uid == uid)
                .Where(i => i.QueryType == configType)
                .Where(i => i.Id < exactEndId)
                .ToListAsync()
                .ConfigureAwait(false);

            AppendEntitiesToModels(items, gachaItems);
        }

        return Response<List<SimpleGachaItem>>.Success(userId.ToString(), gachaItems);
    }

    /// <summary>
    /// 上传祈愿记录
    /// </summary>
    /// <param name="gachaData">祈愿数据</param>
    /// <returns>上传成功</returns>
    [HttpPost("Upload")]
    public async Task<IActionResult> UploadAsync([FromBody] SimpleGachaData gachaData)
    {
        int userId = this.GetUserId();
        string uid = gachaData.Uid;

        try
        {
            List<EntityGachaItem> entities = new();
            AppendModelsToEntities(gachaData.Items, entities, userId, gachaData.Uid, gachaData.IsTrusted);
            int count = await appDbContext.GachaItems.AddRangeAndSaveAsync(entities).ConfigureAwait(false);

            return Model.Response.Response.Success($"成功上传了 {count} 条数据");
        }
        catch
        {
            await appDbContext.GachaItems
                .Where(i => i.UserId == userId)
                .Where(i => i.Uid == uid)
                .ExecuteDeleteAsync()
                .ConfigureAwait(false);

            return Model.Response.Response.Fail(ReturnCode.GachaLogDatabaseOperationFailed, "数据异常");
        }
    }

    private static void AppendEntitiesToModels(List<EntityGachaItem> items, List<SimpleGachaItem> gachaItems)
    {
        Span<EntityGachaItem> itemSpan = CollectionsMarshal.AsSpan(items);
        ref EntityGachaItem itemAtZero = ref MemoryMarshal.GetReference(itemSpan);
        for (int i = 0; i < itemSpan.Length; i++)
        {
            ref EntityGachaItem item = ref Unsafe.Add(ref itemAtZero, i);

            SimpleGachaItem simple = new()
            {
                GachaType = item.GachaType,
                QueryType = item.QueryType,
                ItemId = item.ItemId,
                Time = item.Time,
                Id = item.Id,
            };

            gachaItems.Add(simple);
        }
    }

    private static void AppendModelsToEntities(List<SimpleGachaItem> gachaItems, List<EntityGachaItem> items, int userId, string uid, bool isTrusted)
    {
        Span<SimpleGachaItem> gachaItemSpan = CollectionsMarshal.AsSpan(gachaItems);
        ref SimpleGachaItem itemAtZero = ref MemoryMarshal.GetReference(gachaItemSpan);
        for (int i = 0; i < gachaItemSpan.Length; i++)
        {
            ref SimpleGachaItem item = ref Unsafe.Add(ref itemAtZero, i);

            EntityGachaItem entity = new()
            {
                UserId = userId,
                Uid = uid,
                Id = item.Id,
                IsTrusted = isTrusted,
                GachaType = item.GachaType,
                QueryType = item.QueryType,
                ItemId = item.ItemId,
                Time = item.Time,
            };

            items.Add(entity);
        }
    }
}