using System.Diagnostics.Contracts;
using System.Numerics;
using Content.Client.GameTicking.Managers;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.Light.EntitySystems;

public sealed partial class SunShadowSystem : SharedSunShadowSystem
{
    [Dependency] private ClientGameTicker _ticker = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private MetaDataSystem _metadata = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var mapQuery = AllEntityQuery<SunShadowCycleComponent, SunShadowComponent>();
        while (mapQuery.MoveNext(out var uid,  out var cycle, out var shadow))
        {
            if (!cycle.Running || cycle.Directions.Count == 0)
                continue;

            var pausedTime = _metadata.GetPauseTime(uid);

            // Core SunShadow Crush fix
            var duration = cycle.Duration.TotalSeconds;

            if (duration <= 0)
                continue;

            var totalSeconds = _timing.CurTime
                .Add(cycle.Offset)
                .Subtract(_ticker.RoundStartTimeSpan)
                .Subtract(pausedTime)
                .TotalSeconds;

            var time = (float)(totalSeconds % duration);

            if (time < 0f)
                time += (float) duration;

            var (direction, alpha) = GetShadow((uid, cycle), time);
            shadow.Direction = direction;
            shadow.Alpha = alpha;
        }
    }

    [Pure]
    public (Vector2 Direction, float Alpha) GetShadow(Entity<SunShadowCycleComponent> entity, float time) // Core SunShadow Crush fix
    {
        var directions = entity.Comp.Directions;

        if (directions.Count == 0 || entity.Comp.Duration.TotalSeconds <= 0)
            return (Vector2.Zero, 0f);

        var ratio = (float)(time / entity.Comp.Duration.TotalSeconds);

        ratio %= 1f;
        if (ratio < 0f)
            ratio += 1f;

        for (var i = directions.Count - 1; i >= 0; i--)
        {
            var dir = directions[i];

            if (ratio >= dir.Ratio)
            {
                var next = directions[(i + 1) % directions.Count];

                var currentRatio = dir.Ratio;
                var nextRatio = i == directions.Count - 1
                    ? next.Ratio + 1f
                    : next.Ratio;

                var range = nextRatio - currentRatio;

                if (range <= 0f)
                    return (dir.Direction, dir.Alpha);

                var diff = (ratio - currentRatio) / range;
                diff = Math.Clamp(diff, 0f, 1f);

                var currentAngle = dir.Direction.ToAngle();
                var nextAngle = next.Direction.ToAngle();

                var angle = Angle.Lerp(currentAngle, nextAngle, diff);

                var lengthDiff = MathF.Pow(diff, 1f / 2f);
                var length = float.Lerp(dir.Direction.Length(), next.Direction.Length(), lengthDiff);

                var vector = angle.ToVec() * length;
                var alpha = float.Lerp(dir.Alpha, next.Alpha, diff);

                return (vector, alpha);
            }
        }
        {
            var last = directions[^1];
            var first = directions[0];

            var currentRatio = last.Ratio;
            var nextRatio = first.Ratio + 1f;
            var wrappedRatio = ratio + 1f;

            var range = nextRatio - currentRatio;

            if (range <= 0f)
                return (last.Direction, last.Alpha);

            var diff = (wrappedRatio - currentRatio) / range;
            diff = Math.Clamp(diff, 0f, 1f);

            var currentAngle = last.Direction.ToAngle();
            var nextAngle = first.Direction.ToAngle();

            var angle = Angle.Lerp(currentAngle, nextAngle, diff);

            var lengthDiff = MathF.Pow(diff, 1f / 2f);
            var length = float.Lerp(last.Direction.Length(), first.Direction.Length(), lengthDiff);

            var vector = angle.ToVec() * length;
            var alpha = float.Lerp(last.Alpha, first.Alpha, diff);

            return (vector, alpha);
        }
    }
}
