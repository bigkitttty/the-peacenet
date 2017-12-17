﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plex.Engine.GraphicsSubsystem;

namespace Plex.Engine.GUI
{
    public class ProgressBar : Control
    {
        private float _value = 0.0f;

        public float Value
        {
            get
            {
                return _value;
            }
            set
            {
                value = MathHelper.Clamp(value, 0f, 1f);
                if (_value == value)
                    return;
                _value = value;
                Invalidate(true);
            }
        }

        protected override void OnPaint(GameTime time, GraphicsContext gfx)
        {
            gfx.Clear(Color.Black);
            gfx.DrawRectangle(0, 0, (int)MathHelper.Lerp(0, Width, _value), Height, Theme.GetAccentColor());
        }
    }
}
