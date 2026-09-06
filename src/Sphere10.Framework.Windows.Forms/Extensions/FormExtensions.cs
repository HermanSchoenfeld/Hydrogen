// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sphere10.Framework.Windows;


namespace Sphere10.Framework;

public static class FormExtensions {

	/// <summary>
	/// Awaitably shows a modal <typeparamref name="T"/> owned by <paramref name="parentForm"/>,
	/// marshalling to the UI thread and using WinForms' native <see cref="Form.ShowDialogAsync(IWin32Window)"/>.
	/// </summary>
	public static Task<DialogResult> ShowDialogAsync<T>(this Form parentForm, CancellationToken cancellationToken = default) where T : Form, new()
		=> parentForm.InvokeAsyncEx(
			async _ => {
				using var form = new T();
				if (parentForm.WindowState == FormWindowState.Minimized) {
					form.StartPosition = FormStartPosition.CenterScreen;
				}
				return await form.ShowDialogAsync(parentForm);
			},
			cancellationToken
		);

	public static void ShowInactiveTopmost(this Form frm) {
		WinAPI.USER32.ShowWindow(frm.Handle, WinAPI.USER32.ShowWindowCommands.ShowNoActivate);
		WinAPI.USER32.SetWindowPos(frm.Handle, WinAPI.USER32.HWND_TOPMOST, frm.Left, frm.Top, frm.Width, frm.Height, WinAPI.USER32.SetWindowPosFlags.SWP_NOACTIVATE);
	}


}

