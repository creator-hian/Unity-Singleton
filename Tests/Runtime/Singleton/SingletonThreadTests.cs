using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Hian.Singleton;

namespace Singleton
{
    /// <summary>
    /// 스레드 스트레스 테스트에 사용되는 다양한 부하 수준을 정의합니다.
    /// </summary>
    public enum StressLevel
    {
        /// <summary>
        /// 낮은 부하 수준
        /// </summary>
        Low,
        /// <summary>
        /// 중간 부하 수준
        /// </summary>
        Medium,
        /// <summary>
        /// 높은 부하 수준
        /// </summary>
        High,
        /// <summary>
        /// 극심한 부하 수준
        /// </summary>
        Extreme
    }

    /// <summary>
    /// 스레드 스트레스 테스트를 위한 데이터 컨테이너 클래스입니다.
    /// </summary>
    public class ThreadStressTestData
    {
        /// <summary>
        /// 테스트에 사용될 태스크의 총 개수입니다.
        /// </summary>
        public int TaskCount { get; }
        /// <summary>
        /// 각 태스크에서 수행할 작업의 횟수입니다.
        /// </summary>
        public int OperationsPerTask { get; }
        /// <summary>
        /// 테스트의 시간 제한(초)입니다.
        /// </summary>
        public int TimeoutSeconds { get; }
        /// <summary>
        /// 컨텍스트 스위칭을 사용할지 여부입니다.
        /// </summary>
        public bool UseContextSwitching { get; }
        /// <summary>
        /// 동기화를 사용할지 여부입니다.
        /// </summary>
        public bool UseSynchronization { get; }
        /// <summary>
        /// 카오스 엔지니어링을 사용할지 여부입니다.
        /// </summary>
        public bool UseChaos { get; }
        /// <summary>
        /// 메모리 압박을 사용할지 여부입니다.
        /// </summary>
        public bool UseMemoryPressure { get; }
        /// <summary>
        /// 네트워크 지연을 사용할지 여부입니다.
        /// </summary>
        public bool UseNetworkLatency { get; }
        /// <summary>
        /// 비정상 종료 시뮬레이션을 사용할지 여부입니다.
        /// </summary>
        public bool UseCrashSimulation { get; }
        /// <summary>
        /// 메모리 블록 크기입니다.
        /// </summary>
        public int MemoryBlockSize { get; }
        /// <summary>
        /// 네트워크 지연 시간 배열입니다.
        /// </summary>
        public int[] NetworkDelays { get; }
        /// <summary>
        /// 비정상 종료 확률입니다.
        /// </summary>
        public int CrashProbability { get; }
        /// <summary>
        /// 테스트의 부하 수준입니다.
        /// </summary>
        public StressLevel Level { get; }
        /// <summary>
        /// 테스트 이름입니다.
        /// </summary>
        public string TestName { get; }

        /// <summary>
        /// ThreadStressTestData 클래스의 생성자입니다.
        /// </summary>
        /// <param name="taskCount">테스트에 사용될 태스크의 총 개수</param>
        /// <param name="operationsPerTask">각 태스크에서 수행할 작업의 횟수</param>
        /// <param name="timeoutSeconds">테스트의 시간 제한(초)</param>
        /// <param name="useContextSwitching">컨텍스트 스위칭 사용 여부</param>
        /// <param name="useSynchronization">동기화 사용 여부</param>
        /// <param name="useChaos">카오스 엔지니어링 사용 여부</param>
        /// <param name="useMemoryPressure">메모리 압박 사용 여부</param>
        /// <param name="useNetworkLatency">네트워크 지연 사용 여부</param>
        /// <param name="useCrashSimulation">비정상 종료 시뮬레이션 사용 여부</param>
        /// <param name="memoryBlockSize">메모리 블록 크기</param>
        /// <param name="networkDelays">네트워크 지연 시간 배열</param>
        /// <param name="crashProbability">비정상 종료 확률</param>
        /// <param name="level">테스트의 부하 수준</param>
        /// <param name="testName">테스트 이름</param>
        public ThreadStressTestData(
            int taskCount,
            int operationsPerTask,
            int timeoutSeconds,
            bool useContextSwitching,
            bool useSynchronization,
            bool useChaos,
            bool useMemoryPressure,
            bool useNetworkLatency,
            bool useCrashSimulation,
            int memoryBlockSize,
            int[] networkDelays,
            int crashProbability,
            StressLevel level,
            string testName)
        {
            TaskCount = taskCount;
            OperationsPerTask = operationsPerTask;
            TimeoutSeconds = timeoutSeconds;
            UseContextSwitching = useContextSwitching;
            UseSynchronization = useSynchronization;
            UseChaos = useChaos;
            UseMemoryPressure = useMemoryPressure;
            UseNetworkLatency = useNetworkLatency;
            UseCrashSimulation = useCrashSimulation;
            MemoryBlockSize = memoryBlockSize;
            NetworkDelays = networkDelays;
            CrashProbability = crashProbability;
            Level = level;
            TestName = testName;
        }

        /// <summary>
        /// 테스트 데이터의 문자열 표현을 반환합니다.
        /// </summary>
        /// <returns>테스트 데이터의 문자열 표현</returns>
        public override string ToString() => $"[{Level}] {TestName}";
    }

    /// <summary>
    /// 싱글톤 패턴의 스레드 안전성을 검증하는 테스트 클래스입니다.
    /// </summary>
    [TestFixture]
    public class SingletonThreadTests
    {
        private static readonly Random _random = new Random();
        private static readonly object _randomLock = new object();

        /// <summary>
        /// 주어진 최대값 미만의 임의의 정수를 반환합니다.
        /// </summary>
        /// <param name="maxValue">최대값</param>
        /// <returns>임의의 정수</returns>
        private static int GetRandomNumber(int maxValue)
        {
            lock (_randomLock)
            {
                return _random.Next(maxValue);
            }
        }

        /// <summary>
        /// 테스트에 사용될 싱글톤 클래스입니다.
        /// </summary>
        private class TestSingleton : Singleton<TestSingleton>, IDisposable
        {
            private int _counter;
            /// <summary>
            /// 싱글톤 인스턴스의 카운터 값입니다.
            /// </summary>
            public int Counter => _counter;
            private readonly object _lock = new object();

            private TestSingleton() { }

            /// <summary>
            /// 카운터를 증가시키는 메서드입니다.
            /// </summary>
            public void Increment()
            {
                ThrowIfDisposed();
                lock (_lock)
                {
                    _counter++;
                    Thread.Sleep(1);
                }
            }

            /// <summary>
            /// 비동기적으로 싱글톤 인스턴스를 가져오는 메서드입니다.
            /// </summary>
            /// <returns>싱글톤 인스턴스</returns>
            public static async Task<TestSingleton> GetInstanceAsync()
            {
                // 비동기 초기화 시뮬레이션
                await Task.Delay(10);
                return Instance;
            }

            /// <summary>
            /// 싱글톤 인스턴스를 정리하는 메서드입니다.
            /// </summary>
            /// <param name="disposing">정리 여부</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // 필요한 정리 작업 수행
                }
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// 스레드 스트레스 테스트 케이스를 제공합니다.
        /// </summary>
        /// <returns>스레드 스트레스 테스트 데이터</returns>
        private static IEnumerable<ThreadStressTestData> ThreadStressTestCases()
        {
            // 저부하 테스트 (각 시나리오별 동일 부하)
            yield return new ThreadStressTestData(
                taskCount: 10,
                operationsPerTask: 100,
                timeoutSeconds: 30,
                useContextSwitching: true,
                useSynchronization: false,
                useChaos: false,
                useMemoryPressure: false,
                useNetworkLatency: false,
                useCrashSimulation: false,
                memoryBlockSize: 0,
                networkDelays: new[] { 50 },
                crashProbability: 0,
                StressLevel.Low,
                "컨텍스트 스위칭 테스트");

            yield return new ThreadStressTestData(
                taskCount: 10,
                operationsPerTask: 100,
                timeoutSeconds: 30,
                useContextSwitching: false,
                useSynchronization: true,
                useChaos: false,
                useMemoryPressure: false,
                useNetworkLatency: false,
                useCrashSimulation: false,
                memoryBlockSize: 0,
                networkDelays: new[] { 50 },
                crashProbability: 0,
                StressLevel.Low,
                "동기화 테스트");

            yield return new ThreadStressTestData(
                taskCount: 10,
                operationsPerTask: 100,
                timeoutSeconds: 30,
                useContextSwitching: false,
                useSynchronization: false,
                useChaos: true,
                useMemoryPressure: false,
                useNetworkLatency: false,
                useCrashSimulation: false,
                memoryBlockSize: 0,
                networkDelays: new int[] { 50 },
                crashProbability: 0,
                StressLevel.Low,
                "카오스 테스트");

            // TODO: 복합테스트의 경우 모든 태스크가 종료되어도 테스트가 종료되지 않는 문제가 있음.
            // // 중부하 테스트
            // yield return new ThreadStressTestData(
            //     taskCount: 10,
            //     operationsPerTask: 10,
            //     timeoutSeconds: 60,
            //     useContextSwitching: true,
            //     useSynchronization: true,
            //     useChaos: false,
            //     useMemoryPressure: true,
            //     useNetworkLatency: false,
            //     useCrashSimulation: false,
            //     memoryBlockSize: 50,
            //     networkDelays: new[] { 50, 100 },
            //     crashProbability: 0,
            //     StressLevel.Low,
            //     "복합 스레드 테스트");
            // // 중부하 테스트
            // yield return new ThreadStressTestData(
            //     taskCount: 100,
            //     operationsPerTask: 100,
            //     timeoutSeconds: 60,
            //     useContextSwitching: true,
            //     useSynchronization: true,
            //     useChaos: false,
            //     useMemoryPressure: true,
            //     useNetworkLatency: false,
            //     useCrashSimulation: false,
            //     memoryBlockSize: 50,
            //     networkDelays: new[] { 50, 100 },
            //     crashProbability: 0,
            //     StressLevel.Medium,
            //     "복합 스레드 테스트");

            // // 고부하 테스트
            // yield return new ThreadStressTestData(
            //     taskCount: 1000,
            //     operationsPerTask: 100,
            //     timeoutSeconds: 300,
            //     useContextSwitching: true,
            //     useSynchronization: true,
            //     useChaos: true,
            //     useMemoryPressure: true,
            //     useNetworkLatency: true,
            //     useCrashSimulation: false,
            //     memoryBlockSize: 100,
            //     networkDelays: new[] { 50, 100, 200 },
            //     crashProbability: 0,
            //     StressLevel.High,
            //     "복합 스레드 테스트");

            // // 극한 부하 테스트
            // yield return new ThreadStressTestData(
            //     taskCount: 5000,
            //     operationsPerTask: 100,
            //     timeoutSeconds: 600,
            //     useContextSwitching: true,
            //     useSynchronization: true,
            //     useChaos: true,
            //     useMemoryPressure: true,
            //     useNetworkLatency: true,
            //     useCrashSimulation: true,
            //     memoryBlockSize: 200,
            //     networkDelays: new[] { 100, 200, 500 },
            //     crashProbability: 1,
            //     StressLevel.Extreme,
            //     "복합 스레드 테스트");

            // 복합 스트레스 테스트 추가
            yield return new ThreadStressTestData(
                taskCount: 10,
                operationsPerTask: 100,
                timeoutSeconds: 300,
                useContextSwitching: true,
                useSynchronization: true,
                useChaos: true,
                useMemoryPressure: true,
                useNetworkLatency: false,
                useCrashSimulation: false,
                memoryBlockSize: 100,
                networkDelays: new[] { 50, 100, 200 },
                crashProbability: 0,
                StressLevel.High,
                "메모리 압박 테스트");

            yield return new ThreadStressTestData(
                taskCount: 10,
                operationsPerTask: 100,
                timeoutSeconds: 300,
                useContextSwitching: true,
                useSynchronization: true,
                useChaos: true,
                useMemoryPressure: false,
                useNetworkLatency: true,
                useCrashSimulation: false,
                memoryBlockSize: 0,
                networkDelays: new[] { 100, 200, 500, 1000 },
                crashProbability: 0,
                StressLevel.High,
                "네트워크 지연 테스트");

            yield return new ThreadStressTestData(
                taskCount: 10,
                operationsPerTask: 100,
                timeoutSeconds: 300,
                useContextSwitching: true,
                useSynchronization: true,
                useChaos: true,
                useMemoryPressure: false,
                useNetworkLatency: false,
                useCrashSimulation: true,
                memoryBlockSize: 0,
                networkDelays: new[] { 50 },
                crashProbability: 5,
                StressLevel.High,
                "비정상 종료 테스트");

            // TODO: 복합테스트의 경우 모든 태스크가 종료되어도 테스트가 종료되지 않는 문제가 있음.
            // // 극한 복합 테스트
            // yield return new ThreadStressTestData(
            //     taskCount: 1000,
            //     operationsPerTask: 100,
            //     timeoutSeconds: 600,
            //     useContextSwitching: true,
            //     useSynchronization: true,
            //     useChaos: true,
            //     useMemoryPressure: true,
            //     useNetworkLatency: true,
            //     useCrashSimulation: true,
            //     memoryBlockSize: 200,
            //     networkDelays: new[] { 100, 200, 500 },
            //     crashProbability: 2,
            //     StressLevel.Extreme,
            //     "극한 복합 스트레스 테스트");
        }

        /// <summary>
        /// 여러 스레드 환경에서 싱글톤 인스턴스의 스레드 안전성을 검증합니다.
        /// </summary>
        /// <param name="testData">스레드 스트레스 테스트 데이터</param>
        [Test]
        [TestCaseSource(nameof(ThreadStressTestCases))]
        [Timeout(600000)]
        public async Task Instance_UnderThreadStress(ThreadStressTestData testData)
        {
            TestContext.WriteLine($"테스트 시작: {testData.TestName}");
            TestContext.WriteLine($"부하 수준: {testData.Level}, 태스크 수: {testData.TaskCount}, 작업 수: {testData.OperationsPerTask}");

            // Arrange
            var tasks = new List<Task>();
            var exceptions = new ConcurrentBag<Exception>();
            var instances = new ConcurrentBag<TestSingleton>();
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(testData.TimeoutSeconds));
            var instance = TestSingleton.Instance;
            var counter = 0;
            var counterLock = new object();
            var startEvent = new ManualResetEventSlim(false);
            var startTime = DateTime.Now;

            try
            {
                // 작업 스레드들 생성
                for (int i = 0; i < testData.TaskCount; i++)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            startEvent.Wait(cts.Token);

                            for (int j = 0; j < testData.OperationsPerTask && !cts.Token.IsCancellationRequested; j++)
                            {
                                if (testData.UseNetworkLatency)
                                {
                                    await Task.Delay(testData.NetworkDelays[GetRandomNumber(testData.NetworkDelays.Length)], cts.Token);
                                }

                                var localInstance = TestSingleton.Instance;
                                instances.Add(localInstance);
                                localInstance.Increment();

                                lock (counterLock)
                                {
                                    counter++;
                                }

                                if (testData.UseContextSwitching && j % 10 == 0)
                                {
                                    await Task.Yield();
                                }
                            }
                        }
                        catch (Exception ex) when (!(ex is OperationCanceledException))
                        {
                            exceptions.Add(ex);
                        }
                    }, cts.Token));
                }

                // 메모리 압박 태스크 (필요한 경우)
                if (testData.UseMemoryPressure)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var memoryBlocks = new List<byte[]>();
                        try
                        {
                            while (!cts.Token.IsCancellationRequested)
                            {
                                memoryBlocks.Add(new byte[1024 * 1024]); // 1MB
                                await Task.Delay(100, cts.Token);
                                if (memoryBlocks.Count > 10)
                                {
                                    memoryBlocks.Clear();
                                    GC.Collect();
                                }
                            }
                        }
                        catch (OperationCanceledException) { }
                        finally
                        {
                            memoryBlocks.Clear();
                            GC.Collect();
                        }
                    }, cts.Token));
                }

                // 모든 작업 시작
                startEvent.Set();

                // 작업 완료 대기
                await Task.WhenAll(tasks);
            }
            catch (OperationCanceledException)
            {
                TestContext.WriteLine("작업이 시간 제한으로 인해 취소되었습니다.");
            }
            finally
            {
                var duration = DateTime.Now - startTime;
                TestContext.WriteLine($"테스트 소요 시간: {duration.TotalSeconds:F2}초");
                TestContext.WriteLine($"최종 카운터 값: {counter}");

                cts.Dispose();
                startEvent.Dispose();
            }

            // Assert
            Assert.That(exceptions, Is.Empty, "예기치 않은 예외가 발생하지 않아야 합니다");
            Assert.That(instances.Distinct().Count(), Is.EqualTo(1), "모든 작업이 동일한 인스턴스를 참조해야 합니다");
            Assert.That(instance.Counter, Is.EqualTo(counter), "모든 증가 연산이 정확하게 수행되어야 합니다");
        }

        /// <summary>
        /// 각 테스트 실행 전에 싱글톤 인스턴스를 재설정합니다.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            // 싱글톤 인스턴스 재설정 로직
            var field = typeof(Singleton<TestSingleton>)
                .GetField("_lazy", BindingFlags.NonPublic | BindingFlags.Static);
            field?.SetValue(null, new Lazy<TestSingleton>(() =>
            {
                var instance = Activator.CreateInstance(typeof(TestSingleton), true) as TestSingleton;
                if (instance == null)
                {
                    throw new InvalidOperationException($"Failed to create instance of {typeof(TestSingleton)}");
                }
                return instance;
            }, LazyThreadSafetyMode.ExecutionAndPublication));
        }

        /// <summary>
        /// 멀티스레드 환경에서 싱글톤 패턴의 스레드 안전성을 검증합니다.
        /// </summary>
        [Test]
        [Order(1)]
        public void SingletonPattern_ThreadSafetyVerification()
        {
            // Arrange
            const int threadCount = 100;
            var instances = new ConcurrentBag<TestSingleton>();
            var exceptions = new ConcurrentBag<Exception>();
            var countdown = new CountdownEvent(threadCount);
            var random = new Random();

            // Act
            for (int i = 0; i < threadCount; i++)
            {
                new Thread(() =>
                {
                    try
                    {
                        Thread.Sleep(random.Next(0, 10)); // 경쟁 상태 유도
                        instances.Add(TestSingleton.Instance);
                        TestSingleton.Instance.Increment();
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions) exceptions.Add(ex);
                    }
                    finally
                    {
                        countdown.Signal();
                    }
                }).Start();
            }

            countdown.Wait();

            // Assert
            Assert.That(exceptions, Is.Empty, "멀티스레드 환경에서 예외가 발생하지 않아야 합니다");
            Assert.That(instances.Distinct().Count(), Is.EqualTo(1), "모든 스레드가 동일한 인스턴스를 참조해야 합니다");
            Assert.That(TestSingleton.Instance.Counter, Is.EqualTo(threadCount), "모든 증가 연산이 안전하게 수행되어야 합니다");
        }

        /// <summary>
        /// 재귀적 접근 시에도 싱글톤 인스턴스가 동일한지 검증합니다.
        /// </summary>
        [Test]
        [Order(2)]
        public void SingletonPattern_RecursiveAccess()
        {
            // Arrange
            const int recursionDepth = 100;
            var instances = new HashSet<TestSingleton>();

            void RecursiveGet(int depth)
            {
                if (depth <= 0) return;
                instances.Add(TestSingleton.Instance);
                RecursiveGet(depth - 1);
            }

            // Act
            RecursiveGet(recursionDepth);

            // Assert
            Assert.That(instances.Count, Is.EqualTo(1), "재귀적 접근에서도 동일한 인스턴스를 반환해야 합니다");
        }

        /// <summary>
        /// 여러 스레드에서 재귀적으로 싱글톤 인스턴스에 접근할 때 스레드 안전성을 검증합니다.
        /// </summary>
        [Test]
        [Order(3)]
        public void SingletonPattern_CrossThreadRecursion()
        {
            // Arrange
            const int depth = 50;
            const int threadCount = 10;
            var exceptions = new ConcurrentBag<Exception>();
            var instances = new ConcurrentBag<TestSingleton>();
            var startEvent = new ManualResetEventSlim(false);
            var completedThreads = 0;

            void RecursiveAccess(int currentDepth)
            {
                if (currentDepth <= 0) return;
                instances.Add(TestSingleton.Instance);
                RecursiveAccess(currentDepth - 1);
            }

            // Act
            var tasks = Enumerable.Range(0, threadCount).Select(_ => Task.Run(() =>
            {
                try
                {
                    // 모든 스레드가 동시에 시작하도록 대기
                    startEvent.Wait();
                    RecursiveAccess(depth);
                    Interlocked.Increment(ref completedThreads);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            })).ToArray();

            // 모든 스레드 시작
            startEvent.Set();

            // 시간 제한 설정
            var completed = Task.WaitAll(tasks, TimeSpan.FromSeconds(30));

            // Assert
            Assert.That(completed, Is.True, "모든 스레드가 시간 내에 완료되어야 합니다");
            Assert.That(exceptions, Is.Empty, "재귀적 접근 중 예외가 발생하지 않아야 합니다");
            Assert.That(instances.Distinct().Count(), Is.EqualTo(1), "모든 재귀 호출에서 동일한 인스턴스를 반환해야 합니다");
            Assert.That(completedThreads, Is.EqualTo(threadCount), "모든 스레드가 완료되어야 합니다");
        }

        /// <summary>
        /// 교착 상태 발생 가능성이 있는 상황에서 싱글톤 패턴이 안전하게 동작하는지 검증합니다.
        /// </summary>
        [Test]
        [Order(4)]
        public void SingletonPattern_DeadlockPrevention()
        {
            // Arrange
            const int threadCount = 10;
            var countdown = new CountdownEvent(threadCount);
            var tasks = new List<Task>();
            var exceptions = new ConcurrentBag<Exception>();
            var lockObject1 = new object();
            var lockObject2 = new object();

            // Act
            for (int i = 0; i < threadCount; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        countdown.Signal();
                        countdown.Wait();

                        // 교착 상태 시나리오 1: 중첩된 락
                        if (i % 2 == 0)
                        {
                            lock (lockObject1)
                            {
                                Thread.Sleep(10); // 교착 상태 유도
                                lock (lockObject2)
                                {
                                    var instance = TestSingleton.Instance;
                                    instance.Increment();
                                }
                            }
                        }
                        else
                        {
                            lock (lockObject2)
                            {
                                Thread.Sleep(10); // 교착 상태 유도
                                lock (lockObject1)
                                {
                                    var instance = TestSingleton.Instance;
                                    instance.Increment();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }));
            }

            // 시간 제한 설정 (더 현실적인 시간으로 조정)
            var completedInTime = Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(10));

            // Assert
            Assert.That(completedInTime, Is.True, "데드락 없이 모든 작업이 완료되어야 합니다");
            Assert.That(exceptions, Is.Empty, "작업 수행 중 예외가 발생하지 않아야 합니다");
            Assert.That(TestSingleton.Instance.Counter, Is.EqualTo(threadCount),
                "모든 증가 연산이 정확하게 수행되어야 합니다");
        }

        /// <summary>
        /// 싱글톤 인스턴스의 동시 Dispose 및 접근 시나리오를 검증합니다.
        /// </summary>
        [Test]
        [Order(5)]
        public void SingletonPattern_ConcurrentDisposalAndAccess()
        {
            // Arrange
            const int threadCount = 100;
            var tasks = new List<Task>();
            var exceptions = new ConcurrentBag<Exception>();
            var successfulAccesses = 0;
            var successfulDisposals = 0;
            var instance = TestSingleton.Instance;
            var syncPoint = new CountdownEvent(threadCount);

            // Act
            for (int i = 0; i < threadCount; i++)
            {
                var index = i;
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        syncPoint.Signal();
                        syncPoint.Wait(); // 모든 스레드가 동시에 시작하도록 동기화

                        if (index % 2 == 0)
                        {
                            try
                            {
                                instance.Dispose();
                                Interlocked.Increment(ref successfulDisposals);
                            }
                            catch (ObjectDisposedException)
                            {
                                // 이미 다른 스레드가 Dispose한 경우 예상되는 예외
                            }
                        }
                        else
                        {
                            try
                            {
                                var temp = TestSingleton.Instance;
                                temp.Increment();
                                Interlocked.Increment(ref successfulAccesses);
                            }
                            catch (ObjectDisposedException)
                            {
                                // Dispose 이후 접근 시도 시 예상되는 예외
                            }
                        }
                    }
                    catch (Exception ex) when (!(ex is ObjectDisposedException))
                    {
                        exceptions.Add(ex);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            // Assert
            Assert.That(exceptions, Is.Empty, "예상치 못한 예외가 발생하지 않아야 합니다");
            Assert.That(successfulDisposals, Is.GreaterThan(0), "최소 하나의 Dispose가 성공해야 합니다");
            Assert.That(successfulAccesses, Is.GreaterThan(0), "Dispose 전에 몇 개의 접근은 성공해야 합니다");
        }

        /// <summary>
        /// 비동기 초기화 시 싱글톤 인스턴스가 올바르게 생성되는지 검증합니다.
        /// </summary>
        [Test]
        [Order(6)]
        public async Task SingletonPattern_AsyncInitialization()
        {
            // Arrange
            const int threadCount = 50;
            var instances = new ConcurrentBag<TestSingleton>();
            var tasks = new List<Task>();

            // Act
            for (int i = 0; i < threadCount; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var instance = await TestSingleton.GetInstanceAsync();
                    instances.Add(instance);
                }));
            }

            await Task.WhenAll(tasks);

            // Assert
            Assert.That(instances.Distinct().Count(), Is.EqualTo(1), "모든 스레드가 동일한 인스턴스를 받아야 합니다");
        }

        /// <summary>
        /// Dispose된 후 싱글톤 인스턴스에 접근 시 예외가 발생하는지 검증합니다.
        /// </summary>
        [Test]
        [Order(7)]
        public void Instance_CalledFromMultipleThreadsAfterDispose_ThrowsException()
        {
            // Arrange
            const int threadCount = 10;
            var tasks = new List<Task>();
            var exceptions = new ConcurrentBag<Exception>();
            var instance = TestSingleton.Instance;
            instance.Dispose(); // Dispose 먼저 호출

            // Act
            for (int i = 0; i < threadCount; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var temp = TestSingleton.Instance; // Dispose 후 Instance 호출
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            // Assert
            Assert.That(exceptions, Is.Not.Empty, "Dispose 후 Instance 호출 시 예외가 발생해야 합니다");
            Assert.That(exceptions, Has.All.TypeOf<ObjectDisposedException>(),
                "Dispose 후 Instance 호출 시 ObjectDisposedException이 발생해야 합니다");
        }
    }
}