// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT NON-AI software license, see the accompanying file
// LICENSE or visit https://sphere10.com/legal/NON-AI-MIT.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using Sphere10.Framework.Application;

namespace Sphere10.Framework.Windows.Forms.Tests;

[TestFixture]
[NonParallelizable]
[Apartment(ApartmentState.STA)]
public class NavigationPaneTests {
	private bool _startedFramework;

	[OneTimeSetUp]
	public void StartFramework() {
		_startedFramework = !Sphere10Framework.Instance.IsStarted;
		if (_startedFramework)
			Sphere10Framework.Instance.Build().Start();
	}

	[OneTimeTearDown]
	public void StopFramework() {
		if (_startedFramework)
			Sphere10Framework.Instance.EndFramework();
	}

	[TestCase(ScreenMode.SingleView)]
	[TestCase(ScreenMode.MultiView)]
	public void SidebarButtonPreservesWidthAndActiveScreen(ScreenMode Mode) {
		using var Form = new BlockMainForm { ScreenMode = Mode };
		var Screen = new NavigationScreen();
		Form.ShowScreen(Screen);
		var SplitContainer = Form.Controls.OfType<SplitContainer>().Single();
		SplitContainer.SplitterDistance = 280;
		var OriginalWidth = SplitContainer.SplitterDistance;
		var Toolbar = Form.Controls.OfType<ToolStrip>().Single(Strip => Strip is not MenuStrip && Strip is not StatusStrip);
		var Toggle = Toolbar.Items.OfType<SidebarToggleButton>().Single();
		Assert.That(Toggle.Checked, Is.True);
		Assert.That(Toggle.AccessibleName, Is.EqualTo("Hide sidebar"));
		Toggle.PerformClick();
		Assert.That(Form.NavigationPaneCollapsed, Is.True);
		Assert.That(SplitContainer.Panel1Collapsed, Is.True);
		Assert.That(Toggle.Checked, Is.False);
		Assert.That(Toggle.AccessibleName, Is.EqualTo("Show sidebar"));
		Assert.That(Toggle.ToolTipText, Does.Contain("Ctrl+Alt+M"));
		Assert.That(Form.ActiveScreen, Is.SameAs(Screen));
		Assert.That(Screen.ActionButton.Owner, Is.SameAs(Toolbar));
		Form.ClientSize = new Size(Form.ClientSize.Width + 120, Form.ClientSize.Height);
		Toggle.PerformClick();
		Assert.That(Form.NavigationPaneCollapsed, Is.False);
		Assert.That(SplitContainer.Panel1Collapsed, Is.False);
		Assert.That(Toggle.Checked, Is.True);
		Assert.That(SplitContainer.SplitterDistance, Is.EqualTo(OriginalWidth));
		Assert.That(Form.ActiveScreen, Is.SameAs(Screen));
	}

	[TestCase(false)]
	[TestCase(true)]
	public void SidebarButtonUsesExistingToolbarWithoutNavigationPanelsOrGutters(bool Collapsed) {
		using var Form = new BlockMainForm { NavigationPaneCollapsed = Collapsed };
		var SplitContainer = Form.Controls.OfType<SplitContainer>().Single();
		var Navigation = SplitContainer.Panel1.Controls.OfType<ApplicationBar>().Single();
		Assert.That(SplitContainer.Panel1.Controls.Cast<Control>(), Is.EqualTo(new Control[] { Navigation }));
		Assert.That(SplitContainer.Panel2.Controls.Cast<Control>(), Is.EqualTo(new Control[] { Form.ScreenHost }));
		Assert.That(Form.ScreenHost.Bounds, Is.EqualTo(SplitContainer.Panel2.ClientRectangle), "The screen host fills the entire content panel without a restore-button gutter");
		if (!Collapsed)
			Assert.That(Navigation.Bounds, Is.EqualTo(SplitContainer.Panel1.ClientRectangle), "The sidebar starts at the top without a separate button header");
		var Toolbar = Form.Controls.OfType<ToolStrip>().Single(Strip => Strip is not MenuStrip && Strip is not StatusStrip);
		var Toggle = Toolbar.Items.OfType<SidebarToggleButton>().Single();
		Assert.That(Toolbar.Items.IndexOf(Toggle), Is.Zero);
		Assert.That(Toggle.Overflow, Is.EqualTo(ToolStripItemOverflow.Never));
		Assert.That(Toggle.DisplayStyle, Is.EqualTo(ToolStripItemDisplayStyle.None));
		Assert.That(Toggle.AccessibilityObject.Role, Is.EqualTo(AccessibleRole.CheckButton));
		Assert.That(Toggle.AccessibilityObject.State.HasFlag(AccessibleStates.Checked), Is.EqualTo(!Collapsed));
	}

	[Test]
	public void SameSidebarButtonSurvivesScreenToolbarRebuildsAndFilledScreens() {
		using var Form = new BlockMainForm { ScreenMode = ScreenMode.MultiView };
		var Toolbar = Form.Controls.OfType<ToolStrip>().Single(Strip => Strip is not MenuStrip && Strip is not StatusStrip);
		var Toggle = Toolbar.Items.OfType<SidebarToggleButton>().Single();
		var NormalScreen = new NavigationScreen();
		var FilledScreen = new NavigationScreen { DisplayMode = ScreenDisplayMode.Filled };
		Form.ShowScreen(NormalScreen);
		for (var Index = 0; Index < 3; Index++) {
			Form.ShowScreen(FilledScreen);
			Assert.That(Toggle.Enabled, Is.False);
			Assert.That(Toggle.Checked, Is.False);
			Assert.That(Form.NavigationPaneCollapsed, Is.False, "A filled screen does not change the saved sidebar preference");
			Form.ShowScreen(NormalScreen);
			Assert.That(Toolbar.Items.OfType<SidebarToggleButton>().Single(), Is.SameAs(Toggle));
			Assert.That(Toggle.IsDisposed, Is.False);
			Assert.That(Toggle.Enabled, Is.True);
			Assert.That(Toggle.Checked, Is.True);
		}
		Toggle.PerformClick();
		Assert.That(Form.NavigationPaneCollapsed, Is.True);
	}

	[Test]
	public void SidebarKeyboardShortcutUpdatesButtonState() {
		using var Form = new ProbeMainForm();
		var Toolbar = Form.Controls.OfType<ToolStrip>().Single(Strip => Strip is not MenuStrip && Strip is not StatusStrip);
		var Toggle = Toolbar.Items.OfType<SidebarToggleButton>().Single();
		Assert.That(Form.ToggleSidebarWithKeyboard(), Is.True);
		Assert.That(Form.NavigationPaneCollapsed, Is.True);
		Assert.That(Toggle.Checked, Is.False);
		Assert.That(Form.ToggleSidebarWithKeyboard(), Is.True);
		Assert.That(Form.NavigationPaneCollapsed, Is.False);
		Assert.That(Toggle.Checked, Is.True);
	}

	[TestCase(ScreenMode.SingleView, false)]
	[TestCase(ScreenMode.SingleView, true)]
	[TestCase(ScreenMode.MultiView, false)]
	[TestCase(ScreenMode.MultiView, true)]
	public void SwitchingAndClosingScreensRetainsNavigationPreference(ScreenMode Mode, bool Collapsed) {
		using var Form = new BlockMainForm { ScreenMode = Mode, NavigationPaneCollapsed = Collapsed };
		var First = new NavigationScreen();
		var Second = new NavigationScreen();
		Form.ShowScreen(First);
		Form.ShowScreen(Second);
		var SplitContainer = Form.Controls.OfType<SplitContainer>().Single();
		Assert.That(SplitContainer.Panel1Collapsed, Is.EqualTo(Collapsed));
		Assert.That(Form.NavigationPaneCollapsed, Is.EqualTo(Collapsed));
		Assert.That(Form.ScreenHost.CloseScreens(Form.ScreenHost.Screens), Is.True);
		Assert.That(SplitContainer.Panel1Collapsed, Is.EqualTo(Collapsed));
		Assert.That(Form.NavigationPaneCollapsed, Is.EqualTo(Collapsed));
	}

	[TestCase(ScreenDisplayMode.Filled, false)]
	[TestCase(ScreenDisplayMode.Filled, true)]
	[TestCase(ScreenDisplayMode.FilledAndMaximized, false)]
	[TestCase(ScreenDisplayMode.FilledAndMaximized, true)]
	public void FilledScreensTemporarilyHideNavigationWithoutChangingPreference(ScreenDisplayMode DisplayMode, bool Collapsed) {
		using var Form = new BlockMainForm { ScreenMode = ScreenMode.MultiView, NavigationPaneCollapsed = Collapsed };
		var NormalScreen = new NavigationScreen();
		var FilledScreen = new NavigationScreen { DisplayMode = DisplayMode };
		Form.ShowScreen(NormalScreen);
		var SplitContainer = Form.Controls.OfType<SplitContainer>().Single();
		var OriginalWidth = SplitContainer.SplitterDistance;
		Form.ShowScreen(FilledScreen);
		Assert.That(SplitContainer.Panel1Collapsed, Is.True);
		Assert.That(Form.NavigationPaneCollapsed, Is.EqualTo(Collapsed));
		Form.ShowScreen(NormalScreen);
		Assert.That(SplitContainer.Panel1Collapsed, Is.EqualTo(Collapsed));
		Assert.That(Form.NavigationPaneCollapsed, Is.EqualTo(Collapsed));
		Form.NavigationPaneCollapsed = false;
		Assert.That(SplitContainer.SplitterDistance, Is.EqualTo(OriginalWidth));
	}

	[Test]
	public void ExpandingAfterWindowShrinksKeepsSplitterWithinClientArea() {
		using var Form = new BlockMainForm();
		var SplitContainer = Form.Controls.OfType<SplitContainer>().Single();
		SplitContainer.SplitterDistance = 500;
		Form.NavigationPaneCollapsed = true;
		Form.ClientSize = new Size(320, Form.ClientSize.Height);
		Assert.That(() => Form.NavigationPaneCollapsed = false, Throws.Nothing);
		Assert.That(SplitContainer.Panel1Collapsed, Is.False);
		Assert.That(SplitContainer.SplitterDistance,
			Is.InRange(SplitContainer.Panel1MinSize, SplitContainer.Width - SplitContainer.SplitterWidth - SplitContainer.Panel2MinSize));
	}

	[TestCase(120, false)]
	[TestCase(144, false)]
	[TestCase(192, false)]
	[TestCase(120, true)]
	[TestCase(144, true)]
	[TestCase(192, true)]
	public void HiddenNavigationWidthFollowsDpiChanges(int NewDpi, bool FilledScreen) {
		using var Form = new ProbeMainForm { ScreenMode = ScreenMode.MultiView };
		var SplitContainer = Form.Controls.OfType<SplitContainer>().Single();
		SplitContainer.SplitterDistance = 281;
		var OriginalWidth = SplitContainer.SplitterDistance;
		var OriginalDpi = Form.DeviceDpi;
		if (FilledScreen)
			Form.ShowScreen(new NavigationScreen { DisplayMode = ScreenDisplayMode.Filled });
		else
			Form.NavigationPaneCollapsed = true;
		Form.RescaleDpi(OriginalDpi, NewDpi);
		if (FilledScreen)
			Form.ShowScreen(new NavigationScreen());
		else
			Form.NavigationPaneCollapsed = false;
		Assert.That(SplitContainer.Panel1Collapsed, Is.False);
		Assert.That(SplitContainer.SplitterDistance, Is.EqualTo((int)Math.Round(OriginalWidth * (double)NewDpi / OriginalDpi)),
			"Restoring a hidden navigation pane must use the current monitor's DPI");
	}

	[Test]
	public void HiddenNavigationWidthDoesNotDriftAfterRepeatedDpiChanges() {
		using var Form = new ProbeMainForm();
		var SplitContainer = Form.Controls.OfType<SplitContainer>().Single();
		SplitContainer.SplitterDistance = 281;
		Form.NavigationPaneCollapsed = true;
		var PreviousDpi = Form.DeviceDpi;
		foreach (var NewDpi in new[] { 120, 144, 192, 120, Form.DeviceDpi }) {
			Form.RescaleDpi(PreviousDpi, NewDpi);
			PreviousDpi = NewDpi;
		}
		Form.NavigationPaneCollapsed = false;
		Assert.That(SplitContainer.SplitterDistance, Is.EqualTo(281));
	}

	[TestCase(false)]
	[TestCase(true)]
	public void DockPreviewFollowsSelectedNavigationPaneWithoutSwitchingScreens(bool Collapsed) {
		using var Form = new ProbeMainForm { ScreenMode = ScreenMode.MultiView, NavigationPaneCollapsed = Collapsed };
		using var FirstBlock = new ApplicationBlock { Name = "First" };
		using var SecondBlock = new ApplicationBlock { Name = "Second" };
		Form.RegisterBlock(FirstBlock);
		Form.RegisterBlock(SecondBlock);
		var FirstPane = Form.PluginBindings[FirstBlock];
		var SecondPane = Form.PluginBindings[SecondBlock];
		FirstPane.CustomSettings.GradientStartColor = Color.LightSkyBlue;
		SecondPane.CustomSettings.GradientStartColor = Color.MidnightBlue;
		var Screen = new NavigationScreen { ApplicationBlock = FirstBlock };
		Form.ShowScreen(Screen);
		var Toolbar = Screen.ActionButton.Owner;
		var LifecycleChanges = 0;
		Screen.ScreenDisplayed += (_, _) => LifecycleChanges++;
		Screen.ScreenHidden += (_, _) => LifecycleChanges++;
		var Navigation = Form.Controls.OfType<SplitContainer>().Single().Panel1.Controls.OfType<ApplicationBar>().Single();
		Assert.That(Form.ScreenHost.TabControl.DockPreviewBackColor, Is.EqualTo(FirstPane.GradientStartColor));
		Form.ClickNavigationButton(Navigation.Items.Single(Item => ReferenceEquals(Item.MenuControl, SecondPane)).Button);
		Assert.That(Navigation.ApplicationBarControl, Is.SameAs(SecondPane));
		Assert.That(Form.ScreenHost.TabControl.DockPreviewBackColor, Is.EqualTo(SecondPane.GradientStartColor));
		Assert.That(Form.ScreenHost.TabControl.DockPreviewForeColor, Is.EqualTo(Color.White));
		FirstPane.CustomSettings.GradientStartColor = Color.LightBlue;
		Assert.That(Form.ScreenHost.TabControl.DockPreviewBackColor, Is.EqualTo(SecondPane.GradientStartColor), "An inactive pane must not recolor the docking preview");
		Form.ClickNavigationButton(Navigation.Items.Single(Item => ReferenceEquals(Item.MenuControl, FirstPane)).Button);
		Assert.That(Navigation.ApplicationBarControl, Is.SameAs(FirstPane));
		Assert.That(Form.ScreenHost.TabControl.DockPreviewBackColor, Is.EqualTo(FirstPane.GradientStartColor));
		Assert.That(Form.ScreenHost.TabControl.DockPreviewForeColor, Is.EqualTo(Color.Black));
		Assert.That(Form.ActiveScreen, Is.SameAs(Screen));
		Assert.That(Screen.ActionButton.Owner, Is.SameAs(Toolbar));
		Assert.That(LifecycleChanges, Is.Zero);
		Assert.That(Form.NavigationPaneCollapsed, Is.EqualTo(Collapsed));
	}

	[Test]
	public void DockPreviewTracksLivePaneSettingsAndSystemThemeChanges() {
		using var Form = new BlockMainForm();
		using var Block = new ApplicationBlock { Name = "Themed" };
		Form.RegisterBlock(Block);
		var Pane = Form.PluginBindings[Block];
		Pane.CustomSettings.GradientStartColor = Color.MidnightBlue;
		Assert.That(Form.ScreenHost.TabControl.DockPreviewBackColor, Is.EqualTo(Color.MidnightBlue));
		Pane.ResetCustomSettings();
		Assert.That(Form.ScreenHost.TabControl.DockPreviewBackColor, Is.EqualTo(Pane.GradientStartColor));
		var Theme = ExplorerBarInfo.Default;
		Theme.TaskPane.GradientStartColor = Color.CornflowerBlue;
		Pane.UseCustomTheme(Theme);
		Assert.That(Form.ScreenHost.TabControl.DockPreviewBackColor, Is.EqualTo(Color.CornflowerBlue), "System theme changes arrive through the pane's BackColorChanged event");
	}

	[Test]
	public void RemovingLastNavigationPaneRestoresDefaultDockPreviewColors() {
		using var Form = new BlockMainForm();
		var DefaultBackColor = Form.ScreenHost.TabControl.DockPreviewBackColor;
		var DefaultForeColor = Form.ScreenHost.TabControl.DockPreviewForeColor;
		var Block = new ApplicationBlock { Name = "Themed" };
		Form.RegisterBlock(Block);
		Form.PluginBindings[Block].CustomSettings.GradientStartColor = Color.MidnightBlue;
		Form.UnregisterBlock(Block);
		Assert.That(Form.ScreenHost.TabControl.DockPreviewBackColor, Is.EqualTo(DefaultBackColor));
		Assert.That(Form.ScreenHost.TabControl.DockPreviewForeColor, Is.EqualTo(DefaultForeColor));
	}

	private class ProbeMainForm : BlockMainForm {
		public void RescaleDpi(int OldDpi, int NewDpi) => RescaleConstantsForDpi(OldDpi, NewDpi);

		public void ClickNavigationButton(SquareButton Button) => InvokeOnClick(Button, EventArgs.Empty);

		public bool ToggleSidebarWithKeyboard() {
			var Message = System.Windows.Forms.Message.Create(Handle, 0x0100, (IntPtr)Keys.M, IntPtr.Zero);
			return ProcessCmdKey(ref Message, Keys.Control | Keys.Alt | Keys.M);
		}
	}

	private class NavigationScreen : ApplicationScreen {
		public NavigationScreen() {
			ActivationMode = ScreenActivationMode.MultiInstance;
			ToolBar = new ToolStrip();
			ActionButton = new ToolStripButton("Screen action");
			ToolBar.Items.Add(ActionButton);
			Controls.Add(ToolBar);
		}

		public ToolStripButton ActionButton { get; }
	}
}
