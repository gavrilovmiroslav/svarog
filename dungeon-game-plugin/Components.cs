using Arch.Core;
using Arch.Core.Utils;
using SFML.Graphics;
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
    public record struct Monster();
    public record struct CameraTarget(float Weight);
    public record struct Sight(int Range, BoolMap? LastFov, Vector2f? LastPosition);
    public record struct Tint(Color At);
    public record struct Position(Vector2f At);
    public record struct RoguesImage(string Image);
    public record struct LerpPosition(Vector2f Source, Vector2f Target, float Time);
    public record struct LerpColor(Color Source, Color Target, float Time);
}
