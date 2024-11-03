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
    public record struct Player(Entity Focus);
    public record struct PlayerFocus();
    public record struct Monster();
    public record struct CameraTarget(float Weight);
    public record struct Sight(int Range, BoolMap? LastFov, Vector2f? LastPosition);
    public record struct Tint(Color At);
    public record struct Position(Vector2f At);
    public record struct RoguesImage(string Image);
    public record struct LerpPosition(Vector2f Source, Vector2f Target, float Time);
    public record struct LerpColor(Color Source, Color Target, float Time);
    public record struct LastKnownPosition(Vector2f? At);
    public record struct Orientation(Vector2f To, Vector2f Last)
    {
        public int Side { get; set; } = -1;

        public Orientation(Vector2f to) : this(to, new Vector2f(0, 0)) 
        { }
        
        public static Orientation Left => new Orientation(new Vector2f(-1, 0), new Vector2f(0, 0));
        public static Orientation Right => new Orientation(new Vector2f(1, 0), new Vector2f(0, 0));

        public bool IsTurnedLeft() => To.X < 0;

        public void Set(Vector2f v)
        {
            if (v.X != 0)
            {
                Side = (int)v.X;
            }

            Last = To;
            To = v;
        }
    }
}