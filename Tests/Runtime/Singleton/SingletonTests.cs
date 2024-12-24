using System;
using Hian.Singleton;
using NUnit.Framework;

namespace Singleton
{
    /// <summary>
    /// <see cref="Singleton{T}"/> 클래스에 대한 테스트를 포함하는 테스트 클래스입니다.
    /// </summary>
    [TestFixture]
    public class SingletonTests
    {
        /// <summary>
        /// 테스트에 사용될 싱글턴 클래스입니다.
        /// </summary>
        private class TestSingleton : Singleton<TestSingleton>, IDisposable
        {
            private int _value;

            /// <summary>
            /// 객체가 Dispose 되었는지 여부를 나타냅니다.
            /// </summary>
            public bool IsDisposed { get; private set; }

            /// <summary>
            /// 싱글턴 객체의 값을 가져옵니다.
            /// </summary>
            public int Value => _value;

            private readonly object _lock = new object();

#pragma warning disable CS0414
            private static bool _isInitialized;
#pragma warning restore CS0414
            /// <summary>
            /// 생성자에서 예외를 강제로 발생시킬지 여부를 설정합니다.
            /// </summary>
            public static bool ForceExceptionInConstructor { get; set; } = false;

            /// <summary>
            /// 싱글턴 객체의 상태를 나타냅니다.
            /// </summary>
            public string State { get; private set; } = "InitialState";

            /// <summary>
            /// 리소스가 해제되었는지 여부를 나타냅니다.
            /// </summary>
            public bool IsResourceReleased { get; private set; } = false;

            /// <summary>
            /// <see cref="TestSingleton"/> 클래스의 생성자입니다.
            /// </summary>
            internal TestSingleton()
            {
                if (ForceExceptionInConstructor)
                {
                    throw new InvalidOperationException("Constructor exception forced.");
                }

                _isInitialized = true;
            }

            /// <summary>
            /// 객체를 Dispose하고 리소스를 해제합니다.
            /// </summary>
            /// <param name="disposing">Dispose 중인지 여부</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    IsDisposed = true;
                    IsResourceReleased = true;
                }
                base.Dispose(disposing);
            }

            /// <summary>
            /// 싱글턴 객체의 값을 1 증가시킵니다.
            /// </summary>
            public void IncrementValue()
            {
                ThrowIfDisposed();
                lock (_lock)
                {
                    _value++;
                }
            }

            /// <summary>
            /// 리소스를 사용하는 메서드입니다.
            /// </summary>
            public void UseResource()
            {
                ThrowIfDisposed();
                // 리소스 사용 로직 (예: 파일 핸들, 네트워크 연결 등)
            }

            /// <summary>
            /// 싱글턴 객체의 상태를 변경합니다.
            /// </summary>
            /// <param name="newState">새로운 상태</param>
            public void ChangeState(string newState)
            {
                ThrowIfDisposed();
                State = newState;
            }

            /// <summary>
            /// 테스트 예외를 발생시킵니다.
            /// </summary>
            public void ThrowException()
            {
                ThrowIfDisposed();
                throw new Exception("Test Exception");
            }
        }

        /// <summary>
        /// 각 테스트 시작 전에 실행되는 설정 메서드입니다.
        /// </summary>
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

        /// <summary>
        /// 싱글턴 인스턴스가 항상 동일한 인스턴스를 반환하는지 테스트합니다.
        /// </summary>
        [Test]
        [Order(1)]
        public void Instance_ReturnsSameInstance()
        {
            // Arrange
            TestSingleton instance1 = TestSingleton.Instance;
            TestSingleton instance2 = TestSingleton.Instance;

            // Assert
            Assert.That(
                instance1,
                Is.EqualTo(instance2),
                "싱글턴은 항상 동일한 인스턴스를 반환해야 합니다"
            );
        }

        /// <summary>
        /// Dispose 후 인스턴스에 접근 시 예외가 발생하는지 테스트합니다.
        /// </summary>
        [Test]
        [Order(2)]
        public void Instance_ThrowsExceptionAfterDispose()
        {
            // Arrange
            TestSingleton instance = TestSingleton.Instance;
            instance.Dispose();

            // Act & Assert
            _ = Assert.Throws<ObjectDisposedException>(
                static () =>
                {
                    _ = TestSingleton.Instance;
                },
                "Dispose 후 인스턴스에 접근하면 ObjectDisposedException이 발생해야 합니다"
            );
        }

        /// <summary>
        /// IncrementValue 메서드가 값을 올바르게 증가시키는지 테스트합니다.
        /// </summary>
        [Test]
        [Order(3)]
        public void IncrementValue_IncrementsValue()
        {
            // Arrange
            TestSingleton instance = TestSingleton.Instance;
            int initialValue = instance.Value;

            // Act
            instance.IncrementValue();

            // Assert
            Assert.That(
                instance.Value,
                Is.EqualTo(initialValue + 1),
                "IncrementValue는 값을 1 증가시켜야 합니다"
            );
        }

        /// <summary>
        /// ChangeState 메서드가 상태를 올바르게 변경하는지 테스트합니다.
        /// </summary>
        [Test]
        [Order(4)]
        public void ChangeState_ChangesState()
        {
            // Arrange
            TestSingleton instance = TestSingleton.Instance;
            string newState = "NewState";

            // Act
            instance.ChangeState(newState);

            // Assert
            Assert.That(instance.State, Is.EqualTo(newState), "상태가 변경되어야 합니다.");
        }

        /// <summary>
        /// ThrowException 메서드가 예외를 올바르게 발생시키는지 테스트합니다.
        /// </summary>
        [Test]
        [Order(5)]
        public void ThrowException_ThrowsException()
        {
            // Arrange
            TestSingleton instance = TestSingleton.Instance;

            // Act & Assert
            _ = Assert.Throws<Exception>(
                () => instance.ThrowException(),
                "ThrowException은 예외를 발생시켜야 합니다"
            );
        }

        /// <summary>
        /// Dispose 후 메서드 호출 시 예외가 발생하는지 테스트합니다.
        /// </summary>
        [Test]
        [Order(6)]
        public void ThrowIfDisposed_ThrowsExceptionAfterDispose()
        {
            // Arrange
            TestSingleton instance = TestSingleton.Instance;
            instance.Dispose();

            // Act & Assert
            _ = Assert.Throws<ObjectDisposedException>(
                () => instance.IncrementValue(),
                "Dispose 후 메서드 호출 시 ObjectDisposedException이 발생해야 합니다"
            );
        }

        /// <summary>
        /// Dispose 메서드가 IsDisposed 속성을 true로 설정하는지 테스트합니다.
        /// </summary>
        [Test]
        [Order(7)]
        public void Dispose_SetsIsDisposedTrue()
        {
            // Arrange
            TestSingleton instance = TestSingleton.Instance;

            // Act
            instance.Dispose();

            // Assert
            Assert.That(instance.IsDisposed, Is.True, "Dispose 후 IsDisposed는 true여야 합니다");
        }

        /// <summary>
        /// Dispose 메서드가 리소스를 해제하는지 테스트합니다.
        /// </summary>
        [Test]
        [Order(8)]
        public void Dispose_ReleasesResources()
        {
            // Arrange
            TestSingleton instance = TestSingleton.Instance;
            instance.UseResource(); // 리소스 사용

            // Act
            instance.Dispose();

            // Assert
            // Dispose 호출 전에 IsResourceReleased 값을 변수에 저장
            bool isResourceReleased = instance.IsResourceReleased;
            Assert.That(isResourceReleased, Is.True, "Dispose 후 리소스가 해제되어야 합니다");
        }

        /// <summary>
        /// IsInitialized 속성이 올바른 값을 반환하는지 테스트합니다.
        /// </summary>
        [Test]
        [Order(9)]
        public void IsInitialized_ReturnsCorrectValue()
        {
            // Assert
            Assert.That(
                TestSingleton.IsInitialized,
                Is.False,
                "인스턴스 접근 전 IsInitialized는 false여야 합니다"
            );

            // Arrange
            TestSingleton instance = TestSingleton.Instance;

            // Assert
            Assert.That(
                TestSingleton.IsInitialized,
                Is.True,
                "인스턴스 접근 후 IsInitialized는 true여야 합니다"
            );

            // Act
            instance.Dispose();

            // Assert
            Assert.That(
                TestSingleton.IsInitialized,
                Is.False,
                "Dispose 후 IsInitialized는 false여야 합니다"
            );
        }

        /// <summary>
        /// protected 생성자를 직접 호출 시 예외가 발생하는지 테스트합니다.
        /// </summary>
        [Test]
        [Order(10)]
        public void ProtectedConstructor_CalledDirectly_ThrowsException()
        {
            // Act & Assert
            _ = Assert.Throws<InvalidOperationException>(
                static () => new TestSingleton(),
                "TestSingleton 내부에서 protected 생성자를 직접 호출하는 것은 예외를 발생시켜야 합니다"
            );
        }
    }
}
