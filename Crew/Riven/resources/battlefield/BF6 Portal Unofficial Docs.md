# BF6 Portal TypeScript SDK Documentation

> **Credits**: Credits goes to "krazyjakee" Jake Cattrall for creating the documentation.
> **Source**: https://github.com/NodotProject/Unofficial-BF6-Portal-SDK-Docs

## Introduction

The BF6 Portal TypeScript SDK is an unofficial documentation project that provides comprehensive guidance for developing custom game modes and logic for Battlefield 6 Portal using TypeScript. This SDK enables modders to create interactive multiplayer experiences by bridging TypeScript code with Godot game engine scenes through an event-driven API architecture.

The SDK centers around the `mod` namespace, which exposes all game functionality including player management, team operations, UI systems, game logic objects, and event handlers. Developers can hook into game lifecycle events, manipulate Godot scene objects via unique identifiers (`ObjId`), and build complex UI hierarchies declaratively. The modlib utility library extends the core API with helper functions for common tasks like array operations, conditional logic, state management, and asynchronous workflows.

## Core Event Handlers

### Game Mode Lifecycle Events
Event handlers that fire during game mode initialization and teardown, providing hooks to set up game logic and perform cleanup operations.

```typescript
// Initialize game mode when it starts
export function OnGameModeStarted() {
    // Set up game variables, spawn points, UI elements
    const team1 = mod.GetTeam(1);
    const team2 = mod.GetTeam(2);

    mod.SetTeamTickets(team1, 100);
    mod.SetTeamTickets(team2, 100);

    // Display initial message to all players
    ShowEventGameModeMessage(mod.CreateMessage("Game Started!"));
}

// Clean up when game mode ends
export function OnGameModeEnding() {
    // Perform cleanup, calculate final scores
    const team1 = mod.GetTeam(1);
    const winner = mod.GetTeamTickets(team1) > 0 ? "Team 1" : "Team 2";

    ShowEventGameModeMessage(mod.CreateMessage(`${winner} Wins!`));
}
```

### Player Connection Events
Event handlers triggered when players join or leave the game session, useful for managing player rosters and displaying welcome/goodbye messages.

```typescript
// Track players joining the game
export function OnPlayerJoinGame(player: mod.Player) {
    const playerId = getPlayerId(player);
    const playerName = mod.GetPlayerName(player);

    // Welcome message to the joining player
    ShowNotificationMessage(
        mod.CreateMessage(`Welcome ${playerName}!`),
        player
    );

    // Broadcast to all other players
    ShowNotificationMessage(
        mod.CreateMessage(`${playerName} joined the game`)
    );
}

// Handle player disconnections
export function OnPlayerLeaveGame(player: mod.Player) {
    const playerName = mod.GetPlayerName(player);

    ShowNotificationMessage(
        mod.CreateMessage(`${playerName} left the game`)
    );
}
```

### Player Combat Events
Event handlers for combat interactions including deployments, deaths, and damage, enabling custom scoring and respawn logic.

```typescript
// Track when players spawn into the world
export function OnPlayerDeployed(player: mod.Player) {
    const team = mod.GetPlayerTeam(player);
    const teamId = getTeamId(team);

    // Give player custom loadout
    mod.GiveGadget(player, mod.Gadgets.MedicBag);

    ShowNotificationMessage(
        mod.CreateMessage("You have deployed!"),
        player
    );
}

// Handle player deaths with full context
export function OnPlayerDied(
    player: mod.Player,
    killer: mod.Player,
    deathType: mod.DeathType,
    weaponUnlock: mod.WeaponUnlock
) {
    const playerName = mod.GetPlayerName(player);
    const killerName = mod.GetPlayerName(killer);

    // Award points to killer
    if (killer && killer !== player) {
        const killerTeam = mod.GetPlayerTeam(killer);
        mod.AddTeamTickets(killerTeam, -1); // Reduce enemy tickets

        ShowNotificationMessage(
            mod.CreateMessage(`You eliminated ${playerName}`),
            killer
        );
    }

    // Custom respawn delay
    mod.SetPlayerRespawnDelay(player, 5.0);
}
```

### Area Trigger Events
Event handlers for spatial triggers that fire when players enter or exit defined volumes in the Godot scene.

```typescript
// Define trigger areas in your Godot scene with ObjId properties
const CAPTURE_ZONE_ID = 100;
const RESTRICTED_AREA_ID = 101;

// Player enters a trigger volume
export function OnPlayerEnterAreaTrigger(
    player: mod.Player,
    trigger: mod.AreaTrigger
) {
    const triggerId = mod.GetObjId(trigger);

    if (triggerId === CAPTURE_ZONE_ID) {
        // Start capture process
        const playerTeam = mod.GetPlayerTeam(player);
        const captureState = getTeamCondition(playerTeam, 1);

        if (captureState.update(true)) {
            ShowNotificationMessage(
                mod.CreateMessage("Capturing objective..."),
                player
            );
        }
    } else if (triggerId === RESTRICTED_AREA_ID) {
        ShowNotificationMessage(
            mod.CreateMessage("WARNING: Leaving combat area!"),
            player
        );
    }
}

// Player exits a trigger volume
export function OnPlayerExitAreaTrigger(
    player: mod.Player,
    trigger: mod.AreaTrigger
) {
    const triggerId = mod.GetObjId(trigger);

    if (triggerId === CAPTURE_ZONE_ID) {
        const playerTeam = mod.GetPlayerTeam(player);
        const captureState = getTeamCondition(playerTeam, 1);
        captureState.update(false);

        ShowNotificationMessage(
            mod.CreateMessage("Left capture zone"),
            player
        );
    }
}
```

### Interact Point Events
Event handler for interactive objects in the scene that players can trigger with a button press.

```typescript
const DOOR_INTERACT_ID = 200;
const WEAPON_STATION_ID = 201;
const doorOpened = new Map<number, boolean>();

export function OnPlayerInteract(
    player: mod.Player,
    interactPoint: mod.InteractPoint
) {
    const interactId = mod.GetObjId(interactPoint);

    if (interactId === DOOR_INTERACT_ID) {
        // Toggle door state
        const isOpen = doorOpened.get(interactId) || false;
        doorOpened.set(interactId, !isOpen);

        ShowNotificationMessage(
            mod.CreateMessage(isOpen ? "Door closing..." : "Door opening..."),
            player
        );
    } else if (interactId === WEAPON_STATION_ID) {
        // Give player special weapon
        mod.GiveWeapon(player, mod.Weapons.M5A3_AssaultRifle);

        ShowNotificationMessage(
            mod.CreateMessage("Weapon equipped!"),
            player
        );
    }
}
```

## Player & Team Management Functions

### Get Player by ID
Retrieve a player object using their unique player ID, essential for tracking and manipulating individual players.

```typescript
// Store player IDs when they join
const activePlayers = new Set<number>();

export function OnPlayerJoinGame(player: mod.Player) {
    const playerId = getPlayerId(player); // Helper: mod.GetObjId(player)
    activePlayers.add(playerId);
}

// Retrieve and manipulate player by ID later
function rewardPlayer(playerId: number, points: number) {
    const player = mod.GetPlayer(playerId);
    if (player) {
        const currentScore = mod.GetPlayerScore(player);
        mod.SetPlayerScore(player, currentScore + points);

        ShowNotificationMessage(
            mod.CreateMessage(`+${points} points!`),
            player
        );
    }
}

// Clean up on disconnect
export function OnPlayerLeaveGame(player: mod.Player) {
    const playerId = getPlayerId(player);
    activePlayers.delete(playerId);
}
```

### Get Team by ID
Retrieve a team object using its team ID for team-based operations and scoring.

```typescript
const TEAM_US = 1;
const TEAM_RU = 2;

export function OnGameModeStarted() {
    const teamUS = mod.GetTeam(TEAM_US);
    const teamRU = mod.GetTeam(TEAM_RU);

    // Initialize team tickets
    mod.SetTeamTickets(teamUS, 150);
    mod.SetTeamTickets(teamRU, 150);

    // Set team names
    mod.SetTeamName(teamUS, "United States");
    mod.SetTeamName(teamRU, "Russian Forces");
}

// Award points to entire team
function awardTeamPoints(teamId: number, tickets: number) {
    const team = mod.GetTeam(teamId);
    const currentTickets = mod.GetTeamTickets(team);
    mod.SetTeamTickets(team, currentTickets + tickets);

    const teamName = mod.GetTeamName(team);
    ShowHighlightedGameModeMessage(
        mod.CreateMessage(`${teamName} +${tickets} tickets`),
        undefined,
        team
    );
}
```

### Get Players in Team
Retrieve all players belonging to a specific team for team-wide operations.

```typescript
function teleportTeamToPosition(teamId: number, position: mod.Vector) {
    const team = mod.GetTeam(teamId);
    const players = getPlayersInTeam(team); // Returns mod.Array of players
    const playerArray = ConvertArray(players); // Convert to standard array

    playerArray.forEach((player, index) => {
        // Offset each player slightly to avoid overlap
        const offset = new mod.Vector(index * 2, 0, 0);
        const playerPos = mod.VectorAdd(position, offset);

        mod.Teleport(player, playerPos, 0);

        ShowNotificationMessage(
            mod.CreateMessage("Teleported to base!"),
            player
        );
    });
}

// Example: Heal all team members
function healTeam(teamId: number) {
    const team = mod.GetTeam(teamId);
    const players = ConvertArray(getPlayersInTeam(team));

    players.forEach(player => {
        mod.SetPlayerHealth(player, 100);
    });

    ShowHighlightedGameModeMessage(
        mod.CreateMessage("Team healed!"),
        undefined,
        team
    );
}
```

## Game Logic Object Functions

### Get HQ (Headquarters)
Retrieve a headquarters object that defines spawn points and team bases, linked via ObjId in the Godot scene.

```typescript
// Define HQ ObjIds matching your Godot scene
const HQ_TEAM1_ID = 10;
const HQ_TEAM2_ID = 11;

export function OnGameModeStarted() {
    const hqTeam1 = mod.GetHQ(HQ_TEAM1_ID);
    const hqTeam2 = mod.GetHQ(HQ_TEAM2_ID);

    // Enable spawning at headquarters
    mod.SetHQEnabled(hqTeam1, true);
    mod.SetHQEnabled(hqTeam2, true);

    // Set which team can spawn at each HQ
    mod.SetHQTeam(hqTeam1, mod.GetTeam(1));
    mod.SetHQTeam(hqTeam2, mod.GetTeam(2));
}

// Disable spawning when HQ is compromised
function disableHQSpawning(hqObjId: number) {
    const hq = mod.GetHQ(hqObjId);
    mod.SetHQEnabled(hq, false);

    ShowEventGameModeMessage(
        mod.CreateMessage("Headquarters compromised! Find new spawn.")
    );
}
```

### Get Capture Point
Retrieve a capture point object for objective-based game modes like Conquest.

```typescript
const CP_A_ID = 20;
const CP_B_ID = 21;
const CP_C_ID = 22;

const captureProgress = new Map<number, number>();

export function OnGameModeStarted() {
    // Initialize capture points
    [CP_A_ID, CP_B_ID, CP_C_ID].forEach(cpId => {
        const cp = mod.GetCapturePoint(cpId);
        mod.SetCapturePointNeutral(cp, true);
        mod.SetCapturePointEnabled(cp, true);
        captureProgress.set(cpId, 0);
    });
}

// Update capture point based on player presence
async function processCapturePoint(cpId: number, capturingTeam: mod.Team) {
    const cp = mod.GetCapturePoint(cpId);
    let progress = captureProgress.get(cpId) || 0;

    while (progress < 100) {
        progress += 1;
        captureProgress.set(cpId, progress);

        mod.SetCapturePointProgress(cp, progress);
        await mod.Wait(0.1);
    }

    // Point captured
    mod.SetCapturePointOwner(cp, capturingTeam);
    const teamName = mod.GetTeamName(capturingTeam);

    ShowEventGameModeMessage(
        mod.CreateMessage(`${teamName} captured the objective!`)
    );
}
```

### Get Area Trigger
Retrieve an area trigger object for zone detection and spatial logic.

```typescript
const SAFEZONE_ID = 30;
const COMBAT_ZONE_ID = 31;

const playersInZone = new Map<number, Set<number>>();

export function OnPlayerEnterAreaTrigger(
    player: mod.Player,
    trigger: mod.AreaTrigger
) {
    const triggerId = mod.GetObjId(trigger);
    const playerId = getPlayerId(player);

    if (!playersInZone.has(triggerId)) {
        playersInZone.set(triggerId, new Set());
    }
    playersInZone.get(triggerId)!.add(playerId);

    if (triggerId === SAFEZONE_ID) {
        // Player cannot take damage in safe zone
        mod.SetPlayerInvulnerable(player, true);
        ShowNotificationMessage(
            mod.CreateMessage("Entered safe zone"),
            player
        );
    } else if (triggerId === COMBAT_ZONE_ID) {
        // Start combat timer
        startCombatTimer(player);
    }
}

export function OnPlayerExitAreaTrigger(
    player: mod.Player,
    trigger: mod.AreaTrigger
) {
    const triggerId = mod.GetObjId(trigger);
    const playerId = getPlayerId(player);

    playersInZone.get(triggerId)?.delete(playerId);

    if (triggerId === SAFEZONE_ID) {
        mod.SetPlayerInvulnerable(player, false);
    }
}

// Count players in zone
function getPlayerCountInZone(zoneId: number): number {
    return playersInZone.get(zoneId)?.size || 0;
}
```

### Get Vehicle Spawner
Retrieve a vehicle spawner object to control vehicle availability and respawn timing.

```typescript
const TANK_SPAWNER_ID = 40;
const HELI_SPAWNER_ID = 41;

export function OnGameModeStarted() {
    const tankSpawner = mod.GetVehicleSpawner(TANK_SPAWNER_ID);
    const heliSpawner = mod.GetVehicleSpawner(HELI_SPAWNER_ID);

    // Configure spawn settings
    mod.SetVehicleSpawnerEnabled(tankSpawner, true);
    mod.SetVehicleSpawnerRespawnDelay(tankSpawner, 60); // 60 second respawn

    mod.SetVehicleSpawnerEnabled(heliSpawner, true);
    mod.SetVehicleSpawnerRespawnDelay(heliSpawner, 90);

    // Set which vehicle type spawns
    mod.SetVehicleSpawnerVehicleType(
        tankSpawner,
        mod.VehicleList.M1A2_Abrams
    );
}

// Disable vehicle spawner temporarily
async function temporarilyDisableVehicleSpawner(spawnerId: number, duration: number) {
    const spawner = mod.GetVehicleSpawner(spawnerId);
    mod.SetVehicleSpawnerEnabled(spawner, false);

    ShowEventGameModeMessage(
        mod.CreateMessage("Vehicle spawning disabled!")
    );

    await mod.Wait(duration);

    mod.SetVehicleSpawnerEnabled(spawner, true);
    ShowEventGameModeMessage(
        mod.CreateMessage("Vehicle spawning enabled!")
    );
}
```

## Player Manipulation Functions

### Teleport Player
Instantly move a player to a specific position and rotation in the game world.

```typescript
// Teleport player to coordinates
function teleportToCoordinates(
    player: mod.Player,
    x: number,
    y: number,
    z: number,
    rotationDegrees: number
) {
    const position = new mod.Vector(x, y, z);
    mod.Teleport(player, position, rotationDegrees);

    ShowNotificationMessage(
        mod.CreateMessage("Teleported!"),
        player
    );
}

// Teleport all players to a rally point
async function teleportTeamToRallyPoint(teamId: number) {
    const team = mod.GetTeam(teamId);
    const players = ConvertArray(getPlayersInTeam(team));
    const rallyPoint = new mod.Vector(100, 0, 50);

    players.forEach((player, index) => {
        const offset = new mod.Vector(
            Math.cos(index * 0.5) * 5,
            0,
            Math.sin(index * 0.5) * 5
        );
        const finalPos = mod.VectorAdd(rallyPoint, offset);

        mod.Teleport(player, finalPos, 0);
    });

    ShowHighlightedGameModeMessage(
        mod.CreateMessage("Team assembled at rally point!"),
        undefined,
        team
    );
}

// Teleport player to another player
function teleportToPlayer(player: mod.Player, targetPlayer: mod.Player) {
    const targetPos = mod.GetPlayerPosition(targetPlayer);
    const targetRot = mod.GetPlayerRotation(targetPlayer);

    mod.Teleport(player, targetPos, targetRot);

    const targetName = mod.GetPlayerName(targetPlayer);
    ShowNotificationMessage(
        mod.CreateMessage(`Teleported to ${targetName}`),
        player
    );
}
```

### Display Notification Message
Show messages to players or teams with various visibility and duration options.

```typescript
// Show message to specific player
function notifyPlayer(player: mod.Player, text: string) {
    ShowNotificationMessage(
        mod.CreateMessage(text),
        player
    );
}

// Broadcast to all players
function broadcastMessage(text: string) {
    ShowNotificationMessage(mod.CreateMessage(text));
}

// Show message to entire team
function notifyTeam(teamId: number, text: string) {
    const team = mod.GetTeam(teamId);
    ShowNotificationMessage(
        mod.CreateMessage(text),
        undefined,
        team
    );
}

// Show custom notification in specific slot with duration
function showTimedNotification(
    player: mod.Player,
    text: string,
    durationSeconds: number
) {
    DisplayCustomNotificationMessage(
        mod.CreateMessage(text),
        mod.CustomNotificationSlots.Slot1,
        durationSeconds,
        player
    );
}

// Clear specific custom notification
function clearNotification(player: mod.Player) {
    ClearCustomNotificationMessage(
        mod.CustomNotificationSlots.Slot1,
        player
    );
}

// Clear all custom notifications for player
function clearAllNotifications(player: mod.Player) {
    ClearAllCustomNotificationMessages(player);
}
```

### Spawn and Unspawn Objects
Dynamically create and remove objects in the game world at runtime.

```typescript
const spawnedObjects: any[] = [];

// Spawn an object at position
function spawnObjectAtLocation(
    objectType: any,
    x: number,
    y: number,
    z: number
) {
    const position = new mod.Vector(x, y, z);
    const rotation = new mod.Vector(0, 0, 0);
    const scale = new mod.Vector(1, 1, 1);

    const obj = mod.SpawnObject(objectType, position, rotation, scale);
    spawnedObjects.push(obj);

    return obj;
}

// Spawn multiple objects in a circle
function spawnObjectsInCircle(
    objectType: any,
    centerX: number,
    centerY: number,
    centerZ: number,
    radius: number,
    count: number
) {
    const center = new mod.Vector(centerX, centerY, centerZ);

    for (let i = 0; i < count; i++) {
        const angle = (i / count) * Math.PI * 2;
        const offset = new mod.Vector(
            Math.cos(angle) * radius,
            0,
            Math.sin(angle) * radius
        );
        const position = mod.VectorAdd(center, offset);
        const rotation = new mod.Vector(0, angle, 0);

        const obj = mod.SpawnObject(objectType, position, rotation);
        spawnedObjects.push(obj);
    }
}

// Clean up all spawned objects
function despawnAllObjects() {
    spawnedObjects.forEach(obj => {
        mod.UnspawnObject(obj);
    });
    spawnedObjects.length = 0;
}

// Spawn temporary object that auto-despawns
async function spawnTemporaryObject(
    objectType: any,
    position: mod.Vector,
    durationSeconds: number
) {
    const obj = mod.SpawnObject(objectType, position, new mod.Vector(0, 0, 0));
    await mod.Wait(durationSeconds);
    mod.UnspawnObject(obj);
}
```

## UI System Functions

### ParseUI - Declarative UI Builder
Build complex UI hierarchies using a JSON-like declarative structure, the recommended approach for creating interfaces.

```typescript
// Create a complete scoreboard UI
export function OnGameModeStarted() {
    const scoreboard = ParseUI({
        type: "Container",
        name: "scoreboard_container",
        size: [600, 400],
        anchor: mod.UIAnchor.Center,
        bgColor: [0, 0, 0, 180],
        bgFill: mod.UIBgFill.Solid,
        depth: mod.UIDepth.Background,
        children: [
            {
                type: "Text",
                name: "scoreboard_title",
                textLabel: "SCOREBOARD",
                size: [600, 60],
                anchor: mod.UIAnchor.TopCenter,
                fontSize: 32,
                fontColor: [255, 255, 255, 255],
            },
            {
                type: "Container",
                name: "team1_section",
                size: [280, 300],
                anchor: mod.UIAnchor.TopLeft,
                position: [10, 70],
                children: [
                    {
                        type: "Text",
                        name: "team1_name",
                        textLabel: "Team 1",
                        size: [280, 40],
                        anchor: mod.UIAnchor.TopCenter,
                        fontSize: 24,
                        fontColor: [100, 150, 255, 255],
                    },
                    {
                        type: "Text",
                        name: "team1_score",
                        textLabel: "Score: 0",
                        size: [280, 30],
                        anchor: mod.UIAnchor.TopCenter,
                        position: [0, 45],
                        fontSize: 20,
                    }
                ]
            },
            {
                type: "Container",
                name: "team2_section",
                size: [280, 300],
                anchor: mod.UIAnchor.TopRight,
                position: [-10, 70],
                children: [
                    {
                        type: "Text",
                        name: "team2_name",
                        textLabel: "Team 2",
                        size: [280, 40],
                        anchor: mod.UIAnchor.TopCenter,
                        fontSize: 24,
                        fontColor: [255, 100, 100, 255],
                    },
                    {
                        type: "Text",
                        name: "team2_score",
                        textLabel: "Score: 0",
                        size: [280, 30],
                        anchor: mod.UIAnchor.TopCenter,
                        position: [0, 45],
                        fontSize: 20,
                    }
                ]
            }
        ]
    });
}

// Create interactive button UI
const menuUI = ParseUI({
    type: "Container",
    name: "main_menu",
    size: [400, 300],
    anchor: mod.UIAnchor.Center,
    children: [
        {
            type: "Button",
            name: "start_button",
            textLabel: "START GAME",
            size: [300, 60],
            anchor: mod.UIAnchor.TopCenter,
            position: [0, 20],
        },
        {
            type: "Button",
            name: "options_button",
            textLabel: "OPTIONS",
            size: [300, 60],
            anchor: mod.UIAnchor.TopCenter,
            position: [0, 90],
        },
        {
            type: "Button",
            name: "quit_button",
            textLabel: "QUIT",
            size: [300, 60],
            anchor: mod.UIAnchor.TopCenter,
            position: [0, 160],
        }
    ]
});
```

### UI Widget Manipulation
Find, show, hide, and update UI widgets dynamically at runtime.

```typescript
// Update scoreboard with real-time data
function updateScoreboard(team1Score: number, team2Score: number) {
    const team1ScoreWidget = mod.FindUIWidgetWithName("team1_score");
    const team2ScoreWidget = mod.FindUIWidgetWithName("team2_score");

    if (team1ScoreWidget) {
        mod.SetUIWidgetText(team1ScoreWidget, `Score: ${team1Score}`);
    }
    if (team2ScoreWidget) {
        mod.SetUIWidgetText(team2ScoreWidget, `Score: ${team2Score}`);
    }
}

// Toggle UI visibility
function toggleScoreboard() {
    const scoreboard = mod.FindUIWidgetWithName("scoreboard_container");
    if (scoreboard) {
        const isVisible = mod.GetUIWidgetVisible(scoreboard);
        mod.SetUIWidgetVisible(scoreboard, !isVisible);
    }
}

// Show UI only to specific player
function showPlayerUI(player: mod.Player, widgetName: string) {
    const widget = mod.FindUIWidgetWithName(widgetName);
    if (widget) {
        mod.SetUIWidgetVisibleForPlayer(widget, player, true);
    }
}

// Update UI text dynamically
function updatePlayerHUD(player: mod.Player) {
    const healthWidget = mod.FindUIWidgetWithName("player_health_text");
    const ammoWidget = mod.FindUIWidgetWithName("player_ammo_text");

    if (healthWidget) {
        const health = mod.GetPlayerHealth(player);
        mod.SetUIWidgetText(healthWidget, `HP: ${Math.floor(health)}`);
    }

    if (ammoWidget) {
        const ammo = mod.GetPlayerAmmo(player);
        mod.SetUIWidgetText(ammoWidget, `Ammo: ${ammo}`);
    }
}
```

### UI Button Event Handler
Handle player interactions with UI buttons for menus and interactive interfaces.

```typescript
// Handle all button clicks
export function OnPlayerUIButtonEvent(
    player: mod.Player,
    widget: mod.UIWidget,
    event: mod.UIButtonEvent
) {
    if (event !== mod.UIButtonEvent.Clicked) return;

    const widgetName = mod.GetUIWidgetName(widget);
    const playerId = getPlayerId(player);

    switch (widgetName) {
        case "start_button":
            handleStartGame(player);
            break;
        case "options_button":
            showOptionsMenu(player);
            break;
        case "quit_button":
            handlePlayerQuit(player);
            break;
        case "team1_join_button":
            assignPlayerToTeam(player, 1);
            break;
        case "team2_join_button":
            assignPlayerToTeam(player, 2);
            break;
        case "loadout_primary_button":
            showWeaponSelection(player, "primary");
            break;
    }
}

function handleStartGame(player: mod.Player) {
    // Hide menu
    const menu = mod.FindUIWidgetWithName("main_menu");
    mod.SetUIWidgetVisible(menu, false);

    // Start game logic
    ShowNotificationMessage(
        mod.CreateMessage("Game starting..."),
        player
    );
}

function assignPlayerToTeam(player: mod.Player, teamId: number) {
    const team = mod.GetTeam(teamId);
    mod.SetPlayerTeam(player, team);

    const teamName = mod.GetTeamName(team);
    ShowNotificationMessage(
        mod.CreateMessage(`Joined ${teamName}`),
        player
    );
}
```

## Utility Helper Functions

### Array Conversion and Filtering
Convert between mod.Array and standard TypeScript arrays, with filtering capabilities for game logic.

```typescript
// Convert mod.Array to standard array for iteration
function getAllPlayerIds(): number[] {
    const allPlayers = mod.GetAllPlayers(); // Returns mod.Array
    const playerArray = ConvertArray(allPlayers);
    return playerArray.map(player => getPlayerId(player));
}

// Filter players by condition
function getPlayersAboveHealth(healthThreshold: number): mod.Player[] {
    const allPlayers = mod.GetAllPlayers();
    const filtered = FilteredArray(allPlayers, (player) => {
        return mod.GetPlayerHealth(player) > healthThreshold;
    });
    return ConvertArray(filtered);
}

// Find all players in combat
function getPlayersInCombat(): mod.Player[] {
    const allPlayers = mod.GetAllPlayers();
    const inCombat = FilteredArray(allPlayers, (player) => {
        return mod.IsPlayerInCombat(player);
    });
    return ConvertArray(inCombat);
}

// Get players by team
function getPlayersByTeam(teamId: number): mod.Player[] {
    const allPlayers = mod.GetAllPlayers();
    const teamPlayers = FilteredArray(allPlayers, (player) => {
        const playerTeam = mod.GetPlayerTeam(player);
        return getTeamId(playerTeam) === teamId;
    });
    return ConvertArray(teamPlayers);
}

// Check if all players meet condition
function areAllPlayersDead(): boolean {
    const allPlayers = mod.GetAllPlayers();
    return IsTrueForAll(allPlayers, (player) => {
        return mod.GetPlayerHealth(player) <= 0;
    });
}

// Check if any player meets condition
function isAnyPlayerAtObjective(objectiveId: number): boolean {
    const allPlayers = mod.GetAllPlayers();
    return IsTrueForAny(allPlayers, (player) => {
        const playerPos = mod.GetPlayerPosition(player);
        const objPos = getObjectivePosition(objectiveId);
        const distance = mod.VectorDistance(playerPos, objPos);
        return distance < 10; // Within 10 units
    });
}
```

### Asynchronous Timing Functions
Manage time-based game logic with async/await patterns for delays and condition polling.

```typescript
// Simple delay
async function countdownTimer(seconds: number) {
    for (let i = seconds; i > 0; i--) {
        ShowEventGameModeMessage(
            mod.CreateMessage(`${i}`)
        );
        await mod.Wait(1);
    }
    ShowEventGameModeMessage(
        mod.CreateMessage("GO!")
    );
}

// Wait until condition is met
async function waitForPlayerToDeploy(player: mod.Player) {
    await WaitUntil(0.5, () => {
        return mod.IsPlayerDeployed(player);
    });

    ShowNotificationMessage(
        mod.CreateMessage("Welcome to the battlefield!"),
        player
    );
}

// Periodic check with timeout
async function waitForObjectiveCapture(
    cpId: number,
    teamId: number,
    timeoutSeconds: number
) {
    const startTime = Date.now();

    await WaitUntil(1.0, () => {
        const cp = mod.GetCapturePoint(cpId);
        const owner = mod.GetCapturePointOwner(cp);

        // Check timeout
        const elapsed = (Date.now() - startTime) / 1000;
        if (elapsed > timeoutSeconds) return true;

        // Check if captured
        return owner && getTeamId(owner) === teamId;
    });
}

// Repeating timer
async function startTicketBleed(teamId: number, ticksPerSecond: number) {
    while (true) {
        await mod.Wait(1.0);

        const team = mod.GetTeam(teamId);
        const currentTickets = mod.GetTeamTickets(team);

        if (currentTickets <= 0) break;

        mod.SetTeamTickets(team, currentTickets - ticksPerSecond);
    }
}
```

### Condition State Management
Track state transitions for triggering one-time actions when conditions change from false to true.

```typescript
// Track per-player conditions
function checkPlayerLowHealth(player: mod.Player) {
    const condition = getPlayerCondition(player, 1); // Condition slot 1
    const health = mod.GetPlayerHealth(player);

    // Triggers only once when health drops below 30
    if (condition.update(health < 30)) {
        ShowNotificationMessage(
            mod.CreateMessage("WARNING: Low health!"),
            player
        );
    }
}

// Track per-team conditions
function checkTeamControl(teamId: number, controlsAllPoints: boolean) {
    const team = mod.GetTeam(teamId);
    const condition = getTeamCondition(team, 1);

    // Triggers only when team first gains control
    if (condition.update(controlsAllPoints)) {
        ShowEventGameModeMessage(
            mod.CreateMessage("Team controls all objectives!")
        );
        awardTeamBonus(teamId, 50);
    }
}

// Track global game conditions
async function monitorGamePhases() {
    while (true) {
        const phase1Condition = getGlobalCondition(1);
        const phase2Condition = getGlobalCondition(2);

        const timeElapsed = getGameTimeElapsed();

        if (phase1Condition.update(timeElapsed > 300)) {
            ShowEventGameModeMessage(
                mod.CreateMessage("Phase 2 beginning!")
            );
            enablePhase2Objectives();
        }

        if (phase2Condition.update(timeElapsed > 600)) {
            ShowEventGameModeMessage(
                mod.CreateMessage("Final phase!")
            );
            enablePhase3Objectives();
        }

        await mod.Wait(1);
    }
}

// Track capture point state changes
function monitorCapturePoint(cpId: number) {
    const cp = mod.GetCapturePoint(cpId);
    const condition = getCapturePointCondition(cp, 1);

    const isContested = mod.IsCapturePointContested(cp);

    // Triggers when point becomes contested
    if (condition.update(isContested)) {
        ShowHighlightedGameModeMessage(
            mod.CreateMessage("Objective contested!")
        );
    }
}
```

### Conditional Logic Helpers
Simplified conditional operations for common patterns in game logic.

```typescript
// Combine multiple boolean conditions
function canPlayerCapture(player: mod.Player, cpId: number): boolean {
    const isAlive = mod.GetPlayerHealth(player) > 0;
    const isDeployed = mod.IsPlayerDeployed(player);
    const inZone = isPlayerInCaptureZone(player, cpId);
    const notInVehicle = !mod.IsPlayerInVehicle(player);

    return And(isAlive, isDeployed, inZone, notInVehicle);
}

// Execute different logic based on condition
function applyGameModeRules(teamId: number) {
    const team = mod.GetTeam(teamId);
    const tickets = mod.GetTeamTickets(team);

    const result = IfThenElse(
        tickets > 50,
        () => "normal",
        () => "critical"
    );

    if (result === "critical") {
        ShowHighlightedGameModeMessage(
            mod.CreateMessage("CRITICAL: Low tickets!"),
            undefined,
            team
        );
    }
}

// String concatenation for messages
function createKillMessage(killerName: string, victimName: string): string {
    return Concat(
        Concat(killerName, " eliminated "),
        victimName
    );
}

// Compare values safely
function checkPlayerScore(player: mod.Player, targetScore: number): boolean {
    const currentScore = mod.GetPlayerScore(player);
    return Equals(currentScore, targetScore);
}
```

## Messaging Display Functions

### Event Game Mode Messages
Display large, center-screen messages for critical game events that demand immediate attention.

```typescript
// Show round start message
function announceRoundStart(roundNumber: number) {
    ShowEventGameModeMessage(
        mod.CreateMessage(`ROUND ${roundNumber}`)
    );
}

// Show team victory message
function announceVictory(teamId: number) {
    const team = mod.GetTeam(teamId);
    const teamName = mod.GetTeamName(team);

    ShowEventGameModeMessage(
        mod.CreateMessage(`${teamName} WINS!`)
    );
}

// Show message to specific team
function announceTeamObjective(teamId: number, objective: string) {
    const team = mod.GetTeam(teamId);
    ShowEventGameModeMessage(
        mod.CreateMessage(objective),
        undefined,
        team
    );
}

// Show message to specific player
function announcePersonalAchievement(player: mod.Player, achievement: string) {
    ShowEventGameModeMessage(
        mod.CreateMessage(achievement),
        player
    );
}
```

### Highlighted Game Mode Messages
Display prominent messages in the world log that stand out from regular notifications.

```typescript
// Highlight important objective changes
function announceObjectiveCaptured(cpId: number, teamId: number) {
    const team = mod.GetTeam(teamId);
    const teamName = mod.GetTeamName(team);
    const objectiveName = getObjectiveName(cpId);

    ShowHighlightedGameModeMessage(
        mod.CreateMessage(`${teamName} captured ${objectiveName}!`)
    );
}

// Announce game state changes
function announceTicketThreshold(teamId: number, ticketsRemaining: number) {
    const team = mod.GetTeam(teamId);
    ShowHighlightedGameModeMessage(
        mod.CreateMessage(`${ticketsRemaining} tickets remaining`),
        undefined,
        team
    );
}

// Broadcast special events
function announceSpecialEvent(eventName: string) {
    ShowHighlightedGameModeMessage(
        mod.CreateMessage(eventName)
    );
}
```

### Custom Notification Messages
Display notifications in specific UI slots with controlled duration for persistent information displays.

```typescript
// Show timer in custom slot
async function showMatchTimer(durationSeconds: number) {
    for (let remaining = durationSeconds; remaining > 0; remaining--) {
        DisplayCustomNotificationMessage(
            mod.CreateMessage(`Time: ${remaining}s`),
            mod.CustomNotificationSlots.Slot1,
            1.0
        );
        await mod.Wait(1);
    }

    ClearCustomNotificationMessage(mod.CustomNotificationSlots.Slot1);
}

// Show player-specific objective reminder
function showObjectiveReminder(player: mod.Player, objective: string) {
    DisplayCustomNotificationMessage(
        mod.CreateMessage(objective),
        mod.CustomNotificationSlots.Slot2,
        10.0,
        player
    );
}

// Show persistent team status
function showTeamStatus(teamId: number, status: string) {
    const team = mod.GetTeam(teamId);
    DisplayCustomNotificationMessage(
        mod.CreateMessage(status),
        mod.CustomNotificationSlots.Slot3,
        30.0,
        undefined,
        team
    );
}

// Clear specific notification slot
function clearTimerDisplay() {
    ClearCustomNotificationMessage(mod.CustomNotificationSlots.Slot1);
}
```

## Summary

The BF6 Portal TypeScript SDK provides a comprehensive framework for creating custom multiplayer game modes through event-driven programming and Godot scene integration. Core use cases include objective-based modes (Conquest, Rush), team deathmatch variants, survival challenges, custom respawn mechanics, dynamic UI systems, and spatial trigger logic. The event system captures all player actions (joins, deaths, interactions), while getter functions retrieve game objects via ObjId linking. Manipulation functions enable teleportation, spawning, player state changes, and team operations.

Integration patterns center on the `mod` namespace for all game API access, `modlib` utilities for common operations, and `ParseUI` for declarative interfaces. State management through `ConditionState` prevents duplicate event firing, while async functions handle time-based logic. Typical workflows involve: (1) defining ObjIds in Godot scenes, (2) implementing event handlers in TypeScript, (3) using getter functions to access scene objects, (4) manipulating game state through API calls, and (5) displaying feedback via the messaging system. The SDK supports rapid prototyping of complex game mechanics while maintaining clean separation between scene structure and game logic code.
