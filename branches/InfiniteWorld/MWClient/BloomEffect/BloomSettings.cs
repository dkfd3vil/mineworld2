namespace MineWorld.BloomEffect
{
    /// <summary>
    ///   Class holds all the settings used to tweak the bloom effect.
    /// </summary>
    public class BloomSettings
    {
        // Name of a preset bloom setting, for display to the user.
        public readonly float BaseIntensity;


        // Independently control the color saturation of the bloom and
        // base images. Zero is totally desaturated, 1.0 leaves saturation
        // unchanged, while higher values increase the saturation level.
        public readonly float BaseSaturation;
        public readonly float BloomIntensity;
        public readonly float BloomSaturation;
        public readonly float BloomThreshold;


        // Controls how much blurring is applied to the bloom image.
        // The typical range is from 1 up to 10 or so.
        public readonly float BlurAmount;
        public readonly string Name;

        /// <summary>
        ///   Table of preset bloom settings, used by the sample program.
        /// </summary>
        public static BloomSettings[] PresetSettings =
            {
                //                Name           Thresh  Blur Bloom  Base  BloomSat BaseSat
                new BloomSettings("Default", 0.3f, 4, 2, 1, 1, 1),
                new BloomSettings("SuperBloom", 0.25f, 4, 2, 1, 2, 1),
                new BloomSettings("Disconnected", 0, 2, 2, 0.1f, 2, 1)
            };

        /// <summary>
        ///   Constructs a new bloom settings descriptor.
        /// </summary>
        public BloomSettings(string name, float bloomThreshold, float blurAmount,
                             float bloomIntensity, float baseIntensity,
                             float bloomSaturation, float baseSaturation)
        {
            Name = name;
            BloomThreshold = bloomThreshold;
            BlurAmount = blurAmount;
            BloomIntensity = bloomIntensity;
            BaseIntensity = baseIntensity;
            BloomSaturation = bloomSaturation;
            BaseSaturation = baseSaturation;
        }
    }
}