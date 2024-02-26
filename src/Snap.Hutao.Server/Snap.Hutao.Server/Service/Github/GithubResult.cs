// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Server.Model.Response;

namespace Snap.Hutao.Server.Service.Github;

public class GithubResult<T>
{
    [MemberNotNullWhen(true, nameof(Data))]
    public bool IsSuccess { get; set; }

    public ReturnCode ReturnCode { get; set; } = ReturnCode.Success;

    public T? Data { get; set; }

    public static GithubResult<T> Success(T data)
    {
        return new GithubResult<T>
        {
            IsSuccess = true,
            Data = data,
        };
    }

    public static GithubResult<T> Error(ReturnCode code)
    {
        return new GithubResult<T>
        {
            IsSuccess = false,
            ReturnCode = code,
        };
    }
}