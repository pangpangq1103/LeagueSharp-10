﻿/**
 * 
 * Love Ya Lads!
 * 
 * 
 **/

using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SebbyLib;
using Color = SharpDX.Color;
using HitChance = SebbyLib.Prediction.HitChance;
using Orbwalking = SebbyLib.Orbwalking;
using PredictionInput = SebbyLib.Prediction.PredictionInput;

namespace SurvivorRyze
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != ChampionName)
                return;

            #region Spells

            Q = new Spell(SpellSlot.Q, 1000f);
            Q.SetSkillshot(0.7f, 55f, float.MaxValue, true, SkillshotType.SkillshotLine);
            W = new Spell(SpellSlot.W, 610f);
            W.SetTargetted(0.103f, 550f);
            E = new Spell(SpellSlot.E, 610f);
            E.SetTargetted(.5f, 550f);
            R = new Spell(SpellSlot.R);
            R.SetSkillshot(2.5f, 450f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            #endregion

            IgniteSlot = Player.GetSpellSlot("summonerdot");
            TearOfGod = new Items.Item(3070, 0);
            Manamune = new Items.Item(3004, 0);
            Archangel = new Items.Item(3003, 0);

            #region Menu

            Menu = new Menu("SurvivorRyze", "SurvivorRyze", true).SetFontStyle(FontStyle.Bold, Color.AliceBlue);

            var OrbwalkerMenu = Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(OrbwalkerMenu);

            var TargetSelectorMenu = Menu.AddSubMenu(new Menu("Target Selector", "TargetSelector"));
            TargetSelector.AddToMenu(TargetSelectorMenu);

            var ComboMenu = Menu.AddSubMenu(new Menu("Combo", "Combo"));
            ComboMenu.AddItem(
                new MenuItem("ComboMode", "Combo Mode:").SetValue(
                        new StringList(new[] {"Burst", "Survivor Mode (Shield)"}))
                    .SetTooltip("Survivor Mode - Will try to stack Shield 99% of the time."));
            ComboMenu.AddItem(new MenuItem("CUseQ", "Cast Q").SetValue(true));
            ComboMenu.AddItem(new MenuItem("CUseW", "Cast W").SetValue(true));
            ComboMenu.AddItem(new MenuItem("CUseE", "Cast E").SetValue(true));
            ComboMenu.AddItem(new MenuItem("CBlockAA", "Block AA in Combo Mode").SetValue(true));
            ComboMenu.AddItem(
                new MenuItem("Combo2TimesMana", "Champion needs to have mana for atleast 2 times (Q/W/E)?").SetValue(
                        false)
                    .SetTooltip(
                        "If it's set to 'false' it'll need atleast mana for Q/W/E [1x] Post in thread if needs a change"));
            ComboMenu.AddItem(new MenuItem("CUseR", "Ultimate (R) in Ultimate Menu"));
            ComboMenu.AddItem(new MenuItem("CUseIgnite", "Use Ignite (Smart)").SetValue(true));

            var HarassMenu = Menu.AddSubMenu(new Menu("Harass", "Harass"));
            HarassMenu.AddItem(new MenuItem("HarassQ", "Use Q").SetValue(true));
            HarassMenu.AddItem(new MenuItem("HarassW", "Use W").SetValue(false));
            HarassMenu.AddItem(new MenuItem("HarassE", "Use E").SetValue(false));
            HarassMenu.AddItem(new MenuItem("HarassManaManager", "Mana Manager (%)").SetValue(new Slider(30, 1, 100)));

            var LaneClearMenu = Menu.AddSubMenu(new Menu("Lane Clear", "LaneClear"));
            LaneClearMenu.AddItem(
                new MenuItem("EnableMouseScroll", "Enable Mouse Scroll to change Spell Farming?").SetValue(true));
            LaneClearMenu.AddItem(
                    new MenuItem("EnableFarming", "Enable Farming with Spells?").SetValue(true)
                        .SetTooltip("You either change the value here by clicking or by Scrolling Down using the mouse"))
                .Permashow(true, "Farming with Spells?");
            LaneClearMenu.AddItem(new MenuItem("UseQLC", "Use Q to LaneClear").SetValue(true));
            LaneClearMenu.AddItem(new MenuItem("UseELC", "Use E to LaneClear").SetValue(true));
            LaneClearMenu.AddItem(
                new MenuItem("LaneClearManaManager", "Mana Manager (%)").SetValue(new Slider(30, 1, 100)));

            var JungleClearMenu = Menu.AddSubMenu(new Menu("Jungle Clear", "JungleClear"));
            JungleClearMenu.AddItem(new MenuItem("UseQJC", "Use Q to JungleClear").SetValue(true));
            JungleClearMenu.AddItem(new MenuItem("UseWJC", "Use W to JungleClear").SetValue(true));
            JungleClearMenu.AddItem(new MenuItem("UseEJC", "Use E to JungleClear").SetValue(true));
            JungleClearMenu.AddItem(
                new MenuItem("JungleClearManaManager", "Mana Manager (%)").SetValue(new Slider(30, 1, 100)));

            var LastHitMenu = Menu.AddSubMenu(new Menu("Last Hit", "LastHit"));
            LastHitMenu.AddItem(new MenuItem("UseQLH", "Use Q to LastHit").SetValue(true));
            LastHitMenu.AddItem(new MenuItem("UseELH", "Use E to LastHit").SetValue(true));
            LastHitMenu.AddItem(new MenuItem("LaneHitManaManager", "Mana Manager (%)").SetValue(new Slider(30, 1, 100)));

            var ItemsMenu = Menu.AddSubMenu(new Menu("Items Menu", "ItemsMenu"));
            ItemsMenu.AddItem(new MenuItem("UsePotions", "Use Potions").SetValue(true));
            ItemsMenu.AddItem(
                new MenuItem("UseSmartPotion", "Use Smart Potion Logic").SetValue(true)
                    .SetTooltip("If Enabled, it'll check if enemy's around so it doesn't waste potions."));
            ItemsMenu.AddItem(
                new MenuItem("UsePotionsAtHPPercent", "Use Potions at HP Percent 'X'").SetValue(new Slider(30, 0, 100)));
            ItemsMenu.AddItem(new MenuItem("UseSeraph", "Use [Seraph's Embrace]?").SetValue(true));
            ItemsMenu.AddItem(new MenuItem("Hextech", "Use [Hextech Gunblade]?").SetValue(true));
            ItemsMenu.AddItem(new MenuItem("Protobelt", "Use [Hextech Protobelt-01]?").SetValue(true));
            ItemsMenu.AddItem(new MenuItem("GLP800", "Use [Hextech GLP-800]?").SetValue(true));
            ItemsMenu.AddItem(
                new MenuItem("StackTear", "Stack Tear/Manamune/Archangel in Fountain?").SetValue(true)
                    .SetTooltip("Stack it in Fountain?"));
            ItemsMenu.AddItem(
                new MenuItem("StackTearNF", "Stack Tear/Manamune/Archangel if You've Blue Buff?").SetValue(false));

            Menu.Item("UseSmartPotion").ValueChanged += (sender, eventArgs) =>
            {
                if (!Menu.Item("UsePotions").GetValue<bool>() && eventArgs.GetNewValue<bool>())
                    Menu.Item("UsePotions").SetValue(true);
            };

            var SkinMenu = Menu.AddSubMenu(new Menu("Skins Menu", "SkinMenu"));
            SkinMenu.AddItem(new MenuItem("SkinID", "Skin ID")).SetValue(new Slider(10, 0, 10));
            var UseSkin = SkinMenu.AddItem(new MenuItem("UseSkin", "Enabled")).SetValue(true);
            UseSkin.ValueChanged += (sender, eventArgs) =>
            {
                if (!eventArgs.GetNewValue<bool>())
                    ObjectManager.Player.SetSkin(ObjectManager.Player.CharData.BaseSkinName,
                        ObjectManager.Player.BaseSkinId);
            };

            var HitChanceMenu = Menu.AddSubMenu(new Menu("HitChance Menu", "HitChance"));
            HitChanceMenu.AddItem(
                new MenuItem("HitChance", "Hit Chance").SetValue(new StringList(new[] {"Medium", "High", "Very High"}, 1)));

            var UltimateMenu = Menu.AddSubMenu(new Menu("Ultimate Menu", "UltMenu"));
            //UltimateMenu.AddItem(new MenuItem("DontREnemyCount", "Don't R If Enemy In 'X' Range").SetValue(new Slider(1000, 0, 2000)));
            //UltimateMenu.AddItem(new MenuItem("DontRIfAlly", "Don't R if Ally is Near Target 'X' Range").SetValue(new Slider(700, 0, 2000)));
            //UltimateMenu.AddItem(new MenuItem("DontRUnderTurret", "Don't use R if enemy is Under Turret").SetValue(true));
            UltimateMenu.AddItem(
                new MenuItem("UseR", "Use R Automatically (Beta)").SetValue(new KeyBind("G".ToCharArray()[0],
                        KeyBindType.Press))
                    .SetTooltip("It'll Use the Ultimate if there's Ally turret nearby to teleport you to it"));
            //UltimateMenu.AddItem(new MenuItem("EnemiesAroundTarget", "Dont R If 'X' Enemies are around the Target").SetValue(new Slider(3, 0, 5)));

            var MiscMenu = Menu.AddSubMenu(new Menu("Misc Menu", "MiscMenu"));
            MiscMenu.AddItem(new MenuItem("KSQ", "Use Q to KS").SetValue(true));
            MiscMenu.AddItem(new MenuItem("KSW", "Use W to KS").SetValue(true));
            MiscMenu.AddItem(new MenuItem("KSE", "Use E to KS").SetValue(true));
            MiscMenu.AddItem(new MenuItem("InterruptWithW", "Use W to Interrupt Channeling Spells").SetValue(true));
            MiscMenu.AddItem(new MenuItem("WGapCloser", "Use W on Enemy GapCloser (Ex. Irelia's Q)").SetValue(true));
            MiscMenu.AddItem(new MenuItem("ChaseWithR", "Use R to Chase (Being Added)"));
            MiscMenu.AddItem(new MenuItem("EscapeWithR", "Use R to Escape (Ultimate Menu)"));
            MiscMenu.AddItem(new MenuItem("Reminders", "Enable [SS AIO Type] Reminders?").SetValue(true));

            var AutoLevelerMenu = Menu.AddSubMenu(new Menu("AutoLeveler Menu", "AutoLevelerMenu"));
            AutoLevelerMenu.AddItem(new MenuItem("AutoLevelUp", "AutoLevel Up Spells?").SetValue(true));
            AutoLevelerMenu.AddItem(
                new MenuItem("AutoLevelUp1", "First: ").SetValue(new StringList(new[] {"Q", "W", "E", "R"}, 3)));
            AutoLevelerMenu.AddItem(
                new MenuItem("AutoLevelUp2", "Second: ").SetValue(new StringList(new[] {"Q", "W", "E", "R"}, 0)));
            AutoLevelerMenu.AddItem(
                new MenuItem("AutoLevelUp3", "Third: ").SetValue(new StringList(new[] {"Q", "W", "E", "R"}, 2)));
            AutoLevelerMenu.AddItem(
                new MenuItem("AutoLevelUp4", "Fourth: ").SetValue(new StringList(new[] {"Q", "W", "E", "R"}, 1)));
            AutoLevelerMenu.AddItem(
                new MenuItem("AutoLvlStartFrom", "AutoLeveler Start from Level: ").SetValue(new Slider(2, 6, 1)));

            var DrawingMenu = Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
            DrawingMenu.AddItem(new MenuItem("DrawQ", "Draw Q Range").SetValue(true));
            DrawingMenu.AddItem(new MenuItem("DrawWE", "Draw W/E Range").SetValue(true));
            DrawingMenu.AddItem(new MenuItem("DrawR", "Draw R Range").SetValue(false));
            DrawingMenu.AddItem(new MenuItem("DrawRMinimap", "Draw R Range | On Minimap").SetValue(true));
            DrawingMenu.AddItem(new MenuItem("DrawSpellFarm", "Draw Spell Farm State? [On/Off]").SetValue(true));

            Menu.AddToMainMenu();

            #endregion

            #region DrawHPDamage

            var dmgAfterShave = new MenuItem("SurvivorRyze.DrawComboDamage", "Draw Combo Damage").SetValue(true);
            var drawFill =
                new MenuItem("SurvivorRyze.DrawColour", "Fill Color", true).SetValue(
                    new Circle(true, System.Drawing.Color.SeaGreen));
            DrawingMenu.AddItem(drawFill);
            DrawingMenu.AddItem(dmgAfterShave);
            DrawDamage.DamageToUnit = CalculateDamage;
            DrawDamage.Enabled = dmgAfterShave.GetValue<bool>();
            DrawDamage.Fill = drawFill.GetValue<Circle>().Active;
            DrawDamage.FillColor = drawFill.GetValue<Circle>().Color;
            dmgAfterShave.ValueChanged +=
                delegate(object sender, OnValueChangeEventArgs eventArgs)
                {
                    DrawDamage.Enabled = eventArgs.GetNewValue<bool>();
                };

            drawFill.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                DrawDamage.Fill = eventArgs.GetNewValue<Circle>().Active;
                DrawDamage.FillColor = eventArgs.GetNewValue<Circle>().Color;
            };

            #endregion

            #region Subscriptions

            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += OnEndScene;
            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnLevelUp += Obj_AI_Base_OnLevelUp;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Game.OnWndProc += OnWndProc;

            #endregion

            Game.PrintChat("<font color='#800040'>[SurvivorSeries] Ryze</font> <font color='#ff6600'>Loaded.</font>");
        }

        private static void OnWndProc(WndEventArgs args)
        {
            if (!Menu.Item("EnableMouseScroll").GetValue<bool>())
                return;

            if (args.Msg == 0x20a)
            {
                Menu.Item("EnableFarming").SetValue(!Menu.Item("EnableFarming").GetValue<bool>());
            }
        }

        private static void OnEndScene(EventArgs args)
        {
            switch (R.Level)
            {
                case 1:
                    RangeR = 1750f;
                    break;
                case 2:
                    RangeR = 3000f;
                    break;
            }

            if (Menu.Item("DrawRMinimap").GetValue<bool>() && (R.Level > 0) && R.IsReady())
            {
#pragma warning disable CS0618 // Type or member is obsolete
                Utility.DrawCircle(Player.Position, RangeR, System.Drawing.Color.DeepPink, 2, 45, true);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            switch (R.Level)
            {
                case 1:
                    RangeR = 1750f;
                    break;
                case 2:
                    RangeR = 3000f;
                    break;
            }
            if (Menu.Item("DrawQ").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, Q.Range, System.Drawing.Color.Aqua);
            if (Menu.Item("DrawWE").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, W.Range, System.Drawing.Color.AliceBlue);
            if (Menu.Item("DrawR").GetValue<bool>())
                Render.Circle.DrawCircle(Player.Position, RangeR, System.Drawing.Color.Orchid);

            if (!Menu.Item("DrawSpellFarm").GetValue<bool>())
                return;

            if (Menu.Item("EnableFarming").GetValue<bool>())
            {
                var drawPos = Drawing.WorldToScreen(Player.Position);
                var textSize = Drawing.GetTextExtent("Spell Farm: ON");
                Drawing.DrawText(drawPos.X - textSize.Width - 70f, drawPos.Y, System.Drawing.Color.Chartreuse, "Spell Farm: ON");
            }
            else
            {
                var drawPos = Drawing.WorldToScreen(Player.Position);
                var textSize = Drawing.GetTextExtent("Spell Farm: OFF");
                Drawing.DrawText(drawPos.X - textSize.Width - 70f, drawPos.Y, System.Drawing.Color.DeepPink, "Spell Farm: OFF");
            }
        }

        private static void AABlock()
        {
            Orbwalker.SetAttack(!Menu.Item("CBlockAA").GetValue<bool>());
            //SebbyLib.OktwCommon.blockAttack = Menu.Item("CBlockAA").GetValue<bool>();
        }

        private static void GotStronger()
        {
            // Reminder
            Game.PrintChat(
                "<font color='#0993F9'>[SS AIO | Reminder]</font> <font color='#FF8800'>You got strong enough, Lower the LaneClear Mana Manager Sliders!</font>");
        }

        private static void Obj_AI_Base_OnLevelUp(Obj_AI_Base sender, EventArgs args)
        {
            if (sender.IsMe && Menu.Item("Reminders").GetValue<bool>() && (ObjectManager.Player.Level >= 6))
                GotStronger();

            if (!sender.IsMe || !Menu.Item("AutoLevelUp").GetValue<bool>() ||
                (ObjectManager.Player.Level < Menu.Item("AutoLvlStartFrom").GetValue<Slider>().Value))
                return;
            if ((lvl2 == lvl3) || (lvl2 == lvl4) || (lvl3 == lvl4))
                return;
            var delay = 700;
            Utility.DelayAction.Add(delay, () => LevelUp(lvl1));
            Utility.DelayAction.Add(delay + 50, () => LevelUp(lvl2));
            Utility.DelayAction.Add(delay + 100, () => LevelUp(lvl3));
            Utility.DelayAction.Add(delay + 150, () => LevelUp(lvl4));
        }

        private static void LevelUp(int indx)
        {
            if (ObjectManager.Player.Level < 4)
            {
                if ((indx == 0) && (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level == 0))
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                if ((indx == 1) && (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Level == 0))
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                if ((indx == 2) && (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Level == 0))
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
            }
            else
            {
                if (indx == 0)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.Q);
                if (indx == 1)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.W);
                if (indx == 2)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.E);
                if (indx == 3)
                    ObjectManager.Player.Spellbook.LevelSpell(SpellSlot.R);
            }
        }

        private static void PotionsCheck()
        {
            if (Menu.Item("UseSmartPotion").GetValue<bool>())
                if (Player.CountEnemiesInRange(800) == 0)
                    return;

            if (Player.HasBuff("RegenerationPotion") || Player.HasBuff("ItemMiniRegenPotion") ||
                Player.HasBuff("ItemCrystalFlaskJungle") || Player.HasBuff("ItemDarkCrystalFlask") ||
                Player.HasBuff("ItemCrystalFlask"))
                return;

            if (Player.HealthPercent <= Menu.Item("UsePotionsAtHPPercent").GetValue<Slider>().Value)
                if (HPPotion.IsReady())
                    HPPotion.Cast();
                else if (Biscuit.IsReady())
                    Biscuit.Cast();
                else if (FlaskHunterJG.IsReady())
                    FlaskHunterJG.Cast();
                else if (FlaskCorruptJG.IsReady())
                    FlaskCorruptJG.Cast();
                else if (FlaskRef.IsReady())
                    FlaskRef.Cast();
        }

        private static void SeraphUsage()
        {
            var incomingdmg = OktwCommon.GetIncomingDamage(Player, 1);
            if (Seraph.IsReady() && Menu.Item("UseSeraph").GetValue<bool>())
            {
                var shieldint = Player.Mana*0.2 + 150;
                if ((incomingdmg > Player.Health) && (incomingdmg < Player.Health + shieldint))
                    Seraph.Cast();
            }
        }

        private static void ItemsChecks()
        {
            if (GLP800.IsReady())
            {
                var t = TargetSelector.GetTarget(GLP800.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget())
                    if (Menu.Item("GLP800").GetValue<bool>() &&
                        (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo))
                        GLP800.Cast(Prediction.GetPrediction(t, 0.5f).CastPosition);
            }

            if (Protobelt.IsReady())
            {
                var t = TargetSelector.GetTarget(Protobelt.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget())
                    if (Menu.Item("Protobelt").GetValue<bool>() &&
                        (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo))
                        Protobelt.Cast(Prediction.GetPrediction(t, 0.5f).CastPosition);
            }

            if (Hextech.IsReady())
            {
                var t = TargetSelector.GetTarget(Hextech.Range, TargetSelector.DamageType.Magical);
                if (t.IsValidTarget())
                    if (Menu.Item("Hextech").GetValue<bool>() &&
                        (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo))
                        Hextech.Cast(t);
            }
        }

        private static void StackItems()
        {
            if (Player.InFountain() ||
                (Player.HasBuff("CrestoftheAncientGolem") && (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.None) &&
                 Menu.Item("StackTearNF").GetValue<bool>())) // Add if Player has Blue Buff
                if (Items.HasItem(3004, Player) || Items.HasItem(3003, Player) || Items.HasItem(3070, Player) ||
                    Items.HasItem(3072, Player) || Items.HasItem(3073, Player) || Items.HasItem(3008, Player))
                    Q.Cast(Player.ServerPosition);
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsRecalling())
                return;

            if (Menu.Item("StackTear").GetValue<bool>())
                StackItems();
            SeraphUsage();
            ItemsChecks();
            KSCheck();
            PotionsCheck();
            if (Menu.Item("UseSkin").GetValue<bool>())
                Player.SetSkin(Player.CharData.BaseSkinName, Menu.Item("SkinID").GetValue<Slider>().Value);
            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    AABlock();
                    Combo();
                    ComboPlusCheck(); // Beta - Will be improved
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    JungleClear();
                    LaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    LastHit();
                    break;
                case Orbwalking.OrbwalkingMode.None:
                    Orbwalker.SetMovement(true);
                    Orbwalker.SetAttack(true);
                    break;
            }
            if (Menu.Item("UseR").GetValue<KeyBind>().Active)
            {
                Orbwalker.SetMovement(false);
                Orbwalker.SetAttack(false);
                REscape();
            }
            //AutoLeveler
            if (Menu.Item("AutoLevelUp").GetValue<bool>())
            {
                lvl1 = Menu.Item("AutoLevelUp1").GetValue<StringList>().SelectedIndex;
                lvl2 = Menu.Item("AutoLevelUp2").GetValue<StringList>().SelectedIndex;
                lvl3 = Menu.Item("AutoLevelUp3").GetValue<StringList>().SelectedIndex;
                lvl4 = Menu.Item("AutoLevelUp4").GetValue<StringList>().SelectedIndex;
            }
        }

        private static void JungleClear()
        {
            if (Player.ManaPercent < Menu.Item("JungleClearManaManager").GetValue<Slider>().Value)
                return;

            var jgcq = Menu.Item("UseQJC").GetValue<bool>();
            var jgcw = Menu.Item("UseWJC").GetValue<bool>();
            var jgce = Menu.Item("UseEJC").GetValue<bool>();

            var mob =
                MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Neutral,
                    MinionOrderTypes.MaxHealth).FirstOrDefault();
            if (mob == null)
                return;
            if (jgcq && jgce && Q.IsReady() && E.IsReady())
            {
                Q.Cast(mob.Position);
                E.CastOnUnit(mob);
                Q.Cast(mob.Position);
                if (jgcw && W.IsReady() && !Q.IsReady())
                {
                    W.CastOnUnit(mob);
                    Q.Cast(mob.Position);
                }
            }
            else if (jgcq && jgce && !Q.IsReady() && E.IsReady())
            {
                E.CastOnUnit(mob);
                Q.Cast(mob.Position);
                if (jgcw && W.IsReady() && !Q.IsReady())
                {
                    W.CastOnUnit(mob);
                    Q.Cast(mob.Position);
                }
            }
            else if (jgcq && jgce && jgcw && !Q.IsReady() && !E.IsReady() && W.IsReady())
            {
                W.CastOnUnit(mob);
                Q.Cast(mob.Position);
                if (E.IsReady())
                {
                    E.CastOnUnit(mob);
                    Q.Cast(mob.Position);
                }
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Menu.Item("WGapCloser").GetValue<bool>() || (Player.Mana < W.Instance.ManaCost + Q.Instance.ManaCost))
                return;

            var t = gapcloser.Sender;

            if (gapcloser.End.Distance(Player.ServerPosition) < W.Range)
                W.Cast(t);
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero t,
            Interrupter2.InterruptableTargetEventArgs args)
        {
            var WCast = Menu.Item("InterruptWithW").GetValue<bool>();
            if (!WCast || !t.IsValidTarget(W.Range) || !W.IsReady()) return;
            W.Cast(t);
        }

        private static void KSCheck()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            // If Target's not in Q Range or there's no target or target's invulnerable don't fuck with him
            if ((target == null) || !target.IsValidTarget(Q.Range) || target.IsInvulnerable)
                return;

            var ksQ = Menu.Item("KSQ").GetValue<bool>();
            var ksW = Menu.Item("KSW").GetValue<bool>();
            var ksE = Menu.Item("KSE").GetValue<bool>();

            // KS
            if (ksQ && (OktwCommon.GetKsDamage(target, Q) > target.Health) && target.IsValidTarget(Q.Range))
                SebbySpell(Q, target);
            if (ksW && (OktwCommon.GetKsDamage(target, W) > target.Health) && target.IsValidTarget(W.Range))
                W.CastOnUnit(target);
            if (ksE && (OktwCommon.GetKsDamage(target, E) > target.Health) && target.IsValidTarget(E.Range))
                E.CastOnUnit(target);
        }

        public static bool RyzeCharge0()
        {
            return Player.HasBuff("ryzeqiconnocharge");
        }

        public static bool RyzeCharge1()
        {
            return Player.HasBuff("ryzeqiconhalfcharge");
        }

        public static bool RyzeCharge2()
        {
            return Player.HasBuff("ryzeqiconfullcharge");
        }

        private static void SebbySpell(Spell QR, Obj_AI_Base target)
        {
            var CoreType2 = SebbyLib.Prediction.SkillshotType.SkillshotLine;
            var aoe2 = false;

            if (QR.Type == SkillshotType.SkillshotCircle)
            {
                CoreType2 = SebbyLib.Prediction.SkillshotType.SkillshotCircle;
                aoe2 = true;
            }

            if ((QR.Width > 80) && !QR.Collision)
                aoe2 = true;

            var predInput2 = new PredictionInput
            {
                Aoe = aoe2,
                Collision = QR.Collision,
                Speed = QR.Speed,
                Delay = QR.Delay,
                Range = QR.Range,
                From = Player.ServerPosition,
                Radius = QR.Width,
                Unit = target,
                Type = CoreType2
            };
            var poutput2 = SebbyLib.Prediction.Prediction.GetPrediction(predInput2);

            if ((QR.Speed != float.MaxValue) && OktwCommon.CollisionYasuo(Player.ServerPosition, poutput2.CastPosition))
                return;

            if (Menu.Item("HitChance").GetValue<StringList>().SelectedIndex == 0)
            {
                if (poutput2.Hitchance >= HitChance.Medium)
                    QR.Cast(poutput2.CastPosition);
            }
            else if (Menu.Item("HitChance").GetValue<StringList>().SelectedIndex == 1)
            {
                if (poutput2.Hitchance >= HitChance.High)
                    QR.Cast(poutput2.CastPosition);
            }
            else if (Menu.Item("HitChance").GetValue<StringList>().SelectedIndex == 2)
            {
                if (poutput2.Hitchance >= HitChance.VeryHigh)
                    QR.Cast(poutput2.CastPosition);
            }
        }

        private static float QGetRealDamage(Obj_AI_Base target)
        {
            if (!target.HasBuff("RyzeE"))
                return Q.GetDamage(target);
            if (((E.IsReady() && !Q.IsReady()) || (E.IsReady() && Q.IsReady()) || (!E.IsReady() && Q.IsReady())) &&
                target.HasBuff("RyzeE"))
            {
                switch (E.Level)
                {
                    case 1:
                        QRealDamage = Q.GetDamage(target)/40*100;
                        break;
                    case 2:
                        QRealDamage = Q.GetDamage(target)/55*100;
                        break;
                    case 3:
                        QRealDamage = Q.GetDamage(target)/70*100;
                        break;
                    case 4:
                        QRealDamage = Q.GetDamage(target)/85*100;
                        break;
                    case 5:
                        QRealDamage = Q.GetDamage(target)/100*100;
                        break;
                }
                //Game.PrintChat("Inside V2 qRealDamage:" + QRealDamage);
                return QRealDamage;
            }
            //Game.PrintChat("Inside else at end:" + Q.GetDamage(target));
            return Q.GetDamage(target);
        }

        private static void ComboPlusCheck()
        {
            // Combo
            var CUseQ = Menu.Item("CUseQ").GetValue<bool>();
            //var CUseW = Menu.Item("CUseW").GetValue<bool>();
            var CUseE = Menu.Item("CUseE").GetValue<bool>();
            // Checks
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            // If Target's not in Q Range or there's no target or target's invulnerable don't fuck with him
            if ((target == null) || !target.IsValidTarget(Q.Range) || target.IsInvulnerable)
                return;

            var ryzeebuffed =
                MinionManager.GetMinions(Player.Position, Q.Range)
                    .Find(x => x.HasBuff("RyzeE") && x.IsValidTarget(Q.Range));
            var noebuffed =
                MinionManager.GetMinions(Player.Position, Q.Range)
                    .Find(x => x.IsValidTarget(Q.Range) && (x.Distance(target) < 200));

            if (CUseQ && CUseE && target.IsValidTarget(Q.Range))
                if ((ryzeebuffed != null) && ryzeebuffed.IsValidTarget(Q.Range))
                {
                    if (ryzeebuffed.Health < QGetRealDamage(ryzeebuffed))
                    {
                        //Game.PrintChat("<font color='#9400D3'>DEBUG: Spread</font>");
                        if (!Q.IsReady() && E.IsReady())
                        {
                            E.CastOnUnit(ryzeebuffed);
                            Q.Cast(ryzeebuffed);
                            //Game.PrintChat("<font color='#9400D3'>DEBUG: Spreading [Reset with E]</font>");
                        }
                        Q.Cast(ryzeebuffed);
                    }
                    if (target.HasBuff("RyzeE") && (target.Distance(ryzeebuffed) < 200) &&
                        ryzeebuffed.IsValidTarget(Q.Range))
                    {
                        //Game.PrintChat("<font color='#9400D3'>DEBUG: Got to Part 1</font>");
                        Q.Cast(ryzeebuffed);
                    }
                    else if (!target.HasBuff("RyzeE"))
                    {
                        E.CastOnUnit(target);
                        if (target.Distance(ryzeebuffed) < 200)
                            Q.Cast(ryzeebuffed);
                    }
                }
                else if ((ryzeebuffed == null) || !ryzeebuffed.IsValidTarget())
                {
                    if ((noebuffed != null) && noebuffed.IsValidTarget(E.Range) &&
                        (noebuffed.Health < QGetRealDamage(noebuffed)))
                        if (E.IsReady())
                        {
                            E.CastOnUnit(noebuffed);
                            if (Q.IsReady())
                                Q.Cast(noebuffed);
                            //Game.PrintChat("<font color='#9400D3'>DEBUG: Spreading [Reset with E]</font>");
                        }
                }
        }

        private static void ModeChanger()
        {
            if (Menu.Item("ModeChangerOnLowHP").GetValue<bool>())
            {
                if (Player.HealthPercent < Menu.Item("ModeChangerHPToChange").GetValue<Slider>().Value)
                {
                    //
                }
            }
        }

        private static void Combo()
        {
            // Combo
            var CUseQ = Menu.Item("CUseQ").GetValue<bool>();
            var CUseW = Menu.Item("CUseW").GetValue<bool>();
            var CUseE = Menu.Item("CUseE").GetValue<bool>();
            // Checks
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            // If Target's not in Q Range or there's no target or target's invulnerable don't fuck with him
            if ((target == null) || !target.IsValidTarget(Q.Range) || target.IsInvulnerable)
                return;

            switch (Menu.Item("ComboMode").GetValue<StringList>().SelectedIndex)
            {
                case 0:

                    #region Burst Mode

                    // Execute the Lad
                    if (Menu.Item("CUseIgnite").GetValue<bool>() &&
                        (target.Health <
                         OktwCommon.GetIncomingDamage(target) +
                         Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite)))
                        Player.Spellbook.CastSpell(IgniteSlot, target);
                    if (Player.Mana >= Q.Instance.ManaCost + W.Instance.ManaCost + E.Instance.ManaCost)
                    {
                        if (CUseW && target.IsValidTarget(W.Range) && W.IsReady())
                            W.CastOnUnit(target);
                        if (CUseQ && target.IsValidTarget(Q.Range))
                            SebbySpell(Q, target);
                        if (CUseE && target.IsValidTarget(E.Range) && E.IsReady())
                            E.CastOnUnit(target);
                    }
                    else
                    {
                        if (CUseW && target.IsValidTarget(W.Range) && W.IsReady())
                            W.CastOnUnit(target);
                        if (CUseQ && target.IsValidTarget(Q.Range))
                            SebbySpell(Q, target);
                        if (CUseE && target.IsValidTarget(E.Range) && E.IsReady())
                            E.CastOnUnit(target);
                    }

                    #endregion

                    break;
                case 1:

                    #region SurvivorMode

                    if ((Q.Level >= 1) && (W.Level >= 1) && (E.Level >= 1))
                    {
                        if (!target.IsValidTarget(W.Range - 15f) && Q.IsReady())
                            SebbySpell(Q, target);
                        // Try having Full Charge if either W or E spells are ready...
                        if (RyzeCharge1() && Q.IsReady() && (W.IsReady() || E.IsReady()))
                        {
                            if (E.IsReady())
                                E.Cast(target);
                            if (W.IsReady())
                                W.Cast(target);
                        }
                        // Rest in Piece XDDD
                        if (RyzeCharge1() && !E.IsReady() && !W.IsReady())
                            SebbySpell(Q, target);

                        if (RyzeCharge0() && !E.IsReady() && !W.IsReady())
                            SebbySpell(Q, target);

                        if (!RyzeCharge2())
                        {
                            E.Cast(target);
                            W.Cast(target);
                        }
                        else
                        {
                            SebbySpell(Q, target);
                        }
                    }
                    else
                    {
                        if (target.IsValidTarget(Q.Range) && Q.IsReady())
                            SebbySpell(Q, target);

                        if (target.IsValidTarget(W.Range) && W.IsReady())
                            W.Cast(target);

                        if (target.IsValidTarget(E.Range) && E.IsReady())
                            E.Cast(target);
                    }

                    #endregion

                    break;
            }
        }

        private static void Harass()
        {
            // Harass
            var HarassUseQ = Menu.Item("HarassQ").GetValue<bool>();
            var HarassUseW = Menu.Item("HarassW").GetValue<bool>();
            var HarassUseE = Menu.Item("HarassE").GetValue<bool>();
            // Checks
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var ryzeebuffed =
                MinionManager.GetMinions(Player.Position, E.Range)
                    .Find(x => x.HasBuff("RyzeE") && x.IsValidTarget(E.Range));
            // If Target's not in Q Range or there's no target or target's invulnerable don't fuck with him
            if ((target == null) || !target.IsValidTarget(Q.Range) || target.IsInvulnerable)
                return;

            // Execute the Lad
            if (Player.ManaPercent > Menu.Item("HarassManaManager").GetValue<Slider>().Value)
            {
                if (HarassUseW && target.IsValidTarget(W.Range))
                    W.CastOnUnit(target);
                if (HarassUseQ && target.IsValidTarget(Q.Range))
                    SebbySpell(Q, target);
                if (HarassUseE && ryzeebuffed.IsValidTarget() && (target.Distance(ryzeebuffed) < 200))
                    E.CastOnUnit(ryzeebuffed);
                else if (HarassUseE && (!ryzeebuffed.IsValidTarget() || (ryzeebuffed == null)) &&
                         target.IsValidTarget(W.Range))
                    E.CastOnUnit(target);
            }
        }

        private static void LastHit()
        {
            var useQ = Menu.Item("UseQLH").GetValue<bool>();
            var useE = Menu.Item("UseELH").GetValue<bool>();
            // To be Done
            if (Player.ManaPercent > Menu.Item("LaneHitManaManager").GetValue<Slider>().Value)
            {
                var specialQ = Cache.GetMinions(Player.Position, Q.Range, MinionTeam.Enemy).OrderBy(x => x.Distance(Player.Position));
                if (Q.IsReady() && useQ)
                {
                    foreach (var omfgabriel in specialQ)
                    {
                        if (omfgabriel.Health < QGetRealDamage(omfgabriel))
                        {
                            Q.Cast(omfgabriel);
                        }
                    }
                }
                var allMinionsQ = Cache.GetMinions(Player.ServerPosition, Q.Range, MinionTeam.Enemy);
                var allMinionsE = Cache.GetMinions(Player.ServerPosition, E.Range, MinionTeam.Enemy);
                if (Q.IsReady() && useQ)
                {
                    if (allMinionsQ.Count > 0)
                        foreach (var minion in allMinionsQ)
                        {
                            if (!minion.IsValidTarget() || (minion == null))
                                return;
                            if (minion.Health < QGetRealDamage(minion))
                                Q.Cast(minion);
                        }
                }
                else if (E.IsReady() && useE)
                {
                    if (allMinionsE.Count > 0)
                        foreach (var minion in allMinionsE)
                        {
                            if (!minion.IsValidTarget() || (minion == null))
                                return;
                            if (minion.Health < E.GetDamage(minion))
                                E.CastOnUnit(minion);
                        }
                }
            }
        }

        private static void LaneClear()
        {
            if (!Menu.Item("EnableFarming").GetValue<bool>())
                return;

            // LaneClear | Notes: Rework on early levels not using that much abilities since Spell Damage is lower, higher Lvl is fine
            if (Menu.Item("UseQLC").GetValue<bool>() || Menu.Item("UseELC").GetValue<bool>())
                if (Player.ManaPercent > Menu.Item("LaneClearManaManager").GetValue<Slider>().Value)
                {
                    var ryzeebuffed =
                        MinionManager.GetMinions(Player.Position, Q.Range)
                            .Find(x => x.HasBuff("RyzeE") && x.IsValidTarget(Q.Range));
                    var ryzenotebuffed =
                        MinionManager.GetMinions(Player.Position, Q.Range)
                            .Find(x => !x.HasBuff("RyzeE") && x.IsValidTarget(Q.Range));
                    var allMinionsQ = Cache.GetMinions(Player.ServerPosition, Q.Range, MinionTeam.Enemy);
                    var allMinions = Cache.GetMinions(Player.ServerPosition, E.Range, MinionTeam.Enemy);
                    if (Q.IsReady() && !E.IsReady())
                        if (allMinionsQ.Count > 0)
                            foreach (var minion in allMinionsQ)
                            {
                                if (!minion.IsValidTarget() || (minion == null))
                                    return;
                                if (minion.Health < QGetRealDamage(minion))
                                    Q.Cast(minion);
                            }
                    if (!Q.IsReady() && (Q.Level > 0) && E.IsReady())
                        if (ryzeebuffed != null)
                        {
                            if ((ryzeebuffed.Health < E.GetDamage(ryzeebuffed) + QGetRealDamage(ryzeebuffed) + 20) &&
                                ryzeebuffed.IsValidTarget(E.Range))
                            {
                                E.CastOnUnit(ryzeebuffed);
                                if (Q.IsReady())
                                    Q.Cast(ryzeebuffed);

                                Orbwalker.ForceTarget(ryzeebuffed);
                            }
                        }
                        else if (ryzeebuffed == null)
                        {
                            foreach (var minion in allMinions)
                                if (minion.IsValidTarget(E.Range) &&
                                    (minion.Health < E.GetDamage(minion) + QGetRealDamage(minion) + 20))
                                {
                                    E.CastOnUnit(minion);
                                    if (Q.IsReady())
                                        Q.Cast(ryzeebuffed);
                                }
                        }
                    if (Q.IsReady() && E.IsReady())
                        if (ryzeebuffed != null)
                        {
                            if ((ryzeebuffed.Health <
                                 Q.GetDamage(ryzeebuffed) + E.GetDamage(ryzeebuffed) + Q.GetDamage(ryzeebuffed)) &&
                                ryzeebuffed.IsValidTarget(E.Range))
                            {
                                Q.Cast(ryzeebuffed);
                                if (ryzeebuffed.IsValidTarget(E.Range))
                                    E.CastOnUnit(ryzeebuffed);
                                if (!E.IsReady() && Q.IsReady())
                                    Q.Cast(ryzeebuffed);
                            }
                        }
                        else if (ryzeebuffed == null)
                        {
                            if ((ryzenotebuffed.Health <
                                 Q.GetDamage(ryzenotebuffed) + E.GetDamage(ryzenotebuffed) + Q.GetDamage(ryzenotebuffed)) &&
                                ryzenotebuffed.IsValidTarget(E.Range))
                            {
                                Q.Cast(ryzenotebuffed);
                                if (ryzenotebuffed.IsValidTarget(E.Range))
                                {
                                    Orbwalker.ForceTarget(ryzenotebuffed);
                                    E.CastOnUnit(ryzenotebuffed);
                                }
                                if (!E.IsReady() && Q.IsReady())
                                    Q.Cast(ryzenotebuffed);
                            }
                        }
                }
        } // LaneClear End

        private static void REscape()
        {
            switch (R.Level)
            {
                case 1:
                    RangeR = 1750f;
                    break;
                case 2:
                    RangeR = 3000f;
                    break;
            }
            var NearByTurrets =
                ObjectManager.Get<Obj_AI_Turret>().Find(turret => (turret.Distance(Player) < RangeR) && turret.IsAlly);
            if (NearByTurrets != null)
                R.Cast(NearByTurrets.Position);
        }

        //RUsage

        private static float CalculateDamage(Obj_AI_Base enemy)
        {
            float damage = 0;
            if (Q.IsReady() || (Player.Mana <= Q.Instance.ManaCost + E.Instance.ManaCost))
                damage += QGetRealDamage(enemy);
            else if (Q.IsReady() || (Player.Mana <= Q.Instance.ManaCost))
                damage += Q.GetDamage(enemy);

            if (W.IsReady() || (Player.Mana <= W.Instance.ManaCost + W.Instance.ManaCost))
                damage += W.GetDamage(enemy) + W.GetDamage(enemy);
            else if (W.IsReady() || (Player.Mana <= W.Instance.ManaCost))
                damage += W.GetDamage(enemy);

            if (E.IsReady() || (Player.Mana <= E.Instance.ManaCost + E.Instance.ManaCost))
                damage += E.GetDamage(enemy) + E.GetDamage(enemy);
            else if (E.IsReady() || (Player.Mana <= E.Instance.ManaCost))
                damage += E.GetDamage(enemy);

            if (Menu.Item("CUseIgnite").GetValue<bool>())
                damage += (float) Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);

            return damage;
        }

        #region Declaration

        private static Spell Q, W, E, R;
        private static SpellSlot IgniteSlot;
        private static Items.Item TearOfGod;
        private static Items.Item Manamune;
        private static Items.Item Archangel;

        #region Items

        public static Items.Item
            HPPotion = new Items.Item(2003, 0),
            Flask = new Items.Item(2041, 0),
            Biscuit = new Items.Item(2010, 0),
            FlaskRef = new Items.Item(2031, 0),
            FlaskHunterJG = new Items.Item(2032, 0),
            FlaskCorruptJG = new Items.Item(2033, 0),
            Protobelt = new Items.Item(3152, 850f),
            GLP800 = new Items.Item(3030, 800f),
            Hextech = new Items.Item(3146, 700f),
            Seraph = new Items.Item(3040, 0);

        #endregion

        private static Orbwalking.Orbwalker Orbwalker;
        private static Menu Menu;

        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }

        private const string ChampionName = "Ryze";
        private static int lvl1, lvl2, lvl3, lvl4;
        private static float RangeR;
        private static float QRealDamage;

        #endregion
    }
}