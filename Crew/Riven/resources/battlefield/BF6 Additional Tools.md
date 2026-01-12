# BF6 Portal Additional Tools Documentation

## 1. bfportal-vitest-mock
**Source**: https://github.com/link1345/bfportal-vitest-mock
**Description**: A mocking tool that makes it easier to unit test Battlefield 6 Portal (BF Portal) TypeScript scripts with Vitest.

### Installation
```bash
npm install -D bfportal-vitest-mock vitest typescript
```

### Usage Overview
1.  **Prepare the SDK .d.ts**: Place the official BF Portal SDK `index.d.ts` inside your project (e.g., `./code/mod/index.d.ts`).
2.  **Generate the Support File**: Run the generate CLI command to create a project-specific support file.
    ```bash
    npx bfportal-vitest-mock generate --sdk ./code/mod/index.d.ts --out ./test-support/bfportal-vitest-mock.generated.ts
    ```
3.  **Mocking mod in Test Setup**: Create a Vitest setup file (e.g., `test/setup/bfportal.ts`) and call `setupBfPortalMock`.
    ```typescript
    import { setupBfPortalMock, type BfPortalModMock } from "../test-support/bfportal-vitest-mock.generated";
    import stringkeys from "../src/strings.json";

    export let modMock: BfPortalModMock;

    beforeEach(() => {
        modMock = setupBfPortalMock({
            GetObjId: () => 1,
            Message(msg, arg0, arg1, arg2) {
                return { __test: true, msg, args: [arg0, arg1, arg2] } as unknown as mod.Message;
            },
        });
        (modMock as any).stringkeys = stringkeys;
    });
    ```
4.  **Using It in Test Code**: Use `mod` and `modMock` in your tests.
    ```typescript
    import { describe, it, expect } from "vitest";
    import { modMock } from "../setup/bfportal";
    import { OnPlayerJoinGame } from "../../src/WarFactory";

    describe("OnPlayerJoinGame", () => {
        it("shows a message when a player joins the game", async () => {
            const fakePlayer = { __test: true } as unknown as mod.Player;
            await OnPlayerJoinGame(fakePlayer);
            expect(modMock.DisplayNotificationMessage).toHaveBeenCalled();
        });
    });
    ```

---

## 2. BF6 Portal Quickstart
**Source**: https://github.com/gazreyn/bf6-portal-quickstart
**Description**: A TypeScript-based development environment for creating Battlefield 6 Portal scripts with modern tooling, string localization, and helpful VS Code snippets.

### Features
-   **String Localization System**: Automatically extracts and manages localized strings using the `s` macro.
    ```typescript
    import { s } from "./lib/string-macro";
    const message = mod.Message(s`Hello, {}!`, s`World`);
    ```
    Build output generates `Strings.json` with deduplicated keys.
-   **VS Code Event Snippets**: Includes snippets for all supported Battlefield Portal events (e.g., `OnPlayerDeployed`, `OnGameModeStarted`).
-   **Build System**: Bundles TypeScript code into a single `Script.js` file and extracts strings to `Strings.json`.

### Project Structure
```
src/
├── main.ts             # Entry point
├── lib/
│   └── string-macro.ts # String localization macro
.vscode/
└── events.code-snippets # VS Code snippets
dist/
├── Script.js           # Compiled script
└── Strings.json        # Extracted localized strings
```

---

## 3. BF Log Watcher
**Source**: https://github.com/gazreyn/bf-log-watcher
**Description**: A simple Node.js script that watches and monitors Battlefield™ 6 Portal log files for real-time updates.

### How it works
The script monitors the `PortalLog.txt` file in your system's temporary directory (`%TEMP%\Battlefield™ 6\PortalLog.txt`) and displays new content as it's written to the file.

### Usage
```bash
npm start
```
The script will display new log entries in real-time. Stop with `Ctrl+C`.

### Dependencies
-   `chokidar`: File system watcher for monitoring log file changes.
