// DesignatorEqualityComparer.cs
// Copyright Karel Kroeze, -2020

using System.Collections.Generic;
using Verse;

namespace CommandPalette
{
    public class DesignatorEqualityComparer : IEqualityComparer<Designator>
    {
        public bool Equals( Designator x, Designator y )
        {
            if ( x         == null || y         == null ) return false;
            return x.Label == y.Label && x.icon == y.icon;
        }

        public int GetHashCode( Designator obj )
        {
            return obj.Label.GetHashCode() * obj.icon.GetHashCode();
        }
    }
}