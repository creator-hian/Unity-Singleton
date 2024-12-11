using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using FAMOZ.Singleton;

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

        // 중부하 테스트
        yield return new ThreadStressTestData(
            taskCount: 100,
            operationsPerTask: 100,
            timeoutSeconds: 60,
            useContextSwitching: true,
            useSynchronization: true,
            useChaos: false,
            useMemoryPressure: true,
            useNetworkLatency: false,
            useCrashSimulation: false,
            memoryBlockSize: 50,
            networkDelays: new[] { 50, 100 },
            crashProbability: 0,
            StressLevel.Medium,
            "복합 스레드 테스트");

        // 고부하 테스트
        yield return new ThreadStressTestData(
            taskCount: 1000,
            operationsPerTask: 100,
            timeoutSeconds: 300,
            useContextSwitching: true,
            useSynchronization: true,
            useChaos: true,
            useMemoryPressure: true,
            useNetworkLatency: true,
            useCrashSimulation: false,
            memoryBlockSize: 100,
            networkDelays: new[] { 50, 100, 200 },
            crashProbability: 0,
            StressLevel.High,
            "복합 스레드 테스트");

        // 극한 부하 테스트
        yield return new ThreadStressTestData(
            taskCount: 5000,
            operationsPerTask: 100,
            timeoutSeconds: 600,
            useContextSwitching: true,
            useSynchronization: true,
            useChaos: true,
            useMemoryPressure: true,
            useNetworkLatency: true,
            useCrashSimulation: true,
            memoryBlockSize: 200,
            networkDelays: new[] { 100, 200, 500 },
            crashProbability: 1,
            StressLevel.Extreme,
            "복합 스레드 테스트");

        // 복합 스트레스 테스트 추가
        yield return new ThreadStressTestData(
            taskCount: 100,
            operationsPerTask: 1000,
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
            taskCount: 100,
            operationsPerTask: 1000,
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
            taskCount: 100,
            operationsPerTask: 1000,
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

        // 극한 복합 테스트
        yield return new ThreadStressTestData(
            taskCount: 1000,
            operationsPerTask: 100,
            timeoutSeconds: 600,
            useContextSwitching: true,
            useSynchronization: true,
            useChaos: true,
            useMemoryPressure: true,
            useNetworkLatency: true,
            useCrashSimulation: true,
            memoryBlockSize: 200,
            networkDelays: new[] { 100, 200, 500 },
            crashProbability: 2,
            StressLevel.Extreme,
            "극한 복합 스트레스 테스트");
    }

    [Test]
    [TestCaseSource(nameof(ThreadStressTestCases))]
    [Timeout(60000)]
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
        var barrier = new Barrier(testData.TaskCount);
        var startTime = DateTime.Now;

        // 부하 수준에 따른 설정
        var contextSwitchInterval = testData.Level switch
        {
            StressLevel.Low => 100,
            StressLevel.Medium => 50,
            StressLevel.High => 20,
            StressLevel.Extreme => 10,
            _ => 100
        };

        var chaosInterval = testData.Level switch
        {
            StressLevel.Low => 1000,
            StressLevel.Medium => 500,
            StressLevel.High => 200,
            StressLevel.Extreme => 100,
            _ => 1000
        };

        // Act
        // 메모리 압박 태스크
        if (testData.UseMemoryPressure)
        {
            tasks.Add(Task.Run(async () =>
            {
                var memoryBlocks = new List<byte[]>();
                try
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        memoryBlocks.Add(new byte[testData.MemoryBlockSize * 1024 * 1024]);
                        await Task.Delay(100, cts.Token);

                        if (GetRandomNumber(100) < 20)
                        {
                            memoryBlocks.Clear();
                            GC.Collect(2, GCCollectionMode.Forced);
                        }
                    }
                }
                catch (OutOfMemoryException)
                {
                    memoryBlocks.Clear();
                    GC.Collect();
                }
            }, cts.Token));
        }

        // 작업 스레드들
        for (int i = 0; i < testData.TaskCount; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    if (testData.UseSynchronization)
                    {
                        barrier.SignalAndWait(cts.Token);
                    }

                    for (int j = 0; j < testData.OperationsPerTask && !cts.Token.IsCancellationRequested; j++)
                    {
                        // 네트워크 지연 시뮬레이션
                        if (testData.UseNetworkLatency)
                        {
                            await Task.Delay(testData.NetworkDelays[GetRandomNumber(testData.NetworkDelays.Length)], cts.Token);
                        }

                        // 비정상 종료 시뮬레이션
                        if (testData.UseCrashSimulation && GetRandomNumber(100) < testData.CrashProbability)
                        {
                            throw new Exception("의도적 오류 발생");
                        }

                        var localInstance = TestSingleton.Instance;
                        instances.Add(localInstance);
                        localInstance.Increment();

                        lock (counterLock)
                        {
                            counter++;
                        }

                        if (testData.UseContextSwitching && j % contextSwitchInterval == 0)
                        {
                            await Task.Yield();
                        }

                        if (testData.UseChaos && j % chaosInterval == 0)
                        {
                            switch (GetRandomNumber(4))
                            {
                                case 0: // GC 압박
                                    GC.Collect(2, GCCollectionMode.Forced);
                                    break;
                                case 1: // 스레드 지연
                                    await Task.Delay(GetRandomNumber(10));
                                    break;
                                case 2: // CPU 압박
                                    var temp = new byte[1024];
                                    Array.Sort(temp);
                                    break;
                                case 3: // 컨텍스트 스위칭
                                    Thread.Sleep(1);
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    exceptions.Add(ex);
                    
                    // 복구 시도
                    try
                    {
                        var recoveryInstance = TestSingleton.Instance;
                        recoveryInstance.Increment();
                    }
                    catch (Exception retryEx)
                    {
                        exceptions.Add(retryEx);
                    }
                }
            }, cts.Token));
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            TestContext.WriteLine("작업이 시간 제한으로 인해 취소되었습니다.");
        }

        var duration = DateTime.Now - startTime;
        TestContext.WriteLine($"테스트 소요 시간: {duration.TotalSeconds:F2}초");

        // Assert
        if (exceptions.Count > 0)
        {
            TestContext.WriteLine($"발생한 예외 수: {exceptions.Count}");
            foreach (var ex in exceptions.Take(5))
            {
                TestContext.WriteLine($"예외 발생: {ex}");
            }
        }

        Assert.That(exceptions, Is.Empty, 
            "스레드 스트레스 상황에서도 예외가 발생하지 않아야 합니다");
        Assert.That(instances.Distinct().Count(), Is.EqualTo(1),
            "모든 태스크가 동일한 인스턴스를 참조해야 합니다");
        Assert.That(instance.Counter, Is.EqualTo(counter),
            "모든 증가 연산이 정확하게 수행되어야 합니다");

        TestContext.WriteLine($"최종 카운터 값: {counter}");
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
        var barrier = new Barrier(threadCount);

        void RecursiveAccess(int currentDepth)
        {
            if (currentDepth <= 0) return;
            instances.Add(TestSingleton.Instance);
            barrier.SignalAndWait(); // 든 스레드가 동시에 다음 단계로 진행
            RecursiveAccess(currentDepth - 1);
        }

        // Act
        var tasks = Enumerable.Range(0, threadCount).Select(_ => Task.Run(() =>
        {
            try
            {
                RecursiveAccess(depth);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        })).ToArray();

        Task.WaitAll(tasks);

        // Assert
        Assert.That(exceptions, Is.Empty, "재귀적 접근 중 예외가 발생하지 않아야 합니다");
        Assert.That(instances.Distinct().Count(), Is.EqualTo(1), "모든 재귀 호출에서 동일한 인스턴스를 반환해야 합니다");
    }

    [Test]
    [Order(4)]
    public void SingletonPattern_DeadlockPrevention()
    {
        // Arrange
        const int threadCount = 10;
        var barrier = new Barrier(threadCount);
        var tasks = new List<Task>();
        var exceptions = new ConcurrentBag<Exception>();
        var lockObject = new object();

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    barrier.SignalAndWait(); // 모든 스레드가 동시에 시작

                    lock (lockObject)
                    {
                        var instance = TestSingleton.Instance;
                        Thread.Sleep(10); // 교착 상태 유도
                        instance.Increment();
                    }

                    // 다른 락과의 상호작용
                    Monitor.Enter(TestSingleton.Instance);
                    try
                    {
                        Thread.Sleep(10);
                        TestSingleton.Instance.Increment();
                    }
                    finally
                    {
                        Monitor.Exit(TestSingleton.Instance);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }));
        }

        // 시간 제한 설정
        var completedInTime = Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(5));

        // Assert
        Assert.That(completedInTime, Is.True, "데드락 없이 모든 작업이 완료되어야 합니다");
        Assert.That(exceptions, Is.Empty, "작업 수행 중 예외가 발생하지 않아야 합니다");
    }

    [Test]
    [Order(5)]
    public void SingletonPattern_ConcurrentDisposalAndAccess()
    {
        // Arrange
        const int threadCount = 100;
        var tasks = new List<Task>();
        var exceptions = new ConcurrentBag<Exception>();
        var instance = TestSingleton.Instance;

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    if (i % 2 == 0)
                    {
                        instance.Dispose();
                    }
                    else
                    {
                        var temp = TestSingleton.Instance;
                        temp.Increment();
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        Assert.That(exceptions, Has.All.TypeOf<ObjectDisposedException>(),
            "모든 예외는 ObjectDisposedException이어야 합니다");
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