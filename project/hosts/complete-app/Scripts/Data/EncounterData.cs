using System;
using Godot;

namespace UltimaMagic.Data;

[GlobalClass]
public partial class EncounterData : Resource
{
    [Export]
    public EnemyGroup[] PossibleGroups { get; set; } = Array.Empty<EnemyGroup>();
}
