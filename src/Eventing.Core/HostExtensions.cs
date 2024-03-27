// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Eventing.Core;

namespace Microsoft.Extensions.Hosting;

public static class HostExtensions { 

    public static LiveListProvider WithProvider(this IHost host, string providerName)
    {
        var liveListProvider = host.Services.GetKeyedService<LiveListProvider>(providerName);

        if(liveListProvider == null)
        {
            throw new InvalidOperationException();
        }

        return host.Services.GetKeyedService<LiveListProvider>(providerName)!;
    }
}
