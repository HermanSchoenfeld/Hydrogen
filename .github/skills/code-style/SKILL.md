---
name: code-style
description: Baseline C# style for this repo — Egyptian braces, tabs, PascalCase locals, member ordering, file-scoped namespaces, license header. Apply to every code change.
---

# Code Style Skill

Apply these rules to every file you create or edit. Full rules live in [../../copilot-instructions.md](../../copilot-instructions.md); this is the working checklist.

## Formatting
- Opening braces at **end of line** (K&R / Egyptian), never on a new line.
- **Tabs** for indentation (width 4); spaces only for aligning comments.
- `var` over explicit types wherever possible.
- Omit braces on single-statement `if`/`foreach`/etc.
- No redundant `else` after `return`/`throw`.
- Don't wrap lines unless > ~170 chars. If wrapping after `(`, put `)` on its own line at the originating indent.
- Base/sibling constructor calls (`: base(...)`, `: this(...)`) on the next line, tab-indented.

## Naming
- PascalCase for types, methods, properties, non-private fields — **and for local variables and parameters**.
- `_camelCase` for private fields.
- Self-describing names; no cryptic abbreviations.

## Structure
- File-scoped namespaces (`namespace X;`), following `CompanyName.Product.Tier.Domain` — not folder structure.
- `ImplicitUsings` is disabled: add explicit `using` directives.
- Nullable enabled; `LangVersion` latest.

## Member order in a class
1. Events
2. Private fields
3. Protected fields
4. Constructors (simple → complex), finalizers
5. Public properties
6. Internal/protected/private properties
7. Public methods
8. Internal/protected/private methods
9. Inner types

## Comments
- Comment logical segments inside method bodies; one blank line between segments and between members; no blank lines within a segment.

## New files
Every new source file starts with the license header:
```csharp
// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT NON-AI software license, see the accompanying file
// LICENSE or visit https://sphere10.com/legal/NON-AI-MIT.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.
```
