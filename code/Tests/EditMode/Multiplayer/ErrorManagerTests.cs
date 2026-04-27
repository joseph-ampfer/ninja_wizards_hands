using NUnit.Framework;
using UnityEngine;

public class ErrorManagerTests
{
    private GameObject errorManagerObject;

    [SetUp]
    public void SetUp()
    {
        if (ErrorManager.Instance != null)
        {
            Object.DestroyImmediate(ErrorManager.Instance.gameObject);
        }

        errorManagerObject = new GameObject("ErrorManager");
        errorManagerObject.AddComponent<ErrorManager>();
    }

    [TearDown]
    public void TearDown()
    {
        if (errorManagerObject != null)
        {
            Object.DestroyImmediate(errorManagerObject);
        }
    }

    [Test]
    public void Awake_CreatesSingletonInstance()
    {
        Assert.IsNotNull(ErrorManager.Instance);
    }

    [Test]
    public void AddError_AppendsAndRaisesEvent()
    {
        var manager = ErrorManager.Instance;
        int eventCount = 0;
        manager.OnErrorsChanged += () => eventCount++;

        manager.AddError("first");
        manager.AddError("second");

        Assert.AreEqual(2, eventCount);
        Assert.AreEqual(2, manager.Errors.Count);
        Assert.AreEqual("first", manager.Errors[0]);
        Assert.AreEqual("second", manager.Errors[1]);
    }

    [Test]
    public void DismissError_RemovesMatchingAndRaisesEvent()
    {
        var manager = ErrorManager.Instance;
        int eventCount = 0;
        manager.OnErrorsChanged += () => eventCount++;

        manager.AddError("a");
        manager.AddError("b");
        manager.DismissError("a");

        Assert.AreEqual(3, eventCount);
        Assert.AreEqual(1, manager.Errors.Count);
        Assert.AreEqual("b", manager.Errors[0]);
    }

    [Test]
    public void ClearErrors_EmptiesListAndRaisesEvent()
    {
        var manager = ErrorManager.Instance;
        int eventCount = 0;
        manager.OnErrorsChanged += () => eventCount++;

        manager.AddError("x");
        manager.ClearErrors();

        Assert.AreEqual(2, eventCount);
        Assert.AreEqual(0, manager.Errors.Count);
    }

    [Test]
    public void Awake_WhenInstanceExists_DestroyDuplicateAndKeepsOriginal()
    {
        var first = ErrorManager.Instance;
        var duplicateObject = new GameObject("ErrorManagerDuplicate");
        duplicateObject.AddComponent<ErrorManager>();

        Assert.AreSame(first, ErrorManager.Instance);
        Object.DestroyImmediate(duplicateObject);
    }
}
