{
  "example_logic": {
    "title": "Sample Working Logic: Player Movement & Condition",
    "description": "This example shows a simple logic flow: when a player presses a button, the character moves forward if a condition is met.",
    "blocks": [
      {
        "block": "On Button Press",
        "image": "assets/img/icons/events.png",
        "snap_options": ["event", "output"]
      },
      {
        "block": "If Condition",
        "image": "assets/img/icons/conditions.png",
        "snap_options": ["input", "C-shaped"]
      },
      {
        "block": "Move Forward",
        "image": "assets/img/icons/actions.png",
        "snap_options": ["input", "output"]
      }
    ],
    "connections": [
      {"from": 0, "to": 1, "type": "next"},
      {"from": 1, "to": 2, "type": "child"}
    ],
    "svg": "dev/docs/ui_mockup.svg"
  }
}
