using System;
using DeeDeeR.DnD.Core.Enums;

namespace DeeDeeR.DnD.Core.Systems
{
    /// <summary>
    /// Stateless helper for Experience Point (XP) thresholds and level calculation,
    /// per D&amp;D 2024 PHB Chapter 2.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Supports both XP-based and milestone leveling modes via <see cref="LevelingMode"/>.
    /// </para>
    /// <para>
    /// Under <see cref="LevelingMode.Milestone"/> leveling, XP is not tracked and
    /// <see cref="IsReadyToLevelUp"/> always returns <see langword="false"/> — level
    /// advancement is a narrative decision made by the Dungeon Master.
    /// </para>
    /// </remarks>
    public sealed class XPSystem
    {
        // D&D 2024 PHB XP thresholds — index = level (1–20).
        // Index 0 is unused; Level 1 always requires 0 XP.
        private static readonly int[] XpThresholds =
        {
            /*  0 */ 0,
            /*  1 */ 0,
            /*  2 */ 300,
            /*  3 */ 900,
            /*  4 */ 2_700,
            /*  5 */ 6_500,
            /*  6 */ 14_000,
            /*  7 */ 23_000,
            /*  8 */ 34_000,
            /*  9 */ 48_000,
            /* 10 */ 64_000,
            /* 11 */ 85_000,
            /* 12 */ 100_000,
            /* 13 */ 120_000,
            /* 14 */ 140_000,
            /* 15 */ 165_000,
            /* 16 */ 195_000,
            /* 17 */ 225_000,
            /* 18 */ 265_000,
            /* 19 */ 305_000,
            /* 20 */ 355_000,
        };

        /// <summary>The minimum character level (inclusive).</summary>
        public const int MinLevel = 1;

        /// <summary>The maximum character level (inclusive).</summary>
        public const int MaxLevel = 20;

        // ── Threshold lookup ──────────────────────────────────────────────────

        /// <summary>
        /// Returns the total XP required to reach <paramref name="level"/>.
        /// Level 1 requires 0 XP.
        /// </summary>
        /// <param name="level">The target level (1–20).</param>
        /// <returns>The total accumulated XP needed to reach that level.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="level"/> is outside the range [1, 20].
        /// </exception>
        public int GetXPThreshold(int level)
        {
            if (level < MinLevel || level > MaxLevel)
                throw new ArgumentOutOfRangeException(
                    nameof(level), level, $"Level must be between {MinLevel} and {MaxLevel}.");
            return XpThresholds[level];
        }

        // ── Level from XP ─────────────────────────────────────────────────────

        /// <summary>
        /// Returns the level (1–20) that corresponds to <paramref name="totalXP"/> accumulated
        /// experience points.
        /// </summary>
        /// <remarks>
        /// Negative XP is treated as 0 and returns level 1.
        /// XP beyond the level-20 threshold returns level 20.
        /// </remarks>
        /// <param name="totalXP">Total accumulated XP (clamped ≥ 0 internally).</param>
        /// <returns>The character level for the given XP total (1–20).</returns>
        public int GetLevelFromXP(int totalXP)
        {
            if (totalXP < 0) totalXP = 0;

            int level = MinLevel;
            for (int l = MaxLevel; l >= MinLevel; l--)
            {
                if (totalXP >= XpThresholds[l])
                {
                    level = l;
                    break;
                }
            }
            return level;
        }

        // ── Level-up readiness ────────────────────────────────────────────────

        /// <summary>
        /// Returns whether a character is ready to advance from <paramref name="currentLevel"/>
        /// to the next level, given <paramref name="totalXP"/> and the active
        /// <paramref name="levelingMode"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Under <see cref="LevelingMode.ExperiencePoints"/>: returns <see langword="true"/>
        /// when <paramref name="totalXP"/> meets the next level's threshold.
        /// Always returns <see langword="false"/> when <paramref name="currentLevel"/> is
        /// already at the maximum (20).
        /// </para>
        /// <para>
        /// Under <see cref="LevelingMode.Milestone"/>: always returns
        /// <see langword="false"/> — level-up is a DM narrative decision, not a calculation.
        /// </para>
        /// </remarks>
        /// <param name="currentLevel">The character's current level (1–20).</param>
        /// <param name="totalXP">Total accumulated XP.</param>
        /// <param name="levelingMode">Whether XP-based or milestone leveling is active.</param>
        /// <returns>
        /// <see langword="true"/> if the character should level up; otherwise
        /// <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="currentLevel"/> is outside the range [1, 20].
        /// </exception>
        public bool IsReadyToLevelUp(int currentLevel, int totalXP, LevelingMode levelingMode)
        {
            if (currentLevel < MinLevel || currentLevel > MaxLevel)
                throw new ArgumentOutOfRangeException(
                    nameof(currentLevel), currentLevel,
                    $"Level must be between {MinLevel} and {MaxLevel}.");

            if (levelingMode == LevelingMode.Milestone) return false;
            if (currentLevel == MaxLevel)               return false;

            return totalXP >= XpThresholds[currentLevel + 1];
        }

        // ── XP to next level ──────────────────────────────────────────────────

        /// <summary>
        /// Returns the XP still needed to reach the next level from the character's
        /// current accumulated <paramref name="totalXP"/>.
        /// Returns 0 when the character is already at level 20.
        /// </summary>
        /// <param name="totalXP">Total accumulated XP (clamped ≥ 0 internally).</param>
        /// <returns>
        /// XP required to reach the next level, or 0 if already at maximum level.
        /// </returns>
        public int GetXPToNextLevel(int totalXP)
        {
            if (totalXP < 0) totalXP = 0;
            int currentLevel = GetLevelFromXP(totalXP);
            if (currentLevel == MaxLevel) return 0;
            return XpThresholds[currentLevel + 1] - totalXP;
        }
    }
}