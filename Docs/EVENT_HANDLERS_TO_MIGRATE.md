# Event Handlers to Migrate from Serenity to Cortex

**Date:** January 2026  
**Source:** `d:\_Code_\Serenity\data\source\MainWindow\MainWindow.axaml.cs`  
**Destination:** `D:\_Code_\Cortex\Cortex.App\Views\CortexView.axaml.cs`

---

## 📋 Complete List of Event Handlers

### File Management

#### `AddFiles_Click` (Line 387-471)
- **Purpose:** Opens file picker, adds selected files as sources
- **Code:** Lines 387-471 in MainWindow.axaml.cs
- **Dependencies:** 
  - `MainViewModel` (as DataContext)
  - `StorageProvider` (Avalonia)
  - `FileHelper.ShellOpen` (helper)
  - `vm.AddSourceFromExternalAsync` (ViewModel method)
- **Migration Notes:** 
  - Change `MainViewModel` to `CortexViewModel`
  - Update namespace references
  - Keep Avalonia StorageProvider usage

#### `AddUrl_Click` (Line 473-488)
- **Purpose:** Prompts for URL, adds URL as source
- **Code:** Lines 473-488
- **Dependencies:**
  - `DialogHelper.PromptAsync`
  - `vm.AddSourceFromExternalAsync` (ViewModel method)
- **Migration Notes:**
  - Need to create `DialogHelper` in Cortex or use Avalonia dialogs directly

### Source Management

#### `SourceOpen_Click` (Line 490-495)
- **Purpose:** Opens source file in default application
- **Code:** Lines 490-495
- **Dependencies:**
  - `TryGetDataContext<SourceDocument>`
  - `FileHelper.ShellOpen`
- **Migration Notes:**
  - Generic helper method - can be reused

#### `SourceCopyPath_Click` (Line 497-505)
- **Purpose:** Copies source file path to clipboard
- **Code:** Lines 497-505
- **Dependencies:**
  - `CopyToClipboardAsync` (helper method)
- **Migration Notes:**
  - Clipboard helper - standard Avalonia

#### `SourceRename_Click` (Line 507-518)
- **Purpose:** Renames a source document
- **Code:** Lines 507-518
- **Dependencies:**
  - `DialogHelper.PromptAsync`
  - `vm.RenameSourceAsync` (ViewModel method)
- **Migration Notes:**
  - Standard rename pattern

#### `SourceOnlyInclude_Click` (Line 520-527)
- **Purpose:** Sets source as only included (excludes others)
- **Code:** Lines 520-527
- **Dependencies:**
  - `vm.OnlyIncludeSourceAsync` (ViewModel method)
- **Migration Notes:**
  - Simple command execution

#### `SourceExclude_Click` (Line 529-537)
- **Purpose:** Excludes source from context
- **Code:** Lines 529-537
- **Dependencies:**
  - `vm.SetSourceIncludedAsync` (ViewModel method)
- **Migration Notes:**
  - Simple command execution

#### `SourceRemove_Click` (Line 539-547)
- **Purpose:** Removes source from project
- **Code:** Lines 539-547
- **Dependencies:**
  - `vm.RemoveSourceAsync` (ViewModel method)
- **Migration Notes:**
  - Simple command execution

### Notebook/Project Management

#### `ExportNotebooks_Click` (Line 549-578)
- **Purpose:** Exports all notebooks/projects to JSON
- **Code:** Lines 549-578
- **Dependencies:**
  - `StorageProvider.SaveFilePickerAsync`
  - `vm.ExportNotebooksAsync` (ViewModel method)
- **Migration Notes:**
  - Standard file save pattern

#### `ImportNotebooks_Click` (Line 580-608)
- **Purpose:** Imports notebooks/projects from JSON
- **Code:** Lines 580-608
- **Dependencies:**
  - `StorageProvider.OpenFilePickerAsync`
  - `vm.ImportNotebooksAsync` (ViewModel method)
- **Migration Notes:**
  - Standard file open pattern

#### `OpenCortexOutputFolder_Click` (Line 610-615)
- **Purpose:** Opens Cortex output folder
- **Code:** Lines 610-615
- **Dependencies:**
  - `FileHelper.ShellOpen`
  - Path construction (LocalApplicationData)
- **Migration Notes:**
  - Update path to Cortex-specific location

### Artifact Management

#### `ExportArtifactContent_Click` (Line 646-661)
- **Purpose:** Exports artifact content to file
- **Code:** Lines 646-661
- **Dependencies:**
  - `ExportArtifactContentAsync` (helper method)
- **Migration Notes:**
  - Calls async helper

#### `ExportArtifactContentAsync` (Line 663-689)
- **Purpose:** Async helper to export artifact content
- **Code:** Lines 663-689
- **Dependencies:**
  - `StorageProvider.SaveFilePickerAsync`
  - `FileHelper.GetExportSuggestion`
  - `File.WriteAllTextAsync`
- **Migration Notes:**
  - Helper method - keep logic

#### `ArtifactOpenFile_Click` (Line 691-696)
- **Purpose:** Opens artifact file
- **Code:** Lines 691-696
- **Dependencies:**
  - `TryGetDataContext<CortexArtifact>`
  - `FileHelper.ShellOpen`
- **Migration Notes:**
  - Standard file open

#### `ArtifactOpenVisual_Click` (Line 698-703)
- **Purpose:** Opens artifact visual/image file
- **Code:** Lines 698-703
- **Dependencies:**
  - `TryGetDataContext<CortexArtifact>`
  - `FileHelper.ShellOpen`
- **Migration Notes:**
  - Standard file open

#### `ArtifactOpenFolder_Click` (Line 705-718)
- **Purpose:** Opens folder containing artifact file
- **Code:** Lines 705-718
- **Dependencies:**
  - `TryGetDataContext<CortexArtifact>`
  - `Path.GetDirectoryName`
  - `FileHelper.ShellOpen`
- **Migration Notes:**
  - Standard folder open

#### `ArtifactCopyContent_Click` (Line 720-728)
- **Purpose:** Copies artifact content to clipboard
- **Code:** Lines 720-728
- **Dependencies:**
  - `CopyToClipboardAsync` (helper)
- **Migration Notes:**
  - Standard clipboard operation

#### `ArtifactCopyPath_Click` (Line 730-739)
- **Purpose:** Copies artifact file path to clipboard
- **Code:** Lines 730-739
- **Dependencies:**
  - `CopyToClipboardAsync` (helper)
- **Migration Notes:**
  - Standard clipboard operation

#### `ArtifactExportContent_Click` (Line 741-749)
- **Purpose:** Exports artifact content (wrapper)
- **Code:** Lines 741-749
- **Dependencies:**
  - `ExportArtifactContentAsync` (helper)
- **Migration Notes:**
  - Wrapper method

#### `ArtifactRename_Click` (Line 751-762)
- **Purpose:** Renames artifact
- **Code:** Lines 751-762
- **Dependencies:**
  - `DialogHelper.PromptAsync`
  - `vm.RenameArtifactAsync` (ViewModel method)
- **Migration Notes:**
  - Standard rename pattern

#### `ArtifactDelete_Click` (Line 764-772)
- **Purpose:** Deletes artifact
- **Code:** Lines 764-772
- **Dependencies:**
  - `vm.RemoveArtifactAsync` (ViewModel method)
- **Migration Notes:**
  - Simple command execution

---

## 🔧 Helper Methods Needed

### `TryGetDataContext<T>`
- **Purpose:** Extracts data context from sender (used extensively)
- **Location:** Helper method in MainWindow
- **Migration:** Create as static helper in CortexView or separate helper class

### `CopyToClipboardAsync`
- **Purpose:** Copies text to clipboard
- **Location:** Helper method in MainWindow
- **Migration:** Use Avalonia's `TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync()`

### `FileHelper.ShellOpen`
- **Purpose:** Opens file/folder in default application
- **Location:** Serenity.Helpers.FileHelper
- **Migration:** Create Cortex equivalent or use `Process.Start`

### `DialogHelper.PromptAsync`
- **Purpose:** Shows input dialog
- **Location:** Serenity.Helpers.DialogHelper
- **Migration:** Create Cortex equivalent or use Avalonia dialogs directly

### `FileHelper.GetExportSuggestion`
- **Purpose:** Gets file extension and suggested filename for export
- **Location:** Serenity.Helpers.FileHelper
- **Migration:** Create Cortex equivalent

---

## 📝 Migration Checklist

- [ ] Extract all event handlers from Serenity
- [ ] Create helper methods in Cortex
- [ ] Update namespaces (MainViewModel → CortexViewModel)
- [ ] Update dependency references
- [ ] Wire up events in CortexView.axaml
- [ ] Test all event handlers
- [ ] Update error handling
- [ ] Update status messages

---

## 🎯 Priority Order

1. **High Priority** (Core Functionality):
   - AddFiles_Click
   - AddUrl_Click
   - SourceRemove_Click
   - ArtifactDelete_Click
   - ExportArtifactContent_Click

2. **Medium Priority** (User Experience):
   - SourceOpen_Click
   - SourceRename_Click
   - ArtifactOpenFile_Click
   - ArtifactCopyContent_Click

3. **Low Priority** (Nice to Have):
   - SourceOnlyInclude_Click
   - SourceExclude_Click
   - ExportNotebooks_Click
   - ImportNotebooks_Click

---

**Last Updated:** January 2026
