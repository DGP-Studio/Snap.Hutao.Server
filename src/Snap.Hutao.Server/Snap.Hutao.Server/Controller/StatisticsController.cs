// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Mvc;
using Snap.Hutao.Server.Controller.Filter;

namespace Snap.Hutao.Server.Controller;

/// <summary>
/// 统计控制器
/// </summary>
[Route("[controller]")]
[ApiController]
[ServiceFilter(typeof(RequestFilter))]
public class StatisticsController : ControllerBase
{

}