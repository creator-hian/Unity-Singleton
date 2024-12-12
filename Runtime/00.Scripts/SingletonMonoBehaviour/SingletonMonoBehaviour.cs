using UnityEngine;
using Object = UnityEngine.Object;

namespace Hian.Singleton
{
    /// <summary>
    /// Unity MonoBehaviour를 위한 스레드 안전한 싱글톤 패턴을 구현합니다.
    /// </summary>
    /// <typeparam name="T">싱글톤으로 구현할 MonoBehaviour 클래스 타입</typeparam>
    [DisallowMultipleComponent]
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _isQuitting;

        /// <summary>
        /// 비활성화된 게임 오브젝트에서 인스턴스를 검색할지 여부를 나타냅니다.
        /// <para>기본값: true</para>
        /// </summary>
        public static bool FindInactive { get; set; } = true;

        /// <summary>
        /// 싱글톤 인스턴스에 접근합니다.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_isQuitting)
                {
                    Debug.LogWarning($"[SingletonMono] Instance of {typeof(T)} is null because the application is quitting.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        EnsureInitialized();
                    }
                    return _instance;
                }
            }
        }

        /// <summary>
        /// 싱글톤 인스턴스가 초기화되었는지 여부를 반환합니다.
        /// </summary>
        public static bool IsInitialized => _instance != null;

        /// <summary>
        /// 씬 전환 시에도 싱글톤 인스턴스를 유지할지 여부를 설정합니다.
        /// </summary>
        public static bool Persist { get; set; } = true;

        /// <summary>
        /// 싱글톤 인스턴스가 생성될 때 호출됩니다.
        /// </summary>
        protected virtual void OnSingletonAwake() { }

        /// <summary>
        /// MonoBehaviour의 Awake 메서드에서 호출됩니다.
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning($"[SingletonMono] Destroying duplicate instance of {typeof(T)}.");
                DestroyImmediate(this);
                return;
            }

            if (_instance == null)
            {
                _instance = this as T;
                if (Persist)
                {
                    DontDestroyOnLoad(gameObject);
                }
                OnSingletonAwake();
            }
        }

        /// <summary>
        /// MonoBehaviour의 OnDestroy 메서드에서 호출됩니다.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                OnDestroyStatic();
                _instance = null;
            }
        }

        /// <summary>
        /// 애플리케이션 종료 시 호출됩니다.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        /// <summary>
        /// 기존의 싱글톤 인스턴스를 찾습니다.
        /// <para>씬에서 타입 T의 모든 인스턴스를 검색합니다.</para>
        /// </summary>
        /// <returns>기존 인스턴스 또는 없을 경우 null을 반환합니다.</returns>
        private static T FindExistingInstance()
        {
            Object[] objects = FindInactive
                ? Resources.FindObjectsOfTypeAll(typeof(T))
                : FindObjectsByType(typeof(T), FindObjectsSortMode.None);

            if (objects is null or { Length: 0 })
            {
                return null;
            }

            return objects[0] as T;
        }

        /// <summary>
        /// 인스턴스가 파괴될 때 싱글톤 인스턴스를 초기화합니다.
        /// <para>이 메소드는 MonoBehaviour의 OnDestroy 메소드에서 호출됩니다.</para>
        /// </summary>
        private static void OnDestroyStatic()
        {
            lock (_lock)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// 싱글톤 인스턴스를 명시적으로 초기화합니다.
        /// </summary>
        public static void EnsureInitialized()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    // 씬에서 기존 인스턴스 검색
                    _instance = FindExistingInstance();

                    if (_instance == null)
                    {
                        // 새 게임 오브젝트 생성 및 컴포넌트 추가
                        var singletonObject = new GameObject(typeof(T).Name + " (Singleton)");
                        _instance = singletonObject.AddComponent<T>();
                        Debug.LogWarning($"[SingletonMono] Created new instance of {typeof(T)}.");
                    }
                    else
                    {
                        Debug.Log($"[SingletonMono] Found existing instance of {typeof(T)}.");
                    }

                    // DontDestroyOnLoad 설정
                    if (Persist)
                    {
                        DontDestroyOnLoad(_instance.gameObject);
                    }

                    _instance.OnSingletonAwake();
                }
            }
        }
    }
}
