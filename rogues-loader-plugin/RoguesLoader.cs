using SFML.Graphics;
using svarog;
using System.Text.RegularExpressions;

namespace svarog.Plugins
{
    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };
    }

    internal class RoguesLoader(Resources res)
    {
        public bool Load(string resource)
        {
            var bytes = File.ReadAllBytes($"Data//32rogues//{resource}.png");
            var texture = new Texture(bytes);
            res.Textures[resource] = texture;

            var regex = new Regex("""^(\d+)\.(\w+)\. (.+)$""");
            var txt = File.ReadAllLines($"Data//32rogues//{resource}.txt");
            foreach (var line in txt)
            {
                if (line.Trim().Length == 0) continue;
                var match = regex.Match(line);
                if (match.Success)
                {
                    var row = int.Parse(match.Groups[1].Value.Trim()) - 1;
                    var column = match.Groups[2].Value.Trim().ToCharArray()[0] - 97;
                    var name = match.Groups[3].Value.Trim();

                    var dashedName = name.Replace("(", "-").Replace(")", "-").Replace("/", "-").Replace(" ", "-");
                    var key = new Regex("[\\-]+").Replace(dashedName, "_");
                    if (key.EndsWith("_")) key = key[..(key.Length - 1)];
                    key = key.FirstCharToUpper();

                    res.Sprites[key] = new RSprite(texture, new IntRect(column * 32, row * 32, 32, 32));
                    res.NamedSprites.Add(resource, key);
                }
                else
                {
                    Console.WriteLine($"FAILED: {line}");
                }
            }
            return true;
        }
    }
}
