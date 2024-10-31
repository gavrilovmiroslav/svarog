using Arch.Core;
using Arch.Core.Extensions;
using FloodSpill;
using MathNet.Numerics.Statistics.Mcmc;
using SFML.System;
using svarog;
using svarog.Algorithms;
using svarog.Structures;

namespace dungeon_game_plugin
{
    [Plugin(Priority = 505)]
    public class LerpSystem : Plugin
    {
        enum ELerpKind
        {
            Position,
            Color,
        }

        record struct LerpInstance(Entity Entity, ELerpKind Kind);
        
        QueryDescription lerpPositionQuery;
        QueryDescription lerpColorQuery;

        Dictionary<LerpInstance, float> times = [];
        Dictionary<Entity, List<LerpPosition>> positionContinuations = [];
        Dictionary<Entity, List<LerpColor>> colorContinuations = [];

        private static LerpSystem Instance;

        public LerpSystem()
        {
            Instance = this;
        }

        public static void Add(Entity entity, LerpPosition dp)
        {
            if (Instance.positionContinuations.TryGetValue(entity, out List<LerpPosition>? value))
            {
                value.Add(dp);
            }
            else
            {
                Instance.positionContinuations[entity] = new List<LerpPosition>([dp]);
                Instance.times[new LerpInstance(entity, ELerpKind.Position)] = 0.0f;
            }
        }

        public static void Add(Entity entity, LerpColor dp)
        {
            if (Instance.colorContinuations.TryGetValue(entity, out List<LerpColor>? value))
            {
                value.Add(dp);
            }
            else
            {
                Instance.colorContinuations[entity] = new List<LerpColor>([dp]);
                Instance.times[new LerpInstance(entity, ELerpKind.Color)] = 0.0f;
            }
        }

        public override void Load(Svarog svarog)
        {
            lerpPositionQuery = new QueryDescription().WithAll<Position>();
            lerpColorQuery = new QueryDescription().WithAll<Tint>();
        }

        public override void Frame(Svarog svarog)
        {
            var dt = svarog.clock.ElapsedTime.AsSeconds();
            foreach (var (entity, lerps) in positionContinuations)
            {
                var key = new LerpInstance(entity, ELerpKind.Position);
                if (lerps.Count > 0)
                {
                    var lerp = lerps[0];
                    if (times.TryGetValue(key, out float t))
                    {
                        t += svarog.clock.ElapsedTime.AsSeconds();
                        var dl = Math.Clamp(t / lerp.Time, 0.0f, 1.0f);
                        entity.Get<Position>().At = Lerp.Linear(lerp.Source, lerp.Target, dl);

                        if (dl >= 1.0f)
                        {
                            lerps.RemoveAt(0);
                            if (lerps.Count > 0)
                            {
                                var next = lerps[0];
                                if (next.Source != lerp.Target)
                                {
                                    var dv = next.Target - next.Source;
                                    next.Source = lerp.Target;
                                    next.Target = next.Source + dv;
                                    lerps[0] = next;
                                }
                            }
                            times[key] = 0.0f;
                        }
                        else
                        {
                            times[key] = t;
                        }
                    }
                }
            }
        }
    }
}
