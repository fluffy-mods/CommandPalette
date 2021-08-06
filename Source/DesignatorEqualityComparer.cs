// DesignatorEqualityComparer.cs
// Copyright Karel Kroeze, -2020

using System.Collections.Generic;
using Verse;

namespace CommandPalette {
    public class DesignatorEqualityComparer: IEqualityComparer<Designator> {
        public bool Equals(Designator x, Designator y) {
            if (x == null || y == null) {
                return false;
            }

            return x.Label == y.Label && x.icon == y.icon;
        }

        public int GetHashCode(Designator obj) {
            if (!obj.Label.NullOrEmpty() && obj.icon != null) {
                return obj.Label.GetHashCode() * obj.icon.GetHashCode();
            }

            if (!obj.Label.NullOrEmpty()) {
                return obj.Label.GetHashCode();
            }

            return obj.icon != null ? obj.icon.GetHashCode() : 0;
        }
    }
}
