using Arch.Core;
using SFML.System;
using svarog;
using svarog.Algorithms;

namespace dungeon_game_plugin
{
    [Plugin(Priority = 2)]
    public class PlayerControlsSystem : Plugin
    {
        QueryDescription playerPositionQuery;
        BoolMap floorPlan;

        public override void Register(Svarog svarog)
        {
            playerPositionQuery = new QueryDescription().WithAll<Player, Position>();
        }

        public override void Load(Svarog svarog)
        {
            floorPlan = svarog.resources.GetFromBag<IntMap>("dungeon: room id map").ToBoolMap(p => p == 0);
            DungeonGamePlugin.OnLevelGenerated += (o, e) =>
            {
                floorPlan = svarog.resources.GetFromBag<IntMap>("dungeon: room id map").ToBoolMap(p => p == 0);
            };
        }

        public override void Frame(Svarog svarog)
        {
            var vector = new Vector2f(0, 0);
            if (svarog.keyboard.IsJustPressed(SFML.Window.Keyboard.Scancode.W))
            {
                vector.Y = -1;
            }
            else if (svarog.keyboard.IsJustPressed(SFML.Window.Keyboard.Scancode.S))
            {
                vector.Y = 1;
            }
            else if (svarog.keyboard.IsJustPressed(SFML.Window.Keyboard.Scancode.A))
            {
                vector.X = -1;
            }
            else if (svarog.keyboard.IsJustPressed(SFML.Window.Keyboard.Scancode.D))
            {
                vector.X = 1;
            }

            if (vector.SqrMagnitude() > 0.0f)
            {
                svarog.world.Query(in playerPositionQuery, (Entity entity, ref Player player, ref Position position) =>
                {
                    var p = position.At + vector;
                    if (!floorPlan.Values[(int)p.X, (int)p.Y])
                    {
                        LerpSystem.Add(entity, new LerpPosition() { Source = position.At, Target = position.At + vector, Time = 0.15f });
                    }
                });
            }
        }
    }
}
