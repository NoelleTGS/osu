﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles
{
    public partial class HitCirclePlacementBlueprint : HitObjectPlacementBlueprint
    {
        public new HitCircle HitObject => (HitCircle)base.HitObject;

        private readonly HitCirclePiece circlePiece;

        [Resolved]
        private OsuHitObjectComposer? composer { get; set; }

        public HitCirclePlacementBlueprint()
            : base(new HitCircle())
        {
            InternalChild = circlePiece = new HitCirclePiece();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            BeginPlacement();
        }

        protected override void Update()
        {
            base.Update();

            circlePiece.UpdateFrom(HitObject);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == MouseButton.Left)
            {
                EndPlacement(true);
                return true;
            }

            return base.OnMouseDown(e);
        }

        public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double fallbackTime)
        {
            var result = composer?.TrySnapToNearbyObjects(screenSpacePosition, fallbackTime);
            result ??= composer?.TrySnapToDistanceGrid(screenSpacePosition);
            if (composer?.TrySnapToPositionGrid(result?.ScreenSpacePosition ?? screenSpacePosition, result?.Time ?? fallbackTime) is SnapResult gridSnapResult)
                result = gridSnapResult;
            result ??= new SnapResult(screenSpacePosition, fallbackTime);

            UpdateTimeAndPosition(result);
            HitObject.Position = ToLocalSpace(result.ScreenSpacePosition);
            return result;
        }
    }
}
