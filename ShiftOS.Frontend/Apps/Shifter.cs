﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using ShiftOS.Engine;
using ShiftOS.Frontend.GUI;

namespace ShiftOS.Frontend.Apps
{
    [WinOpen("shifter")]
    [DefaultTitle("Shifter")]
    [Launcher("Shifter", false, null, "Customization")]
    public class Shifter : Control, IShiftOSWindow
    {
        private const int metaWidth = 150;
        private Button _apply = null;
        private List<Button> _meta = new List<Button>();
        private Skin _skin = null;
        private string _metaCurrent = "";
        private string _catCurrent = "";
        private List<Button> _catList = new List<Button>();

        public Shifter()
        {
            _apply = new GUI.Button();

            AddControl(_apply);
            _apply.Width = metaWidth;
            _apply.Height = 50;
            _skin = JsonConvert.DeserializeObject<Skin>(JsonConvert.SerializeObject(SkinEngine.LoadedSkin));
        }

        protected override void OnLayout(GameTime gameTime)
        {
            base.OnLayout(gameTime);
            _apply.X = 10;
            _apply.Y = Height - _apply.Height - 10;
            _apply.Text = "Apply Changes";

            int metay = 10;
            foreach(var btn in _meta)
            {
                btn.X = 10;
                btn.Y = metay;
                btn.Height = 25;
                btn.Width = metaWidth;
                metay += btn.Height + 10;
            }

            int catY = Height - 10;
            foreach(var btn in _catList)
            {
                btn.Width = metaWidth;
                btn.Height = 25;
                catY -= btn.Height;
                btn.Y = catY;
                catY -= 10;
                btn.X = 20 + metaWidth;
            }
        }

        public void ResetMetaListing()
        {
            var type = _skin.GetType();
            List<string> metanames = new List<string>();
            foreach (var field in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                var meta = field.GetCustomAttributes(false).FirstOrDefault(x => x is ShifterMetaAttribute) as ShifterMetaAttribute;
                if(meta != null)
                {
                    if (!metanames.Contains(meta.Meta))
                        metanames.Add(meta.Meta);
                }
            }
            while(_meta.Count > 0)
            {
                RemoveControl(_meta[0]);
                ; ; ; ; ; ; ; ; ; ; ; ; ; ; ; //IT'S THE SEMICOLON PARTYFEST
                //that's actually valid C#
                //like, VS isn't fucking freaking out
                //wtf
                //Microsoft, you're drunk.
                _meta.RemoveAt(0);

            }
            foreach (var meta in metanames)
            {
                var button = new Button();
                button.Click += () =>
                {
                    _metaCurrent = meta;
                    ResetCategoryListing();
                };
                button.Text = Localization.Parse(meta);
                AddControl(button);
                _meta.Add(button);
            }
        }

        public void ResetCategoryListing()
        {
            while(_catList.Count > 0)
            {
                RemoveControl(_catList[0]);
                _catList.RemoveAt(0);
            }
            List<string> catnames = new List<string>();
            var type = _skin.GetType();
            foreach (var field in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                var meta = field.GetCustomAttributes(false).FirstOrDefault(x => x is ShifterMetaAttribute) as ShifterMetaAttribute;
                if (meta != null)
                {
                    if(meta.Meta == _metaCurrent)
                    {
                        var cat = field.GetCustomAttributes(false).FirstOrDefault(x => x is ShifterCategoryAttribute) as ShifterCategoryAttribute;
                        if(cat != null)
                        {
                            if (!catnames.Contains(cat.Category))
                                catnames.Add(cat.Category);
                        }
                    }
                }
            }

            foreach (var meta in catnames)
            {
                var button = new Button();
                button.Click += () =>
                {
                    _catCurrent = meta;
                    //ResetValueEditor();
                };
                button.Text = Localization.Parse(meta);
                AddControl(button);
                _catList.Add(button);
            }

        }

        public void OnLoad()
        {
            ResetMetaListing();
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
        }
    }
}
