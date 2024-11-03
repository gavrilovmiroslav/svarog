using Arch.Core;
using Arch.Core.Extensions;
using SFML.System;
using Stateless.Graph;
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
            playerPositionQuery = new QueryDescription().WithAll<Player, Position, Orientation>();
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
            else if (svarog.keyboard.IsJustPressed(SFML.Window.Keyboard.Scancode.Q))
            {
                vector.X = -1;
                vector.Y = -1;
            }
            else if (svarog.keyboard.IsJustPressed(SFML.Window.Keyboard.Scancode.E))
            {
                vector.X = 1;
                vector.Y = -1;
            }
            else if (svarog.keyboard.IsJustPressed(SFML.Window.Keyboard.Scancode.Z))
            {
                vector.X = -1;
                vector.Y = 1;
            }
            else if (svarog.keyboard.IsJustPressed(SFML.Window.Keyboard.Scancode.C))
            {
                vector.X = 1;
                vector.Y = 1;
            }

            if (vector.SqrMagnitude() > 0.0f)
            {
                var justLook = svarog.keyboard.IsDown(SFML.Window.Keyboard.Scancode.LShift);
                svarog.world.Query(in playerPositionQuery, (Entity entity, ref Player player, ref Position position, ref Orientation orientation) =>
                {
                    if (!justLook)
                    {
                        var p = position.At + vector;
                        if (!floorPlan.Values[(int)p.X, (int)p.Y])
                        {
                            LerpSystem.Add(entity, new LerpPosition() { Source = position.At, Target = position.At + vector, Time = 0.25f });
                        }
                    }

                    orientation.Set(vector);
                    player.Focus.Get<Position>().At = position.At + orientation.To.Normalized() * entity.Get<Sight>().Range * (justLook ? 1 : 0.5f);
                });
            }
        }
    }
}
