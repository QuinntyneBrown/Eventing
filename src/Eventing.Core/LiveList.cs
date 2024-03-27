// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading.Channels;

namespace Eventing.Core;

public class LiveList<T>: IObservable<T> {

    private IList<Subscription> _subscriptions = new List<Subscription>();

    private readonly CancellationTokenSource cancellationTokenSource;

    private readonly object _gate = new object();

    private readonly LiveListProvider _provider;

    public string Name { get; set; }

    public LiveList()
    {

    }

    public LiveList(ChannelReader<T> liveListSource, LiveListProvider liveListProvider)
    {
        cancellationTokenSource = new CancellationTokenSource();

        _ = Task.Run(async () =>
        {
            while(!cancellationTokenSource.Token.IsCancellationRequested)
            {
                var item = await liveListSource.ReadAsync(cancellationTokenSource.Token).ConfigureAwait(false);

                Broadcast(item);
            }
        });
    }

    public IDisposable Subscribe(Action<T> onNext)
    {
        var observer = new LiveListObserver<T>(onNext);

        return Subscribe(observer);
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        if(observer is LiveListObserver<T> liveListObserver)
        {
            return AddSubscription(liveListObserver);
        }
        
        throw new NotSupportedException(observer.GetType().Name);
    }

    public void Unsubscribe(IObserver<T> observer)
    {
        RemoveSubscription(observer);

        if(_subscriptions.Count == 0)
        {
            cancellationTokenSource.Cancel();

            _provider.Stop(Name);
        }
    }

    public void Broadcast(T item)
    {
        var subscriptions = _subscriptions;

        for (int i = 0; i < subscriptions.Count; i++)
        {
            subscriptions[i].Observer.OnNext(item);
        }
    }

    protected void BroadcastError(Exception error)
    {
        var subscriptions = _subscriptions;
        for (int i = 0; i < subscriptions.Count; i++)
        {
            subscriptions[i].Observer.OnError(error);
        }
    }

    protected void BroadcastOnCompleted()
    {
        var subscriptions = _subscriptions;
        for (int i = 0; i < subscriptions.Count; i++)
        {
            subscriptions[i].Observer.OnCompleted();
        }
    }

    private Subscription AddSubscription(LiveListObserver<T> observer)
    {
        lock (_gate)
        {
            var newList = new List<Subscription>(_subscriptions);
            var subscription = new Subscription() { LiveList = this, Observer = observer };
            newList.Add(subscription);
            Interlocked.Exchange(ref _subscriptions, newList);
            return subscription;
        }
    }

    private void RemoveSubscription(IObserver<T> observer)
    {
        lock (_gate)
        {
            var newList = new List<Subscription>(_subscriptions);

            var subscription = newList.FirstOrDefault(s => s.Observer == observer);
            
            if (subscription != null)
            {
                newList.Remove(subscription);
                Interlocked.Exchange(ref _subscriptions, newList);
            }
        }
    }

    internal class Subscription : IDisposable
    {
        public required LiveList<T> LiveList;
        public required LiveListObserver<T> Observer;

        public void Dispose()
        {
            LiveList.Unsubscribe(Observer);
        }
    }

}

