using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

public class SingletonScriptableObjectTest
{
    private const string ResourcePath = "Assets/Resources/TestSingleton.asset";

    [SetUp]
    public void SetUp()
    {
        // 테스트에 필요한 에셋을 미리 생성
        if (!File.Exists(ResourcePath))
        {
            var asset = ScriptableObject.CreateInstance<TestSingletonSO>();
            UnityEditor.AssetDatabase.CreateAsset(asset, ResourcePath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }
    }

    [TearDown]
    public void TearDown()
    {
        // 테스트에 사용된 에셋 정리
        if (File.Exists(ResourcePath))
        {
            UnityEditor.AssetDatabase.DeleteAsset(ResourcePath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }
    }

    [Test]
    public void Instance_WhenCalled_ReturnsSameInstance()
    {
        // Arrange & Act
        var instance1 = TestSingletonSO.Instance;
        var instance2 = TestSingletonSO.Instance;

        // Assert
        Assert.IsNotNull(instance1);
        Assert.AreEqual(instance1, instance2);
    }

    [Test]
    public void IsValid_WhenInstanceExists_ReturnsTrue()
    {
        // Arrange & Act
        var instance = TestSingletonSO.Instance;

        // Assert
        Assert.IsTrue(TestSingletonSO.IsValid);
    }

    [Test]
    public void IsValid_WhenApplicationQuitting_ReturnsFalse()
    {
        // Arrange
        var instance = TestSingletonSO.Instance;
        TestSingletonSO.SimulateApplicationQuit();

        // Act & Assert
        Assert.IsFalse(TestSingletonSO.IsValid);

        // Cleanup
        TestSingletonSO.SimulateApplicationRestart();
    }

    [Test]
    public void Instance_WhenAccessedFromMultipleThreads_ReturnsSameInstance()
    {
        // Arrange
        // 메인 스레드에서 먼저 인스턴스를 생성
        var mainThreadInstance = TestSingletonSO.Instance;

        TestSingletonSO instance1 = null;
        TestSingletonSO instance2 = null;

        // Act
        // 다른 스레드에서는 이미 생성된 인스턴스만 접근
        Parallel.Invoke(
            () => instance1 = TestSingletonSO.Instance,
            () => instance2 = TestSingletonSO.Instance
        );

        // Assert
        Assert.IsNotNull(mainThreadInstance);
        Assert.IsNotNull(instance1);
        Assert.IsNotNull(instance2);
        Assert.AreEqual(mainThreadInstance, instance1);
        Assert.AreEqual(mainThreadInstance, instance2);
    }
}
