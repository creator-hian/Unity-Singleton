using UnityEngine;

[CreateAssetMenu(fileName = "TestSingleton", menuName = "Test/TestSingleton")]
public class TestSingletonSO : SingletonScriptableObject<TestSingletonSO>
{
    private class TestAssetPath : AssetPath
    {
        protected override string ResourcesLoadPath => "TestSingleton";
    }

    static TestSingletonSO()
    {
        SetPath(new TestAssetPath());
    }

    public static void SimulateApplicationQuit()
    {
        System.Reflection.FieldInfo field =
            typeof(SingletonScriptableObject<TestSingletonSO>).GetField(
                "_applicationIsQuitting",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic
            );
        field.SetValue(null, true);
    }

    public static void SimulateApplicationRestart()
    {
        System.Reflection.FieldInfo field =
            typeof(SingletonScriptableObject<TestSingletonSO>).GetField(
                "_applicationIsQuitting",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic
            );
        field.SetValue(null, false);
    }
}
