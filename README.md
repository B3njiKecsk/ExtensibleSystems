# Extensible Systems

This repository contains selected source code from my gameplay systems and technical design project.

The code accompanies the architectural case study in my portfolio and provides implementation examples for the design decisions discussed there.

## Where to Start

- **Entity.cs** — Shared gameplay abstraction for plants, zombies, and projectiles.
- **Plant.cs** — Developer-facing gameplay API and reusable plant behaviour, an example of an Entity Subclass.
- **PlantRegistry.cs** — Centralized content registration and construction.
- **GameManager.cs / LevelManager.cs** — Game lifecycle, simulation coordination.
- **EntityActor.cs** — Boundary between gameplay entities and their Unity representations.
- **TileData.cs** — Tile state, plant placement, and zombie movement notifications.
- **Equipment.cs / Armor.cs** — Composition-based equipment and damage handling.

The portfolio page explains the motivation behind these systems, the trade-offs involved, and the areas I would redesign in a theoretical second iteration.
