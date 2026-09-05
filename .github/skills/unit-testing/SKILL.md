---
name: unit-testing
description: NUnit constraint-model tests only — Assert.That, never ClassicAssert or legacy Assert. Trigger when writing or editing tests.
---

# Unit Testing Skill

## Framework
- NUnit only. Test projects: `<PackageReference Include="NUnit" .../>`, `NUnit3TestAdapter`, `Microsoft.NET.Test.Sdk`.

## Assertions — constraint model exclusively
```csharp
// ✅ Correct
Assert.That(result, Is.EqualTo(expected));
Assert.That(flag, Is.True);
Assert.That(collection, Is.Not.Empty);
Assert.That(() => Foo(), Throws.InstanceOf<InvalidOperationException>());

// ❌ Never
ClassicAssert.AreEqual(expected, result);
Assert.AreEqual(expected, result);
Assert.IsTrue(flag);
```
- Add a descriptive failure message where it aids diagnosis:
  `Assert.That(result, Is.True, "Signature must verify against the correct public key");`

## Structure
- `[TestFixture]`, `[Test]`, `[TestCase(...)]`, `[Values(...)]`, `[Repeat(n)]` as appropriate.
- `[Parallelizable(ParallelScope.Children)]` on fixtures unless tests share mutable state.
- `Tools.NUnit` helpers exist (e.g. 2D array formatting) — prefer them over hand-rolled output.
- New test files get the standard license header (see [code-style](../code-style/SKILL.md)).
