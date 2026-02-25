namespace NeoModLoader.api;

public partial class ModConfig
{
    private abstract class ConfigStateEntry
    {
        public abstract ConfigItemType Type { get; }
        public abstract object BoxedValue { get; }
    }

    private sealed class BoolStateEntry : ConfigStateEntry
    {
        public bool Value;
        public override ConfigItemType Type => ConfigItemType.SWITCH;
        public override object BoxedValue => Value;
    }

    private sealed class FloatStateEntry : ConfigStateEntry
    {
        public float Value;
        public override ConfigItemType Type => ConfigItemType.SLIDER;
        public override object BoxedValue => Value;
    }

    private sealed class IntStateEntry : ConfigStateEntry
    {
        public int Value;
        public override ConfigItemType Type => ConfigItemType.INT_SLIDER;
        public override object BoxedValue => Value;
    }

    private sealed class TextStateEntry : ConfigStateEntry
    {
        public string Value = "";
        public override ConfigItemType Type => ConfigItemType.TEXT;
        public override object BoxedValue => Value;
    }

    private sealed class SelectStateEntry : ConfigStateEntry
    {
        public int Value;
        public override ConfigItemType Type => ConfigItemType.SELECT;
        public override object BoxedValue => Value;
    }
}
