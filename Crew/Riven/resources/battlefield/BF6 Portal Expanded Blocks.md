import * as Blockly from 'blockly';

export const bf6PortalExpandedBlocks = Blockly.common.createBlockDefinitionsFromJsonArray([
{
    "message0": "MOD Mod Name: %1 Description: %2 Game Parameters: %3",
    "args0": [
      {
        "type": "field_input",
        "name": "MOD_NAME",
        "text": "MyGameMode"
      },
      {
        "type": "field_input",
        "name": "DESCRIPTION",
        "text": "Description"
      },
      {
        "type": "input_statement",
        "name": "RULES_CONDITIONS_SUBROUTINES",
        "check": [
          "Rule",
          "Condition",
          "Subroutine"
        ]
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#4A4A4A",
    "deletable": false,
    "movable": false,
    "type": "MOD_BLOCK"
  },
{
    "message0": "RULE Rule Name: %1 Event Type: Scope: Is Global: %2 Conditions: %3 Actions: %4",
    "args0": [
      {
        "type": "field_input",
        "name": "RULE_NAME",
        "text": "New Rule"
      },
      {
        "type": "field_input",
        "name": "IS_GLOBAL"
      },
      {
        "type": "input_statement",
        "name": "CONDITIONS",
        "check": "Condition"
      },
      {
        "type": "input_statement",
        "name": "ACTIONS",
        "check": [
          "Action",
          "SubroutineReference",
          "ControlAction"
        ]
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#A285E6",
    "type": "RULE_HEADER"
  },
{
    "message0": "Condition",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#45B5B5",
    "type": "CONDITION_BLOCK"
  },
{
    "message0": "Action",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#B5A045",
    "type": "ACTION_BLOCK"
  },
{
    "message0": "SUBROUTINE: Logic: %1",
    "args0": [
      {
        "type": "input_statement",
        "name": "ACTIONS_CONDITIONS",
        "check": [
          "Action",
          "Condition"
        ]
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#E6A85C",
    "type": "SUBROUTINE_BLOCK"
  },
{
    "message0": "Call Subroutine:",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#E6A85C",
    "type": "SUBROUTINE_REFERENCE_BLOCK"
  },
{
    "message0": "Control Action:",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#A285E6",
    "type": "CONTROL_ACTION_BLOCK"
  },
{
    "message0": "AIBattlefieldBehaviour Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#B5A045",
    "type": "AIBATTLEFIELDBEHAVIOUR"
  },
{
    "message0": "AIDefendPositionBehaviour Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#B5A045",
    "type": "AIDEFENDPOSITIONBEHAVIOUR"
  },
{
    "message0": "AIIdleBehaviour Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#B5A045",
    "type": "AIIDLEBEHAVIOUR"
  },
{
    "message0": "AILOSMoveTOBehaviour Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#B5A045",
    "type": "AILOSMOVETOBEHAVIOUR"
  },
{
    "message0": "AIMoveToBehaviour Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#B5A045",
    "type": "AIMOVETOBEHAVIOUR"
  },
{
    "message0": "AIParachuteBehaviour Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#B5A045",
    "type": "AIPARACHUTEBEHAVIOUR"
  },
{
    "message0": "AIValidateMoveToBehaviour Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#B5A045",
    "type": "AIVALIDATEMOVETOBEHAVIOUR"
  },
{
    "message0": "AIWaypointIdleBehaviour Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#B5A045",
    "type": "AIWAYPOINTIDLEBEHAVIOUR"
  },
{
    "message0": "Set Player Health Player: %1 Health: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "HEALTH"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#C2185B",
    "type": "SETPLAYERHEALTH"
  },
{
    "message0": "SetPlayerLoadout Player: %1 Loadout: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "LOADOUT"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#B5A045",
    "type": "SETPLAYERLOADOUT"
  },
{
    "message0": "Teleport Player: %1 Location: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "LOCATION"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#B5A045",
    "type": "TELEPORT"
  },
{
    "message0": "End Round Winning Team: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "WINNING_TEAM"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#5D4037",
    "type": "ENDROUND"
  },
{
    "message0": "PauseRound",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#B5A045",
    "type": "PAUSEROUND"
  },
{
    "message0": "AI Battlefield Behavior Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#D32F2F",
    "type": "AIBATTLEFIELDBEHAVIOR"
  },
{
    "message0": "AI Defend Position Behavior Player: %1 Position: %2 Radius: %3 Time: %4",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "POSITION"
      },
      {
        "type": "field_input",
        "name": "RADIUS"
      },
      {
        "type": "field_input",
        "name": "TIME"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#D32F2F",
    "type": "AIDEFENDPOSITIONBEHAVIOR"
  },
{
    "message0": "AI Idle Behavior Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#D32F2F",
    "type": "AIIDLEBEHAVIOR"
  },
{
    "message0": "AI Move To Behavior Player: %1 Position: %2 Sprint: %3",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "POSITION"
      },
      {
        "type": "field_input",
        "name": "SPRINT"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#D32F2F",
    "type": "AIMOVETOBEHAVIOR"
  },
{
    "message0": "AI Parachute Behavior Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#D32F2F",
    "type": "AIPARACHUTEBEHAVIOR"
  },
{
    "message0": "AI Waypoint Idle Behavior Player: %1 Time: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "TIME"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#D32F2F",
    "type": "AIWAYPOINTIDLEBEHAVIOR"
  },
{
    "message0": "AI Follow Player Ai Player: %1 Target Player: %2 Distance: %3",
    "args0": [
      {
        "type": "field_input",
        "name": "AI_PLAYER"
      },
      {
        "type": "field_input",
        "name": "TARGET_PLAYER"
      },
      {
        "type": "field_input",
        "name": "DISTANCE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#D32F2F",
    "type": "AIFOLLOWPLAYER"
  },
{
    "message0": "AI Hold Position Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#D32F2F",
    "type": "AIHOLDPOSITION"
  },
{
    "message0": "AI Attack Target Ai Player: %1 Target Player: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "AI_PLAYER"
      },
      {
        "type": "field_input",
        "name": "TARGET_PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#D32F2F",
    "type": "AIATTACKTARGET"
  },
{
    "message0": "Set AI Behavior Player: %1 Behavior Mode: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "BEHAVIOR_MODE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#D32F2F",
    "type": "SETAIBEHAVIOR"
  },
{
    "message0": "Deploy AI Team: %1 Soldier Type: %2 Position: %3 Kit: %4",
    "args0": [
      {
        "type": "field_input",
        "name": "TEAM"
      },
      {
        "type": "field_input",
        "name": "SOLDIER_TYPE"
      },
      {
        "type": "field_input",
        "name": "POSITION"
      },
      {
        "type": "field_input",
        "name": "KIT"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#D32F2F",
    "type": "DEPLOYAI"
  },
{
    "message0": "Despawn AI Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#D32F2F",
    "type": "DESPAWNAI"
  },
{
    "message0": "Set AI Spawn Location Team: %1 Position: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "TEAM"
      },
      {
        "type": "field_input",
        "name": "POSITION"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#D32F2F",
    "type": "SETAISPAWNLOCATION"
  },
{
    "message0": "Set AI Health Player: %1 Amount: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "AMOUNT"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#D32F2F",
    "type": "SETAIHEALTH"
  },
{
    "message0": "Set AI Team Player: %1 Team Id: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "TEAM_ID"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#D32F2F",
    "type": "SETAITEAM"
  },
{
    "message0": "Get AI Health Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#D32F2F",
    "type": "GETAIHEALTH"
  },
{
    "message0": "Get AI Team Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#D32F2F",
    "type": "GETAITEAM"
  },
{
    "message0": "AI Is Alive Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#D32F2F",
    "type": "AIISALIVE"
  },
{
    "message0": "Create Array",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#0097A7",
    "type": "CREATEARRAY"
  },
{
    "message0": "Array Length Array: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "ARRAY"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#0097A7",
    "type": "ARRAYLENGTH"
  },
{
    "message0": "Get Element Array: %1 Index: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "ARRAY"
      },
      {
        "type": "field_input",
        "name": "INDEX"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#0097A7",
    "type": "GETELEMENT"
  },
{
    "message0": "Set Element Array: %1 Index: %2 Value: %3",
    "args0": [
      {
        "type": "field_input",
        "name": "ARRAY"
      },
      {
        "type": "field_input",
        "name": "INDEX"
      },
      {
        "type": "field_input",
        "name": "VALUE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#0097A7",
    "type": "SETELEMENT"
  },
{
    "message0": "Append To Array Array: %1 Value: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "ARRAY"
      },
      {
        "type": "field_input",
        "name": "VALUE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#0097A7",
    "type": "APPENDTOARRAY"
  },
{
    "message0": "Remove From Array Array: %1 Index: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "ARRAY"
      },
      {
        "type": "field_input",
        "name": "INDEX"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#0097A7",
    "type": "REMOVEFROMARRAY"
  },
{
    "message0": "Find First Array: %1 Value: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "ARRAY"
      },
      {
        "type": "field_input",
        "name": "VALUE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#0097A7",
    "type": "FINDFIRST"
  },
{
    "message0": "Sort Array Array: %1 Order: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "ARRAY"
      },
      {
        "type": "field_input",
        "name": "ORDER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#0097A7",
    "type": "SORTARRAY"
  },
{
    "message0": "LoadMusic Music Id: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "MUSIC_ID"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#455A64",
    "type": "LOADMUSIC"
  },
{
    "message0": "PlayMusic Music Id: %1 Players: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "MUSIC_ID"
      },
      {
        "type": "field_input",
        "name": "PLAYERS"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#455A64",
    "type": "PLAYMUSIC"
  },
{
    "message0": "SetMusicParam Music Id: %1 Param: %2 Players: %3",
    "args0": [
      {
        "type": "field_input",
        "name": "MUSIC_ID"
      },
      {
        "type": "field_input",
        "name": "PARAM"
      },
      {
        "type": "field_input",
        "name": "PLAYERS"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#455A64",
    "type": "SETMUSICPARAM"
  },
{
    "message0": "UnloadMusic Music Id: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "MUSIC_ID"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#455A64",
    "type": "UNLOADMUSIC"
  },
{
    "message0": "PlaySound Sound Id: %1 Position: %2 Players: %3 Volume: %4 Pitch: %5",
    "args0": [
      {
        "type": "field_input",
        "name": "SOUND_ID"
      },
      {
        "type": "field_input",
        "name": "POSITION"
      },
      {
        "type": "field_input",
        "name": "PLAYERS"
      },
      {
        "type": "field_input",
        "name": "VOLUME"
      },
      {
        "type": "field_input",
        "name": "PITCH"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#455A64",
    "type": "PLAYSOUND"
  },
{
    "message0": "PlayVO Vo Id: %1 Speaker: %2 Listener: %3 Players: %4",
    "args0": [
      {
        "type": "field_input",
        "name": "VO_ID"
      },
      {
        "type": "field_input",
        "name": "SPEAKER"
      },
      {
        "type": "field_input",
        "name": "LISTENER"
      },
      {
        "type": "field_input",
        "name": "PLAYERS"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#455A64",
    "type": "PLAYVO"
  },
{
    "message0": "StopSound Sound Id: %1 Players: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "SOUND_ID"
      },
      {
        "type": "field_input",
        "name": "PLAYERS"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#455A64",
    "type": "STOPSOUND"
  },
{
    "message0": "Set Player Camera Player: %1 Camera Mode: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "CAMERA_MODE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#37474F",
    "type": "SETPLAYERCAMERA"
  },
{
    "message0": "Lock Camera to Target Player: %1 Target: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "TARGET"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#37474F",
    "type": "LOCKCAMERATOTARGET"
  },
{
    "message0": "Camera Shake Player: %1 Intensity: %2 Duration: %3",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "INTENSITY"
      },
      {
        "type": "field_input",
        "name": "DURATION"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#37474F",
    "type": "CAMERASHAKE"
  },
{
    "message0": "Set Camera FOV Player: %1 Fov: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "FOV"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#37474F",
    "type": "SETCAMERAFOV"
  },
{
    "message0": "Reset Camera Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#37474F",
    "type": "RESETCAMERA"
  },
{
    "message0": "First Person Camera",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#37474F",
    "type": "FIRSTPERSONCAMERA"
  },
{
    "message0": "Third Person Camera",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#37474F",
    "type": "THIRDPERSONCAMERA"
  },
{
    "message0": "Free Camera",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#37474F",
    "type": "FREECAMERA"
  },
{
    "message0": "Spectator Camera",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#37474F",
    "type": "SPECTATORCAMERA"
  },
{
    "message0": "Equal A: %1 B: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "A"
      },
      {
        "type": "field_input",
        "name": "B"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "EQUAL"
  },
{
    "message0": "Not Equal A: %1 B: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "A"
      },
      {
        "type": "field_input",
        "name": "B"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "NOTEQUAL"
  },
{
    "message0": "Less Than A: %1 B: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "A"
      },
      {
        "type": "field_input",
        "name": "B"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "LESSTHAN"
  },
{
    "message0": "Less Than Or Equal Value A: %1 Value B: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "VALUE_A"
      },
      {
        "type": "field_input",
        "name": "VALUE_B"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#45B5B5",
    "type": "LESSTHANOREQUAL"
  },
{
    "message0": "Greater Than A: %1 B: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "A"
      },
      {
        "type": "field_input",
        "name": "B"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "GREATERTHAN"
  },
{
    "message0": "Greater Than Or Equal Value A: %1 Value B: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "VALUE_A"
      },
      {
        "type": "field_input",
        "name": "VALUE_B"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#45B5B5",
    "type": "GREATERTHANOREQUAL"
  },
{
    "message0": "Play Effect Effect Type: %1 Location: %2 Scale: %3",
    "args0": [
      {
        "type": "field_input",
        "name": "EFFECT_TYPE"
      },
      {
        "type": "field_input",
        "name": "LOCATION"
      },
      {
        "type": "field_input",
        "name": "SCALE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#263238",
    "type": "PLAYEFFECT"
  },
{
    "message0": "Stop Effect Effect Id: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "EFFECT_ID"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#263238",
    "type": "STOPEFFECT"
  },
{
    "message0": "Particle Effect Particle Type: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PARTICLE_TYPE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#263238",
    "type": "PARTICLEEFFECT"
  },
{
    "message0": "Explosion Effect Explosion Type: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "EXPLOSION_TYPE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#263238",
    "type": "EXPLOSIONEFFECT"
  },
{
    "message0": "Screen Flash Player: %1 Color: %2 Duration: %3",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "COLOR"
      },
      {
        "type": "field_input",
        "name": "DURATION"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#263238",
    "type": "SCREENFLASH"
  },
{
    "message0": "Screen Fade Player: %1 Fade Type: %2 Duration: %3",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "FADE_TYPE"
      },
      {
        "type": "field_input",
        "name": "DURATION"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#263238",
    "type": "SCREENFADE"
  },
{
    "message0": "Apply Screen Filter Player: %1 Filter Type: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "FILTER_TYPE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#263238",
    "type": "APPLYSCREENFILTER"
  },
{
    "message0": "DeployEmplacement Emplacement Id: %1 Position: %2 Rotation: %3",
    "args0": [
      {
        "type": "field_input",
        "name": "EMPLACEMENT_ID"
      },
      {
        "type": "field_input",
        "name": "POSITION"
      },
      {
        "type": "field_input",
        "name": "ROTATION"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#8D6E63",
    "type": "DEPLOYEMPLACEMENT"
  },
{
    "message0": "On Game Start",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#5D4037",
    "type": "ON_START"
  },
{
    "message0": "On Player Join Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#5D4037",
    "type": "ON_PLAYER_JOIN"
  },
{
    "message0": "Event Attacker",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#5D4037",
    "type": "EVENTATTACKER"
  },
{
    "message0": "Event Damage",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#5D4037",
    "type": "EVENTDAMAGE"
  },
{
    "message0": "Event Location",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#5D4037",
    "type": "EVENTLOCATION"
  },
{
    "message0": "Event Player",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#5D4037",
    "type": "EVENTPLAYER"
  },
{
    "message0": "Event Team",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#5D4037",
    "type": "EVENTTEAM"
  },
{
    "message0": "Event Victim",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#5D4037",
    "type": "EVENTVICTIM"
  },
{
    "message0": "Event Weapon",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#5D4037",
    "type": "EVENTWEAPON"
  },
{
    "message0": "Get Gamemode",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#5D4037",
    "type": "GETGAMEMODE"
  },
{
    "message0": "Set Gamemode Gamemode: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "GAMEMODE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#5D4037",
    "type": "SETGAMEMODE"
  },
{
    "message0": "Enable Friendly Fire Enabled: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "ENABLED"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#5D4037",
    "type": "ENABLEFRIENDLYFIRE"
  },
{
    "message0": "Set Score Team: %1 Score: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "TEAM"
      },
      {
        "type": "field_input",
        "name": "SCORE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#5D4037",
    "type": "SETSCORE"
  },
{
    "message0": "Get Score Team: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "TEAM"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#5D4037",
    "type": "GETSCORE"
  },
{
    "message0": "Set Time Limit Time Limit: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "TIME_LIMIT"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#5D4037",
    "type": "SETTIMELIMIT"
  },
{
    "message0": "Get Time Limit",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#5D4037",
    "type": "GETTIMELIMIT"
  },
{
    "message0": "Wait Seconds: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "SECONDS"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "WAIT"
  },
{
    "message0": "Wait Until Condition: %1 Timeout: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "CONDITION"
      },
      {
        "type": "field_input",
        "name": "TIMEOUT"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "WAITUNTIL"
  },
{
    "message0": "Break",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "BREAK"
  },
{
    "message0": "Continue",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "CONTINUE"
  },
{
    "message0": "If Condition: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "CONDITION"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "IF"
  },
{
    "message0": "While Condition: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "CONDITION"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "WHILE"
  },
{
    "message0": "And A: %1 B: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "A"
      },
      {
        "type": "field_input",
        "name": "B"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "AND"
  },
{
    "message0": "Or A: %1 B: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "A"
      },
      {
        "type": "field_input",
        "name": "B"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "OR"
  },
{
    "message0": "Not A: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "A"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "NOT"
  },
{
    "message0": "True",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "TRUE"
  },
{
    "message0": "False",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "FALSE"
  },
{
    "message0": "Greater Than Or Equal A: %1 B: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "A"
      },
      {
        "type": "field_input",
        "name": "B"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "GREATERTHANEQUAL"
  },
{
    "message0": "Less Than Or Equal A: %1 B: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "A"
      },
      {
        "type": "field_input",
        "name": "B"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "LESSTHANEQUAL"
  },
{
    "message0": "ForVariable From Value: %1 To Value: %2 By Value: %3",
    "args0": [
      {
        "type": "field_input",
        "name": "FROM_VALUE"
      },
      {
        "type": "field_input",
        "name": "TO_VALUE"
      },
      {
        "type": "field_input",
        "name": "BY_VALUE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "FORVARIABLE"
  },
{
    "message0": "Add Value A: %1 Value B: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "VALUE_A"
      },
      {
        "type": "field_input",
        "name": "VALUE_B"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "ADD"
  },
{
    "message0": "Subtract Value A: %1 Value B: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "VALUE_A"
      },
      {
        "type": "field_input",
        "name": "VALUE_B"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "SUBTRACT"
  },
{
    "message0": "Multiply Value A: %1 Value B: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "VALUE_A"
      },
      {
        "type": "field_input",
        "name": "VALUE_B"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "MULTIPLY"
  },
{
    "message0": "Divide Value A: %1 Value B: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "VALUE_A"
      },
      {
        "type": "field_input",
        "name": "VALUE_B"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "DIVIDE"
  },
{
    "message0": "Power Base: %1 Exponent: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "BASE"
      },
      {
        "type": "field_input",
        "name": "EXPONENT"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "POWER"
  },
{
    "message0": "Square Root Value: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "VALUE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "SQUAREROOT"
  },
{
    "message0": "Absolute Value: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "VALUE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "ABSOLUTE"
  },
{
    "message0": "Modulo Dividend: %1 Divisor: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "DIVIDEND"
      },
      {
        "type": "field_input",
        "name": "DIVISOR"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#1976D2",
    "type": "MODULO"
  },
{
    "message0": "SetObjectiveState Objective: %1 State: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "OBJECTIVE"
      },
      {
        "type": "field_input",
        "name": "STATE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#F9A825",
    "type": "SETOBJECTIVESTATE"
  },
{
    "message0": "GetObjectiveState Objective: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "OBJECTIVE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#F9A825",
    "type": "GETOBJECTIVESTATE"
  },
{
    "message0": "Comment Text: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "TEXT"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#9E9E9E",
    "type": "COMMENT"
  },
{
    "message0": "Get Player By Id Player Id: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER_ID"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#C2185B",
    "type": "GETPLAYERBYID"
  },
{
    "message0": "Get Player Name Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#C2185B",
    "type": "GETPLAYERNAME"
  },
{
    "message0": "Get Player Health Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#C2185B",
    "type": "GETPLAYERHEALTH"
  },
{
    "message0": "Teleport Player Player: %1 Position: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "POSITION"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#C2185B",
    "type": "TELEPORTPLAYER"
  },
{
    "message0": "Kill Player Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#C2185B",
    "type": "KILLPLAYER"
  },
{
    "message0": "Get Player Team Player: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#C2185B",
    "type": "GETPLAYERTEAM"
  },
{
    "message0": "Set Player Team Player: %1 Team: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "TEAM"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#C2185B",
    "type": "SETPLAYERTEAM"
  },
{
    "message0": "Call Subroutine Subroutine Name:",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#E6A85C",
    "type": "CALLSUBROUTINE"
  },
{
    "message0": "Return",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#E6A85C",
    "type": "RETURN"
  },
{
    "message0": "Vector X: %1 Y: %2 Z: %3",
    "args0": [
      {
        "type": "field_input",
        "name": "X"
      },
      {
        "type": "field_input",
        "name": "Y"
      },
      {
        "type": "field_input",
        "name": "Z"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#212121",
    "type": "VECTOR"
  },
{
    "message0": "Vector Towards Start Pos: %1 End Pos: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "START_POS"
      },
      {
        "type": "field_input",
        "name": "END_POS"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#212121",
    "type": "VECTORTOWARDS"
  },
{
    "message0": "Distance Between Position A: %1 Position B: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "POSITION_A"
      },
      {
        "type": "field_input",
        "name": "POSITION_B"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#212121",
    "type": "DISTANCEBETWEEN"
  },
{
    "message0": "X Component Of Vector: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "VECTOR"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#212121",
    "type": "XCOMPONENTOF"
  },
{
    "message0": "Y Component Of Vector: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "VECTOR"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#212121",
    "type": "YCOMPONENTOF"
  },
{
    "message0": "Z Component Of Vector: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "VECTOR"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#212121",
    "type": "ZCOMPONENTOF"
  },
{
    "message0": "Normalize Vector: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "VECTOR"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#212121",
    "type": "NORMALIZE"
  },
{
    "message0": "Dot Product Vector A: %1 Vector B: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "VECTOR_A"
      },
      {
        "type": "field_input",
        "name": "VECTOR_B"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#212121",
    "type": "DOTPRODUCT"
  },
{
    "message0": "Cross Product Vector A: %1 Vector B: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "VECTOR_A"
      },
      {
        "type": "field_input",
        "name": "VECTOR_B"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#212121",
    "type": "CROSSPRODUCT"
  },
{
    "message0": "Magnitude Of Vector: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "VECTOR"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#212121",
    "type": "VECTORMAGNITUDE"
  },
{
    "message0": "Up",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#212121",
    "type": "UP"
  },
{
    "message0": "Down",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#212121",
    "type": "DOWN"
  },
{
    "message0": "Left",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#212121",
    "type": "LEFT"
  },
{
    "message0": "Right",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#212121",
    "type": "RIGHT"
  },
{
    "message0": "Forward",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#212121",
    "type": "FORWARD"
  },
{
    "message0": "Backward",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#212121",
    "type": "BACKWARD"
  },
{
    "message0": "Show Message Player: %1 Message: %2 Duration: %3",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "MESSAGE"
      },
      {
        "type": "field_input",
        "name": "DURATION"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#607D8B",
    "type": "SHOWMESSAGE"
  },
{
    "message0": "Show Big Message Player: %1 Title: %2 Subtitle: %3 Duration: %4",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "TITLE"
      },
      {
        "type": "field_input",
        "name": "SUBTITLE"
      },
      {
        "type": "field_input",
        "name": "DURATION"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#607D8B",
    "type": "SHOWBIGMESSAGE"
  },
{
    "message0": "Show Notification Player: %1 Text: %2 Icon: %3",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "TEXT"
      },
      {
        "type": "field_input",
        "name": "ICON"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#607D8B",
    "type": "SHOWNOTIFICATION"
  },
{
    "message0": "Set HUD Visible Player: %1 Hud Element: %2 Visible: %3",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "HUD_ELEMENT"
      },
      {
        "type": "field_input",
        "name": "VISIBLE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#607D8B",
    "type": "SETHUDVISIBLE"
  },
{
    "message0": "Update HUD Text Player: %1 Hud Id: %2 Text: %3",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "HUD_ID"
      },
      {
        "type": "field_input",
        "name": "TEXT"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#607D8B",
    "type": "UPDATEHUDTEXT"
  },
{
    "message0": "Create Custom HUD Player: %1 Hud Config: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "HUD_CONFIG"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#607D8B",
    "type": "CREATECUSTOMHUD"
  },
{
    "message0": "Create World Marker Location: %1 Icon: %2 Text: %3",
    "args0": [
      {
        "type": "field_input",
        "name": "LOCATION"
      },
      {
        "type": "field_input",
        "name": "ICON"
      },
      {
        "type": "field_input",
        "name": "TEXT"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#607D8B",
    "type": "CREATEWORLDMARKER"
  },
{
    "message0": "Remove World Marker Marker Id: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "MARKER_ID"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#607D8B",
    "type": "REMOVEWORLDMARKER"
  },
{
    "message0": "Set Objective Marker Player: %1 Location: %2 Text: %3",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "LOCATION"
      },
      {
        "type": "field_input",
        "name": "TEXT"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#607D8B",
    "type": "SETOBJECTIVEMARKER"
  },
{
    "message0": "Update Scoreboard Entries: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "ENTRIES"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#607D8B",
    "type": "UPDATESCOREBOARD"
  },
{
    "message0": "Show Scoreboard Player: %1 Visible: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "VISIBLE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#607D8B",
    "type": "SHOWSCOREBOARD"
  },
{
    "message0": "Number",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#0288D1",
    "type": "NUMBER"
  },
{
    "message0": "String",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#0288D1",
    "type": "STRING"
  },
{
    "message0": "Boolean",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#0288D1",
    "type": "BOOLEAN"
  },
{
    "message0": "Set Variable Variable: %1 Value: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "VARIABLE"
      },
      {
        "type": "field_input",
        "name": "VALUE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#0288D1",
    "type": "SETVARIABLE"
  },
{
    "message0": "Get Variable Variable Name: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "VARIABLE_NAME"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#0288D1",
    "type": "GETVARIABLE"
  },
{
    "message0": "Spawn Vehicle Vehicle Type: %1 Location: %2 Team: %3",
    "args0": [
      {
        "type": "field_input",
        "name": "VEHICLE_TYPE"
      },
      {
        "type": "field_input",
        "name": "LOCATION"
      },
      {
        "type": "field_input",
        "name": "TEAM"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#E64A19",
    "type": "SPAWNVEHICLE"
  },
{
    "message0": "Despawn Vehicle Vehicle: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "VEHICLE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#E64A19",
    "type": "DESPAWNVEHICLE"
  },
{
    "message0": "Tank",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#E64A19",
    "type": "VEHICLETYPETANK"
  },
{
    "message0": "APC",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#E64A19",
    "type": "VEHICLETYPEAPC"
  },
{
    "message0": "Helicopter",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#E64A19",
    "type": "VEHICLETYPEHELICOPTER"
  },
{
    "message0": "Jet",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#E64A19",
    "type": "VEHICLETYPEJET"
  },
{
    "message0": "Transport",
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#E64A19",
    "type": "VEHICLETYPETRANSPORT"
  },
{
    "message0": "Get Vehicle Health Vehicle: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "VEHICLE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#E64A19",
    "type": "GETVEHICLEHEALTH"
  },
{
    "message0": "Set Vehicle Health Vehicle: %1 Health: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "VEHICLE"
      },
      {
        "type": "field_input",
        "name": "HEALTH"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#E64A19",
    "type": "SETVEHICLEHEALTH"
  },
{
    "message0": "Get Vehicle Driver Vehicle: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "VEHICLE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#E64A19",
    "type": "GETVEHICLEDRIVER"
  },
{
    "message0": "Eject from Vehicle Player: %1 Vehicle: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "PLAYER"
      },
      {
        "type": "field_input",
        "name": "VEHICLE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#E64A19",
    "type": "EJECTFROMVEHICLE"
  },
{
    "message0": "Lock Vehicle Vehicle: %1 Team: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "VEHICLE"
      },
      {
        "type": "field_input",
        "name": "TEAM"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#E64A19",
    "type": "LOCKVEHICLE"
  },
{
    "message0": "Set Vehicle Speed Vehicle: %1 Speed: %2",
    "args0": [
      {
        "type": "field_input",
        "name": "VEHICLE"
      },
      {
        "type": "field_input",
        "name": "SPEED"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#E64A19",
    "type": "SETVEHICLESPEED"
  },
{
    "message0": "Disable Vehicle Vehicle: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "VEHICLE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#E64A19",
    "type": "DISABLEVEHICLE"
  },
{
    "message0": "Enable Vehicle Vehicle: %1",
    "args0": [
      {
        "type": "field_input",
        "name": "VEHICLE"
      }
    ],
    "previousStatement": null,
    "nextStatement": null,
    "output": null,
    "colour": "#E64A19",
    "type": "ENABLEVEHICLE"
  },
]);
