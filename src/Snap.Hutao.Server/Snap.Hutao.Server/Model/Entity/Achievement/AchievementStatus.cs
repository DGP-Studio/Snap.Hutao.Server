﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Entity.Achievement;

public enum AchievementStatus /*: int*/
{
    STATUS_UNRECOGNIZED = -1,
    STATUS_INVALID = 0,
    STATUS_UNFINISHED = 1,
    STATUS_FINISHED = 2,
    STATUS_REWARD_TAKEN = 3,
}