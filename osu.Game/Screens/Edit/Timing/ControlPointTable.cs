// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Screens.Edit.Timing
{
    public class ControlPointTable : TableContainer
    {
        private const float horizontal_inset = 20;
        private const float row_height = 25;
        private const int text_size = 14;

        private readonly FillFlowContainer backgroundFlow;

        [Resolved]
        private Bindable<IEnumerable<ControlPoint>> selectedPoints { get; set; }

        public ControlPointTable()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding { Horizontal = horizontal_inset };
            RowSize = new Dimension(GridSizeMode.Absolute, row_height);

            AddInternal(backgroundFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Depth = 1f,
                Padding = new MarginPadding { Horizontal = -horizontal_inset },
                Margin = new MarginPadding { Top = row_height }
            });
        }

        public IEnumerable<ControlPoint> ControlPoints
        {
            set
            {
                Content = null;
                backgroundFlow.Clear();

                if (value?.Any() != true)
                    return;

                var grouped = value.GroupBy(cp => cp.Time, cp => cp);

                foreach (var group in grouped)
                {
                    backgroundFlow.Add(new RowBackground { Action = () => selectedPoints.Value = group });
                }

                Columns = createHeaders();
                Content = grouped.Select((s, i) => createContent(i, s)).ToArray().ToRectangular();
            }
        }

        private TableColumn[] createHeaders()
        {
            var columns = new List<TableColumn>
            {
                new TableColumn(string.Empty, Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
                new TableColumn("Time", Anchor.Centre, new Dimension(GridSizeMode.AutoSize)),
                new TableColumn("Attributes", Anchor.Centre),
            };

            return columns.ToArray();
        }

        private Drawable[] createContent(int index, IGrouping<double, ControlPoint> controlPoints) => new Drawable[]
        {
            new OsuSpriteText
            {
                Text = $"#{index + 1}",
                Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold),
                Margin = new MarginPadding(10)
            },
            new OsuSpriteText
            {
                Text = $"{controlPoints.Key:n0}ms",
                Font = OsuFont.GetFont(size: text_size, weight: FontWeight.Bold)
            },
            new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                ChildrenEnumerable = controlPoints.Select(createAttribute).Where(c => c != null),
                Padding = new MarginPadding(10),
                Spacing = new Vector2(10)
            },
        };

        private Drawable createAttribute(ControlPoint controlPoint)
        {
            if (controlPoint.AutoGenerated)
                return null;

            switch (controlPoint)
            {
                case TimingControlPoint timing:
                    return new RowAttribute("timing", $"{60000 / timing.BeatLength:n1}bpm {timing.TimeSignature}");

                case DifficultyControlPoint difficulty:

                    return new RowAttribute("difficulty", $"{difficulty.SpeedMultiplier:n2}x");

                case EffectControlPoint effect:
                    return new RowAttribute("effect", $"{(effect.KiaiMode ? "Kiai " : "")}{(effect.OmitFirstBarLine ? "NoBarLine " : "")}");

                case SampleControlPoint sample:
                    return new RowAttribute("sample", $"{sample.SampleBank} {sample.SampleVolume}%");
            }

            return null;
        }

        protected override Drawable CreateHeader(int index, TableColumn column) => new HeaderText(column?.Header ?? string.Empty);

        private class HeaderText : OsuSpriteText
        {
            public HeaderText(string text)
            {
                Text = text.ToUpper();
                Font = OsuFont.GetFont(size: 12, weight: FontWeight.Black);
            }
        }

        public class RowBackground : OsuClickableContainer
        {
            private const int fade_duration = 100;

            private readonly Box hoveredBackground;

            public RowBackground()
            {
                RelativeSizeAxes = Axes.X;
                Height = 25;

                AlwaysPresent = true;

                CornerRadius = 3;
                Masking = true;

                Children = new Drawable[]
                {
                    hoveredBackground = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                hoveredBackground.Colour = colours.Blue;
            }

            protected override bool OnHover(HoverEvent e)
            {
                hoveredBackground.FadeIn(fade_duration, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                hoveredBackground.FadeOut(fade_duration, Easing.OutQuint);
                base.OnHoverLost(e);
            }
        }
    }
}
