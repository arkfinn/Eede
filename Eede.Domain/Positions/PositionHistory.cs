﻿using System;

namespace Eede.Domain.Positions
{
    public class PositionHistory
    {
        public readonly Position Start;
        public readonly Position Last;
        public readonly Position Now;

        public PositionHistory(Position start)
        {
            Start = start;
            Last = start;
            Now = start;
        }

        private PositionHistory(Position start, Position last, Position now)
        {
            Start = start;
            Last = last;
            Now = now;
        }


        public PositionHistory Update(Position now)
        {
            return new PositionHistory(Start, Now, now);
        }
    }
}
