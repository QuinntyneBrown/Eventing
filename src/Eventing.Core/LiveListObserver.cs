// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Eventing.Core;

public class LiveListObserver<T> : IObserver<T>
{
    private readonly Action<T> onNext;

    public LiveListObserver(Action<T> onNext)
    {
        ArgumentNullException.ThrowIfNull(onNext);

        this.onNext = onNext;
    }

    public void OnCompleted()
    {

    }

    public void OnError(Exception error)
    {

    }

    public void OnNext(T value)
    {
        onNext.Invoke(value);
    }
}
