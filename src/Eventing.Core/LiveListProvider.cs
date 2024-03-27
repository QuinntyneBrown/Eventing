// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Eventing.Core;

public abstract class LiveListProvider
{
    private readonly ILogger<LiveListProvider> _logger;

    public static ConcurrentDictionary<string, LiveList<object>> _liveListDictionary = new ConcurrentDictionary<string, LiveList<object>>();

    public LiveListProvider(ILogger<LiveListProvider> logger){
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string Name { get; set; }
    public virtual void MapLiveList<T>(string liveListName, Action<T> onNext)
    {
        _liveListDictionary.TryGetValue(liveListName, out LiveList<object> liveList);

        if(liveList is LiveList<T> l)
        {
            l.Subscribe(onNext);
        }
        
    }

    public virtual void Stop(string liveListName)
    {
        _liveListDictionary.Remove(liveListName, out _);
    }
}

