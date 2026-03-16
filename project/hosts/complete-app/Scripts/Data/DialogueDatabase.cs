using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace UltimaMagic.Data;

public static class DialogueDatabase
{
    private const string DialogueDataPath = "res://Resources/Data/dialogue.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static Dictionary<string, DialogueEntry>? _entries;

    public static DialogueEntry GetEntry(string dialogueId)
    {
        EnsureLoaded();

        if (!string.IsNullOrWhiteSpace(dialogueId)
            && _entries != null
            && _entries.TryGetValue(dialogueId, out var entry))
        {
            return new DialogueEntry
            {
                Name = entry.Name,
                Lines = (string[])entry.Lines.Clone()
            };
        }

        return new DialogueEntry
        {
            Name = "System",
            Lines = [$"Missing dialogue entry: {dialogueId}"]
        };
    }

    private static void EnsureLoaded()
    {
        if (_entries != null)
        {
            return;
        }

        if (!Godot.FileAccess.FileExists(DialogueDataPath))
        {
            GD.PushError($"Dialogue data file not found at '{DialogueDataPath}'.");
            _entries = new Dictionary<string, DialogueEntry>();
            return;
        }

        try
        {
            var json = Godot.FileAccess.GetFileAsString(DialogueDataPath);
            _entries = JsonSerializer.Deserialize<Dictionary<string, DialogueEntry>>(json, JsonOptions)
                ?? new Dictionary<string, DialogueEntry>();
        }
        catch (JsonException exception)
        {
            GD.PushError($"Failed to parse dialogue data: {exception.Message}");
            _entries = new Dictionary<string, DialogueEntry>();
        }
    }

    public sealed class DialogueEntry
    {
        public string Name { get; set; } = "Narrator";

        public string[] Lines { get; set; } = Array.Empty<string>();
    }
}
