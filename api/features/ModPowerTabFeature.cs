namespace NeoModLoader.api.features;

public abstract class ModPowerTabFeature : ModObjectFeature<PowersTab> {
  public abstract bool PositionButton(PowerButton button);
}