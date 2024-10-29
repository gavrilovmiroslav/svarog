using Arch.Core.Utils;
using SFML.System;
using svarog.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dungeon_game_plugin
{
    public record struct Player();
    public record struct CameraTarget(float Weight);
    public record struct Sight(int Range, BoolMap? LastFov, Vector2i? LastPosition);
    public record struct Position(Vector2i At);
    public record struct RoguesImage(string Image);
}
