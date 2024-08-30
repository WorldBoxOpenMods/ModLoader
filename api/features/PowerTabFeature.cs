namespace NeoModLoader.api.features;

public abstract class PowerTabFeature : ModObjectFeature<PowersTab> {
  public abstract bool PositionButton(PowerButton button);
}