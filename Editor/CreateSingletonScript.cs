using System.IO;
using UnityEditor;
using UnityEngine;

public abstract class CreateSingletonScript
{
    protected const string BASE_MENU_PATH = "Assets/Create/Scripting/Singleton/";
    protected const string BASE_TEMPLATE_PATH =
        "Packages/com.creator-hian.unity.singleton/Editor/ScriptTemplate/";

    protected abstract string TemplatePath { get; }
    protected abstract string DefaultFileName { get; }
    protected abstract string MenuItemName { get; }

    protected void CreateScript()
    {
        string templatePath = TemplatePath;
        string targetDir = GetSelectedPathOrFallback();
        string fileName = Path.Combine(targetDir, DefaultFileName);

        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, fileName);
    }

    private static string GetSelectedPathOrFallback()
    {
        string path = "Assets";

        foreach (Object obj in Selection.GetFiltered<Object>(SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }
}

// Singleton 클래스 생성
public class CreateSingletonClass : CreateSingletonScript
{
    private const string MENU_ITEM = BASE_MENU_PATH + "Singleton Class";

    [MenuItem(MENU_ITEM)]
    public static void Create()
    {
        new CreateSingletonClass().CreateScript();
    }

    protected override string TemplatePath => BASE_TEMPLATE_PATH + "SingletonTemplate.cs.txt";
    protected override string DefaultFileName => "NewSingleton.cs";
    protected override string MenuItemName => MENU_ITEM;
}

// SingletonMonoBehaviour 클래스 생성
public class CreateSingletonMonoBehaviour : CreateSingletonScript
{
    private const string MENU_ITEM = BASE_MENU_PATH + "Singleton MonoBehaviour";

    [MenuItem(MENU_ITEM)]
    public static void Create()
    {
        new CreateSingletonMonoBehaviour().CreateScript();
    }

    protected override string TemplatePath =>
        BASE_TEMPLATE_PATH + "SingletonMonoBehaviourTemplate.cs.txt";
    protected override string DefaultFileName => "NewSingletonMonoBehaviour.cs";
    protected override string MenuItemName => MENU_ITEM;
}

// SingletonScriptableObject 클래스 생성
public class CreateSingletonScriptableObject : CreateSingletonScript
{
    private const string MENU_ITEM = BASE_MENU_PATH + "Singleton ScriptableObject";

    [MenuItem(MENU_ITEM)]
    public static void Create()
    {
        new CreateSingletonScriptableObject().CreateScript();
    }

    protected override string TemplatePath =>
        BASE_TEMPLATE_PATH + "SingletonScriptableObjectTemplate.cs.txt";
    protected override string DefaultFileName => "NewSingletonScriptableObject.cs";
    protected override string MenuItemName => MENU_ITEM;
}
