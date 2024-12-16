# 테스트 목록

## SingletonTests.cs

### Instance_ReturnsSameInstance

- 싱글톤 인스턴스가 항상 동일한 인스턴스를 반환하는지 확인합니다.

### Instance_ThrowsExceptionAfterDispose

- Dispose 후 인스턴스에 접근 시 `ObjectDisposedException`이 발생하는지 확인합니다.

### IncrementValue_IncrementsValue

- `IncrementValue` 메서드가 값을 올바르게 증가시키는지 확인합니다.

### ChangeState_ChangesState

- `ChangeState` 메서드가 상태를 올바르게 변경하는지 확인합니다.

### ThrowException_ThrowsException

- `ThrowException` 메서드가 예외를 올바르게 발생시키는지 확인합니다.

### ThrowIfDisposed_ThrowsExceptionAfterDispose

- Dispose 후 메서드 호출 시 `ObjectDisposedException`이 발생하는지 확인합니다.

### Dispose_SetsIsDisposedTrue

- `Dispose` 메서드가 `IsDisposed` 속성을 true로 설정하는지 확인합니다.

### Dispose_ReleasesResources

- `Dispose` 메서드가 리소스를 해제하는지 확인합니다.

### IsInitialized_ReturnsCorrectValue

- `IsInitialized` 속성이 올바른 값을 반환하는�� 확인합니다.

### ProtectedConstructor_CalledDirectly_ThrowsException

- protected 생성자를 직접 호출 시 예외가 발생하는지 확인합니다.

## SingletonMonoBehaviourTests.cs

### Instance_ReturnsSameInstance

- `Instance` 프로퍼티가 항상 동일한 인스턴스를 반환하는지 확인합니다.

### Awake_Called

- 싱글톤 인스턴스가 생성될 때 `Awake` 메서드가 호출되는지 확인합니다.

### OnDestroy_Called

- 싱글톤 인스턴스가 파괴될 때 `OnDestroy` 메서드가 호출되는지 확인합니다.

### DontDestroyOnLoad_PersistsAcrossScenes

- `DontDestroyOnLoad` 설정된 싱글톤 인스턴스가 씬 전환 후에도 유지되는지 확인합니다.

### MultipleInstances_OnlyFirstInstancePersists

- 여러 개의 싱글톤 인스턴스가 생성될 때 첫 번째 인스턴스만 유지되는지 확인합니다.

### InheritedSingleton_WorksCorrectly

- 상속받은 싱글톤 클래스가 올바르게 동작하는지 확인합니다.

### MultithreadedAccess_ReturnsSameInstance

- 멀티 스레드 환경에서 싱글톤 인스턴스 접근이 안전하게 이루어지는지 확인합니다.

### SceneTransition_PersistsCorrectly

- 씬 전환 후에도 싱글톤 인스턴스가 유지되고 `Awake` 메서드가 호출되는지 확인합니다.

### ExceptionHandling_NoExceptionsOnAccess

- 싱글톤 인스턴스 접근 시 예외가 발생하지 않는지 확인합니다.

### FindInactive_FindsInactiveInstance

- `FindInactive` 설정이 활성화되었을 때 비활성화된 싱글톤 인스턴스를 찾을 수 있는지 확인합니다.

### Persist_False_CreatesNewInstanceAfterSceneTransition

- `Persist` 속성이 false일 때 씬 전환 후 새로운 인스턴스가 생성되는지 확인합니다.

### Persist_ChangedAfterSceneTransition_AffectsSingletonBehavior

- 씬 전환 후 `Persist` 값을 변경하고, 해당 변경 사항이 싱글톤 동작에 영향을 미치는지 확인합니다.

### Persist_ChangedBeforeEnsureInitialized_AffectsSingletonBehavior

- `Persist` 속성을 변경한 후 `EnsureInitialized`를 호출했을 때 싱글톤 인스턴스의 동작이 예상대로 이루어지는지 확인합니다.

### FindInactive_ActiveAndInactiveInstances_ReturnsCorrectInstance

- 활성화된 싱글톤 오브젝트와 비활성화된 싱글톤 오브젝트가 동시에 존재할 때 `FindInactive` 속성 값에 따라 어떤 인스턴스를 반환하는지 확인합니다.

### FindInactive_MultipleInactiveInstances_ReturnsFirstFound

- 비활성화된 싱글톤 오브젝트가 여러 개 있을 때 `FindInactive` 속성이 true일 때 어떤 인스턴스를 반환하는지 확인합니다.

### EnsureInitialized_CalledMultipleTimes_KeepsExistingInstance

- `EnsureInitialized` 메서드를 여러 번 호출했을 때 싱글톤 인스턴스가 중복 생성되지 않고, 기존 인스턴스를 유지하는지 확인합니다.

### EnsureInitialized_IsInitializedValue

- `EnsureInitialized` 메서드 호출 전후 `IsInitialized` 값 테스트

### ExceptionHandling_ExceptionDuringCreation

- 싱글톤 인스턴스 생성 중 예외 발생 테스트

### ExceptionHandling_ExceptionInAwake

- `Awake` 메서드에서 예외 발생 테스트

### MultithreadedAccess_MultipleThreads

- 더 많은 스레드에서 동시 접근 테스트

### MultipleSingletons_InitializationOrder

- 여러 싱글톤 간 초기화 순서 테스트

### OnSingletonAwake_CustomInitialization

- `OnSingletonAwake` 메서드에서 커스텀 초기화가 실행되는지 테스트합니다.

### Performance_InstanceAccess

- 싱글톤 인스턴스 접근 성능을 테스트합니다.

## SingletonThreadTests.cs

### Instance_UnderThreadStress

- 여러 스레드 환경에서 싱글톤 인스턴스의 스레드 안전성을 검증합니다.

### SingletonPattern_ThreadSafetyVerification

- 멀티스레드 환경에서 싱글톤 패턴의 스레드 안전성을 검증합니다.

### SingletonPattern_RecursiveAccess

- 재귀적 접근 시에도 싱글톤 인스턴스가 동일한지 검증합니다.

### SingletonPattern_CrossThreadRecursion

- 여러 스레드에서 재귀적으로 싱글톤 인스턴스에 접근할 때 스레드 안전성을 검증합니다.

### SingletonPattern_DeadlockPrevention

- 교착 상태 발생 가능성이 있는 상황에서 싱글톤 패턴이 안전하게 동작하는지 검증합니다.

### SingletonPattern_ConcurrentDisposalAndAccess

- 싱글톤 인스턴스의 동시 Dispose 및 접근 시나리오를 검증합니다.

### SingletonPattern_AsyncInitialization

- 비동기 초기화 시 싱글톤 인스턴스가 올바르게 생성되는지 검증합니다.

### Instance_CalledFromMultipleThreadsAfterDispose_ThrowsException

- Dispose된 후 싱글톤 인스턴스에 접근 시 예외가 발생하는지 검증합니다.
