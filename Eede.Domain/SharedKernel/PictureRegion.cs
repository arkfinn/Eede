using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Eede.Domain.SharedKernel
{
    public readonly struct PictureRegion : IEnumerable<PictureArea>
    {
        private readonly IEnumerable<PictureArea> Areas;

        public PictureRegion(PictureArea area)
        {
            Areas = new[] { area };
        }

        public PictureRegion(IEnumerable<PictureArea> areas)
        {
            Areas = areas ?? Array.Empty<PictureArea>();
        }

        public bool IsEmpty => Areas == null || !Areas.Any() || Areas.All(a => a.IsEmpty);

        public PictureArea GetBoundingBox()
        {
            if (IsEmpty) return new PictureArea(new Position(0, 0), new PictureSize(0, 0));
            var first = Areas.First(a => !a.IsEmpty);
            return Areas.Where(a => !a.IsEmpty).Aggregate(first, (current, next) => current.Combine(next));
        }

        public IEnumerator<PictureArea> GetEnumerator() => (Areas ?? Array.Empty<PictureArea>()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static implicit operator PictureRegion(PictureArea area) => new PictureRegion(area);
    }
}
