public enum TerrainType : byte
{
    Empty,       // Luft
    Dirt,        // destructable ground
    Stairs,      // destructable and passable ground
    Steel,       // indesctructable ground
    Fire,        // immediate death through fire
    Water,       // immediate death through drowning
    Bolt,        // immediate death through electric shock
    OneWayLeft,  // Nur von links nach rechts durchgrabbar
    OneWayRight, // Nur von rechts nach links durchgrabbar
    Goal,        // Level Ziel
    Count
}