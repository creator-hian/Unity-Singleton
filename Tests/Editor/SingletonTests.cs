using System;
using System.Threading;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Hian.Singleton;
using System.Collections.Generic;
using System.Threading.Tasks;

[TestFixture]
public class SingletonTests
{
    private const string ExpectedState = "InitialState";


    private class TestSingleton : Singleton<TestSingleton>, IDisposable
    {
        private int _value;
        public bool IsDisposed { get; private set; }
        public int Value => _value;

        private readonly object _lock = new object();

        private static bool _isInitialized;
        public static bool ForceExceptionInConstructor { get; set; } = false;

        public string State { get; private set; } = "InitialState";

        private readonly bool _isFeatureEnabled;

        private TestSingleton()
        {
            if (ForceExceptionInConstructor)
            {
                throw new InvalidOperationException("Constructor exception forced.");
            }

            _isInitialized = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                IsDisposed = true;
            }
            base.Dispose(disposing);
        }

        public void IncrementValue()
        {
            ThrowIfDisposed();
            lock (_lock)
            {
                _value++;
            }
        }

        public void MethodThatThrowsException()
        {
            throw new InvalidOperationException("의도된 예외 발생");
        }

        public bool IsFeatureEnabled()
        {
            return _isFeatureEnabled;
        }
    }

    [SetUp]
    public void Setup()
    {
        TestSingleton.ForceExceptionInConstructor = false;

        // 싱글톤 초기화
        var lazyField = typeof(Singleton<TestSingleton>)
            .GetField("_lazy", BindingFlags.NonPublic | BindingFlags.Static);
        var isCreatingInstanceField = typeof(Singleton<TestSingleton>)
            .GetField("_isCreatingInstance", BindingFlags.NonPublic | BindingFlags.Static);

        lazyField?.SetValue(null, new Lazy<TestSingleton>(() =>
        {
            // 인스턴스 생성 플래그 설정
            isCreatingInstanceField?.SetValue(null, true);

            var instance = Activator.CreateInstance(typeof(TestSingleton), true) as TestSingleton;

            // 인스턴스 생성 플래그 해제
            isCreatingInstanceField?.SetValue(null, false);

            if (instance == null)
            {
                throw new InvalidOperationException($"Failed to create instance of {typeof(TestSingleton)}");
            }
            return instance;
        }, LazyThreadSafetyMode.ExecutionAndPublication));
    }

    [Test]
    [Order(1)]
    public void SingletonPattern_EnsuresSingleInstance()
    {
        // Arrange & Act
        var instance1 = TestSingleton.Instance;
        instance1.IncrementValue();
        var instance2 = TestSingleton.Instance;
        instance2.IncrementValue();

        // Assert
        Assert.That(instance2, Is.SameAs(instance1), "싱글톤은 항상 같은 인스턴스를 반환해야 합니다");
        Assert.That(instance1.Value, Is.EqualTo(2), "두 참조가 같은 인스턴스를 가리키므로 값이 공유되어야 합니다");
    }

    [Test]
    [Order(2)]
    public void SingletonPattern_PreventsDirectInstantiation()
    {
        Assert.Throws<MissingMethodException>(() =>
        {
            var instance = Activator.CreateInstance<TestSingleton>();
        }, "싱글톤은 직접 인스턴스화할 수 없어야 합니다");
    }

    [Test]
    [Order(3)]
    public void SingletonPattern_ReflectionProtection()
    {
        var constructor = typeof(TestSingleton).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new Type[0],
            null);

        var exception = Assert.Throws<TargetInvocationException>(() =>
        {
            var instance = constructor.Invoke(new object[0]);
        }, "리플렉션을 통한 인스턴스 생성은 실패해야 합니다");

        Assert.That(exception.InnerException, Is.TypeOf<InvalidOperationException>());
    }

    [Test]
    [Order(4)]
    public void DisposedInstance_PreventsFurtherOperations()
    {
        var instance = TestSingleton.Instance;
        instance.Dispose();

        Assert.That(instance.IsDisposed, Is.True);
        Assert.Throws<ObjectDisposedException>(() => instance.IncrementValue());
    }

    [Test]
    [Order(5)]
    public void SingletonPattern_LazyInitialization()
    {
        // Act & Assert
        Assert.That(TestSingleton.IsInitialized, Is.False, "인스턴스 접근 전에는 초기화되지 않아야 합니다");
        var instance = TestSingleton.Instance;
        Assert.That(TestSingleton.IsInitialized, Is.True, "인스턴스 접근 후에는 초기화되어야 합니다");
    }

    [Test]
    [Order(6)]
    public void SingletonPattern_InitializationExceptionHandling()
    {
        // Arrange
        TestSingleton.ForceExceptionInConstructor = true;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            var instance = TestSingleton.Instance;
        }, "초기화 중 예외가 발생해야 합니다");
    }


    [Test]
    [Order(7)]
    public void SingletonPattern_ExceptionSafety()
    {
        // Arrange
        var instance = TestSingleton.Instance;

        // Act
        Assert.Throws<InvalidOperationException>(() =>
        {
            instance.MethodThatThrowsException();
        });

        // Assert
        Assert.That(instance.State, Is.EqualTo(ExpectedState), "예외 발생 후에도 상태가 일관적이어야 합니다");
    }

    [Test]
    [Order(8)]
    public void SingletonPattern_PreventsRecreationAfterDisposal()
    {
        // Arrange
        var instance = TestSingleton.Instance;
        instance.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() =>
        {
            var newInstance = TestSingleton.Instance;
        }, "Dispose된 후에 새로운 인스턴스를 생성할 수 없어야 합니다");
    }


    [TearDown]
    public void TearDown()
    {
        // TearDown에서 강제 예외 플래그 초기화
        TestSingleton.ForceExceptionInConstructor = false;

        try
        {
            if (TestSingleton.IsInitialized && TestSingleton.Instance != null)
            {
                TestSingleton.Instance.Dispose();
            }
        }
        catch (ObjectDisposedException)
        {
            // 이미 Dispose된 경우 무시
        }
    }
}