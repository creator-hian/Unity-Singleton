using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hian.Singleton;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SingletonMonoBehaviour
{
    /// <summary>
    /// 빈 씬에서 SingletonMonoBehaviour의 동작을 테스트하는 클래스입니다.
    /// </summary>
    public class SingletonEmptySceneTests
    {
        private const string TestSceneName = "Singleton.Empty";
        private string _originalSceneName;

        /// <summary>
        /// 각 테스트 시작 전 실행되는 설정 메서드입니다.
        /// 테스트 씬을 로드하고 활성화하며, 기존의 모든 게임 오브젝트를 삭제합니다.
        /// </summary>
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // 현재 씬 이름 저장
            _originalSceneName = SceneManager.GetActiveScene().name;

            // 테스트 씬 로드
            yield return SceneManager.LoadSceneAsync(TestSceneName, LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(TestSceneName));

            // 테스트 시작 전에 모든 게임 오브젝트 삭제
            foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                Object.Destroy(go);
            }
            
            // Persist 속성 기본값으로 설정
            TestSingletonMonoBehaviour.Persist = true;
            InheritedTestSingletonMonoBehaviour.Persist = true;
            yield return null;
        }

        /// <summary>
        /// 각 테스트 종료 후 실행되는 정리 메서드입니다.
        /// 테스트 씬을 언로드하고 원래 씬을 다시 로드합니다.
        /// </summary>
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // 테스트 씬 언로드
            yield return SceneManager.UnloadSceneAsync(TestSceneName);

            // 원래 씬으로 복귀
            yield return SceneManager.LoadSceneAsync(_originalSceneName);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(_originalSceneName));
        }

        /// <summary>
        /// Instance 프로퍼티가 항상 동일한 인스턴스를 반환하는지 테스트합니다.
        /// </summary>
        [UnityTest]
        public IEnumerator Instance_ReturnsSameInstance()
        {
            // Arrange
            var instance1 = TestSingletonMonoBehaviour.Instance;
            yield return null; // Awake 실행 대기

            // Act
            var instance2 = TestSingletonMonoBehaviour.Instance;

            // Assert
            Assert.That(instance1, Is.EqualTo(instance2), "Instance는 항상 동일한 인스턴스를 반환해야 합니다");
        }

        /// <summary>
        /// 싱글톤 인스턴스가 생성될 때 Awake 메서드가 호출되는지 테스트합니다.
        /// </summary>
        [UnityTest]
        public IEnumerator Awake_Called()
        {
            // Arrange
            var instance = TestSingletonMonoBehaviour.Instance;
            yield return null; // Awake 실행 대기

            // Act & Assert
            Assert.That(instance.AwakeCalled, Is.True, "Awake가 호출되어야 합니다");
        }

        /// <summary>
        /// 싱글톤 인스턴스가 파괴될 때 OnDestroy 메서드가 호출되는지 테스트합니다.
        /// </summary>
        [UnityTest]
        public IEnumerator OnDestroy_Called()
        {
            // Arrange
            var instance = TestSingletonMonoBehaviour.Instance;
            yield return null; // Awake 실행 대기

            // Act
            Object.Destroy(instance.gameObject);
            yield return null; // OnDestroy 실행 대기

            // Assert
            Assert.That(instance.OnDestroyCalled, Is.True, "OnDestroy가 호출되어야 합니다");
        }

        /// <summary>
        /// DontDestroyOnLoad 설정된 싱글톤 인스턴스가 씬 전환 후에도 유지되는지 테스트합니다.
        /// </summary>
        [UnityTest]
        public IEnumerator DontDestroyOnLoad_PersistsAcrossScenes()
        {
            // Arrange
            var instance = TestSingletonMonoBehaviour.Instance;
            yield return null; // Awake 실행 대기

            // Act
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 씬 다시 로드
            yield return null; // 씬 로드 대기

            // Assert
            Assert.That(TestSingletonMonoBehaviour.Instance, Is.EqualTo(instance), "DontDestroyOnLoad로 설정된 인스턴스는 씬 전환 후에도 유지되어야 합니다");
        }

        /// <summary>
        /// 여러 개의 싱글톤 인스턴스가 생성될 때 첫 번째 인스턴스만 유지되는지 테스트합니다.
        /// </summary>
        [UnityTest]
        public IEnumerator MultipleInstances_OnlyFirstInstancePersists()
        {
            // Arrange
            var instance1 = new GameObject().AddComponent<TestSingletonMonoBehaviour>();
            yield return null; // Awake 실행 대기
            var instance2 = new GameObject().AddComponent<TestSingletonMonoBehaviour>();
            yield return null; // Awake 실행 대기

            // Assert
            Assert.That(TestSingletonMonoBehaviour.Instance, Is.EqualTo(instance1), "첫 번째 인스턴스만 유지되어야 합니다");
            Assert.That(instance2, Is.Null, "두 번째 인스턴스는 삭제되어야 합니다");
        }

        /// <summary>
        /// 상속받은 싱글톤 클래스가 올바르게 동작하는지 테스트합니다.
        /// </summary>
        [UnityTest]
        public IEnumerator InheritedSingleton_WorksCorrectly()
        {
            // Arrange
            var instance = InheritedTestSingletonMonoBehaviour.Instance;
            yield return null; // Awake 실행 대기

            // Assert
            Assert.That(instance, Is.Not.Null, "상속받은 싱글턴 인스턴스가 생성되어야 합니다");
            Assert.That(instance.AwakeCalled, Is.True, "상속받은 싱글턴의 Awake가 호출되어야 합니다");
        }

        /// <summary>
        /// 멀티 스레드 환경에서 싱글톤 인스턴스 접근이 안전하게 이루어지는지 테스트합니다.
        /// </summary>
        [UnityTest]
        public IEnumerator MultithreadedAccess_ReturnsSameInstance()
        {
            // Arrange
            var instance1 = TestSingletonMonoBehaviour.Instance;
            yield return null; // Awake 실행 대기

            TestSingletonMonoBehaviour instance2 = null;
            TestSingletonMonoBehaviour instance3 = null;
            int thread1Value = 0;
            int thread2Value = 0;

            // Act
            var task1 = Task.Run(() => { 
                instance2 = TestSingletonMonoBehaviour.Instance;
                instance2.TestValue = 10;
                thread1Value = instance2.TestValue;
            });
            var task2 = Task.Run(() => { 
                instance3 = TestSingletonMonoBehaviour.Instance;
                thread2Value = instance3.TestValue;
            });

            Task.WaitAll(task1, task2);

            // Assert
            Assert.That(instance2, Is.EqualTo(instance1), "멀티 스레드 환경에서 Instance는 동일한 인스턴스를 반환해야 합니다.");
            Assert.That(instance3, Is.EqualTo(instance1), "멀티 스레드 환경에서 Instance는 동일한 인스턴스를 반환해야 합니다.");
            Assert.That(thread1Value, Is.EqualTo(10), "스레드 1에서 변경된 값이 반영되어야 합니다.");
            Assert.That(thread2Value, Is.EqualTo(10), "스레드 2에서 변경된 값이 반영되어야 합니다.");
        }

        /// <summary>
        /// 씬 전환 후에도 싱글톤 인스턴스가 유지되고 Awake 메서드가 호출되는지 테스트합니다.
        /// </summary>
        [UnityTest]
        public IEnumerator SceneTransition_PersistsCorrectly()
        {
            // Arrange
            var instance1 = TestSingletonMonoBehaviour.Instance;
            yield return null; // Awake 실행 대기

            // Act
            var loadSceneAsync = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name, LoadSceneMode.Additive);
            while (!loadSceneAsync.isDone)
            {
                yield return null;
            }
            var newScene = SceneManager.GetSceneByName(SceneManager.GetActiveScene().name);
            SceneManager.SetActiveScene(newScene);
            yield return null; // 씬 로드 대기

            var instance2 = TestSingletonMonoBehaviour.Instance;
            yield return null; // Awake 실행 대기

            // Assert
            Assert.That(instance2, Is.EqualTo(instance1), "씬 전환 후에도 싱글톤 인스턴스는 유지되어야 합니다.");
            Assert.That(instance2.AwakeCalled, Is.True, "씬 전환 후에도 Awake가 호출되어야 합니다.");

            // Cleanup
            yield return SceneManager.UnloadSceneAsync(newScene);
        }

        /// <summary>
        /// 싱글톤 인스턴스 접근 시 예외가 발생하지 않는지 테스트합니다.
        /// </summary>
        [UnityTest]
        public IEnumerator ExceptionHandling_NoExceptionsOnAccess()
        {
            // Arrange
            // Act & Assert (예외가 발생하지 않아야 함)
            Assert.DoesNotThrow(() =>
            {
                var instance = TestSingletonMonoBehaviour.Instance;
                // ReSharper disable once UnusedVariable
                var _ = instance.gameObject; // null 참조 예외 방지
            }, "싱글톤 인스턴스 접근 시 예외가 발생하면 안 됩니다.");
            yield return null; // Awake 실행 대기
        }

        /// <summary>
        /// FindInactive 설정이 활성화되었을 때 비활성화된 싱글톤 인스턴스를 찾을 수 있는지 테스트합니다.
        /// </summary>
        [UnityTest]
        public IEnumerator FindInactive_FindsInactiveInstance()
        {
            // Arrange
            var go = new GameObject();
            var instance = go.AddComponent<TestSingletonMonoBehaviour>();
            go.SetActive(false);
            yield return null; // Awake 실행 대기

            // Act
            SingletonMonoBehaviour<TestSingletonMonoBehaviour>.FindInactive = true;
            var foundInstance = TestSingletonMonoBehaviour.Instance;

            // Assert
            Assert.That(foundInstance, Is.EqualTo(instance), "비활성화된 인스턴스를 찾아야 합니다.");

            // Cleanup
            Object.Destroy(go);
            yield return null;
        }

        /// <summary>
        /// Persist 속성이 false일 때 씬 전환 후 새로운 인스턴스가 생성되는지 테스트합니다.
        /// </summary>
        [UnityTest]
        public IEnumerator Persist_False_CreatesNewInstanceAfterSceneTransition()
        {
            // Arrange
            TestSingletonMonoBehaviour.Persist = false;
            var instance1 = TestSingletonMonoBehaviour.Instance;
            yield return null; // Awake 실행 대기

            // Act
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            yield return null; // 씬 로드 대기

            var instance2 = TestSingletonMonoBehaviour.Instance;
            yield return null; // Awake 실행 대기

            // Assert
            Assert.That(instance2, Is.Not.EqualTo(instance1), "Persist가 false일 때 씬 전환 후 새로운 인스턴스가 생성되어야 합니다.");
        }

        /// <summary>
        /// 씬 전환 후 Persist 값을 변경하고, 해당 변경 사항이 싱글톤 동작에 영향을 미치는지 테스트합니다.
        /// </summary>
        [UnityTest]
        public IEnumerator Persist_ChangedAfterSceneTransition_AffectsSingletonBehavior()
        {
            // Arrange
            TestSingletonMonoBehaviour.Persist = false; // 최초 인스턴스 생성 시 Persist를 false로 설정
            var instance1 = TestSingletonMonoBehaviour.Instance;
            yield return null; // Awake 실행 대기

            // Act
            var loadSceneAsync = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
            while (!loadSceneAsync.isDone)
            {
                yield return null;
            }
            yield return null; // 씬 로드 대기

            TestSingletonMonoBehaviour.Persist = true; // 씬 전환 후 Persist를 true로 변경
            TestSingletonMonoBehaviour.EnsureInitialized();
            var instance2 = TestSingletonMonoBehaviour.Instance;
            yield return null; // Awake 실행 대기

            // Assert
            Assert.That(instance2, Is.Not.EqualTo(instance1), "Persist 값이 변경되었으므로 씬 전환 후 새로운 인스턴스가 생성되어야 합니다.");
        }

        /// <summary>
        /// Persist 속성을 변경한 후 EnsureInitialized를 호출했을 때 싱글톤 인스턴스의 동작이 예상대로 이루어지는지 확인합니다.
        /// </summary>
        [UnityTest]
        public IEnumerator Persist_ChangedBeforeEnsureInitialized_AffectsSingletonBehavior()
        {
            // Arrange
            TestSingletonMonoBehaviour.Persist = true;
            var instance1 = TestSingletonMonoBehaviour.Instance;
            yield return null; // Awake 실행 대기

            // Act
            TestSingletonMonoBehaviour.Persist = false;
            TestSingletonMonoBehaviour.EnsureInitialized();
            var instance2 = TestSingletonMonoBehaviour.Instance;
            yield return null; // Awake 실행 대기

            // Assert
            Assert.That(instance2, Is.EqualTo(instance1), "Persist 값이 변경되었지만 EnsureInitialized 호출 후에도 기존 인스턴스가 유지되어야 합니다.");
        }

        /// <summary>
        /// 활성화된 싱글톤 오브젝트와 비활성화된 싱글톤 오브젝트가 동시에 존재할 때 FindInactive 속성 값에 따라 어떤 인스턴스를 반환하는지 확인합니다.
        /// </summary>
        [UnityTest]
        public IEnumerator FindInactive_ActiveAndInactiveInstances_ReturnsCorrectInstance()
        {
            // Arrange
            var go1 = new GameObject();
            var instance1 = go1.AddComponent<TestSingletonMonoBehaviour>();

            var go2 = new GameObject();
            var instance2 = go2.AddComponent<TestSingletonMonoBehaviour>();
            go2.SetActive(false);
            yield return null; // Awake 실행 대기

            // Act & Assert
            // FindInactive가 true일 때 비활성화된 인스턴스를 찾아야 합니다.
            SingletonMonoBehaviour<TestSingletonMonoBehaviour>.FindInactive = true;
            var foundInstance1 = TestSingletonMonoBehaviour.Instance;
            Assert.That(foundInstance1, Is.EqualTo(instance1), "FindInactive가 true일 때 활성화된 인스턴스를 찾아야 합니다.");

            // FindInactive가 false일 때 활성화된 인스턴스를 찾아야 합니다.
            SingletonMonoBehaviour<TestSingletonMonoBehaviour>.FindInactive = false;
            var foundInstance2 = TestSingletonMonoBehaviour.Instance;
            Assert.That(foundInstance2, Is.EqualTo(instance1), "FindInactive가 false일 때 활성화된 인스턴스를 찾아야 합니다.");

            // Cleanup
            Object.Destroy(go1);
            Object.Destroy(go2);
            yield return null;
        }

        /// <summary>
        /// 비활성화된 싱글톤 오브젝트가 여러 개 있을 때 FindInactive 속성이 true일 때 어떤 인스턴스를 반환하는지 확인합니다.
        /// </summary>
        [UnityTest]
        public IEnumerator FindInactive_MultipleInactiveInstances_ReturnsFirstFound()
        {
            // Arrange
            var go1 = new GameObject();
            var instance1 = go1.AddComponent<TestSingletonMonoBehaviour>();
            go1.SetActive(false);

            var go2 = new GameObject();
            var instance2 = go2.AddComponent<TestSingletonMonoBehaviour>();
            go2.SetActive(false);
            yield return null; // Awake 실행 대기

            // Act & Assert
            // FindInactive가 true일 때 첫 번째 비활성화된 인스턴스를 찾아야 합니다.
            SingletonMonoBehaviour<TestSingletonMonoBehaviour>.FindInactive = true;
            var foundInstance = TestSingletonMonoBehaviour.Instance;
            Assert.That(foundInstance, Is.EqualTo(instance1), "비활성화된 인스턴스 중 첫 번째 인스턴스를 찾아야 합니다.");

            // Cleanup
            Object.Destroy(go1);
            Object.Destroy(go2);
            yield return null;
        }

        /// <summary>
        /// EnsureInitialized 메서드를 여러 번 호출했을 때 싱글톤 인스턴스가 중복 생성되지 않고, 기존 인스턴스를 유지하는지 확인합니다.
        /// </summary>
        [UnityTest]
        public IEnumerator EnsureInitialized_CalledMultipleTimes_KeepsExistingInstance()
        {
            // Arrange
            var instance1 = TestSingletonMonoBehaviour.Instance;
            yield return null; // Awake 실행 대기

            // Act
            TestSingletonMonoBehaviour.EnsureInitialized();
            TestSingletonMonoBehaviour.EnsureInitialized();
            var instance2 = TestSingletonMonoBehaviour.Instance;
            yield return null; // Awake 실행 대기

            // Assert
            Assert.That(instance2, Is.EqualTo(instance1), "EnsureInitialized를 여러 번 호출해도 기존 인스턴스를 유지해야 합니다.");
        }

        /// <summary>
        /// EnsureInitialized 메서드 호출 전후 IsInitialized 값 테스트
        /// </summary>
        [UnityTest]
        public IEnumerator EnsureInitialized_IsInitializedValue()
        {
            // Arrange
            Assert.That(TestSingletonMonoBehaviour.IsInitialized, Is.False, "EnsureInitialized 호출 전에는 IsInitialized가 false여야 합니다.");

            // Act
            TestSingletonMonoBehaviour.EnsureInitialized();
            yield return null; // Awake 실행 대기

            // Assert
            Assert.That(TestSingletonMonoBehaviour.IsInitialized, Is.True, "EnsureInitialized 호출 후에는 IsInitialized가 true여야 합니다.");
        }

        /// <summary>
        /// 싱글톤 인스턴스 생성 중 예외 발생 테스트
        /// </summary>
        [UnityTest]
        public IEnumerator ExceptionHandling_ExceptionDuringCreation()
        {
            TestSingletonMonoBehaviourWithException instance = null;

            // Assert
            LogAssert.Expect(LogType.Exception, "Exception: Test Exception");

            // Arrange
            instance = new GameObject().AddComponent<TestSingletonMonoBehaviourWithException>();
            var _ = instance.gameObject; // null 참조 예외 방지
            yield return null; // Awake 실행 대기
        }

        /// <summary>
        /// Awake 메서드에서 예외 발생 테스트
        /// </summary>
        [UnityTest]
        public IEnumerator ExceptionHandling_ExceptionInAwake()
        {
            // Arrange
            LogAssert.Expect(LogType.Exception, "Exception: Test Exception in Awake");

            var instance = TestSingletonMonoBehaviourWithExceptionInAwake.Instance;
            // ReSharper disable once UnusedVariable
            var _ = instance.gameObject; // null 참조 예외 방지
            yield return null; // Awake 실행 대기
        }

        /// <summary>
        /// 더 많은 스레드에서 동시 접근 테스트
        /// </summary>
        [UnityTest]
        public IEnumerator MultithreadedAccess_MultipleThreads()
        {
            // Arrange
            var instance1 = TestSingletonMonoBehaviour.Instance;
            yield return null; // Awake 실행 대기

            TestSingletonMonoBehaviour instance2 = null;
            TestSingletonMonoBehaviour instance3 = null;
            TestSingletonMonoBehaviour instance4 = null;
            TestSingletonMonoBehaviour instance5 = null;

            // Act
            var task1 = Task.Run(() => { instance2 = TestSingletonMonoBehaviour.Instance; });
            var task2 = Task.Run(() => { instance3 = TestSingletonMonoBehaviour.Instance; });
            var task3 = Task.Run(() => { instance4 = TestSingletonMonoBehaviour.Instance; });
            var task4 = Task.Run(() => { instance5 = TestSingletonMonoBehaviour.Instance; });

            Task.WaitAll(task1, task2, task3, task4);

            // Assert
            Assert.That(instance2, Is.EqualTo(instance1), "멀티 스레드 환경에서 Instance는 동일한 인스턴스를 반환해야 합니다.");
            Assert.That(instance3, Is.EqualTo(instance1), "멀티 스레드 환경에서 Instance는 동일한 인스턴스를 반환해야 합니다.");
            Assert.That(instance4, Is.EqualTo(instance1), "멀티 스레드 환경에서 Instance는 동일한 인스턴스를 반환해야 합니다.");
            Assert.That(instance5, Is.EqualTo(instance1), "멀티 스레드 환경에서 Instance는 동일한 인스턴스를 반환해야 합니다.");
        }

        /// <summary>
        /// 여러 싱글톤 간 초기화 순서 테스트
        /// </summary>
        [UnityTest]
        public IEnumerator MultipleSingletons_InitializationOrder()
        {
            // Arrange
            var instance1 = TestSingletonMonoBehaviourA.Instance;
            yield return null; // Awake 실행 대기
            var instance2 = TestSingletonMonoBehaviourB.Instance;
            yield return null; // Awake 실행 대기

            // Assert
            Assert.That(instance1, Is.Not.Null, "싱글톤 A 인스턴스가 생성되어야 합니다.");
            Assert.That(instance2, Is.Not.Null, "싱글톤 B 인스턴스가 생성되어야 합니다.");
            Assert.That(instance1.AwakeCalled, Is.True, "싱글톤 A의 Awake가 호출되어야 합니다.");
            Assert.That(instance2.AwakeCalled, Is.True, "싱글톤 B의 Awake가 호출되어야 합니다.");
        }


        /// <summary>
        /// OnSingletonAwake 메서드에서 커스텀 초기화가 실행되는지 테스트합니다.
        /// </summary>
        [UnityTest]
        public IEnumerator OnSingletonAwake_CustomInitialization()
        {
            // Arrange
            var instance = TestSingletonMonoBehaviourWithCustomAwake.Instance;
            yield return null; // Awake 실행 대기

            // Assert
            Assert.That(instance.IsCustomInitialized, Is.True, "OnSingletonAwake에서 커스텀 초기화가 실행되어야 합니다.");
        }

        /// <summary>
        /// 싱글톤 인스턴스 접근 성능을 테스트합니다.
        /// </summary>
        [Test]
        [TestCaseSource(nameof(PerformanceTestCases))]
        public void Performance_InstanceAccess(int iterations, int timeoutMilliseconds)
        {
            // Arrange
            var instance = TestSingletonMonoBehaviour.Instance;

            var stopwatch = new System.Diagnostics.Stopwatch();

            // Act
            stopwatch.Start();
            for (int i = 0; i < iterations; i++)
            {
                var _ = TestSingletonMonoBehaviour.Instance;
            }
            stopwatch.Stop();

            // Assert
            Debug.Log($"[SingletonMono] {iterations}회 싱글톤 인스턴스 접근 시간: {stopwatch.ElapsedMilliseconds}ms");
            if (stopwatch.ElapsedMilliseconds > timeoutMilliseconds)
            {
                Assert.Fail($"테스트가 {timeoutMilliseconds}ms 내에 완료되어야 합니다. 실제 소요 시간: {stopwatch.ElapsedMilliseconds}ms");
            }
        }

        private static IEnumerable<object[]> PerformanceTestCases()
        {
            yield return new object[] { 1000, 1 }; // 1000회 반복, 1ms 제한
            yield return new object[] { 10000, 1 }; // 10000회 반복, 1ms 제한
            yield return new object[] { 100000, 10 }; // 100000회 반복, 10ms 제한
            yield return new object[] { 1000000, 100 }; // 1000000회 반복, 100ms 제한
        }

        /// <summary>
        /// 싱글톤을 상속받아 구현한 테스트 클래스입니다.
        /// </summary>
        public class InheritedTestSingletonMonoBehaviour : SingletonMonoBehaviour<InheritedTestSingletonMonoBehaviour>
        {
            /// <summary>
            /// Awake 메서드 호출 여부를 저장하는 프로퍼티입니다.
            /// </summary>
            public bool AwakeCalled { get; private set; }

            /// <summary>
            /// Awake 메서드를 오버라이드하여 호출 여부를 기록합니다.
            /// </summary>
            protected override void Awake()
            {
                base.Awake();
                AwakeCalled = true;
            }
        }

        /// <summary>
        /// 싱글톤 테스트를 위한 클래스입니다.
        /// </summary>
        public class TestSingletonMonoBehaviour : SingletonMonoBehaviour<TestSingletonMonoBehaviour>
        {
            public bool AwakeCalled { get; private set; }
            public bool OnDestroyCalled { get; private set; }
            private int _testValue;
            private readonly object _testValueLock = new object();
            public int TestValue
            {
                get
                {
                    lock (_testValueLock)
                    {
                        return _testValue;
                    }
                }
                set
                {
                    lock (_testValueLock)
                    {
                        _testValue = value;
                    }
                }
            }

            protected override void Awake()
            {
                base.Awake();
                AwakeCalled = true;
            }

            protected override void OnDestroy()
            {
                base.OnDestroy();
                OnDestroyCalled = true;
            }
        }

        /// <summary>
        /// 싱글톤 테스트를 위한 클래스 (생성 시 예외 발생)
        /// </summary>
        public class TestSingletonMonoBehaviourWithException : SingletonMonoBehaviour<TestSingletonMonoBehaviourWithException>
        {
            protected override void Awake()
            {
                base.Awake();
                throw new System.Exception("Test Exception");
            }
        }

        /// <summary>
        /// 싱글톤 테스트를 위한 클래스 (Awake에서 예외 발생)
        /// </summary>
        public class TestSingletonMonoBehaviourWithExceptionInAwake : SingletonMonoBehaviour<TestSingletonMonoBehaviourWithExceptionInAwake>
        {
            public static bool AwakeCalled { get; private set; }
            protected override void Awake()
            {
                base.Awake();
                AwakeCalled = true;
                throw new System.Exception("Test Exception in Awake");
            }
        }

        /// <summary>
        /// 싱글톤 테스트를 위한 클래스 A
        /// </summary>
        public class TestSingletonMonoBehaviourA : SingletonMonoBehaviour<TestSingletonMonoBehaviourA>
        {
            public bool AwakeCalled { get; private set; }
            protected override void Awake()
            {
                base.Awake();
                AwakeCalled = true;
            }
        }

        /// <summary>
        /// 싱글톤 테스트를 위한 클래스 B
        /// </summary>
        public class TestSingletonMonoBehaviourB : SingletonMonoBehaviour<TestSingletonMonoBehaviourB>
        {
            public bool AwakeCalled { get; private set; }
            protected override void Awake()
            {
                base.Awake();
                AwakeCalled = true;
            }
        }


        /// <summary>
        /// 싱글톤 테스트를 위한 클래스 (OnSingletonAwake에서 커스텀 초기화)
        /// </summary>
        public class TestSingletonMonoBehaviourWithCustomAwake : SingletonMonoBehaviour<TestSingletonMonoBehaviourWithCustomAwake>
        {
            public bool IsCustomInitialized { get; private set; }

            protected override void OnSingletonAwake()
            {
                base.OnSingletonAwake();
                IsCustomInitialized = true;
            }
        }

    }
}
