using System;
using NUnit.Framework;
using Hian.Singleton;

[TestFixture]
public class SingletonTests
{
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

        public bool IsResourceReleased { get; private set; } = false;

        internal TestSingleton()
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
                IsResourceReleased = true;
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

        public void UseResource()
        {
            ThrowIfDisposed();
            // 리소스 사용 로직 (예: 파일 핸들, 네트워크 연결 등)
        }

        public void ChangeState(string newState)
        {
            ThrowIfDisposed();
            State = newState;
        }

        public void ThrowException()
        {
            ThrowIfDisposed();
            throw new Exception("Test Exception");
        }
    }

    [SetUp]
    public void SetUp()
    {
        // 각 테스트 시작 전에 싱글턴 인스턴스 Dispose 및 재생성
        TestSingleton.ForceExceptionInConstructor = false;
        if (TestSingleton.IsInitialized)
        {
            TestSingleton.Instance.Dispose();
        }
        // 인스턴스 재생성
        Singleton<TestSingleton>.ResetInstance();
    }

    [Test]
    [Order(1)]
    public void Instance_ReturnsSameInstance()
    {
        // Arrange
        var instance1 = TestSingleton.Instance;
        var instance2 = TestSingleton.Instance;

        // Assert
        Assert.That(instance1, Is.EqualTo(instance2), "싱글턴은 항상 동일한 인스턴스를 반환해야 합니다");
    }

    [Test]
    [Order(2)]
    public void Instance_ThrowsExceptionAfterDispose()
    {
        // Arrange
        var instance = TestSingleton.Instance;
        instance.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() =>
        {
            var _ = TestSingleton.Instance;
        }, "Dispose 후 인스턴스에 접근하면 ObjectDisposedException이 발생해야 합니다");
    }

    [Test]
    [Order(3)]
    public void IncrementValue_IncrementsValue()
    {
        // Arrange
        var instance = TestSingleton.Instance;
        int initialValue = instance.Value;

        // Act
        instance.IncrementValue();

        // Assert
        Assert.That(instance.Value, Is.EqualTo(initialValue + 1), "IncrementValue는 값을 1 증가시켜야 합니다");
    }

    [Test]
    [Order(4)]
    public void ChangeState_ChangesState()
    {
        // Arrange
        var instance = TestSingleton.Instance;
        var newState = "NewState";

        // Act
        instance.ChangeState(newState);

        // Assert
        Assert.That(instance.State, Is.EqualTo(newState), "상태가 변경되어야 합니다.");
    }

    [Test]
    [Order(5)]
    public void ThrowException_ThrowsException()
    {
        // Arrange
        var instance = TestSingleton.Instance;

        // Act & Assert
        Assert.Throws<Exception>(() => instance.ThrowException(), "ThrowException은 예외를 발생시켜야 합니다");
    }

    [Test]
    [Order(6)]
    public void ThrowIfDisposed_ThrowsExceptionAfterDispose()
    {
        // Arrange
        var instance = TestSingleton.Instance;
        instance.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => instance.IncrementValue(), "Dispose 후 메서드 호출 시 ObjectDisposedException이 발생해야 합니다");
    }

    [Test]
    [Order(7)]
    public void Dispose_SetsIsDisposedTrue()
    {
        // Arrange
        var instance = TestSingleton.Instance;

        // Act
        instance.Dispose();

        // Assert
        Assert.That(instance.IsDisposed, Is.True, "Dispose 후 IsDisposed는 true여야 합니다");
    }

    [Test]
    [Order(8)]
    public void Dispose_ReleasesResources()
    {
        // Arrange
        var instance = TestSingleton.Instance;
        instance.UseResource(); // 리소스 사용

        // Act
        instance.Dispose();

        // Assert
        // Dispose 호출 전에 IsResourceReleased 값을 변수에 저장
        var isResourceReleased = instance.IsResourceReleased;
        Assert.That(isResourceReleased, Is.True, "Dispose 후 리소스가 해제되어야 합니다");
    }

    [Test]
    [Order(9)]
    public void IsInitialized_ReturnsCorrectValue()
    {
        // Assert
        Assert.That(TestSingleton.IsInitialized, Is.False, "인스턴스 접근 전 IsInitialized는 false여야 합니다");

        // Arrange
        var instance = TestSingleton.Instance;

        // Assert
        Assert.That(TestSingleton.IsInitialized, Is.True, "인스턴스 접근 후 IsInitialized는 true여야 합니다");

        // Act
        instance.Dispose();

        // Assert
        Assert.That(TestSingleton.IsInitialized, Is.False, "Dispose 후 IsInitialized는 false여야 합니다");
    }

    [Test]
    [Order(10)]
    public void ProtectedConstructor_CalledDirectly_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new TestSingleton(),
            "TestSingleton 내부에서 protected 생성자를 직접 호출하는 것은 예외를 발생시켜야 합니다");
    }
}
