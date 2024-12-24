using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace Hian.Singleton
{
    public abstract partial class Singleton<T>
    {
        // 테스트 전용 메서드 (internal)
        [Conditional("UNITY_INCLUDE_TESTS")]
        internal static void ResetInstance()
        {
            _lazy = new Lazy<T>(
                static () =>
                {
                    try
                    {
                        if (Activator.CreateInstance(typeof(T), true) is not T instance)
                        {
                            string errorMessage =
                                $"Failed to create instance of {typeof(T)}. Activator.CreateInstance returned null.";
                            Debug.WriteLine($"[Singleton Error] {errorMessage}");
                            throw new InvalidOperationException(errorMessage);
                        }
                        return instance;
                    }
                    catch (TargetInvocationException ex)
                    {
                        Exception innerEx = ex.InnerException ?? ex;
                        string errorMessage =
                            $"Failed to initialize singleton of type {typeof(T)}. Inner Exception: {innerEx.Message}";
                        Debug.WriteLine($"[Singleton Error] {errorMessage}\n{innerEx.StackTrace}");
                        throw new InvalidOperationException(errorMessage, innerEx);
                    }
                    catch (Exception ex)
                    {
                        string errorMessage =
                            $"Failed to initialize singleton of type {typeof(T)}. Exception: {ex.Message}";
                        Debug.WriteLine($"[Singleton Error] {errorMessage}\n{ex.StackTrace}");
                        throw new InvalidOperationException(errorMessage, ex);
                    }
                },
                LazyThreadSafetyMode.ExecutionAndPublication
            );

            _isInitialized = 0;
        }
    }
}
