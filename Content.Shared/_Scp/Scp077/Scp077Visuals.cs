using Robust.Shared.Serialization;

namespace Content.Shared._Scp.Scp077;

[Serializable, NetSerializable]
public enum Scp077Visuals : byte
{
    RuneState,
    GlowVisible, // Видно ли свечение вообще
    GlowColor    // Синее или зеленое
}

[Serializable, NetSerializable]
public enum Scp077GlowType : byte
{
    None,
    Blue,
    Green
}