# Unity-Singleton

Unity에서 사용할 수 있는 Singleton 패턴 구현을 제공하는 패키지입니다.

## 요구사항

- Unity 2021.3 이상
- .NET Standard 2.1

## 개요

이 패키지는 Unity 프로젝트에서 Singleton 패턴을 쉽게 구현할 수 있도록 도와주는 기능을 제공합니다. MonoBehaviour를 상속받는 클래스에서 싱글톤 패턴을 적용하거나, 일반 C# 클래스에서 싱글톤을 구현할 때 사용할 수 있습니다.

## 주요 기능

### 일반 C# 클래스용 Singleton 구현 (`Singleton<T>`)

- 스레드 안전한 싱글톤 인스턴스 생성
- Lazy initialization 지원

#### `IDisposable` 인터페이스를 통한 리소스 관리

- 싱글톤 인스턴스가 더 이상 필요하지 않을 때 리소스를 해제할 수 있습니다.

```csharp
public class MySingleton : Singleton<MySingleton>
{
    private UnityEngine.Object _resource;

            public MySingleton()
            {
                _resource = Resources.Load("MyResource");
            }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Resources.UnloadAsset(_resource);
            _resource = null;
        }
        base.Dispose(disposing);
    }
}

// 사용 예시
var instance = MySingleton.Instance;
// ... 싱글톤 사용 ...
instance.Dispose(); // 리소스 해제
```

<!-- markdownlint-disable MD033 -->
<p style="background-color:#fff3cd; border: 1px solid #ffeeba; color: #856404; padding: 10px; margin: 10px 0;">
    ⚠️ <strong>주의!</strong> <code>Dispose</code> 메서드는 싱글톤 인스턴스가 더 이상 필요하지 않을 때 리소스를 해제하기 위해 사용됩니다. <code>Dispose</code>를 호출한 후에는 싱글톤 인스턴스를 다시 사용하려는 시도를 하지 않도록 주의해야 합니다. 예를 들어, 게임 종료 시 또는 특정 모듈이 더 이상 필요하지 않을 때 <code>Dispose</code>를 호출할 수 있습니다.
</p>
<!-- markdownlint-enable MD033 -->

<!-- markdownlint-disable MD033 -->
<p style="background-color:#f8d7da; border: 1px solid #f5c6cb; color: #721c24; padding: 10px; margin: 10px 0;">
    ⚠️ <strong>매우 중요!</strong> <code>Dispose</code> 메서드를 호출한 후에는 <strong>해당 싱글톤 인스턴스를 다시 사용할 수 없습니다.</strong> <code>Instance</code> 속성에 접근하면 <strong><code>ObjectDisposedException</code>이 발생합니다.</strong>
</p>
<!-- markdownlint-enable MD033 -->

#### 리플렉션을 이용한 생성자 호출 방지

- 싱글톤 클래스의 생성자를 직접 호출하는 것을 방지하여 싱글톤 패턴을 강제합니다.

#### 싱글톤 인스턴스 초기화 여부 확인 기능 (`IsInitialized`)

- 싱글톤 인스턴스가 초기화되었는지 여부를 확인할 수 있습니다.

```csharp
if (MySingleton.IsInitialized)
{
    // 싱글톤 인스턴스가 초기화된 경우
    var instance = MySingleton.Instance;
}
else
{
    // 싱글톤 인스턴스가 초기화되지 않은 경우
}
```

#### 싱글톤 인스턴스 Dispose 여부 확인 기능 (`IsInstanceDisposed`)

- 싱글톤 인스턴스가 Dispose되었는지 여부를 확인할 수 있습니다.

```csharp
var instance = MySingleton.Instance;
instance.Dispose();
if (MySingleton.IsInstanceDisposed)
{
    // 싱글톤 인스턴스가 Dispose된 경우
}
```

<!-- markdownlint-disable MD033 -->
<p style="background-color:#f8d7da; border: 1px solid #f5c6cb; color: #721c24; padding: 10px; margin: 10px 0;">
    ⚠️ <strong>매우 중요!</strong> <code>Dispose</code> 메서드를 호출한 후에는 <strong>해당 싱글톤 인스턴스를 다시 사용할 수 없습니다.</strong> <code>Instance</code> 속성에 접근하면 <strong><code>ObjectDisposedException</code>이 발생합니다.</strong>
</p>
<!-- markdownlint-enable MD033 -->

### MonoBehaviour 기반의 Singleton 구현 (`SingletonMonoBehaviour<T>`)

- 스레드 안전한 싱글톤 인스턴스 생성
- 씬 전환 시에도 유지되는 싱글톤 (DontDestroyOnLoad 지원)
- 비활성화된 게임 오브젝트에서 인스턴스 검색 가능
- 명시적 초기화 지원 (`EnsureInitialized()`)
- 중복 인스턴스 방지
- 사용자 정의 초기화 로직 추가 가능 (`OnSingletonAwake()`)

```csharp
public class MyMonoSingleton : SingletonMonoBehaviour<MyMonoSingleton>
{
    public bool IsInitialized { get; private set; }
    public bool Persist { get; set; } = true;

    protected override void OnSingletonAwake()
    {
        base.OnSingletonAwake();
        IsInitialized = true;
        Persist = true;
    }
}

// 사용 예시
var instance = MyMonoSingleton.Instance;
// ... 싱글톤 사용 ...
```

- Thread-safe Singleton 구현
- Lazy initialization 지원

### ScriptableObject 기반의 Singleton 구현 (`SingletonScriptableObject<T>`)

- 스레드 안전한 싱글톤 인스턴스 생성
- Lazy initialization 지원
- Resources 폴더 기반 자동 에셋 로드
- 에디터 환경에서 자동 에셋 생성 지원
- 경로 검증 및 안전한 에셋 관리

#### 사용 예시

```csharp
[CreateAssetMenu(fileName = "GameConfig", menuName = "Config/GameConfig")]
public class GameConfig : SingletonScriptableObject<GameConfig>
{
protected class ConfigPath : AssetPath
{
protected override string ResourcesLoadPath => "Config/GameConfig";
}
// 정적 생성자에서 경로 설정
static GameConfig()
{
SetPath(new ConfigPath());
}
public float gameSpeed = 1.0f;
public bool isSoundEnabled = true;
}
// 사용 방법
void Start()
{
float speed = GameConfig.Instance.gameSpeed;
bool soundEnabled = GameConfig.Instance.isSoundEnabled;
}
```
<!-- markdownlint-disable MD033 -->
<p style="background-color:#fff3cd; border: 1px solid #ffeeba; color: #856404; padding: 10px; margin: 10px 0;">
    ⚠️ <strong>주의!</strong> ScriptableObject 싱글톤을 사용하기 위해서는 반드시 Resources 폴더 내에 해당 에셋이 존재해야 합니다. 에디터 모드에서는 자동으로 생성되지만, 빌드 시에는 수동으로 에셋을 생성하고 올바른 경로에 위치시켜야 합니다.
</p>
<!-- markdownlint-enable MD033 -->

## 설치 방법

### UPM을 통한 설치 (Git URL 사용)

#### 선행 조건

- Git 클라이언트(최소 버전 2.14.0)가 설치되어 있어야 합니다.
- Windows 사용자의 경우 `PATH` 시스템 환경 변수에 Git 실행 파일 경로가 추가되어 있어야 합니다.

#### 설치 방법 1: Package Manager UI 사용

1. Unity 에디터에서 Window > Package Manager를 엽니다.
2. 좌측 상단의 + 버튼을 클릭하고 "Add package from git URL"을 선택합니다.

   ![Package Manager Add Git URL](https://i.imgur.com/1tCNo66.png)
3. 다음 URL을 입력합니다:

```text
https://github.com/creator-hian/Unity-Singleton.git
```

4. 'Add' 버튼을 클릭합니다.

   ![Package Manager Add Button](https://i.imgur.com/yIiD4tT.png)

#### 설치 방법 2: manifest.json 직접 수정

1. Unity 프로젝트의 `Packages/manifest.json` 파일을 열어 다음과 같이 dependencies 블록에 패키지를 추가하세요:

```json
{
  "dependencies": {
    "com.creator-hian.unity.singleton": "https://github.com/creator-hian/Unity-Singleton.git",
    ...
  }
}
```

#### 특정 버전 설치

특정 버전을 설치하려면 URL 끝에 #{version} 을 추가하세요:

```json
{
  "dependencies": {
    "com.creator-hian.unity.singleton": "https://github.com/creator-hian/Unity-Singleton.git#1.0.0",
    ...
  }
}
```

#### 참조 문서

- [Unity 공식 매뉴얼 - Git URL을 통한 패키지 설치](https://docs.unity3d.com/kr/2023.2/Manual/upm-ui-giturl.html)

## 문서

각 기능에 대한 자세한 설명은 해당 기능의 README를 참조하세요:

## 원작성자

- [Hian](https://github.com/creator-hian)

## 기여자

## 라이센스

[라이센스 정보 추가 필요]
