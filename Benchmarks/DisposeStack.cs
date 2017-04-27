using System;
using System.Collections.Generic;

namespace Benchmarks
{
    public sealed class DisposeStack : IDisposable
    {
        private readonly Stack<IDisposable> _objects = new Stack<IDisposable>();

        public T Add<T>(T disposableObject) where T : IDisposable
        {
            _objects.Push(disposableObject);
            return disposableObject;
        }

        public void AddMultiple<T>(IEnumerable<T> disposableObjects) where T : IDisposable
        {
            foreach (var d in disposableObjects)
            {
                _objects.Push(d);
            }
        }

        public void Dispose()
        {
            foreach (var d in _objects)
            {
                d.Dispose();
            }
        }
    }
}
