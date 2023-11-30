// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

namespace Snap.Hutao.Server.Model.Upload;

public sealed class HutaoUploadLog
{
    public string Id { get; set; } = default!;

    public long Time { get; set; }

    public string Info { get; set; } = default!;
}
