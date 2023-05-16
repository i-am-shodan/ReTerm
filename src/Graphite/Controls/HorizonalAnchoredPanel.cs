using System;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Graphite.Controls
{
    public class HorizonalAnchoredPanel : Control
    {
        private Control left;
        public Control Left
        {
            get => left;
            set
            {
                left = value;
                RecalculateChildPositions();
            }
        }

        private Control right;
        public Control Right
        {
            get => right;
            set
            {
                right = value;
                RecalculateChildPositions();
            }
        }

        public override PointF Location
        {
            get => base.Location;
            set
            {
                base.Location = value;
                RecalculateChildPositions();
            }
        }

        private void RecalculateChildPositions()
        {
            if (left != null)
            {
                left.Location = Location;
            }

            if (right != null)
            {
                right.Location = new PointF(Location.X + Size.Width - right.Size.Width, Location.Y);
            }
        }

        public override void Draw(Image<Rgb24> buffer)
        {
            left.Draw(buffer);
            right.Draw(buffer);
        }
    }
}
