---
name: winforms-ui
description: WinForms screens, wizards, and application blocks. Trigger when creating ApplicationScreen, WizardBuilder wizards, ApplicationBlockBuilder navigation, or CrudGrid screens.
---

# WinForms UI Skill

## Application blocks & screens
- Navigation via `ApplicationBlock` + `ApplicationBlockBuilder`:
  ```csharp
  var block =
	  new ApplicationBlockBuilder()
		  .WithName("Admin")
		  .WithDefaultScreen<DashboardScreen>()
		  .AddMenu(mb => mb.AddScreenItem<UsersScreen>())
		  .Build();
  ```
- Derive screens from `ApplicationScreen`.
- `CrudGrid` for data-bound grid screens backed by `IDataSource<T>` (see [data-source](../data-source/SKILL.md)).

## Wizards
Use `WizardBuilder<T>` (`src/Sphere10.Framework.Windows.Forms/Wizard/WizardBuilder.cs`):
```csharp
var wizard =
	new WizardBuilder<MyModel>()
		.WithTitle("Setup")
		.WithModel(model)
		.AddScreen(new StepOneScreen())
		.AddScreen(new StepTwoScreen())
		.OnFinished(async m => Result.Success)
		.OnCancelled(m => Result.Success)
		.Build();
```
- `Build()` requires a title, at least one screen, and a finish function (enforced via `Guard.Ensure`).
- Follow the [builder-pattern](../builder-pattern/SKILL.md) skill when extending wizard configuration.
