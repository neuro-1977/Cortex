# Cortex Migration Plan from Serenity

**Date:** January 2026  
**Status:** In Progress  
**Goal:** Complete migration of Cortex code from Serenity to standalone Cortex project

---

## 📋 Executive Summary

This document outlines the comprehensive plan to migrate Cortex code from Serenity to the standalone Cortex project at `D:\_Code_\Cortex`. The migration includes UI components, ViewModels, event handlers, and ensuring the Cortex project is fully standalone.

---

## 🎯 Migration Objectives

1. **Complete Standalone Status:** Cortex should run independently with no Serenity dependencies
2. **Migrate Removed UI Code:** Move all CortexView-related code removed from Serenity
3. **Event Handlers:** Migrate all event handlers that were in MainWindow.axaml.cs
4. **Update Namespaces:** Change from `Serenity.*` to `Cortex.*` namespaces
5. **Documentation:** Update all documentation to reflect standalone status

---

## 📦 What to Migrate

### 1. Event Handlers (from MainWindow.axaml.cs)

**File Management:**
- `AddFiles_Click` - Opens file picker, adds sources
- `AddUrl_Click` - Prompts for URL, adds source

**Source Management:**
- `SourceOpen_Click` - Opens source file
- `SourceCopyPath_Click` - Copies source path
- `SourceRename_Click` - Renames source
- `SourceOnlyInclude_Click` - Includes source in context
- `SourceExclude_Click` - Excludes source from context
- `SourceRemove_Click` - Removes source

**Notebook/Project Management:**
- `ExportNotebooks_Click` - Exports notebooks
- `ImportNotebooks_Click` - Imports notebooks
- `OpenCortexOutputFolder_Click` - Opens output folder

**Artifact Management:**
- `ExportArtifactContent_Click` - Exports artifact content
- `ExportArtifactContentAsync` - Async export method
- `ArtifactOpenFile_Click` - Opens artifact file
- `ArtifactOpenVisual_Click` - Opens artifact visual
- `ArtifactOpenFolder_Click` - Opens artifact folder
- `ArtifactCopyContent_Click` - Copies artifact content
- `ArtifactCopyPath_Click` - Copies artifact path
- `ArtifactExportContent_Click` - Exports artifact content
- `ArtifactRename_Click` - Renames artifact
- `ArtifactDelete_Click` - Deletes artifact

### 2. Keyboard Shortcuts

**Cortex-Specific Shortcuts:**
- Ctrl+S (when in Cortex tab) - Save project
- Ctrl+O (when in Cortex tab) - Import notebooks
- Ctrl+N - New notebook/project
- Ctrl+W - Close artifact
- Delete - Remove selected artifact/source
- Arrow Keys (Up/Down) - Navigate artifacts
- Left/Right Arrows - Navigate slides (when artifact selected)
- F5 - Refresh artifact graph

**Implementation:** These should be in CortexView.axaml.cs code-behind

### 3. UI Components

**CortexView.axaml:**
- Already exists in Cortex project
- Needs namespace updates (`Serenity.Views` → `Cortex.App.Views`)
- May need event handler wiring

**CortexView.axaml.cs:**
- Needs event handlers migrated from MainWindow
- Update namespaces
- Wire up keyboard shortcuts

### 4. ViewModel Updates

**CortexViewModel.cs:**
- Already exists
- Needs namespace updates (`Serenity.ViewModels` → `Cortex.App.ViewModels`)
- Check for any Serenity-specific dependencies

---

## 🔧 Namespace Updates Required

### Files to Update:

1. **Cortex.App/Views/CortexView.axaml**
   - `xmlns:vm="clr-namespace:Serenity.ViewModels"` → `xmlns:vm="clr-namespace:Cortex.App.ViewModels"`
   - `xmlns:models="clr-namespace:Serenity.Models"` → `xmlns:models="clr-namespace:Cortex.App.Models"` (if needed)
   - `xmlns:converters="clr-namespace:Serenity.Converters"` → Create converters or reference from shared lib

2. **Cortex.App/Views/CortexView.axaml.cs**
   - `namespace Serenity.Views` → `namespace Cortex.App.Views`

3. **Cortex.App/ViewModels/CortexViewModel.cs**
   - `namespace Serenity.ViewModels` → `namespace Cortex.App.ViewModels`
   - Update `using Serenity.*` to appropriate Cortex namespaces

4. **Cortex.Core references:**
   - Keep `Serenity.Cortex.Core.*` OR migrate to `Cortex.Core.*`
   - Decision: Keep as-is for now (shared library)

---

## 📝 Step-by-Step Migration Plan

### Phase 1: Event Handlers Migration ⚠️ CRITICAL

**Priority:** High  
**Status:** Not Started

1. **Extract Event Handlers from Serenity**
   - Copy all Cortex event handlers from `MainWindow.axaml.cs`
   - Document which handlers exist and what they do

2. **Create Event Handlers in CortexView.axaml.cs**
   - Add event handlers to `CortexView.axaml.cs`
   - Wire up to ViewModel methods
   - Update namespace references

3. **Wire Up UI Events**
   - Update `CortexView.axaml` to call code-behind handlers
   - Test all UI interactions

### Phase 2: Keyboard Shortcuts

**Priority:** Medium  
**Status:** Not Started

1. **Implement Keyboard Shortcuts in CortexView**
   - Add `KeyDown` event handler to CortexView
   - Implement all Cortex-specific shortcuts
   - Test keyboard navigation

### Phase 3: Namespace Updates

**Priority:** High  
**Status:** Not Started

1. **Update All Namespaces**
   - Change `Serenity.*` to `Cortex.App.*` where appropriate
   - Keep `Serenity.Cortex.Core.*` (shared library) OR migrate
   - Update all XAML namespace declarations

2. **Resolve Dependencies**
   - Identify any Serenity-specific dependencies
   - Create Cortex equivalents or extract to shared library
   - Update project references

### Phase 4: Standalone App Setup

**Priority:** High  
**Status:** In Progress

1. **App.axaml Setup**
   - Configure Avalonia application
   - Set up resources and styles
   - Configure MainWindow

2. **MainWindow Setup**
   - Create or update MainWindow for Cortex
   - Set up tab structure (if needed) or single view
   - Configure window properties

3. **Program.cs Setup**
   - Configure application startup
   - Set up dependency injection (if used)
   - Configure logging and error handling

### Phase 5: Testing & Validation

**Priority:** High  
**Status:** Not Started

1. **Build Verification**
   - Ensure project builds without errors
   - Fix any compilation errors
   - Resolve missing references

2. **Functionality Testing**
   - Test all event handlers
   - Test keyboard shortcuts
   - Test artifact generation
   - Test source management
   - Test chat functionality

3. **Integration Testing**
   - Test with Cortex.Core library
   - Test with AI services
   - Test media generation

### Phase 6: Documentation

**Priority:** Medium  
**Status:** Ongoing

1. **Update Documentation**
   - Update README.md
   - Update PROJECT_BIBLE.md
   - Create migration completion notes
   - Document any breaking changes

---

## 🔍 Files to Review/Update

### Serenity Files (Source)
- `data/source/MainWindow/MainWindow.axaml.cs` - Event handlers (lines to be identified)
- `data/source/ViewModels/MainViewModel.cs` - Cortex methods (keep for Podcast Studio, document)

### Cortex Files (Destination)
- `Cortex.App/Views/CortexView.axaml` - Update namespaces
- `Cortex.App/Views/CortexView.axaml.cs` - Add event handlers
- `Cortex.App/ViewModels/CortexViewModel.cs` - Update namespaces
- `Cortex.App/App.axaml` - Configure app
- `Cortex.App/Program.cs` - Configure startup
- `Cortex.App/MainWindow.axaml` - Create/update main window

---

## ⚠️ Dependencies to Resolve

### Serenity-Specific Dependencies

1. **Converters**
   - `Serenity.Converters.*` - Need to create or extract to shared lib
   - Examples: CrewThumbnailConverter, CrewSelectionConverter, etc.

2. **Helpers**
   - `Serenity.Helpers.*` - May need Cortex equivalents
   - Examples: DialogHelper, FileHelper, PathHelper

3. **Services**
   - Check for any Serenity-specific services
   - Extract or create Cortex equivalents

### Shared Dependencies (Keep)

1. **Cortex.Core**
   - `Serenity.Cortex.Core.*` - Keep as shared library OR migrate namespace
   - Decision needed: Keep `Serenity.Cortex.Core` or change to `Cortex.Core`

2. **Models**
   - May reference `Serenity.Models` - Need to update or migrate

---

## 🎯 Migration Decision Points

### Decision 1: Cortex.Core Namespace

**Options:**
- **A:** Keep `Serenity.Cortex.Core` (shared between projects)
- **B:** Migrate to `Cortex.Core` (Cortex owns it)

**Recommendation:** **Option B** - Migrate to `Cortex.Core` for true independence
**Impact:** Requires updating both Serenity and Cortex references

### Decision 2: Converters Location

**Options:**
- **A:** Create Cortex.App.Converters (duplicate code)
- **B:** Move to shared Cortex.Core.Converters (requires shared lib)

**Recommendation:** **Option B** - Move common converters to Cortex.Core

### Decision 3: Helpers Location

**Options:**
- **A:** Create Cortex.App.Helpers (duplicate code)
- **B:** Move to shared Cortex.Core.Helpers
- **C:** Keep minimal, Cortex-specific only

**Recommendation:** **Option C** - Keep only Cortex-specific helpers

---

## 📊 Progress Tracking

### Completed ✅
- [x] Cortex project structure created
- [x] CortexView.axaml exists
- [x] CortexViewModel.cs exists
- [x] Initial documentation

### In Progress ⚠️
- [ ] Event handlers migration
- [ ] Namespace updates
- [ ] Standalone app setup

### Pending 📋
- [ ] Keyboard shortcuts
- [ ] Testing & validation
- [ ] Documentation updates
- [ ] Dependency resolution

---

## 🚀 Next Immediate Steps

1. **Extract Event Handlers**
   - Read MainWindow.axaml.cs
   - Identify all Cortex event handlers
   - Document their functionality

2. **Update CortexView.axaml.cs**
   - Add event handler methods
   - Wire up to ViewModel
   - Update namespaces

3. **Update Namespaces**
   - Start with CortexView.axaml
   - Update CortexViewModel.cs
   - Fix compilation errors

4. **Test Build**
   - Attempt to build Cortex project
   - Fix errors systematically
   - Document issues

---

**Last Updated:** January 2026  
**Next Review:** After Phase 1 completion
