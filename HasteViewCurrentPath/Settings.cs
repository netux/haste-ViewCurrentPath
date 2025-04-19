using Landfall.Haste;
using UnityEngine.Localization;
using Zorro.Settings;

namespace HasteViewCurrentPath;

public static class Settings
{
    static readonly string Category = "View Current Path";

    [HasteSetting]
    public class RendererSetting : EnumSetting<RendererSetting.Renderer>, IExposedSetting
    {
        public override void ApplyValue()
        {
            if (!ViewCurrentPath.IsHooked)
            {
                return;
            }

            ViewCurrentPath.SetRenderer(SelectedRendererType);
        }

        public Type SelectedRendererType {
            get => Value switch
            {
                Renderer.IconWithTextFallback => typeof(CurrentPathIconRenderer),
                Renderer.Text => typeof(CurrentPathTextRenderer),
                _ => throw new NotImplementedException(),
            };
        }

        public override List<LocalizedString> GetLocalizedChoices() => [
            new UnlocalizedString("Icon (with text fallback)"),
            new UnlocalizedString("Text"),
        ];

        protected override Renderer GetDefaultValue() => Renderer.IconWithTextFallback;

        public string GetCategory() => Category;

        public LocalizedString GetDisplayName() => new UnlocalizedString("Preferred Renderer");

        public enum Renderer
        {
            IconWithTextFallback,
            Text
        }
    }
}

