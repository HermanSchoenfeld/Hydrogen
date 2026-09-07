// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Dev Age
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sphere10.Framework.Windows.Forms.SourceGrid.DevAgeControls;

/// <summary>
/// Summary description for DropDownCustom.
/// </summary>
public class DropDown : System.Windows.Forms.Form {
	private Point StartLocation = new Point(0, 0);
	private System.Windows.Forms.Panel panelContainer;
	private bool _calculatingLocation;

	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;

	public DropDown() {
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
	}

	/// <summary>
	/// Constructor to create a dropdown form used to display the innerControl specified.
	/// It is responsability of the caller to dispose the innerControl.
	/// </summary>
	/// <param name="innerControl"></param>
	/// <param name="parentControl"></param>
	/// <param name="owner"></param>
	public DropDown(Control innerControl, Control parentControl, System.Windows.Forms.Form owner) : this() {
		Owner = owner;
		m_InnerControl = innerControl;
		m_ParentControl = parentControl;
	}

	/// <summary>
	/// Clean up any resources being used.
	/// </summary>
	protected override void Dispose(bool disposing) {
		if (disposing) {
			if (components != null) {
				components.Dispose();
			}
		}
		base.Dispose(disposing);
	}

	#region Windows Form Designer generated code

	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent() {
		this.panelContainer = new System.Windows.Forms.Panel();
		this.SuspendLayout();
		// 
		// panelContainer
		// 
		this.panelContainer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.panelContainer.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panelContainer.Location = new System.Drawing.Point(0, 0);
		this.panelContainer.Name = "panelContainer";
		this.panelContainer.Size = new System.Drawing.Size(84, 48);
		this.panelContainer.TabIndex = 0;
		// 
		// ctlDropDownCustom
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(84, 48);
		this.ControlBox = false;
		this.Controls.Add(this.panelContainer);
		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		this.MaximizeBox = false;
		this.MinimizeBox = false;
		this.Name = "ctlDropDownCustom";
		this.ShowInTaskbar = false;
		this.Text = "ctlDropDownCustom";
		this.Visible = false;
		this.StartPosition = FormStartPosition.Manual;
		this.Deactivate += new System.EventHandler(this.DropDown_Deactivate);
		this.Layout += new LayoutEventHandler(DropDown_Layout);
		this.ResumeLayout(false);

	}

	#endregion

	private Control m_ParentControl = null;
	private Control m_InnerControl = null;

	public Control ParentControl {
		get { return m_ParentControl; }
		set { m_ParentControl = value; }
	}

	public Control InnerControl {
		get { return m_InnerControl; }
		set { m_InnerControl = value; }
	}

	void DropDown_Layout(object sender, LayoutEventArgs e) {
		SuspendLayout();
		CalcLocation();
		ResumeLayout(false);
	}

	private void CalcLocation() {
		if (_calculatingLocation || m_InnerControl == null || m_ParentControl == null || m_InnerControl.IsDisposed || m_ParentControl.IsDisposed)
			return;
		_calculatingLocation = true;
		using var LocationScope = Tools.Scope.ExecuteOnDispose(() => _calculatingLocation = false);
		var ParentRectangle = m_ParentControl.RectangleToScreen(m_ParentControl.ClientRectangle);
		var WorkingArea = Screen.FromRectangle(ParentRectangle).WorkingArea;
		var SpaceAbove = Tools.Values.ClipValue(ParentRectangle.Top - WorkingArea.Top, 0, WorkingArea.Height);
		var SpaceBelow = Tools.Values.ClipValue(WorkingArea.Bottom - ParentRectangle.Bottom, 0, WorkingArea.Height);
		var MaximumHeight = Math.Max(1, Math.Max(SpaceAbove, SpaceBelow));
		var BorderSize = panelContainer.Size - panelContainer.ClientSize;
		var ContentSize = new Size(Math.Min(m_InnerControl.Width, Math.Max(1, WorkingArea.Width - BorderSize.Width)),
			Math.Min(m_InnerControl.Height, Math.Max(1, MaximumHeight - BorderSize.Height)));
		m_InnerControl.Size = ContentSize;
		ClientSize = ContentSize + BorderSize;
		var Left = ParentRectangle.Left + Width <= WorkingArea.Right ? ParentRectangle.Left : ParentRectangle.Right - Width;
		var Top = Height <= SpaceBelow ? ParentRectangle.Bottom : ParentRectangle.Top - Height;
		StartLocation = new Point(Tools.Values.ClipValue(Left, WorkingArea.Left, Math.Max(WorkingArea.Left, WorkingArea.Right - Width)),
			Tools.Values.ClipValue(Top, WorkingArea.Top, Math.Max(WorkingArea.Top, WorkingArea.Bottom - Height)));
		Location = StartLocation;
	}

	private void InnerControl_SizeChanged(object Sender, EventArgs Args) => CalcLocation();

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
		if ((m_Flags & DropDownFlags.CloseOnEscape) == DropDownFlags.CloseOnEscape) {
			if (keyData == Keys.Escape) {
				DialogResult = DialogResult.Cancel;
				CloseDropDown();
				//return true; altrimenti alcuni controlli che gestiscono i tasti non funzionano bene (ad esempio i controlli UITypeEditor)
			}
		}

		if ((m_Flags & DropDownFlags.CloseOnEnter) == DropDownFlags.CloseOnEnter) {
			if (keyData == Keys.Enter) {
				DialogResult = DialogResult.OK;
				CloseDropDown();
				//return true; altrimenti alcuni controlli che gestiscono i tasti non funzionano bene (ad esempio i controlli UITypeEditor)
			}
		}

		return base.ProcessCmdKey(ref msg, keyData);
	}

	private DropDownFlags m_Flags = DropDownFlags.CloseOnEnter | DropDownFlags.CloseOnEscape;

	public DropDownFlags DropDownFlags {
		get { return m_Flags; }
		set { m_Flags = value; }
	}

	private void DropDown_Deactivate(object sender, System.EventArgs e) {
		CloseDropDown();
	}

	private bool m_bShowed = false;
	public void ShowDropDown() {
		if (m_bShowed)
			return;
		m_bShowed = true;

		if (InnerControl == null)
			throw new ApplicationException("InnerControl is null");
		if (ParentControl == null)
			throw new ApplicationException("ParentControl is null");
		if (Owner == null)
			throw new ApplicationException("Owner is null");

		OnDropDownOpen(EventArgs.Empty);
	}

	public void CloseDropDown() {
		if (m_bShowed == false)
			return;

		OnDropDownClosed(EventArgs.Empty);

		m_bShowed = false;
	}

	protected virtual void OnDropDownOpen(EventArgs e) {
		if (DropDownOpen != null)
			DropDownOpen(this, e);

		panelContainer.Controls.Add(m_InnerControl);
		m_InnerControl.SizeChanged += InnerControl_SizeChanged;
		using var ContentScope = Tools.Scope.ExecuteOnDispose(() => {
			m_InnerControl.SizeChanged -= InnerControl_SizeChanged;
			// The editor owns the control; removing it prevents disposal by this form.
			panelContainer.Controls.Remove(m_InnerControl);
		});
		CalcLocation();

		Show();

		//This code simulate a ShowDialog. ShowDialog cannot be used because I need to receive the deactivate event to close the window.
		// This is not the best solution anyway because the parent for is deactivated and the user experience it is not the best.
		while (m_bShowed) {
			System.Windows.Forms.Application.DoEvents();
			System.Threading.Thread.Sleep(1); //To prevent the CPU to work on 100%
		}
	}

	protected virtual void OnDropDownClosed(EventArgs e) {
		Owner.Activate();

		Hide();

		if (DropDownClosed != null)
			DropDownClosed(this, e);
	}

	public event EventHandler DropDownOpen;
	public event EventHandler DropDownClosed;
}


[Flags]
public enum DropDownFlags {
	None = 0,

	/// <summary>
	/// Close the DropDown whe the user press the escape key, return DialogResult.Cancel
	/// </summary>
	CloseOnEscape = 1,

	/// <summary>
	/// Close the DropDown whe the user press the enter key, return DialogResult.OK
	/// </summary>
	CloseOnEnter = 2
}

