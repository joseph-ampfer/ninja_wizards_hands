# Unit Tests for Wizards Game

This directory contains Edit Mode unit tests for the core gameplay logic of the Wizards Game.

## Test Coverage

The test suite includes 53+ unit tests covering:

- **GestureMapperTests** (10 tests): String-to-enum gesture conversion
- **GesturePairTests** (12 tests): Gesture pair struct equality and hashing
- **GestureSequenceTests** (13 tests): Gesture sequence matching and comparison
- **SpellBookTests** (13 tests): Spell lookup and spellbook validation
- **SpellCasterTests** (16 tests): Player stats, mana, and health management
- **GameManagerTests** (6 tests): Singleton pattern and player management

## Running Tests in Unity

### Option 1: Unity Test Runner Window

1. Open Unity Editor
2. Go to **Window > General > Test Runner**
3. Click the **EditMode** tab
4. Click **Run All** to run all tests
5. Or expand the test tree and run individual test classes

### Option 2: Command Line

Run tests from command line using Unity's batch mode:

```bash
"C:\Program Files\Unity\Hub\Editor\<VERSION>\Editor\Unity.exe" ^
  -runTests ^
  -batchmode ^
  -projectPath "C:\Users\jampf\Wizards Game\Wizards Game Clean" ^
  -testResults results.xml ^
  -testPlatform EditMode ^
  -logFile test.log
```

Replace `<VERSION>` with your Unity version (e.g., `2022.3.10f1`).

## Test Structure

```
Assets/
├── Scripts/
│   ├── Game.asmdef              # Main game assembly definition
│   └── [game scripts]
└── Tests/
    └── EditMode/
        ├── GameTests.asmdef     # Test assembly definition
        ├── GestureMapperTests.cs
        ├── GesturePairTests.cs
        ├── GestureSequenceTests.cs
        ├── SpellBookTests.cs
        ├── SpellCasterTests.cs
        └── GameManagerTests.cs
```

## Test Principles

- **Isolated Logic**: Tests focus on business logic without external dependencies
- **ScriptableObjects**: Created via `ScriptableObject.CreateInstance()` in tests
- **MonoBehaviours**: Tested by creating GameObjects with components in SetUp
- **No External Dependencies**: UI, VFX, and Animation references are avoided or mocked

## Troubleshooting

### Tests Don't Appear in Test Runner

1. Make sure `Game.asmdef` and `GameTests.asmdef` are properly imported
2. Reimport the test files: Right-click > Reimport
3. Restart Unity Editor

### Compilation Errors

1. Ensure Unity Test Framework package is installed
2. Check that all game scripts compile successfully
3. Verify assembly references in `GameTests.asmdef`

### Tests Fail

1. Check the test output in the Test Runner window
2. Review the error messages and stack traces
3. Ensure no conflicting GameManager instances exist in the scene

## Adding New Tests

1. Create a new `.cs` file in `Assets/Tests/EditMode/`
2. Use NUnit framework attributes: `[Test]`, `[SetUp]`, `[TearDown]`
3. Follow naming convention: `ClassNameTests.cs`
4. Use descriptive test method names: `MethodName_Scenario_ExpectedResult`

Example:
```csharp
using NUnit.Framework;

public class MyClassTests
{
    [Test]
    public void MyMethod_ValidInput_ReturnsExpectedValue()
    {
        // Arrange
        var instance = new MyClass();
        
        // Act
        var result = instance.MyMethod(validInput);
        
        // Assert
        Assert.AreEqual(expectedValue, result);
    }
}
```

## Continuous Integration

To integrate these tests into CI/CD pipeline, use Unity's command line test runner in your CI configuration (GitHub Actions, Jenkins, etc.).

Example GitHub Actions workflow:
```yaml
- name: Run tests
  uses: game-ci/unity-test-runner@v2
  with:
    testMode: EditMode
```

## Further Reading

- [Unity Test Framework Documentation](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)
- [NUnit Documentation](https://docs.nunit.org/)
- [Unity Assembly Definitions](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html)

