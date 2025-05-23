﻿using System;
using FrostfallSaga.Core.Fight;
using FrostfallSaga.Fight;
using FrostfallSaga.Fight.Abilities;
using FrostfallSaga.Fight.Effects;
using FrostfallSaga.Fight.FightCells;
using FrostfallSaga.Fight.Fighters;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FrostfallSaga.EditModeTests.FightTests.FighterTests
{
    public class FighterUseActiveAbilityTests
    {
        private Fighter attacker;
        private int fireDamages;
        private FightHexGrid grid;


        [SetUp]
        public void Setup()
        {
            // Set up grid
            grid = CommonTestsHelper.CreatePlainGridFightForTest();

            // Set up attacker
            attacker = FightTestsHelper.CreateFighter();
            attacker.SetStatsForTests();
            Debug.Log(grid.ToString());
            Debug.Log(attacker.ToString());
            FightTestsHelper.SetupFighterPositionOnGrid(grid, attacker, new Vector2Int(0, 0));

            fireDamages = ((MagicalDamageEffect)attacker.ActiveAbilities[0].effects[0]).MagicalDamageAmount;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(grid.gameObject);
            grid = null;

            Object.DestroyImmediate(attacker.gameObject);
            attacker = null;

            fireDamages = 0;
        }

        [Test]
        public void UseActiveAbility_OneReceiver_Test()
        {
            // Arrange

            Fighter receiver = FightTestsHelper.CreateFighter();
            receiver.SetStatsForTests();
            FightTestsHelper.SetupFighterPositionOnGrid(grid, receiver, new Vector2Int(0, 1));
            ActiveAbilitySO activeAbilityToUse = attacker.ActiveAbilities[0];
            MagicalDamageEffect magicalDamageEffect = (MagicalDamageEffect)activeAbilityToUse.effects[0];

            int expectedActionPoints = attacker.GetStatsForTests().actionPoints - activeAbilityToUse.ActionPointsCost;
            int expectedReceiverHealth = receiver.GetStatsForTests().health -
                                         fireDamages +
                                         receiver.GetStatsForTests().magicalResistances[EMagicalElement.FIRE];

            FightCell[] targetedCells = { receiver.cell };

            // Act
            attacker.onActiveAbilityEnded += (attacker, usedAbility) =>
            {
                /// ASSERTS ///
                // Check actions points have been decreased
                Assert.AreEqual(expectedActionPoints, attacker.GetStatsForTests().actionPoints);

                // Check effects have been applied
                Assert.AreEqual(expectedReceiverHealth, receiver.GetStatsForTests().health);
            };
            attacker.UseActiveAbility(activeAbilityToUse, targetedCells);
        }

        [Test]
        public void UseActiveAbility_MultipleReceivers_Test()
        {
            // Arrange
            FightHexGrid grid = CommonTestsHelper.CreatePlainGridFightForTest();
            Debug.Log("TEST");
            Fighter attacker = FightTestsHelper.CreateFighter();
            attacker.SetStatsForTests();
            FightTestsHelper.SetupFighterPositionOnGrid(grid, attacker, new Vector2Int(0, 0));

            Fighter receiver = FightTestsHelper.CreateFighter();
            receiver.SetStatsForTests();
            FightTestsHelper.SetupFighterPositionOnGrid(grid, receiver, new Vector2Int(0, 1));
            receiver.name = "Receiver 1";

            Fighter receiver2 = FightTestsHelper.CreateFighter();
            receiver2.SetStatsForTests();
            FightTestsHelper.SetupFighterPositionOnGrid(grid, receiver2, new Vector2Int(1, 0));
            receiver2.name = "Receiver 2";
            receiver2.GetStatsForTests().magicalResistances[EMagicalElement.FIRE] = 0;

            ActiveAbilitySO activeAbilityToUse = attacker.ActiveAbilities[0];

            int expectedActionPoints = attacker.GetStatsForTests().actionPoints - activeAbilityToUse.ActionPointsCost;
            int expectedReceiverHealth = receiver.GetStatsForTests().health -
                                         fireDamages +
                                         receiver.GetStatsForTests().magicalResistances[EMagicalElement.FIRE];
            int expectedReceiverHealth2 = receiver2.GetStatsForTests().health -
                                          fireDamages +
                                          receiver2.GetStatsForTests().magicalResistances[EMagicalElement.FIRE];

            FightCell[] targetedCells =
            {
                receiver.cell,
                receiver2.cell,
                (FightCell)grid.Cells[new Vector2Int(1, 1)]
            };

            // Act
            attacker.onActiveAbilityEnded += (attacker, usedAbility) =>
            {
                /// ASSERTS ///
                // Check actions points have been decreased
                Assert.AreEqual(expectedActionPoints, attacker.GetStatsForTests().actionPoints);

                // Check effects have been applied
                Assert.AreEqual(expectedReceiverHealth, receiver.GetStatsForTests().health);
                Assert.AreEqual(expectedReceiverHealth2, receiver2.GetStatsForTests().health);
            };
            attacker.UseActiveAbility(activeAbilityToUse, targetedCells);
        }

        [Test]
        public void UseActiveAbility_NoFighterOnCells_Test()
        {
            // Arrange
            FightHexGrid grid = CommonTestsHelper.CreatePlainGridFightForTest();
            Fighter attacker = FightTestsHelper.CreateFighter();
            attacker.SetStatsForTests();
            FightTestsHelper.SetupFighterPositionOnGrid(grid, attacker, new Vector2Int(0, 0));
            Fighter notTargetedFighter = FightTestsHelper.CreateFighter();
            notTargetedFighter.SetStatsForTests();
            FightTestsHelper.SetupFighterPositionOnGrid(grid, notTargetedFighter, new Vector2Int(0, 1));

            ActiveAbilitySO activeAbilityToUse = attacker.ActiveAbilities[0];

            int expectedActionPoints = attacker.GetStatsForTests().actionPoints - activeAbilityToUse.ActionPointsCost;
            int expectedNotTargetedFighterHealth = notTargetedFighter.GetStatsForTests().health;

            FightCell[] targetedCells = { (FightCell)grid.Cells[new Vector2Int(1, 1)] };

            // Act
            attacker.onActiveAbilityEnded += (attacker, usedAbility) =>
            {
                /// ASSERTS ///
                // Check actions points have been decreased
                Assert.AreEqual(expectedActionPoints, attacker.GetStatsForTests().actionPoints);

                // Check no effect have been applied to other fighter
                Assert.AreEqual(expectedNotTargetedFighterHealth, notTargetedFighter.GetStatsForTests().health);
            };
            attacker.UseActiveAbility(activeAbilityToUse, targetedCells);
        }

        [Test]
        public void UseDirectAttack_NoTargetCells_Test()
        {
            // Arrange
            FightHexGrid grid = CommonTestsHelper.CreatePlainGridFightForTest();
            Fighter attacker = FightTestsHelper.CreateFighter();
            attacker.SetStatsForTests();
            FightTestsHelper.SetupFighterPositionOnGrid(grid, attacker, new Vector2Int(0, 0));
            ActiveAbilitySO activeAbilityToUse = attacker.ActiveAbilities[0];
            FightCell[] targetedCells = { };

            // Act
            Assert.Throws<ArgumentException>(() => attacker.UseActiveAbility(activeAbilityToUse, targetedCells));
        }

        [Test]
        public void UseDirectAttack_NotEnoughActionPoints_Test()
        {
            // Arrange
            FightHexGrid grid = CommonTestsHelper.CreatePlainGridFightForTest();
            Fighter attacker = FightTestsHelper.CreateFighter();
            attacker.SetStatsForTests();
            attacker.GetStatsForTests().actionPoints = 1;
            FightTestsHelper.SetupFighterPositionOnGrid(grid, attacker, new Vector2Int(0, 0));
            ActiveAbilitySO activeAbilityToUse = attacker.ActiveAbilities[0];
            FightCell[] targetedCells = { (FightCell)grid.Cells[new Vector2Int(0, 1)] };

            // Act
            Assert.Throws<InvalidOperationException>(() =>
                attacker.UseActiveAbility(activeAbilityToUse, targetedCells));
        }
    }
}