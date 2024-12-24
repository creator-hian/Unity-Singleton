using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AddScenesToBuild
{
    [MenuItem("Tools/Add Package[Singleton] Scenes to Build")]
    public static void AddPackageScenesToBuild()
    {
        string packageRootPath = "Packages/com.creator-hian.unity.singleton"; // 패키지 루트 경로

        // 스캔할 씬 폴더 경로
        List<string> sceneFolderPaths = new List<string>()
        {
            "Tests/Runtime/SingletonMonoBehaviour",
        };

        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();

        // 기존 빌드 씬 목록 가져오기
        buildScenes.AddRange(EditorBuildSettings.scenes);

        // sceneFolderPaths의 경로에 대하여 packageRootPath를 더해서 경로를 만들어야 함
        for (int i = 0; i < sceneFolderPaths.Count; i++)
        {
            sceneFolderPaths[i] = Path.Combine(packageRootPath, sceneFolderPaths[i]);
        }

        // 지정된 폴더에서 모든 씬 파일 검색
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", sceneFolderPaths.ToArray());
        foreach (string guid in sceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);

            // 이미 빌드 씬에 추가되어 있는지 확인
            if (!buildScenes.Any(scene => scene.path == scenePath))
            {
                buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                Debug.Log("Added scene to build: " + scenePath);
            }
            else
            {
                Debug.Log("Scene already in build settings: " + scenePath);
            }
        }

        // 빌드 씬 목록 업데이트
        EditorBuildSettings.scenes = buildScenes.ToArray();
        Debug.Log("Build settings updated.");
    }
}
