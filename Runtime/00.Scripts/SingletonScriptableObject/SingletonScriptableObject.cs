using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 싱글톤 패턴이 적용된 ScriptableObject 추상 클래스입니다.
/// </summary>
/// <typeparam name="T">싱글톤으로 관리할 ScriptableObject 타입</typeparam>
public abstract class SingletonScriptableObject<T> : ScriptableObject
    where T : SingletonScriptableObject<T>
{
    // 멀티스레딩 환경에서 안전하게 인스턴스를 생성하기 위한 락 객체입니다.
    private static readonly object _lock = new object();

    // 인스턴스를 지연 생성(Lazy Initialization)하기 위한 Lazy<T> 객체입니다.
    private static readonly Lazy<T> _lazyInstance = new Lazy<T>(CreateInstance);

    // 애플리케이션 종료 여부를 나타내는 플래그입니다.
    private static bool _applicationIsQuitting;

    /// <summary>
    /// 에셋의 경로를 정의하는 추상 클래스입니다.
    /// 상속받는 클래스에서 반드시 구현해야 합니다.
    /// </summary>
    protected abstract class AssetPath
    {
        private const string RESOURCES_PATH = "Assets/Resources/";
        private static readonly System.Text.RegularExpressions.Regex ValidResourcePathPattern =
            new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9_/][a-zA-Z0-9_/]*$");

        private string _validatedPath;

        /// <summary>
        /// Resources 폴더 내 에셋의 상대 경로입니다.
        /// 예: "Settings/GameConfig" 또는 "GameConfig"
        /// </summary>
        protected abstract string ResourcesLoadPath { get; }

        /// <summary>
        /// Resources.Load()에서 사용할 검증된 경로입니다.
        /// </summary>
        public string LoadPath
        {
            get
            {
                if (_validatedPath == null)
                {
                    ValidateResourcePath(ResourcesLoadPath);
                    _validatedPath = ResourcesLoadPath;
                }
                return _validatedPath;
            }
        }

        /// <summary>
        /// 에디터에서 에셋을 생성할 전체 경로입니다.
        /// </summary>
        public string AssetCreatePath => $"{RESOURCES_PATH}{LoadPath}.asset";

        private void ValidateResourcePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("ResourcePath cannot be null or empty.");
            }

            if (!ValidResourcePathPattern.IsMatch(path))
            {
                throw new InvalidOperationException(
                    $"Invalid ResourcePath format: '{path}'\n"
                        + "ResourcePath must:\n"
                        + "- Contain only letters, numbers, underscores, and forward slashes\n"
                        + "- Not start with a slash\n"
                        + "- Not contain spaces or special characters"
                );
            }

            if (path.Contains("//"))
            {
                throw new InvalidOperationException(
                    $"Invalid ResourcePath: '{path}'\n"
                        + "ResourcePath cannot contain consecutive slashes"
                );
            }
        }
    }

    /// <summary>
    /// 에셋 경로 정보를 제공하는 인스턴스입니다.
    /// </summary>
    private static AssetPath _path;

    /// <summary>
    /// 에셋 경로 정보를 설정합니다.
    /// </summary>
    protected static void SetPath(AssetPath path)
    {
        _path = path;
    }

    /// <summary>
    /// 싱글톤 인스턴스를 반환합니다.
    /// </summary>
    public static T Instance => _lazyInstance.Value;

    /// <summary>
    /// 싱글톤 인스턴스가 유효한지 여부를 반환합니다.
    /// 애플리케이션이 종료 중이거나 인스턴스가 생성되지 않았으면 false를 반환합니다.
    /// </summary>
    public static bool IsValid => !_applicationIsQuitting && _lazyInstance.IsValueCreated;

    /// <summary>
    /// 싱글톤 인스턴스를 생성합니다.
    /// </summary>
    /// <returns>생성된 싱글톤 인스턴스</returns>
    private static T CreateInstance()
    {
        lock (_lock)
        {
            if (_path == null)
            {
                LogError("Asset path is not defined. Call SetPath in derived class.");
                return null;
            }

            T instance = Resources.Load<T>(_path.LoadPath);

            if (instance == null)
            {
                LogError($"Failed to load instance from path: {_path.LoadPath}");

                // 에디터 환경에서는 에셋을 생성하고, 런타임 환경에서는 에러 처리를 합니다.
#if UNITY_EDITOR
                instance = EditorHelper.CreateAssetInEditor();
#else
                HandleRuntimeAssetMissing();
#endif
            }

            return instance;
        }
    }

    /// <summary>
    /// OnDisable()은 게임 오브젝트가 비활성화되거나 삭제될 때 호출되는 Unity 이벤트 함수입니다.
    /// 이 메서드는 애플리케이션 종료 여부를 설정하는 데 사용됩니다.
    /// </summary>
    protected virtual void OnDisable()
    {
        if (Application.isPlaying)
        {
            _applicationIsQuitting = true;
        }
    }

    /// <summary>
    /// 에디터 환경에서만 호출되는 디버그 로그 메서드입니다.
    /// </summary>
    /// <param name="message">로그 메시지</param>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private static void LogDebugInfo(string message)
    {
        Debug.Log($"[{typeof(T).Name}] {message}");
    }

    /// <summary>
    /// 에러 로그 메서드입니다.
    /// </summary>
    /// <param name="message">에러 메시지</param>
    private static void LogError(string message)
    {
        Debug.LogError($"[{typeof(T).Name}] {message}");
    }

#if UNITY_EDITOR
    /// <summary>
    /// 에디터 환경에서 에셋을 생성하고 관리하는 헬퍼 클래스입니다.
    /// </summary>
    public static class EditorHelper
    {
        /// <summary>
        /// 에디터 환경에서 에셋을 생성합니다.
        /// </summary>
        /// <returns>생성된 에셋</returns>
        public static T CreateAssetInEditor()
        {
            T asset = CreateInstance<T>();
            string path = _path.AssetCreatePath;

            EnsureDirectoryExists(Path.GetDirectoryName(path));
            UnityEditor.AssetDatabase.CreateAsset(asset, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            LogDebugInfo($"Created new asset at: {path}");
            return asset;
        }

        /// <summary>
        /// 경로에 디렉토리가 없으면 생성합니다.
        /// </summary>
        /// <param name="directoryPath">디렉토리 경로</param>
        private static void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                _ = Directory.CreateDirectory(directoryPath);
            }
        }

        /// <summary>
        /// CreateAssetMenu로 에셋을 생성할 때 호출되는 메서드입니다.
        /// </summary>
        public static void CreateAsset(T asset)
        {
            if (_path == null)
            {
                LogError("Asset path is not defined. Call SetPath in derived class.");
                return;
            }

            string path = _path.AssetCreatePath;
            EnsureDirectoryExists(Path.GetDirectoryName(path));
            UnityEditor.AssetDatabase.CreateAsset(asset, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            LogDebugInfo($"Created new asset at: {path}");
        }

        public static void CreateAssetFromMenu()
        {
            if (_path == null)
            {
                LogError("Asset path is not defined. Call SetPath in derived class.");
                return;
            }

            string path = _path.AssetCreatePath;

            // 이미 존재하는지 확인
            if (File.Exists(path))
            {
                LogError($"Asset already exists at: {path}");
                UnityEditor.Selection.activeObject = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(
                    path
                );
                return;
            }

            T asset = CreateInstance<T>();
            EnsureDirectoryExists(Path.GetDirectoryName(path));
            UnityEditor.AssetDatabase.CreateAsset(asset, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            LogDebugInfo($"Created new asset at: {path}");
            UnityEditor.Selection.activeObject = asset;
        }
    }
#endif
}
