// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT NON-AI software license, see the accompanying file
// LICENSE or visit https://sphere10.com/legal/NON-AI-MIT.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sphere10.Framework.Windows.Forms;

/// <summary>
/// Awaitable modal extensions for <see cref="IApplicationDialog"/>. Allows application
/// dialogs to be awaited without blocking the calling context, matching the
/// <c>ShowDialogAsync</c> pattern added to WinForms in .NET 9/10.
/// </summary>
public static class ApplicationDialogExtensions {

	/// <summary>Awaits a modal <paramref name="dialog"/> shown ownerless.</summary>
	public static Task<DialogResult> ShowDialogAsync(this IApplicationDialog dialog)
		=> ShowDialogAsync(dialog, null);

	/// <summary>Awaits a modal <paramref name="dialog"/> shown with the given <paramref name="owner"/>.</summary>
	public static Task<DialogResult> ShowDialogAsync(this IApplicationDialog dialog, IWin32Window owner) {
		Guard.ArgumentNotNull(dialog, nameof(dialog));

		// Prefer the built-in WinForms Form.ShowDialogAsync when the dialog is a Form.
		if (dialog is Form form)
			return owner != null ? form.ShowDialogAsync(owner) : form.ShowDialogAsync();

		var completion = new TaskCompletionSource<DialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);
		var invoker = owner as Control;
		if (invoker == null || !invoker.IsHandleCreated)
			invoker = System.Windows.Forms.Application.OpenForms.Cast<Form>().FirstOrDefault(Form => Form.IsHandleCreated && !Form.IsDisposed);

		void ShowDialog() {
			try {
				var result = owner != null ? dialog.ShowDialog(owner) : dialog.ShowDialog();
				completion.TrySetResult(result);
			} catch (Exception error) {
				completion.TrySetException(error);
			}
		}

		// Always post the synchronous fallback so the caller receives its task before the dialog opens.
		if (invoker != null)
			invoker.BeginInvoke((Action)ShowDialog);
		else {
			var Context = SynchronizationContext.Current as WindowsFormsSynchronizationContext;
			Guard.Ensure(Context != null, "A WinForms UI context is required to show an application dialog.");
			Context.Post(_ => ShowDialog(), null);
		}

		return completion.Task;
	}
}
