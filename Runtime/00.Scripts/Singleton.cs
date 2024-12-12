using System;
using System.Threading;
using System.Reflection;
using System.Diagnostics;

namespace Hian.Singleton
{
    /// <summary>
    /// 향상된 thread-safe 싱글턴 패턴 구현
    /// </summary>
    /// <typeparam name="T">싱글턴으로 구현할 클래스 타입</typeparam>
    public abstract partial class Singleton<T> where T : Singleton<T>, IDisposable
    {
        private static Lazy<T> _lazy = new Lazy<T>(() =>
        {
            try
            {
                var instance = Activator.CreateInstance(typeof(T), true) as T;
                if (instance == null)
                {
                    var errorMessage = $"Failed to create instance of {typeof(T)}. Activator.CreateInstance returned null.";
                    Debug.WriteLine($"[Singleton Error] {errorMessage}");
                    throw new InvalidOperationException(errorMessage);
                }
                return instance;
            }
            catch (TargetInvocationException ex)
            {
                var innerEx = ex.InnerException ?? ex;
                var errorMessage = $"Failed to initialize singleton of type {typeof(T)}. Inner Exception: {innerEx.Message}";
                Debug.WriteLine($"[Singleton Error] {errorMessage}\n{innerEx.StackTrace}");
                throw new InvalidOperationException(errorMessage, innerEx);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Failed to initialize singleton of type {typeof(T)}. Exception: {ex.Message}";
                Debug.WriteLine($"[Singleton Error] {errorMessage}\n{ex.StackTrace}");
                throw new InvalidOperationException(errorMessage, ex);
            }
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        private bool _disposed;
        private static bool _isInitializing;
        private static int _isInitialized;

        protected Singleton()
        {
            if (!_isInitializing)
            {
                // 리플렉션으로 생성자를 호출했는지 확인
                if (Assembly.GetCallingAssembly() != Assembly.GetExecutingAssembly())
                {
                    throw new InvalidOperationException($"Cannot create instance of singleton {typeof(T)} directly. Use Instance property instead.");
                }
            }
        }

        public static T Instance
        {
            get
            {
                if (_lazy.IsValueCreated && _lazy.Value._disposed)
                {
                    throw new ObjectDisposedException(typeof(T).FullName, "The singleton instance has been disposed.");
                }
                try
                {
                    _isInitializing = true;
                    var instance = _lazy.Value;
                    _isInitialized = 1;

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
                Interlocked.Exchange(ref _isInitialized, 0);
            }
            _disposed = true;
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        ~Singleton()
        {
            Dispose(false);
        }

        /// <summary>
        /// 싱글턴 인스턴스가 초기화되었는지 여부를 반환합니다. (주로 디버깅 및 테스트 용도로 사용)
        /// </summary>
        public static bool IsInitialized => _isInitialized != 0;

        // Dispose 여부를 확인하는 속성 추가
        public static bool IsInstanceDisposed => _lazy.IsValueCreated && _lazy.Value._disposed;
    }
}