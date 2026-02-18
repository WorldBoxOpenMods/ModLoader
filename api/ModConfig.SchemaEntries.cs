namespace NeoModLoader.api;

public partial class ModConfig
{
    private abstract class ConfigSchemaEntry
    {
        public string Id = "";
        public string IconPath = "";
        public string CallBack = "";
        public abstract ConfigItemType Type { get; }
        public abstract ConfigStateEntry CreateDefaultState();
        public abstract ConfigStateEntry NormalizeState(ConfigStateEntry pState);
        public abstract void ApplyMeta(ModConfigItem pItem);
    }

    private sealed class SwitchSchemaEntry : ConfigSchemaEntry
    {
        public bool DefaultValue;
        public override ConfigItemType Type => ConfigItemType.SWITCH;
        public override ConfigStateEntry CreateDefaultState() => new BoolStateEntry { Value = DefaultValue };
        public override ConfigStateEntry NormalizeState(ConfigStateEntry pState) =>
            pState is BoolStateEntry typed ? typed : CreateDefaultState();
        public override void ApplyMeta(ModConfigItem pItem)
        {
        }
    }

    private sealed class TextSchemaEntry : ConfigSchemaEntry
    {
        public string DefaultValue = "";
        public override ConfigItemType Type => ConfigItemType.TEXT;
        public override ConfigStateEntry CreateDefaultState() => new TextStateEntry { Value = DefaultValue ?? "" };
        public override ConfigStateEntry NormalizeState(ConfigStateEntry pState) =>
            pState is TextStateEntry typed ? typed : CreateDefaultState();
        public override void ApplyMeta(ModConfigItem pItem)
        {
        }
    }

    private sealed class FloatSliderSchemaEntry : ConfigSchemaEntry
    {
        public float DefaultValue;
        public float MinValue;
        public float MaxValue;
        public override ConfigItemType Type => ConfigItemType.SLIDER;
        public override ConfigStateEntry CreateDefaultState()
        {
            float min = MinValue;
            float max = MaxValue < min ? min : MaxValue;
            return new FloatStateEntry { Value = Math.Max(min, Math.Min(max, DefaultValue)) };
        }

        public override ConfigStateEntry NormalizeState(ConfigStateEntry pState)
        {
            if (pState is not FloatStateEntry typed) return CreateDefaultState();
            float min = MinValue;
            float max = MaxValue < min ? min : MaxValue;
            return new FloatStateEntry
            {
                Value = Math.Max(min, Math.Min(max, typed.Value))
            };
        }

        public override void ApplyMeta(ModConfigItem pItem)
        {
            float min = MinValue;
            float max = MaxValue < min ? min : MaxValue;
            pItem.SetFloatRange(min, max);
        }
    }

    private sealed class IntSliderSchemaEntry : ConfigSchemaEntry
    {
        public int DefaultValue;
        public int MinValue;
        public int MaxValue;
        public override ConfigItemType Type => ConfigItemType.INT_SLIDER;
        public override ConfigStateEntry CreateDefaultState()
        {
            int min = MinValue;
            int max = MaxValue < min ? min : MaxValue;
            return new IntStateEntry { Value = Math.Max(min, Math.Min(max, DefaultValue)) };
        }

        public override ConfigStateEntry NormalizeState(ConfigStateEntry pState)
        {
            if (pState is not IntStateEntry typed) return CreateDefaultState();
            int min = MinValue;
            int max = MaxValue < min ? min : MaxValue;
            return new IntStateEntry
            {
                Value = Math.Max(min, Math.Min(max, typed.Value))
            };
        }

        public override void ApplyMeta(ModConfigItem pItem)
        {
            int min = MinValue;
            int max = MaxValue < min ? min : MaxValue;
            pItem.SetIntRange(min, max);
        }
    }

    private sealed class SelectSchemaEntry : ConfigSchemaEntry
    {
        public int DefaultValue;
        public string[] Options = Array.Empty<string>();
        public string OptionsRaw = "";
        public override ConfigItemType Type => ConfigItemType.SELECT;
        public override ConfigStateEntry CreateDefaultState()
        {
            int selected = ClampSelectIndex(DefaultValue, Options);
            return new SelectStateEntry { Value = selected };
        }

        public override ConfigStateEntry NormalizeState(ConfigStateEntry pState)
        {
            if (pState is not SelectStateEntry typed) return CreateDefaultState();
            return new SelectStateEntry
            {
                Value = ClampSelectIndex(typed.Value, Options)
            };
        }

        public override void ApplyMeta(ModConfigItem pItem)
        {
            pItem.TextVal = OptionsRaw;
        }
    }
}
