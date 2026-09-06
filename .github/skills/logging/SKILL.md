---
name: logging
description: Log via ILogger/SystemLog with decorators and sinks — never Console.WriteLine. Trigger when adding diagnostics or log output.
---

# Logging Skill

## Emit log messages
- Application-wide: `SystemLog.Info(...)`, `SystemLog.Error(...)`, etc. (backed by a `MulticastLogger`).
- Never `Console.WriteLine` for logging.
- Register sinks: `SystemLog.RegisterLogger(logger)`.

## Implementing a logger
- Implement `ILogger` (methods `Debug`, `Info`, `Warning`, `Error`, `Exception`, plus `Options`) or derive `LoggerBase`, which filters by `LogOptions` and delegates to `Log(LogLevel, string)`.
- Default options: `VerboseProfile` in debug builds, `StandardProfile` otherwise (via `Tools.Runtime.IsDebugBuild`).

## Composition
- Decorators wrap an inner `ILogger`: `TimestampLogger`, `ThreadIdLogger`, `PrefixLogger`, `SynchronizedLogger`, `AsyncLogger`.
- Sinks: `ConsoleLogger`, `DebugLogger`, `FileAppendLogger`, `RollingFileLogger`, `TextWriterLogger`, `EventLogLogger` (Windows), `TextBoxLogger` (WinForms).
