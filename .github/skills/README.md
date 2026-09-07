# Sphere10 Framework Agent Skills

Task-specific skills for working in this repository. Load the skill that matches the task before writing code.

| Skill | Use when |
|---|---|
| [code-style](code-style/SKILL.md) | Any code change. Baseline formatting, naming, ordering, license header. |
| [oo-design](oo-design/SKILL.md) | Designing any new type, hierarchy, or service. Interface → base → concrete → decorator layering and OO idioms. |
| [decorator-pattern](decorator-pattern/SKILL.md) | Layering behavior over an existing abstraction with a `FooDecorator` (`ExtendedListDecorator` reference). |
| [tools-namespace](tools-namespace/SKILL.md) | Adding or using a `Tools.*` static utility, or reaching for a raw BCL call that a tool covers. |
| [builder-pattern](builder-pattern/SKILL.md) | Creating a fluent `FooBuilder` with chainable `With*`/`Add*` methods and a terminal `.Build()`. |
| [state-machines](state-machines/SKILL.md) | Application workflows and session lifecycles with Stateless, typed triggers, hierarchical states, guarded transitions, and async actions. |
| [serialization](serialization/SKILL.md) | Writing `IItemSerializer<T>` implementations, using `SerializerBuilder`/`SerializerFactory`, or touching endian-aware I/O. |
| [guards-and-scopes](guards-and-scopes/SKILL.md) | Argument/invariant validation (`Guard`), or resource/state cleanup via disposable scopes. |
| [disposable-scopes](disposable-scopes/SKILL.md) | Acquiring a resource, entering a state, locking, or guaranteed cleanup — the `IScope`/`Tools.Scope`/synchronization/transaction scope idiom. |
| [data-source](data-source/SKILL.md) | Implementing `IDataSource<T>` sync/async CRUD hierarchies (`SyncBatchDataSourceBase`, `AsyncBatchDataSourceBase`, etc.). |
| [crypto](crypto/SKILL.md) | Hashing, random bytes, digital signatures, key derivation. Never raw BCL crypto. |
| [logging](logging/SKILL.md) | Adding log output; `ILogger`, `SystemLog`, decorators and sinks. |
| [winforms-ui](winforms-ui/SKILL.md) | WinForms screens, wizards (`WizardBuilder`), application blocks (`ApplicationBlockBuilder`), `CrudGrid`. |
| [job-scheduler](job-scheduler/SKILL.md) | Recurring/one-shot background jobs via `JobBuilder` and `Scheduler`. |
| [unit-testing](unit-testing/SKILL.md) | Writing or modifying NUnit tests. |

These skills supplement [../copilot-instructions.md](../copilot-instructions.md), which remains authoritative.
