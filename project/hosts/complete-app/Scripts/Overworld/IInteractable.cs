namespace UltimaMagic.Overworld;

public interface IInteractable
{
    string InteractionPrompt { get; }

    void Interact(Player player);
}
