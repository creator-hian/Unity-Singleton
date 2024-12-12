using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Hian.Singleton;

public enum StressLevel
{
    Low,
    Medium,
    High,
    Extreme
}

public class ThreadStressTestData
{
    public int TaskCount { get; }
    public int OperationsPerTask { get; }
    public int TimeoutSeconds { get; }
    public bool UseContextSwitching { get; }
    public bool UseSynchronization { get; }
    public bool UseChaos { get; }
    public bool UseMemoryPressure { get; }
    public bool UseNetworkLatency { get; }
    public bool UseCrashSimulation { get; }
    public int MemoryBlockSize { get; }
    public int[] NetworkDelays { get; }
    public int CrashProbability { get; }
    public StressLevel Level { get; }
    public string TestName { get; }

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

    public override string ToString() => $"[{Level}] {TestName}";
}

[TestFixture]
public class SingletonThreadTests
{
    private static readonly Random _random = new Random();
    private static readonly object _randomLock = new object();

    private static int GetRandomNumber(int maxValue)
    {
        lock (_randomLock)
        {
            return _random.Next(maxValue);
        }
    }

    private class TestSingleton : Singleton<TestSingleton>, IDisposable
    {
        private int _counter;
        public int Counter => _counter;
        private readonly object _lock = new object();

        private TestSingleton() { }

        public void Increment()
        {
            ThrowIfDisposed();
            lock (_lock)
            {
                _counter++;
                Thread.Sleep(1);
            }
        }

        public static async Task<TestSingleton> GetInstanceAsync()
        {
            // 비동기 초기화 시뮬레이션
            await Task.Delay(10);
            return Instance;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 필요한 정리 작업 수행
            }
            base.Dispose(disposing);
        }
    }

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
        Assert.That(instances.Distinct().Count(), Is.EqualTo(1), "모든 스드가 동일한 인스턴스를 참조해야 합니다");
        Assert.That(TestSingleton.Instance.Counter, Is.EqualTo(threadCount), "모든 증가 연산이 안전하게 수행되어야 합니다");
    }

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
}