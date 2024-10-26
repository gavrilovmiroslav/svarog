using SFML.Graphics;
using Stateless;

namespace svarog
{
    public abstract class GenerativePlugin : Plugin
    {
        public enum EProcgen
        {
            Generation,
            Playback,
        }

        public enum ETrigger
        {
            Done,
            Restart,
        }

        protected Sprite sprite = new();
        protected Color[] colors = new Color[]
        {
            new Color(0, 0, 0, 127),
            new Color(0xFF0000AA), // Red
            new Color(0x00FF00AA), // Green
            new Color(0x0000FFAA), // Blue
            new Color(0xFFFF00AA), // Yellow
            new Color(0xFF00FFAA), // Magenta
            new Color(0x00FFFFAA), // Cyan
            new Color(0x800000AA), // Maroon
            new Color(0x808000AA), // Olive
            new Color(0x008000AA), // Dark Green
            new Color(0x800080AA), // Purple
            new Color(0x008080AA), // Teal
            new Color(0x000080AA), // Navy
            new Color(0xFFA500AA), // Orange
            new Color(0xA52A2AAA), // Brown
            new Color(0x8B4513AA), // Saddle Brown
            new Color(0xDEB887AA), // Burlywood
            new Color(0xD2691EAA), // Chocolate
            new Color(0xF4A460AA), // Sandy Brown
            new Color(0xFA8072AA), // Salmon
            new Color(0xE9967AAA), // Dark Salmon
            new Color(0xFF4500AA), // Orange Red
            new Color(0xFF6347AA), // Tomato
            new Color(0xFF7F50AA), // Coral
            new Color(0xFFD700AA), // Gold
            new Color(0xEEE8AAAA), // Pale Goldenrod
            new Color(0xF0E68CAA), // Khaki
            new Color(0xBDB76BAA), // Dark Khaki
            new Color(0x7FFF00AA), // Chartreuse
            new Color(0x7CFC00AA), // Lawn Green
            new Color(0xADFF2FAA), // Green Yellow
            new Color(0x32CD32AA), // Lime Green
            new Color(0x9ACD32AA), // Yellow Green
            new Color(0x6B8E23AA), // Olive Drab
            new Color(0x556B2FAA), // Dark Olive Green
            new Color(0x66CDAAAA), // Medium Aquamarine
            new Color(0x4682B4AA), // Steel Blue
            new Color(0x5F9EA0AA), // Cadet Blue
            new Color(0xB0C4DEAA), // Light Steel Blue
            new Color(0xADD8E6AA), // Light Blue
            new Color(0x87CEEBAA), // Sky Blue
            new Color(0x87CEFAAA), // Light Sky Blue
            new Color(0x00BFFFAA), // Deep Sky Blue
            new Color(0x1E90FFAA), // Dodger Blue
            new Color(0x6495EDAA), // Cornflower Blue
            new Color(0x7B68EEAA), // Medium Slate Blue
            new Color(0x6A5ACDAA), // Slate Blue
            new Color(0x483D8BAA), // Dark Slate Blue
            new Color(0x9370DBAA), // Medium Purple
            new Color(0x8A2BE2AA), // Blue Violet
            new Color(0x9400D3AA), // Dark Violet
            new Color(0x9932CCAA), // Dark Orchid
            new Color(0xBA55D3AA), // Medium Orchid
            new Color(0xDA70D6AA), // Orchid
            new Color(0xEE82EEAA), // Violet
            new Color(0xDDA0DDAA), // Plum
            new Color(0xD8BFD8AA), // Thistle
            new Color(0xE6E6FAAA), // Lavender
            new Color(0xFFF0F5AA), // Lavender Blush
            new Color(0xFFE4E1AA), // Misty Rose
            new Color(0xFFDAB9AA), // Peach Puff
            new Color(0xFFF5EEAA),  // Seashell
            new Color(0xCD5C5CAA), // Indian Red
            new Color(0xF08080AA), // Light Coral
            new Color(0xFF69B4AA), // Hot Pink
            new Color(0xFF1493AA), // Deep Pink
            new Color(0xC71585AA), // Medium Violet Red
            new Color(0xDB7093AA), // Pale Violet Red
            new Color(0xB22222AA), // Firebrick
            new Color(0xFFFAF0AA), // Floral White
            new Color(0xFDF5E6AA), // Old Lace
            new Color(0xF5FFFAAA)  // Mint Cream
        };

        private string name;

        public GenerativePlugin(string name)
        {
            this.name = name;
        }

        public abstract void Generate(Svarog instance, StateMachine<EProcgen, ETrigger> sm);

        public override void Load(Svarog instance)
        {
            var sm = instance.resources.CreateStateMachine<EProcgen, ETrigger>($"{name}-procgen", EProcgen.Generation);

            var generate = () =>
            {
                Generate(instance, sm);
                sm.Fire(ETrigger.Done);
            };

            sm.Configure(EProcgen.Generation)
                .OnEntry(generate)
                .OnActivate(generate)
                .Permit(ETrigger.Done, EProcgen.Playback)
                .Ignore(ETrigger.Restart);

            sm.Configure(EProcgen.Playback)
                .OnEntry(() =>
                {
                    Console.WriteLine("DONE!");
                })
                .Permit(ETrigger.Restart, EProcgen.Generation)
                .Ignore(ETrigger.Done);

            sm.Activate();
        }

        public override void Frame(Svarog instance)
        {
            if (instance.mouse.IsJustPressed(SFML.Window.Mouse.Button.Right))
            {
                var sm = instance.resources.GetStateMachine<EProcgen, ETrigger>($"{name}-procgen");
                sm?.Fire(ETrigger.Restart);
            }
        }
    }
}
