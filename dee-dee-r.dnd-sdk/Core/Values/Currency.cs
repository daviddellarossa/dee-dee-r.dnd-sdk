using System;

namespace DeeDeeR.DnD.Core.Values
{
    /// <summary>
    /// An immutable representation of a currency amount across all five denominations.
    /// </summary>
    public readonly struct Currency : IEquatable<Currency>
    {
        /// <summary>Copper pieces.</summary>
        public readonly int CP;

        /// <summary>Silver pieces (1 SP = 10 CP).</summary>
        public readonly int SP;

        /// <summary>Electrum pieces (1 EP = 50 CP).</summary>
        public readonly int EP;

        /// <summary>Gold pieces (1 GP = 100 CP).</summary>
        public readonly int GP;

        /// <summary>Platinum pieces (1 PP = 1000 CP).</summary>
        public readonly int PP;

        /// <summary>
        /// Creates a currency amount. Each denomination is clamped to zero — negative values are not
        /// valid. Compare <see cref="TotalInCopper"/> totals before subtracting to avoid per-denomination clamping.
        /// </summary>
        public Currency(int cp = 0, int sp = 0, int ep = 0, int gp = 0, int pp = 0)
        {
            CP = Math.Max(0, cp);
            SP = Math.Max(0, sp);
            EP = Math.Max(0, ep);
            GP = Math.Max(0, gp);
            PP = Math.Max(0, pp);
        }

        /// <summary>Total value expressed in copper pieces. Uses <c>long</c> to avoid overflow with large amounts.</summary>
        public long TotalInCopper => (long)CP + (long)SP * 10 + (long)EP * 50 + (long)GP * 100 + (long)PP * 1000;

        /// <summary>Adds two currency amounts denomination by denomination.</summary>
        public static Currency operator +(Currency a, Currency b) =>
            new Currency(a.CP + b.CP, a.SP + b.SP, a.EP + b.EP, a.GP + b.GP, a.PP + b.PP);

        /// <summary>
        /// Subtracts <paramref name="b"/> from <paramref name="a"/> denomination by denomination.
        /// Each denomination is independently clamped to zero — use <see cref="TotalInCopper"/> to
        /// compare totals before subtracting if cross-denomination borrowing is required.
        /// </summary>
        public static Currency operator -(Currency a, Currency b) =>
            new Currency(a.CP - b.CP, a.SP - b.SP, a.EP - b.EP, a.GP - b.GP, a.PP - b.PP);

        /// <summary>Creates a currency amount containing only gold pieces.</summary>
        public static Currency FromGold(int gp) => new Currency(gp: gp);

        /// <inheritdoc/>
        public bool Equals(Currency other) =>
            CP == other.CP && SP == other.SP && EP == other.EP && GP == other.GP && PP == other.PP;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is Currency other && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(CP, SP, EP, GP, PP);

        /// <summary>Returns a human-readable string listing all non-zero denominations (e.g. "1pp 10gp 5sp 3cp"). Returns "0cp" for a zero amount.</summary>
        public override string ToString()
        {
            if (PP == 0 && GP == 0 && EP == 0 && SP == 0 && CP == 0)
                return "0cp";

            var parts = new System.Text.StringBuilder();
            if (PP > 0) parts.Append($"{PP}pp ");
            if (GP > 0) parts.Append($"{GP}gp ");
            if (EP > 0) parts.Append($"{EP}ep ");
            if (SP > 0) parts.Append($"{SP}sp ");
            if (CP > 0) parts.Append($"{CP}cp ");
            return parts.ToString().TrimEnd();
        }

        /// <summary>Returns true if both currency amounts are identical across all five denominations.</summary>
        public static bool operator ==(Currency a, Currency b) => a.Equals(b);

        /// <summary>Returns true if the currency amounts differ in any denomination.</summary>
        public static bool operator !=(Currency a, Currency b) => !a.Equals(b);

        /// <summary>A currency amount with zero of every denomination.</summary>
        public static readonly Currency Zero = new Currency();
    }
}
