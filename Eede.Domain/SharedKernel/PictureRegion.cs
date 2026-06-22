#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Eede.Domain.SharedKernel
{
    public readonly struct PictureRegion : IEnumerable<PictureArea>
    {
        private readonly IEnumerable<PictureArea>? Areas;

        public PictureRegion(PictureArea area)
        {
            Areas = new[] { area };
        }

        public PictureRegion(IEnumerable<PictureArea> areas)
        {
            Areas = areas ?? Array.Empty<PictureArea>();
        }

        public bool IsEmpty
        {
            get
            {
                if (Areas == null) return true;
                foreach (var area in Areas)
                {
                    if (!area.IsEmpty) return false;
                }
                return true;
            }
        }

        public PictureArea GetBoundingBox()
        {
            if (Areas == null) return new PictureArea(new Position(0, 0), new PictureSize(0, 0));

            PictureArea? combined = null;
            foreach (var area in Areas)
            {
                if (area.IsEmpty) continue;

                if (combined == null)
                {
                    combined = area;
                }
                else
                {
                    combined = combined.Value.Combine(area);
                }
            }

            return combined ?? new PictureArea(new Position(0, 0), new PictureSize(0, 0));
        }

        public IEnumerator<PictureArea> GetEnumerator() => (Areas ?? Array.Empty<PictureArea>()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static implicit operator PictureRegion(PictureArea area) => new PictureRegion(area);
    }
}
