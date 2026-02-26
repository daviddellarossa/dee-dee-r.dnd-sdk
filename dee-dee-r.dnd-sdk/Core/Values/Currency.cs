using System;

namespace DeeDeeR.DnD.Core.Values
{
    /// <summary>
    /// An immutable representation of a currency amount across all five denominations.
    /// </summary>
    public readonly struct Currency : IEquatable<Currency>
    {
        public readonly int CP; // Copper
        public readonly int SP; // Silver   (= 10 CP)
        public readonly int EP; // Electrum (= 50 CP)
        public readonly int GP; // Gold     (= 100 CP)
        public readonly int PP; // Platinum (= 1000 CP)

        public Currency(int cp = 0, int sp = 0, int ep = 0, int gp = 0, int pp = 0)
        {
            CP = cp;
            SP = sp;
            EP = ep;
            GP = gp;
            PP = pp;
        }

        /// <summary>Total value expressed in copper pieces.</summary>
        public int TotalInCopper => CP + SP * 10 + EP * 50 + GP * 100 + PP * 1000;

        public static Currency operator +(Currency a, Currency b) =>
            new Currency(a.CP + b.CP, a.SP + b.SP, a.EP + b.EP, a.GP + b.GP, a.PP + b.PP);

        public static Currency operator -(Currency a, Currency b) =>
            new Currency(a.CP - b.CP, a.SP - b.SP, a.EP - b.EP, a.GP - b.GP, a.PP - b.PP);

        public static Currency FromGold(int gp) => new Currency(gp: gp);

        public bool Equals(Currency other) =>
            CP == other.CP && SP == other.SP && EP == other.EP && GP == other.GP && PP == other.PP;

        public override bool Equals(object obj) => obj is Currency other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(CP, SP, EP, GP, PP);
        public override string ToString() => $"{GP}gp {SP}sp {CP}cp";

        public static bool operator ==(Currency a, Currency b) => a.Equals(b);
        public static bool operator !=(Currency a, Currency b) => !a.Equals(b);

        public static readonly Currency Zero = new Currency();
    }
}
