﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Plex.Engine;
using Plex.Frontend.GraphicsSubsystem;

namespace Plex.Frontend.Apps
{
    [Launcher("Upgrades", false, null, "Utilities")]
    [DefaultTitle("Upgrades")]
    [WinOpen("upgrademgr")]
    public class CodeShop : GUI.Control, IPlexWindow
    {
        private GUI.ListBox upgradelist = null;
        private ShiftoriumUpgrade selectedUpgrade = null;
        private GUI.ProgressBar upgradeprogress = null;
        private GUI.Button buy = null;

        public CodeShop()
        {
            Width = 720;
            Height = 480;
        }

        protected override void OnLayout(GameTime gameTime)
        {
            try
            {
                upgradelist.X = 30;
                upgradelist.Y = 75;
                upgradelist.Width = this.Width / 2;
                upgradelist.Width -= 30;
                upgradelist.Height = this.Height - upgradelist.Y - 75;
                upgradeprogress.X = upgradelist.X;
                upgradeprogress.Y = upgradelist.Y + upgradelist.Height + 10;
                upgradeprogress.Width = upgradelist.Width;
                upgradeprogress.Height = 24;
                upgradeprogress.Maximum = Upgrades.GetDefaults().Count;
                upgradeprogress.Value = SaveSystem.CurrentSave.CountUpgrades();
                buy.X = Width - buy.Width - 15;
                buy.Y = Height - buy.Height - 15;
                buy.Visible = (selectedUpgrade != null);

            }
            catch
            {

            }
        }

        public void OnLoad()
        {
            buy = new GUI.Button();
            buy.Text = "Buy upgrade";
            buy.AutoSize = true;
            buy.Font = SkinEngine.LoadedSkin.MainFont;
            buy.Click += () =>
            {
                if(Upgrades.Buy(selectedUpgrade.ID, selectedUpgrade.Cost) == true)
                {
                    Engine.Infobox.Show("Upgrade installed!", "You have successfully bought and installed the " + selectedUpgrade.Name + " upgrade for " + selectedUpgrade.Cost + " Experience.");
                    SelectUpgrade(null);
                    PopulateList();
                }
                else
                {
                    Engine.Infobox.Show("Insufficient funds.", "You do not have enough Experience to buy this upgrade. You need " + (selectedUpgrade.Cost - SaveSystem.CurrentSave.Experience) + " more.");
                }
            };
            AddControl(buy);
            upgradelist = new GUI.ListBox();
            upgradeprogress = new GUI.ProgressBar();
            AddControl(upgradeprogress);
            AddControl(upgradelist);
            upgradelist.SelectedIndexChanged += () =>
            {
                string itemtext = upgradelist.SelectedItem.ToString();
                var upg = Upgrades.GetAvailable().FirstOrDefault(x => $"{x.Category}: {x.Name} - {x.Cost}CP" == itemtext);
                if(upg != null)
                {
                    SelectUpgrade(upg);
                }
            };
            PopulateList();
        }

        public void SelectUpgrade(ShiftoriumUpgrade upgrade)
        {
            if(selectedUpgrade != upgrade)
            {
                selectedUpgrade = upgrade;
                Invalidate();
            }
        }

        public void PopulateList()
        {
            upgradelist.ClearItems();
            foreach(var upgrade in Upgrades.GetAvailable())
            {
                upgradelist.AddItem($"{upgrade.Category}: {upgrade.Name} - {upgrade.Cost}CP");
                Invalidate();
            }
        }

        public void OnSkinLoad()
        {
        }

        public bool OnUnload()
        {
            return true;
        }

        public void OnUpgrade()
        {
            PopulateList();
        }

        protected override void OnPaint(GraphicsContext gfx)
        {
            base.OnPaint(gfx);

            string title = "Welcome to the Shiftorium!";
            string desc = @"The Shiftorium is a place where you can buy upgrades for your computer. These upgrades include hardware enhancements, kernel and software optimizations and features, new programs, upgrades to existing programs, and more.

As you continue through your job, going further up the ranks, you will unlock additional upgrades which can be found here. You may also find upgrades which are not available within the Shiftorium when hacking more difficult and experienced targets. These upgrades are very rare and hard to find, though. You'll find them in the ""Installed Upgrades"" list.";

            if(selectedUpgrade != null)
            {
                title = selectedUpgrade.Category + ": " + selectedUpgrade.Name;

                desc = selectedUpgrade.Description;
            }

            int wrapwidth = (Width - (upgradelist.X + upgradelist.Width)) - 45;
            var titlemeasure = GraphicsContext.MeasureString(title, SkinEngine.LoadedSkin.Header2Font, wrapwidth);

            var descmeasure = GraphicsContext.MeasureString(desc, SkinEngine.LoadedSkin.MainFont, wrapwidth);

            int availablewidth = Width - (upgradelist.X + upgradelist.Width);
            int titlelocx = (availablewidth - (int)titlemeasure.X) / 2;
            titlelocx += (Width - availablewidth);
            int titlelocy = 30;
            gfx.DrawString(title, titlelocx, titlelocy, SkinEngine.LoadedSkin.ControlTextColor.ToMonoColor(), SkinEngine.LoadedSkin.Header2Font);

            int desclocy = (Height - (int)descmeasure.Y) / 2;
            int desclocx = (Width - availablewidth) + ((availablewidth - (int)descmeasure.X) / 2);
            gfx.DrawString(desc, desclocx, desclocy, SkinEngine.LoadedSkin.ControlTextColor.ToMonoColor(), SkinEngine.LoadedSkin.MainFont, wrapwidth);

            string shiftorium = "Shiftorium";
            var smeasure = GraphicsContext.MeasureString(shiftorium, SkinEngine.LoadedSkin.HeaderFont);
            gfx.DrawString(shiftorium, upgradelist.X + ((upgradelist.Width - (int)smeasure.X) / 2), 20, SkinEngine.LoadedSkin.ControlTextColor.ToMonoColor(), SkinEngine.LoadedSkin.HeaderFont);
        }
    }
}
