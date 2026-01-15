---
name: resumework
description: This is a new rule
---

# Overview

I---
name: cortex-development-cycle
description: Autonomous Development Cycle - Self-Looping Workflow for Cortex
---

# Master "Resume Working" Prompt for Cortex Development

**Purpose:** Autonomous, methodical, plan-driven development workflow that continuously loops until all tasks are complete. This prompt is designed to be self-sustaining and adaptive.

**⚠️ CRITICAL: After completing Step 10 (Restart Autonomous Loop), you MUST re-read this MASTER_RESUME_WORK_PROMPT.md file from the beginning (Step 1) to continue the loop. Step 10 explicitly triggers this restart.**

---

## 🎯 Core Directive

You are continuing development on the Cortex project. You MUST be methodical, follow established plans, check for errors at every step, and NEVER deviate from the project's guidelines. You are NOT to add duplicate features, skip error checking, or work without a clear plan.

**⚠️ NUMBER 1 RULE: ALWAYS use `data/docs/` folder for ALL documentation. NEVER use `Docs/` at root.**

**⚠️ AUTONOMOUS LOOP: After completing Step 10 (Restart Autonomous Loop), immediately return to Step 1 to continue working. Re-read this entire MASTER_RESUME_WORK_PROMPT.md file to maintain context.**

**⚠️ CRITICAL: NO DEVIATION FROM PLANS**
- **NEVER deviate from documented plans** - Only implement what is explicitly documented in `data/docs/` files
- **NEVER imagine or invent features** - Do not add functionality that is not in the plans
- **NEVER do "other things"** - Stick strictly to the identified tasks from documentation
- **If a plan is 100% implemented:** Double-check all code, refactor if necessary, then form a new plan from remaining tasks
- **A plan is only complete when ALL tasks in ALL documents have been completed** - Check all documentation files for remaining tasks before considering work done

**⚠️ CORTEX-SPECIFIC: Namespace Migration**
- **ALWAYS use `Cortex.*` namespaces** - Never use `Serenity.*` namespaces in Cortex code
- **Update all references** - Change `Serenity.ViewModels` → `Cortex.App.ViewModels`, `Serenity.Views` → `Cortex.App.Views`, etc.
- **Core library:** Use `Cortex.Core.*` for business logic, not `Serenity.Cortex.Core.*`
- **Check for Serenity dependencies** - Remove or replace all Serenity-specific dependencies

---

## 📋 Pre-Work Checklist (MANDATORY - Do This First)

### Step 1: Understand Current State
1. **Read `data/docs/DEVELOPMENT_LOG.md`** - Check current cycle number and recent work
2. **Read `data/docs/DOCUMENTATION_INDEX.md`** - Understand documentation structure
3. **Read `data/docs/CORTEX_STATUS.md`** - Review current Cortex status
4. **Read `data/docs/MIGRATION_PLAN.md`** - Check migration progress and remaining tasks
5. **Read `data/docs/PROJECT_BIBLE.md`** - Understand Cortex architecture and features
6. **Check `data/docs/EVENT_HANDLERS_TO_MIGRATE.md`** - Identify unwired event handlers
7. **Review `data/docs/UPGRADE_ROADMAP.md`** - Check for upcoming upgrades from Serenity
8. **Check for documentation consolidation opportunities** - Look for redundant or mergeable docs in `data/docs/`
9. **Review recent user feedback** - Check for any new requirements or changes requested

### Step 2: Verify Build Status
1. **Run `dotnet build --no-incremental`** - MUST have 0 errors before proceeding
2. **If errors exist:** Fix them immediately, do NOT proceed with new features
3. **Check console output** - Look for warnings that might indicate issues
4. **Document build status** - Note any warnings in your work log
5. **IMPORTANT:** We build Cortex as a standalone application. The build should produce `Cortex.exe` in the output directory.

### Step 3: Identify Next Task
1. **Check for high-priority items** from `data/docs/MIGRATION_PLAN.md`
2. **Check `data/docs/EVENT_HANDLERS_TO_MIGRATE.md`** for unwired event handlers
3. **Check `data/docs/CORTEX_STATUS.md`** for incomplete setup items
4. **Check namespace issues** - Look for any remaining `Serenity.*` references
5. **Check for missing dependencies** - Identify any Serenity-specific dependencies that need replacement
6. **Check ALL documentation files in `data/docs/`** - Ensure you've reviewed every file that might contain tasks
7. **Review user feedback in recent conversations** - Prioritize user-requested changes
8. **PRIORITIZE:** Namespace fixes, missing dependencies, unwired event handlers first
9. **DO NOT make assumptions** - Only implement what is documented or explicitly requested
10. **VERIFY plan completeness:** A plan is only complete when ALL tasks in ALL documentation files are done - Check every doc file before considering work complete

### Step 4: Check for Duplicates
1. **Before adding ANY feature:** Search codebase for existing implementation
2. **If feature exists:** Enhance it, don't duplicate it
3. **If better version exists elsewhere:** Replace the basic version with the better one
4. **Document decision:** Note why you're enhancing vs. adding new

---

## 🔄 Development Cycle Workflow (MANDATORY)

**⚠️ 10-STEP PLAN STRUCTURE: This workflow follows a strict 10-step plan. Maintain this structure. Do not deviate from the plan format.**

### Step 4: Planning & Preparation
1. **Select ONE task** from identified opportunities
2. **Read relevant documentation** for that feature/improvement
3. **Check existing code** - Understand current implementation
4. **Research if stuck** - Use web search or codebase search to understand patterns
5. **Plan implementation** - Break into steps
6. **Verify no duplication** - Search codebase thoroughly
7. **Create TODO list** - Use `todo_write` tool with specific, actionable items

### Phase 2: Implementation (Task-by-Task)
For EACH implementation step:

1. **Make the change**
   - Follow MVVM pattern (ObservableProperty, RelayCommand)
   - Use existing services where possible
   - No placeholders - fully implement
   - Wire all UI components (buttons, dropdowns, etc.)
   - **Update namespaces** - Use `Cortex.*` not `Serenity.*`

2. **IMMEDIATELY build and verify**
   ```powershell
   dotnet build --no-incremental
   ```
   - **MUST have 0 errors** - If errors, fix them NOW
   - Do NOT proceed to next step with errors
   - Check console output for warnings
   - **NOTE:** We verify the build compiles. The application should run independently.

3. **Check for runtime issues**
   - Verify bindings are correct
   - Check for null reference possibilities
   - Verify commands are wired
   - Test edge cases mentally

4. **Update TODO status** - Mark step as completed

5. **Repeat** for next step

### Step 6: Error Checking & Code Review (After Each Feature)
1. **Build verification:**
   ```powershell
   dotnet build --no-incremental
   ```
   - Must show: `0 Error(s)`
   - Note any warnings (non-critical NuGet warnings are OK)

2. **Code review:**
   - Check for null checks
   - Verify error handling
   - Check for memory leaks (Dispose patterns)
   - Verify UI thread synchronization (Dispatcher.UIThread.Post)
   - **Check for Serenity namespaces** - Ensure all use `Cortex.*`

3. **Integration check:**
   - Verify feature works with existing code
   - Check for breaking changes
   - Verify API keys are linked (if needed)
   - Verify no Serenity dependencies remain

4. **Plan completeness check (MANDATORY):**
   - **If current plan is 100% implemented:** Double-check all code, refactor if necessary
   - **After refactoring:** Form a new plan from remaining tasks in ALL documentation files
   - **Verify all tasks complete:** Check ALL documentation files (`data/docs/*.md`) for remaining tasks
   - **A plan is only complete when ALL tasks in ALL documents are done** - Do not consider work complete until every doc file is checked

5. **Documentation consolidation (MANDATORY):**
   - **Check for redundant documentation** in `data/docs/` - Look for duplicate or mergeable docs
   - **Consolidate where possible** - Merge related docs, remove duplicates
   - **Update documentation index** - Remove references to deleted/merged docs
   - **Keep project clean** - Don't leave orphaned documentation files
   - **Update relevant `.md` files** in `data/docs/` - Mark completed items, update status
   - **Update `data/docs/DEVELOPMENT_LOG.md`** - Add cycle summary with completed items
   - **Update `data/docs/DOCUMENTATION_INDEX.md`** - Update cycle number, fix references
   - **Update feature status** - Mark in `data/docs/CORTEX_STATUS.md` or `data/docs/MIGRATION_PLAN.md`

### Step 7: Documentation Consolidation & Updates (MANDATORY)
1. **Consolidate documentation (if needed):**
   - Merge redundant docs in `data/docs/`
   - Remove duplicates
   - Update index

2. **Update `data/docs/DEVELOPMENT_LOG.md`:**
   - Add cycle number
   - Add cycle summary with completed items, errors fixed, user feedback addressed

3. **Update relevant feature docs in `data/docs/`:**
   - Mark completed items
   - Update status
   - Note next steps

4. **Update `data/docs/DOCUMENTATION_INDEX.md`:**
   - Update current cycle number
   - Remove references to deleted/merged docs
   - Add new documentation if created

### Step 8: Git Operations (After Each Complete Feature)
**⚠️ CRITICAL RULE: Always commit changes. Only push when explicitly asked by the user.**

1. **Stage all changes:**
   ```powershell
   git add -A
   ```

2. **Commit with descriptive message (MANDATORY - Always do this):**
   ```powershell
   git commit -m "Cycle [N]: [Feature Name] - [Brief description]

   - What was implemented
   - What was fixed
   - What user requested
   - Any breaking changes
   - Next steps"
   ```
   - Include cycle number
   - Be specific about what was implemented
   - Reference user requests and feedback
   - Document what's next
   - **ALWAYS commit after completing a feature/cycle**

3. **Push to repositories (ONLY when user explicitly asks):**
   ```powershell
   git push https://github.com/neuro-1977/Cortex.git main
   git push https://gitlab.com/Neuro1977/Cortex.git main
   ```
   - **DO NOT push automatically** - Only push when user explicitly requests it
   - If user says "commit and push" or "push", then push
   - If user only says "commit", do NOT push

4. **Verify commit succeeded** - Check exit codes

### Step 9: Build Verification (FINAL STEP - Before Continuing)
**This is the MANDATORY final step before moving to the next task/cycle. We verify the application builds successfully.**

1. **Verify build:**
   ```powershell
   dotnet build --no-incremental
   ```
   - **MUST have 0 errors** - If errors, fix them NOW
   - Check console output for warnings
   - Verify all projects build successfully

2. **Verify application structure:**
   - Check that `Cortex.App` builds
   - Check that `Cortex.Core` builds
   - Verify output directory contains expected files

3. **After build verification:**
   - You have completed the cycle
   - All changes are committed
   - Documentation is consolidated and updated
   - Build is verified (0 errors for application)
   - **CRITICAL: Proceed to Step 10 to restart the autonomous loop**

### Step 10: Restart Autonomous Loop (MANDATORY FINAL STEP)
**This step ALWAYS executes after Step 9, regardless of build success or failure.**

1. **CRITICAL: Re-read this entire MASTER_RESUME_WORK_PROMPT.md file from the beginning** - Maintain context and continue workflow
2. **Return to Step 1 (Pre-Work Checklist)** - Start the next cycle immediately
3. **Continue working** - Don't stop until all documented tasks are complete
4. **Adapt as needed** - Incorporate user feedback while working on planned tasks
5. **If build had errors:** Note them but continue with next task - build issues will be addressed in future cycles
6. **Check for namespace issues** - Always verify `Cortex.*` namespaces are used, not `Serenity.*`

**⚠️ AUTONOMOUS LOOP TRIGGER: This step ensures MASTER_RESUME_WORK_PROMPT.md is re-read and the cycle restarts. After completing this step, you MUST immediately begin Step 1 again.**

**Note:** 
- You MUST run `dotnet build --no-incremental` as Step 9 to verify the build
- If build errors occur, note them and proceed to Step 10 (restart loop)
- After Step 9 (regardless of outcome), ALWAYS proceed to Step 10 to restart the loop

---

## 🚫 Critical Rules (NEVER Deviate)

### Rule 1: No Code Removals
- **NEVER** remove existing functionality
- Maintain all existing features
- Enhance, don't replace (unless replacing with better version)

### Rule 2: No Placeholders
- **NEVER** leave `TODO`, `FIXME`, or `NotImplementedException` (except in converters where intentional)
- Fully implement all features
- Wire all UI components
- Add proper error handling

### Rule 3: No Duplication
- **ALWAYS** check for existing features before adding new ones
- If feature exists: Enhance it
- If better version exists elsewhere: Replace basic version with better one
- Document why you're enhancing vs. adding

### Rule 4: Error Checking is MANDATORY
- Build after EVERY change
- Fix errors IMMEDIATELY
- Do NOT proceed with errors present
- Check console output for warnings
- **After each task:** Check if your last task introduced any errors
- **If errors can't be fixed immediately:** Add them to your plan for next cycle

### Rule 5: Documentation is MANDATORY
- **ALWAYS use `data/docs/` folder** - This is the NUMBER 1 RULE
- **Consolidate documentation continuously** - Merge redundant docs, remove duplicates
- **Keep project clean** - Don't leave orphaned or redundant documentation
- Update `data/docs/DEVELOPMENT_LOG.md` after each cycle
- Update relevant feature documentation in `data/docs/`
- Update cycle number in `data/docs/DOCUMENTATION_INDEX.md`
- Document decisions and rationale
- **Before starting new work:** Check for docs that can be consolidated in `data/docs/`
- **Log everything you do** - Every change, every decision, every fix

### Rule 6: Technology Stack is FIXED
- **ONLY** Avalonia for UI
- **ONLY** C# for logic
- **NO** Python or non-C# alternatives (except for existing Python scripts in tools/)
- Even if it requires more code, use C#

### Rule 7: Modularity is REQUIRED
- Split oversized classes into separate files
- Use clear interfaces
- Keep services decoupled
- Follow existing patterns

### Rule 8: API Keys Must Be Linked
- Link all API keys from `config/cortex.env`
- Use existing configuration system
- Don't hardcode keys

### Rule 9: Follow Documentation, Not Assumptions
- **ONLY implement what is documented** in `data/docs/` files
- **ONLY implement what the user explicitly requests**
- **DO NOT make your own decisions** about adding systems or features
- **DO NOT deviate from plans** - Stick strictly to identified tasks
- **DO NOT imagine or invent features** - Only work on documented tasks
- **DO NOT do "other things"** - Focus only on planned work
- **If plan is 100% implemented:** Double-check code, refactor if needed, then form new plan from remaining tasks
- **Research when stuck** - Use web search or codebase search
- **Ask for clarification** if requirements are unclear
- **Plan completeness:** A plan is only complete when ALL tasks in ALL documentation files are done

### Rule 10: User Feedback Integration
- **Adapt to user feedback** while working on outstanding tasks
- **Prioritize user-requested changes** when they conflict with planned work
- **Keep things wired up** as you go
- **Test and error check** after every task
- **Document user feedback** in development log

### Rule 11: Git Commit vs Push (MANDATORY)
- **ALWAYS commit changes** after completing a feature/cycle - This is mandatory
- **ONLY push when explicitly asked** - Do NOT push automatically
- **If user says "commit and push"** - Then commit AND push
- **If user says "commit"** - Then commit only, do NOT push
- **If user says "push"** - Then push (assuming changes are already committed)
- **Default behavior:** Commit always, push only when requested

### Rule 12: Cortex Namespace Migration (CRITICAL)
- **ALWAYS use `Cortex.*` namespaces** - Never use `Serenity.*` in Cortex code
- **Update all references** - Change `Serenity.ViewModels` → `Cortex.App.ViewModels`
- **Core library:** Use `Cortex.Core.*` for business logic
- **Check for Serenity dependencies** - Remove or replace all Serenity-specific dependencies
- **Verify after each change** - Ensure no `Serenity.*` namespaces remain

---

## 🔍 Task Discovery Process (Continuous)

### During Development:
1. **While reading code:** Note inefficiencies, unwired components, optimization opportunities, namespace issues
2. **While implementing:** Identify related improvements
3. **While testing:** Note missing features or usability issues
4. **While documenting:** Identify gaps in documentation

### After Each Feature:
1. **Review what you just built** - Could it be enhanced?
2. **Check related features** - Are they consistent?
3. **Look for patterns** - Can you create reusable components?
4. **Check audits** - Did you address any audit items?
5. **Check for errors introduced** - Did your last task break anything?
6. **Check for namespace issues** - Are all namespaces using `Cortex.*`?

### Before Starting New Feature:
1. **Review `data/docs/MIGRATION_PLAN.md`** - What's next?
2. **Check `data/docs/EVENT_HANDLERS_TO_MIGRATE.md`** - Any incomplete items?
3. **Review recent cycles** in `data/docs/DEVELOPMENT_LOG.md` - What was left incomplete?
4. **Check for dependencies** - Does this feature depend on something else?
5. **Review user feedback** - Any new requests or changes?
6. **Check for namespace issues** - Are there any remaining `Serenity.*` references?

---

## 📝 Documentation Requirements

### Documentation Consolidation (Continuous Task):
1. **Before starting work:** Check for redundant documentation
   - Look for duplicate content across multiple files
   - Identify mergeable documentation (e.g., plan + implementation = single doc)
   - Check for outdated "FINAL" reports or session summaries
   - Look for docs that reference each other but could be one file

2. **During work:** Note consolidation opportunities
   - If you see related docs that could be merged, note it
   - If you create new docs, check if they should merge with existing ones

3. **After each cycle:** Consolidate where possible
   - Merge related documentation files in `data/docs/`
   - Remove redundant or duplicate content
   - Update `data/docs/DOCUMENTATION_INDEX.md` to reflect consolidations
   - Delete orphaned documentation files
   - **Keep project clean** - Documentation should be organized and non-redundant

### After Each Cycle:
1. **Consolidate documentation (if needed):**
   - Merge redundant docs in `data/docs/`
   - Remove duplicates
   - Update index

2. **Update `data/docs/DEVELOPMENT_LOG.md`:**
   - Add cycle number
   - Add cycle summary with:
     - Focus/theme
     - Completed items (✅)
     - Documentation consolidations (if any)
     - Errors encountered and fixed
     - User feedback addressed
     - Status/next steps

3. **Update relevant feature docs in `data/docs/`:**
   - Mark completed items
   - Update status
   - Note next steps

4. **Update `data/docs/DOCUMENTATION_INDEX.md`:**
   - Update current cycle number
   - Remove references to deleted/merged docs
   - Add new documentation if created

---

## 🎯 Priority Framework

### High Priority (Do First):
1. **User-requested changes** - Always prioritize explicit user requests
2. **Namespace fixes** - Change `Serenity.*` to `Cortex.*`
3. **Missing dependencies** - Replace Serenity-specific dependencies
4. **Unwired event handlers** from `data/docs/EVENT_HANDLERS_TO_MIGRATE.md`
5. **Build errors** - Fix any compilation errors
6. **Errors from last task** - Fix any issues introduced

### Medium Priority:
1. **Migration tasks** from `data/docs/MIGRATION_PLAN.md`
2. **Code quality improvements** (refactoring, optimization)
3. **Documentation updates**

### Low Priority:
1. **Nice-to-have features**
2. **UI polish** (unless blocking usability)
3. **Documentation-only updates** (unless critical)

---

## 🔧 Debugging Protocol

### When Errors Occur:
1. **STOP** - Do not proceed
2. **Read error message** - Understand what failed
3. **Check recent changes** - What did you just modify?
4. **Search codebase** - How is this pattern used elsewhere?
5. **Research if needed** - Use web search to understand the issue
6. **Fix immediately** - Don't leave errors
7. **Rebuild and verify** - Must have 0 errors
8. **Document fix** - Note what was wrong and how you fixed it
9. **If can't fix immediately:** Add to plan for next cycle

### When Warnings Occur:
1. **Assess severity** - Is it critical?
2. **NuGet warnings** - Usually OK (dependency resolution)
3. **Code warnings** - Usually need fixing
4. **Document** - Note warnings in cycle summary

### When Build Fails:
1. **Read full error output** - Not just last 25 lines
2. **Check for compilation errors** - Missing references, syntax errors
3. **Check for missing files** - Did you create all needed files?
4. **Verify namespaces** - Are they correct? Using `Cortex.*`?
5. **Check using statements** - Are all imports present?
6. **Check for Serenity dependencies** - Are there any remaining `Serenity.*` references?

---

## 📊 Quality Checklist (Before Committing)

### Code Quality:
- [ ] No compilation errors (0 errors)
- [ ] No critical warnings
- [ ] Proper null checks
- [ ] Error handling present
- [ ] Dispose patterns for IDisposable
- [ ] UI thread synchronization (if needed)
- [ ] No hardcoded paths (use AppContext.BaseDirectory, Environment paths)
- [ ] No magic numbers (use constants)
- [ ] Follows MVVM pattern
- [ ] Uses existing services where possible
- [ ] **All namespaces use `Cortex.*`** - No `Serenity.*` references

### Feature Completeness:
- [ ] Fully implemented (no placeholders)
- [ ] All UI components wired
- [ ] Commands bound correctly
- [ ] Properties observable (ObservableProperty)
- [ ] Error messages user-friendly
- [ ] Works with existing features

### Documentation:
- [ ] Documentation consolidated in `data/docs/` (redundant docs merged/removed)
- [ ] `data/docs/DOCUMENTATION_INDEX.md` updated (references fixed, cycle number updated)
- [ ] `data/docs/DEVELOPMENT_LOG.md` updated with cycle summary
- [ ] Feature documentation updated in `data/docs/` (if applicable)
- [ ] Status marked in relevant plan/roadmap
- [ ] No orphaned or duplicate documentation files
- [ ] **ALL documentation paths use `data/docs/` (NUMBER 1 RULE)**
- [ ] User feedback documented
- [ ] Errors encountered documented

### Testing Readiness:
- [ ] Builds successfully
- [ ] No runtime errors expected
- [ ] Feature can be tested
- [ ] Application runs independently

---

## 🎬 Example Workflow

### Start of Session:
```
1. Read data/docs/DEVELOPMENT_LOG.md → Current cycle: 1
2. Read data/docs/MIGRATION_PLAN.md → Next: Fix namespaces
3. Run dotnet build → 2 errors (Serenity namespaces)
4. Check existing code → Understand current implementation
5. Create TODO: [1] Update App.axaml namespaces, [2] Update CortexView namespaces, [3] Update ViewModel namespaces, [4] Build & test
```

### During Implementation:
```
1. Make change (update namespace)
2. dotnet build → 1 error remaining ✓
3. Fix next namespace issue
4. dotnet build → 0 errors ✓
5. Update TODO status
6. Repeat for next step
```

### After Feature:
```
1. dotnet build → 0 errors ✓
2. Review code: Null checks ✓, Error handling ✓, UI thread ✓, Namespaces ✓
3. Check for documentation consolidation in data/docs/ → Merge related docs if found
4. Update data/docs/DEVELOPMENT_LOG.md → Cycle 1: Namespace migration
5. Update data/docs/MIGRATION_PLAN.md → Mark namespaces as completed
6. Update data/docs/DOCUMENTATION_INDEX.md → Fix any broken references
7. git add -A
8. git commit -m "Cycle 1: Namespace Migration - Updated all Serenity.* to Cortex.*" (ALWAYS commit)
9. git push (ONLY if user explicitly asks - do NOT push automatically)
10. Build verification → Run dotnet build → Check result
11. **CRITICAL: Re-read MASTER_RESUME_WORK_PROMPT.md from beginning (Step 10) → This triggers loop restart → Begin Step 1 again**
```

---

## 🚨 Red Flags (STOP and Fix)

If you encounter any of these, STOP and fix:

1. **Build errors** - Must be 0 errors
2. **Duplicate features** - You're adding something that already exists
3. **Placeholders** - You left TODO/FIXME/NotImplemented
4. **Missing error handling** - Code can throw unhandled exceptions
5. **Hardcoded paths** - Using `D:\_Code_\Cortex` or similar
6. **Missing documentation** - Feature implemented but not documented
7. **Breaking existing features** - Your change broke something else
8. **Skipping error check** - You didn't build after a change
9. **Making assumptions** - Implementing features not in documentation
10. **Not following user feedback** - Ignoring explicit user requests
11. **Serenity namespaces** - Using `Serenity.*` instead of `Cortex.*`
12. **Serenity dependencies** - Any remaining Serenity-specific dependencies

---

## 📚 Reference Documents (Read Before Starting)

### Must Read (ALL in `data/docs/`):
- `data/docs/DEVELOPMENT_LOG.md` - Current state and cycle history
- `data/docs/MIGRATION_PLAN.md` - Migration tasks and status
- `data/docs/CORTEX_STATUS.md` - Current Cortex status
- `data/docs/DOCUMENTATION_INDEX.md` - Documentation structure
- `data/docs/PROJECT_BIBLE.md` - Complete Cortex reference

### Reference as Needed (ALL in `data/docs/`):
- `data/docs/EVENT_HANDLERS_TO_MIGRATE.md` - Unwired event handlers
- `data/docs/UPGRADE_ROADMAP.md` - Upcoming upgrades from Serenity
- `data/docs/SERENITY_INTEGRATION.md` - Integration architecture

---

## 🎯 Success Criteria

A successful development cycle (following 10-step plan):
1. ✅ Pre-work completed (Steps 1-3) - Current state understood, build verified, task identified
2. ✅ Planning completed (Step 4) - Task selected, documented, planned, TODO list created
3. ✅ Implementation completed (Step 5) - All changes made, builds verified, no errors
4. ✅ Code review completed (Step 6) - Errors checked, code reviewed, integration verified
5. ✅ Documentation completed (Step 7) - Docs consolidated, updated, indexed
6. ✅ Git operations completed (Step 8) - Changes committed (and pushed if requested)
7. ✅ Build verification attempted (Step 9) - **Build verified (0 errors)**
8. ✅ Loop restart (Step 10) - **CRITICAL: Re-read MASTER_RESUME_WORK_PROMPT.md from beginning to continue autonomous loop** - **THIS TRIGGERS THE LOOP TO RESTART**
9. ✅ **After Step 10:** Immediately begin Step 1 again (autonomous loop continues)

---

## 💡 Remember

- **Methodical > Fast** - Take time to do it right
- **Enhance > Duplicate** - Always check for existing features
- **Error-free > Feature-complete** - Fix errors before adding features
- **Documented > Implemented** - Update docs as you go
- **Plan-driven > Ad-hoc** - Follow established plans and audits
- **Quality > Quantity** - One well-done feature > multiple incomplete ones
- **Research when stuck** - Don't guess, search and learn
- **Follow documentation** - Only implement what's documented or requested
- **Adapt to feedback** - User requests take priority
- **Log everything** - Every change, every decision, every fix
- **Cortex namespaces** - Always use `Cortex.*`, never `Serenity.*`

---

## 🔄 Autonomous Loop (CRITICAL)

**After completing Step 10 (Restart Autonomous Loop):**

1. **IMMEDIATELY re-read this entire MASTER_RESUME_WORK_PROMPT.md file from the beginning (Step 1)** - This triggers the autonomous loop restart
2. **Return to Step 1 (Pre-Work Checklist)** - Start the next cycle immediately
3. **Continue working** - Don't stop until all documented tasks are complete
4. **Adapt as needed** - Incorporate user feedback while working on planned tasks

**This prompt ensures you:**
- Never skip error checking
- Never duplicate features
- Always follow plans
- Always consolidate documentation (keep project clean)
- Always document work
- Always verify builds
- Always commit properly
- Always find next tasks
- Always work methodically
- **ALWAYS proceed to Step 10 after Step 9, regardless of build outcome**
- **Step 10 ALWAYS triggers re-reading MASTER_RESUME_WORK_PROMPT.md from beginning, restarting the loop**
- **Always re-read MASTER_RESUME_WORK_PROMPT.md from beginning after Step 10 to continue the autonomous loop**
- **Always adapt to user feedback while maintaining planned work**
- **Always research when stuck instead of making assumptions**
- **Always use `Cortex.*` namespaces, never `Serenity.*`**

**Note:** 
- You MUST run `dotnet build --no-incremental` as Step 9 to verify the build
- If build errors occur, note them and proceed to Step 10 (restart loop)
- Step 10 is the MANDATORY final step that triggers the autonomous loop restart by re-reading MASTER_RESUME_WORK_PROMPT.md

---

**END OF MASTER_RESUME_WORK_PROMPT.MD - After completing Step 10, this file is automatically re-read from the beginning (Step 1) to continue the autonomous development loop. The loop continues indefinitely until all documented tasks are complete.**

