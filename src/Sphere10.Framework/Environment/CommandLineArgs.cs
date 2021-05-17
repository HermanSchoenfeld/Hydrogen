﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sphere10.Framework {

	public class CommandLineArgs {

		private const int ArgumentLineLengthPadded = 15;

		public string[] Header { get; }

		public string[] Footer { get; }

		public CommandLineArgCommand[] Commands { get; }

		public CommandLineArgOptions Options { get; } =
			CommandLineArgOptions.DoubleDash | CommandLineArgOptions.SingleDash | CommandLineArgOptions.PrintHelpOnHelp |
			CommandLineArgOptions.ForwardSlash | CommandLineArgOptions.PrintHelpOnH;

		public CommandLineArg[] Arguments { get; }

		public CommandLineArgs(string[] header, string[] footer, CommandLineArgCommand[] subCommands,
		                       CommandLineArg[] arguments, CommandLineArgOptions? options = null) {
			Guard.ArgumentNotNull(header, nameof(header));
			Guard.ArgumentNotNull(arguments, nameof(arguments));
			Guard.ArgumentNotNull(footer, nameof(footer));

			Guard.ArgumentInRange(header.Length, 1, Int32.MaxValue, nameof(header));
			Guard.ArgumentInRange(subCommands.Length, 1, Int32.MaxValue, nameof(subCommands));

			if (options is not null) {
				Options = options.Value;
			}

			Header = header;
			Footer = footer;
			Commands = subCommands;
			Arguments = arguments;
		}

		public Result<CommandLineResults> TryParse(string[] args) {
			var parseResults = Result<CommandLineResults>.Default;
			var argResults = new LookupEx<string, string>();
			var lastResult = new CommandLineResults(new LookupEx<string, CommandLineResults>(), argResults);
			parseResults.Value = lastResult;

			if (!args.Any()) {
				return Result<CommandLineResults>.Error("At least one command required.");
			}

			var parsedCommands = ParseCommands(args);
			var parsedArgs = ParseArgsToLookup(args);

			if (Options.HasFlag(CommandLineArgOptions.PrintHelpOnH)) {
				if (parsedArgs.Contains("H")) {
					argResults.Add("Help", string.Empty);
				}
			}

			if (Options.HasFlag(CommandLineArgOptions.PrintHelpOnHelp)) {
				if (parsedArgs.Contains("Help")) {
					argResults.Add("Help", string.Empty);
				}
			}

			if (!parsedCommands.Any()) {
				return Result<CommandLineResults>.Error("Command is required.");
			}

			foreach (var argument in Arguments) {
				if (argument.Dependencies.Any()) {
					foreach (var dependency in argument.Dependencies) {
						if (!parsedArgs.Contains(dependency)) {
							parseResults.AddError($"Argument {argument.Name} has unmet dependency {dependency}.");
						}
					}
				}

				if (argument.Traits.HasFlag(CommandLineArgTraits.Mandatory))
					if (!parsedArgs.Contains(argument.Name)) {
						parseResults.AddError($"Argument {argument.Name} is required.");
					}

				if (!argument.Traits.HasFlag(CommandLineArgTraits.Multiple))
					if (parsedArgs.CountForKey(argument.Name) > 1) {
						parseResults.AddError($"Argument {argument.Name} supplied more than once but does not support multiple values.");
					}

				if (parseResults.Success && parsedArgs.Contains(argument.Name))
					argResults.AddRange(argument.Name, parsedArgs[argument.Name]);
			}

			CommandLineArgCommand command = null;

			foreach (var commandName in parsedCommands) {
				string name = commandName;
				command = command?.Commands
					          .FirstOrDefault(x => x.Name == name)
				          ?? Commands.FirstOrDefault(x => x.Name == commandName);

				if (command is null) {
					parseResults.AddError($"Unknown command {commandName}.");
					break;
				}

				var commandArgResults = new LookupEx<string, string>();
				var commandResult = new LookupEx<string, CommandLineResults>();

				foreach (var argument in command.Args) {
					if (argument.Dependencies.Any()) {
						foreach (var dependency in argument.Dependencies) {
							if (!parsedArgs.Contains(dependency)) {
								parseResults.AddError($"Argument {argument.Name} has unmet dependency {dependency}.");
							}
						}
					}

					if (argument.Traits.HasFlag(CommandLineArgTraits.Mandatory))
						if (!parsedArgs.Contains(argument.Name)) {
							parseResults.AddError($"Argument {argument.Name} is required.");
						}

					if (!argument.Traits.HasFlag(CommandLineArgTraits.Multiple))
						if (parsedArgs.CountForKey(argument.Name) > 1) {
							parseResults.AddError($"Argument {argument.Name} supplied more than once but does not support multiple values.");
						}

					if (parseResults.Success && parsedArgs.Contains(argument.Name))
						commandArgResults.AddRange(argument.Name, parsedArgs[argument.Name]);
				}

				if (parseResults.Failure)
					break;

				var result = new CommandLineResults(commandResult, commandArgResults);
				lastResult.Commands.Add(commandName, result);
				lastResult = result;
			}

			return parseResults;
		}

		public void PrintHelp() {
			//   Header
			//
			//   Arguments:
			//      --print
			// This will print to the device
			//      --delete                   This will print to the device
			//      --remote                   This will print to the device
			//
			//   Sub-Commands:
			//		remote	                    This subcommand deals with remotes
			//         --add                    This will print to the device
			//         --remove                 This will print to the device
			//         --print                  This will print to the device
			//
			//		push                        This subcommand deals with push
			//         --add                    This will print to the device
			//         --remove                 This will print to the device
			//         --print                  This will print to the device
			//
			//		push                        This subcommand deals with push
			//         --add                    This will print to the device
			//         --print                  This will print to the device
			//	   	   special                  This subcommand deals with push
			//           --add                    This will print to the device
			//           --remove                 This will print to the device

			void PrintCommands(IEnumerable<CommandLineArgCommand> commands, int level = 1) {
				string itemIndentation = string.Empty.PadRight(level * 2);
				
				foreach (var command in commands) {
					string line = (itemIndentation + command.Name).PadRight(ArgumentLineLengthPadded) + "\t\t" + command.Description;
					Console.WriteLine(line);
					
					foreach (var arg in command.Args) {
						string argLine = (itemIndentation + "--" + arg.Name).PadRight(ArgumentLineLengthPadded) + "\t\t" + arg.Description;
						Console.WriteLine(argLine);
					}

					if (command.Commands.Any()) {
						level++;
						PrintCommands(command.Commands, level);
					}
				}
			}

			foreach (var line in Header)
				Console.WriteLine(line);

			if (Arguments.Any()) {
				Console.WriteLine("Arguments:");
				foreach (var arg in Arguments) {
					string line = (" " + "--" + arg.Name).PadRight(ArgumentLineLengthPadded) + "\t\t" + arg.Description;
					Console.WriteLine(line);
				}
			}

			Console.WriteLine("Commands:");
			PrintCommands(Commands);

			foreach (var line in Footer) {
				Console.WriteLine(line);
			}
		}

		private string BuildArgNameMatchPattern() {
			var builder = new StringBuilder();
			builder.Append("(");

			var hasSingle = Options.HasFlag(CommandLineArgOptions.SingleDash);
			var hasSlash = Options.HasFlag(CommandLineArgOptions.ForwardSlash);
			var hasDouble = Options.HasFlag(CommandLineArgOptions.DoubleDash);

			if (hasSingle || hasSlash) {
				builder.Append("[");

				if (Options.HasFlag(CommandLineArgOptions.ForwardSlash))
					builder.Append("/");

				if (Options.HasFlag(CommandLineArgOptions.SingleDash))
					builder.Append("-");

				builder.Append("]");

				if (hasDouble)
					builder.Append("|");
			}

			if (hasDouble)
				builder.Append("--");

			builder.Append(")");
			return builder.ToString();
		}

		private LookupEx<string, string> ParseArgsToLookup(string[] args) {
			var lookupEx = new LookupEx<string, string>(Options.HasFlag(CommandLineArgOptions.CaseSensitive)
				? StringComparer.Ordinal
				: StringComparer.OrdinalIgnoreCase);
			var parameterMatchPattern = BuildArgNameMatchPattern();

			var regex = new Regex("^" + parameterMatchPattern + @"{1}(?<name>\w+)([:=])?(?<value>.+)?$",
				RegexOptions.IgnoreCase | RegexOptions.Compiled);
			char[] trimChars = { '"', '\'', ' ' };

			foreach (var arg in args) {
				var part = regex.Match(arg);

				if (part.Success) {
					var parameter = part.Groups["name"].Value;
					lookupEx.Add(parameter, part.Groups["value"].Value.Trim(trimChars));
				}
			}

			return lookupEx;
		}

		private string[] ParseCommands(string[] args) {
			var results = new List<string>();

			for (int i = 0; i < args.Length; i++) {
				string arg = args[i];

				if (!(arg.StartsWith("-") || arg.StartsWith("/"))) {
					results.Add(arg);
				} else
					break;
			}

			return results.ToArray();
		}
	}
}
