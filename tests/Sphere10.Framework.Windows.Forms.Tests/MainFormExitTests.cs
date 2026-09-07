// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT NON-AI software license, see the accompanying file
// LICENSE or visit https://sphere10.com/legal/NON-AI-MIT.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using Sphere10.Framework.Application;
using WinFormsApplication = System.Windows.Forms.Application;

namespace Sphere10.Framework.Windows.Forms.Tests;

[TestFixture]
[NonParallelizable]
[Apartment(ApartmentState.STA)]
public class MainFormExitTests {
	[TestCase(false)]
	[TestCase(true)]
	public void YesClosesMainWindowAndAllDockedAndDetachedScreens(bool FileExit) => RunWithMessageLoop(async Form => {
		using var Docked = new ExitProbeScreen { Title = "Docked" };
		using var Detached = new ExitProbeScreen { Title = "Detached" };
		Form.ShowScreen(Docked);
		Form.ShowScreen(Detached);
		Form.ScreenHost.UndockScreen(Detached);
		var DetachedWindow = Detached.FindForm()!;
		var DockedHideCount = Docked.HideCount;
		var DetachedHideCount = Detached.HideCount;
		var Closed = false;
		Form.FormClosed += (_, _) => Closed = true;
		Form.RequestClose(FileExit);
		using var Confirmation = await WaitForConfirmation();
		Assert.That(Form.Visible, Is.True, "The initial close must wait for the user's answer.");
		Assert.That(Confirmation.Owner, Is.SameAs(Form));
		Assert.That(Confirmation.Modal, Is.True);
		Assert.That((Sphere10.Framework.Windows.WinAPI.USER32.GetWindowLong(Form.Handle, -16).ToInt64() & 0x08000000) != 0, Is.True);
		Answer(Confirmation, true);
		await WaitUntil(() => Form.IsDisposed);
		Assert.That(Closed, Is.True, "Normal exit must complete the WinForms close lifecycle.");
		Assert.That(DetachedWindow.IsDisposed, Is.True);
		Assert.That(Docked.IsDisposed && Detached.IsDisposed, Is.True);
		Assert.That(Docked.HideCount, Is.EqualTo(DockedHideCount + 1));
		Assert.That(Detached.HideCount, Is.EqualTo(DetachedHideCount + 1));
		Assert.That(Form.ExitCheckCount, Is.EqualTo(1), "Confirmed close must not repeat exit checks.");
	});

	[TestCase(false)]
	[TestCase(true)]
	public void NoKeepsApplicationOpenAndAnotherCloseCanBeConfirmed(bool FileExit) => RunWithMessageLoop(async Form => {
		Form.RequestClose(FileExit);
		using var FirstConfirmation = await WaitForConfirmation();
		Form.RequestClose(FileExit);
		Assert.That(WinFormsApplication.OpenForms.OfType<DialogEx>().Count(), Is.EqualTo(1), "Repeated close requests must share one pending confirmation.");
		Answer(FirstConfirmation, false);
		await WaitUntil(() => !Form.ApplicationExiting);
		Assert.That(Form.IsDisposed, Is.False);
		Assert.That(Form.Visible && Form.Enabled, Is.True);
		Assert.That(Form.ExitCheckCount, Is.Zero);
		Form.RequestClose(FileExit);
		using var SecondConfirmation = await WaitForConfirmation();
		Answer(SecondConfirmation, true);
		await WaitUntil(() => Form.IsDisposed);
		Assert.That(Form.ExitCheckCount, Is.EqualTo(1));
	});

	[Test]
	public void FileExitNoRestoresHideOnCloseBehavior() => RunWithMessageLoop(async Form => {
		Form.CloseAction = FormCloseAction.Hide;
		Form.RequestClose(true);
		using var Confirmation = await WaitForConfirmation();
		Answer(Confirmation, false);
		await WaitUntil(() => !Form.ApplicationExiting);
		Assert.That(Form.CloseAction, Is.EqualTo(FormCloseAction.Hide));
		Form.Close();
		Assert.That(Form.IsDisposed, Is.False);
		Assert.That(Form.Visible, Is.False);
	});

	[TestCase(false)]
	[TestCase(true)]
	public void ExitVetoPreservesWindowsAndCanBeRetried(bool ScreenVeto) => RunWithMessageLoop(async Form => {
		using var Screen = new ExitProbeScreen { CancelHide = ScreenVeto };
		Form.ShowScreen(Screen);
		var Veto = true;
		Form.ApplicationExitingEvent += (_, Args) => Args.Cancel |= !ScreenVeto && Veto;
		Form.RequestClose(true);
		using var FirstConfirmation = await WaitForConfirmation();
		Answer(FirstConfirmation, true);
		await WaitUntil(() => !Form.ApplicationExiting);
		Assert.That(Form.IsDisposed || Screen.IsDisposed, Is.False);
		Assert.That(Form.ScreenHost.ActiveScreen, Is.SameAs(Screen));
		Screen.CancelHide = Veto = false;
		Form.RequestClose(true);
		using var SecondConfirmation = await WaitForConfirmation();
		Answer(SecondConfirmation, true);
		await WaitUntil(() => Form.IsDisposed);
		Assert.That(Screen.IsDisposed, Is.True);
	});

	[Test]
	public void NativeFormClosingObserverCanVetoConfirmedClose() => RunWithMessageLoop(async Form => {
		var Veto = true;
		Form.FormClosing += (_, Args) => {
			if (Form.ExitCheckCount > 0)
				Args.Cancel |= Veto;
		};
		Form.RequestClose(true);
		using var Confirmation = await WaitForConfirmation();
		Answer(Confirmation, true);
		await WaitUntil(() => !Form.ApplicationExiting);
		Assert.That(Form.IsDisposed, Is.False);
		Veto = false;
		Form.RequestClose(true);
		using var NextConfirmation = await WaitForConfirmation();
		Answer(NextConfirmation, true);
		await WaitUntil(() => Form.IsDisposed);
	});

	[Test]
	public void StartupMessageLoopEndsAndFinalizesFrameworkAfterMainFormCloses() {
		Assert.That(Sphere10Framework.Instance.IsStarted, Is.False);
		Exception? Failure = null;
		ExitProbeMainForm? MainForm = null;
		var Finalized = false;
		void FrameworkFinalized() => Finalized = MainForm?.IsDisposed == true;
		Sphere10Framework.Instance.Finalized += FrameworkFinalized;
		using var Cleanup = Tools.Scope.ExecuteOnDispose(() => {
			Sphere10Framework.Instance.Finalized -= FrameworkFinalized;
			if (Sphere10Framework.Instance.IsStarted)
				Sphere10Framework.Instance.EndFramework();
		});
		var PreviousContext = SynchronizationContext.Current;
		using var UiContext = new WindowsFormsSynchronizationContext();
		using var RestoreContext = Tools.Scope.ExecuteOnDispose(() => SynchronizationContext.SetSynchronizationContext(PreviousContext));
		SynchronizationContext.SetSynchronizationContext(UiContext);
		using var Watchdog = new System.Windows.Forms.Timer { Interval = 15000 };
		using var RequestExit = new System.Windows.Forms.Timer { Interval = 25 };
		Watchdog.Tick += (_, _) => {
			Failure = new AssertionException($"The main application loop did not exit. Visible: {MainForm?.Visible}; Disposed: {MainForm?.IsDisposed}; Windows: {DescribeOpenForms()}");
			foreach (var Dialog in WinFormsApplication.OpenForms.Cast<Form>().Reverse().ToArray())
				Dialog.Dispose();
			WinFormsApplication.ExitThread();
		};
		Watchdog.Start();
		RequestExit.Tick += async (_, _) => {
			if (MainForm?.Visible != true)
				return;
			RequestExit.Stop();
			try {
				MainForm.RequestClose(true);
				using var Confirmation = await WaitForConfirmation();
				Answer(Confirmation, true);
			} catch (Exception Error) {
				Failure = Error;
				MainForm.Dispose();
			}
		};
		RequestExit.Start();
		Sphere10Framework.Instance.BuildWinFormsApplication()
			.ConfigureServices(Services => Services.AddSingleton<IProductUsageServices, ProbeUsageServices>())
			.UseMainForm<ExitProbeMainForm>(Form => MainForm = Form)
			.StartWinFormsApplication();
		Assert.That(Failure, Is.Null, Failure?.ToString());
		Assert.That(MainForm?.IsDisposed, Is.True);
		Assert.That(Sphere10Framework.Instance.IsStarted, Is.False);
		Assert.That(Finalized, Is.True, "Framework services must be disposed after the main form completes its close lifecycle.");
	}

	private static void Answer(DialogEx Confirmation, bool Yes)
		=> ((Button)Confirmation.Controls.Find(Yes ? "button2" : "button1", true).Single()).PerformClick();

	private static async Task<DialogEx> WaitForConfirmation() {
		await WaitUntil(() => WinFormsApplication.OpenForms.OfType<DialogEx>().Any(Dialog => Dialog.Visible));
		Assert.That(WinFormsApplication.OpenForms.OfType<DialogEx>().Count(Dialog => Dialog.Visible), Is.EqualTo(1), DescribeOpenForms());
		return WinFormsApplication.OpenForms.OfType<DialogEx>().Single(Dialog => Dialog.Visible);
	}

	private static string DescribeOpenForms()
		=> string.Join(Environment.NewLine, WinFormsApplication.OpenForms.Cast<Form>().Select(Form => $"{Form.GetType().Name}: {Form.Text}; {(Form as ExceptionDialog)?.Exception}"));

	private static async Task WaitUntil(Func<bool> Condition) {
		for (var Attempt = 0; Attempt < 500; Attempt++) {
			if (Condition())
				return;
			await Task.Delay(10);
		}
		throw new AssertionException("The expected exit state was not reached.");
	}

	private static void RunWithMessageLoop(Func<ExitProbeMainForm, Task> Test) {
		var StartFramework = !Sphere10Framework.Instance.IsStarted;
		if (StartFramework)
			Sphere10Framework.Instance.Build().ConfigureServices(Services => Services.AddSingleton<IProductUsageServices, ProbeUsageServices>()).Start();
		using var FrameworkLifetime = Tools.Scope.ExecuteOnDispose(() => {
			if (StartFramework)
				Sphere10Framework.Instance.EndFramework();
		});
		Exception? Failure = null;
		var PreviousContext = SynchronizationContext.Current;
		using var UiContext = new WindowsFormsSynchronizationContext();
		using var RestoreContext = Tools.Scope.ExecuteOnDispose(() => SynchronizationContext.SetSynchronizationContext(PreviousContext));
		SynchronizationContext.SetSynchronizationContext(UiContext);
		using var Owner = new Form { ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-20000, -20000) };
		using var MainForm = new ExitProbeMainForm();
		using var Watchdog = new System.Windows.Forms.Timer { Interval = 15000 };
		Watchdog.Tick += (_, _) => {
			Failure = new AssertionException("The exit test timed out.");
			foreach (var Dialog in WinFormsApplication.OpenForms.Cast<Form>().Reverse().ToArray())
				Dialog.Dispose();
			WinFormsApplication.ExitThread();
		};
		Owner.Shown += async (_, _) => {
			using var CloseOwner = Tools.Scope.ExecuteOnDispose(() => {
				foreach (var Dialog in WinFormsApplication.OpenForms.Cast<Form>().Reverse().ToArray())
					Dialog.Dispose();
			});
			try {
				MainForm.Show(Owner);
				await Test(MainForm);
			} catch (Exception Error) {
				Failure = Error;
			}
		};
		Watchdog.Start();
		WinFormsApplication.Run(Owner);
		Assert.That(Failure, Is.Null, Failure?.ToString());
	}

	private sealed class ExitProbeScreen : ApplicationScreen {
		public ExitProbeScreen() => ActivationMode = ScreenActivationMode.MultiInstance;

		[DefaultValue(false)] public bool CancelHide { get; set; }
		public int HideCount { get; private set; }

		protected override void OnHide(ref bool Cancel) {
			HideCount++;
			Cancel |= CancelHide;
		}
	}

	private sealed class ExitProbeMainForm : BlockMainForm {
		public ExitProbeMainForm() {
			ScreenMode = ScreenMode.MultiView;
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.Manual;
			Location = new Point(40, 40);
			Opacity = 0;
		}

		public int ExitCheckCount { get; private set; }

		public void RequestClose(bool FileExit) {
			if (FileExit)
				MenuStrip.Items.Find("exitToolStripMenuItem", true).Single().PerformClick();
			else
				Close();
		}

		protected override void PopulatePrimingData() => Text = "Exit test";

		protected override void OnFirstActivated() { }

		protected override void OnApplicationExiting(CancelEventArgs Args) {
			ExitCheckCount++;
			base.OnApplicationExiting(Args);
		}
	}

	private sealed class ProbeUsageServices : IProductUsageServices {
		public ProductUsageInformation ProductUsageInformation { get; } = new();
		public IDictionary<string, object> UserEncryptedProperties { get; } = new Dictionary<string, object>();
		public IDictionary<string, object> SystemEncryptedProperties { get; } = new Dictionary<string, object>();
		public void IncrementUsageByOne() { }
	}
}
