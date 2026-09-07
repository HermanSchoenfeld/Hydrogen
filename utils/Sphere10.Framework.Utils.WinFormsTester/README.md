# WinForms Tester

Build and run `Sphere10.Framework.Utils.WinFormsTester.csproj` on Windows. The application starts with `BlockMainForm.ScreenMode = ScreenMode.MultiView` configured through `UseMainForm`. It uses `HighDpiMode.DpiUnaware` so Windows bitmap-scales the complete interface, preserving the legacy screen and dialog layouts. Shared build settings keep `ForceDesignerDPIUnaware=true` for the Visual Studio designer. `UseMainFormSettings()` enables per-user window placement: resize or move the main window, close it through an accepted exit, then restart to restore its size, monitor, maximized state, and left menu width. Settings are written once by the framework shutdown task after accepted exit. The menu starts at 320 logical pixels and can expand to 480, with a smaller limit when the content needs room. Drag the divider to test your own width, including closing with the menu collapsed; moving or resizing does not write settings.

## Screen hosting walkthrough

1. In **Screen hosting**, click **Settings** repeatedly. Settings is a singleton screen type; the same tab and instance number remain selected.
2. Click **New design** twice. Design is a multi-instance screen type; each tab has its own instance number, editable notes, counter, and event log.
3. Type different notes in each tab. Switch between tabs and use **Settings N** / **Design N > Actions**, or its counter toolbar button. Both must operate on the selected instance, and only its menu and toolbar should be merged into the main window.
4. Right-click a tab and choose **Undock**, or drag a selected tab away from the tab headers and release. Try the rightmost tab and the last remaining tab as well. Its notes and counter remain intact. The compact detached window shows its own **File** and **Actions** menus directly, and retains the screen's original toolbar. Check its caption icons for **Re-dock**, **Minimize**, **Maximize/Restore**, and **Close**. Minimize and restore through the taskbar, double-click the title to maximize/restore, and resize using the window edges. The main window continues to show the selected docked screen's controls, or an empty content panel when all screens are detached.
5. Undock Settings and click **Settings** in the main window's navigation again. This should focus the existing detached window without creating another tab.
6. Move the detached window's title bar close to the main tab strip. A highlighted tab-sized **Release to dock** preview and text hint show the insertion point and screen title, using the navigation pane's blue. The tabs should stay stationary without flickering while the pointer remains at that position. Moving the screen body over the workspace should not offer docking. Release to dock, or move away to dismiss the preview. The caption's **Re-dock** icon or **Ctrl+Shift+D** also returns it directly.
7. Reorder tabs by dragging along the tab bar. Tabs move while the mouse is held down; Escape restores the original position. Close tabs with their close button, middle-click, or context menu. Closing a detached window also closes that instance.
8. Select **Use SingleView** to close other open views and hide the tab bar. **Use MultiView** shows the selected screen as a tab again. SingleView caches single-instance screens when navigating away and disposes multi-instance screens.
9. Check **Block switching away…** in a workspace. Attempts to leave, close, undock, redock, or remove that screen through a mode change should be cancelled. Clear the checkbox to continue. Watch the event log; the instance number and notes should survive cancelled operations.
10. Use the **sidebar icon** at the start of the main toolbar, or **Ctrl+Alt+M**, to collapse and restore the entire blue navigation menu. Its tooltip changes between **Hide sidebar** and **Show sidebar**. Its width and collapsed preference survive screen changes; no extra panel or gutter should surround the content when collapsed.
11. With cancellation unchecked, choose **File > Exit** or close the main window, then choose **Yes**. The main window and detached windows should close. Choosing **No** keeps the application open. The confirmation uses native WinForms async modality.
12. Use **Rename tab...** in a screen's toolbar or **File** menu to try short, medium, and long titles. Tabs should grow to fit up to their maximum width, then show an ellipsis. Hover to read the full title. When detached, verify **File > Close screen** closes that screen and honors its cancellation checkbox.
13. Move the application and detached screens between monitors at different scaling settings (for example 100%, 150%, and 200%). Windows should bitmap-scale the complete windows. Check that navigation labels and dialog buttons remain visible and that tab titles, close buttons, docking previews, and menu collapse/restore retain their layout.
14. Open **Plain screen (no bars)** and detach it. Its content should start directly below the caption, with no empty menu, toolbar, or tab row. Edit the notes, re-dock, and detach again to check that the same content survives. Also check a regular Settings or Design screen: only their actual menus and toolbar should occupy space.

The other control test screens remain available under their existing navigation sections and can now be opened side by side. The communications screen polls reports on a component-owned UI timer that stops when its handle is destroyed and resumes when recreated. Switching, detaching, re-docking, and closing that screen must not leave a callback invoking a missing handle. The **Hooks** screen batches worker events for a UI timer, stops its hooks while its handle is unavailable, and releases hook subscriptions and resources on close or direct disposal. Reopening or re-docking it must not leave callbacks updating a disposed screen.

## CRUD Grid walkthrough

Choose **Tests 2 > CRUD Grid** to open a fresh screen each time; each tab has its own data and grid settings.

Enable **Allow cell editing** and **CanUpdate** on the CRUD Grid screen to edit **Address (street)** and **Notes** immediately. The street editor creates an address when needed and preserves its other fields, which remain visible in **Location**. Turning editing or update capability off also closes an active cell editor.

Enable **Left click to deselect** to select an unselected row with one click and clear a selected row with the next click, including slow presses and quick switches between rows. When **Allow cell editing** is also enabled, double-click the editable cell or press **F2** to edit while keeping its row selected. A single click on a checkbox selects or deselects its row; double-click or **F2** changes its value.

The manual **Page Size** accepts **1–9999** in steps of one. **Delete** appears only when deletion is allowed and a row is selected.

Open a record's edit dialog and expand **Address** to edit Street, City, State, and PostCode. For a missing address, choose **(Create new)** first. **Manager** opens a read-only CRUD dropdown configured at 520 × 300 containing the full employee data source and showing only **ID**, **Name**, and **Unsigned Int Field**. The same picker opens from an inline Manager cell. The grid fills the dropdown; **Name** is configured with `ExpandsToFit = true` to take spare width, while ID and Unsigned Int Field keep their content widths. Searching and changing pages keep the popup size stable. Resize it to check that Name follows the available width and automatic page sizing fits complete rows; selecting a row assigns that employee, **Clear** removes the manager, and **Cancel** or Escape keeps the previous value. Manager does not expand recursively. **Cancel** restores the original nested values and shared object references.

## Demo Wizard walkthrough

Choose **Wizard > Wizard Demo**. The first step requires a name and the confirmation checkbox; the age step requires a non-negative whole number, including zero. Invalid **Next** displays validation errors and keeps the current step and model unchanged. Accepted **Next** saves the input. Use **Previous** from the age step to revisit and edit the name. The third step hides Previous as its instructions describe. The confirmation screen shows the collected name and age, and **Finish** passes that same model to the completion action.

Automated coverage for hosting, dragging, navigation, exit, native async dialogs, CRUD grids, and wizard validation and collection is in `tests/Sphere10.Framework.Windows.Forms.Tests`:

```powershell
dotnet test tests/Sphere10.Framework.Windows.Forms.Tests/Sphere10.Framework.Windows.Forms.Tests.csproj
```
