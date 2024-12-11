using System;
using System.Threading;
using System.Reflection;

namespace Hian.Singleton
{
    /// <summary>
    /// 향상된 thread-safe 싱글턴 패턴 구현
    /// </summary>
    /// <typeparam name="T">싱글턴으로 구현할 클래스 타입</typeparam>
    public abstract class Singleton<T> where T : Singleton<T>, IDisposable
    {
        private static readonly Lazy<T> _lazy = new Lazy<T>(() =>
        {
            try
            {
                var instance = Activator.CreateInstance(typeof(T), true) as T;
                if (instance == null)
                {
                    throw new InvalidOperationException($"Failed to create instance of {typeof(T)}");
                }
                return instance;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize singleton of type {typeof(T)}", ex);
            }
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        private bool _disposed;
        private static bool _isInitializing;
        private static bool _isInitialized;

        protected Singleton()
        {
            if (!_isInitializing)
            {
                throw new InvalidOperationException($"Cannot create instance of singleton {typeof(T)} directly. Use Instance property instead.");
            }
        }

        public static T Instance
        {
            get
            {
                try
                {
                    _isInitializing = true;
                    var instance = _lazy.Value;
                    _isInitialized = true;
                    
                    if (instance._disposed)
                    {
                        throw new ObjectDisposedException(typeof(T).FullName);
                    }
                    return instance;
                }
                catch (TargetInvocationException ex)
                {
                    // 리플렉션 예외 언래핑
                    throw ex.InnerException ?? ex;
                }
                finally
                {
                    _isInitializing = false;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // 관리되는 리소스 정리
                _isInitialized = false;
            }
            _disposed = true;
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed || (_lazy.IsValueCreated && _lazy.Value != this))
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        ~Singleton()
        {
            Dispose(false);
        }

        public static bool IsInitialized => _isInitialized;
    }
}