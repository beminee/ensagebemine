using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage.Common.Menu;
using SharpDX;

namespace HuskarSharp.Utilities
{
    public class MenuManager
    {
        public Menu Menu { get; set; }

        public readonly MenuItem ComboMenu;

        public readonly MenuItem ArmletThresholdMenu;

        public readonly MenuItem LifeBreakThreshold;

        public readonly MenuItem SatanicThreshold;

        public MenuManager(string heroName)
        {
            this.Menu = new Menu("HuskarSharp", "HuskarSharp", true, heroName, true);
            this.ComboMenu = new MenuItem("ComboMenu", "ComboMenu").SetValue(new KeyBind('D', KeyBindType.Press));
            this.ArmletThresholdMenu = new MenuItem("ArmletThreshold", "Armlet Threshold").SetValue(new Slider(150, 0, 500)).SetTooltip("Toggle Armlet when below X HP");
            this.LifeBreakThreshold = new MenuItem("LifeBreakThreshold", "Life Break Threshold").SetValue(new Slider(200, 0, 900)).SetTooltip("Don't use Life Break is below X HP");
            this.SatanicThreshold = new MenuItem("SatanicThreshold", "Satanic Threshold").SetValue(new Slider(10, 0, 100)).SetTooltip("Turn on Satanic when below X HP");
            this.Menu.AddItem(LifeBreakThreshold);
            this.Menu.AddItem(ArmletThresholdMenu);
            this.Menu.AddItem(this.ComboMenu);

        }

        public bool ComboOn
        {
            get
            {
                return this.ComboMenu.GetValue<KeyBind>().Active;
            }
        }

        public int HP
        {
            get
            {
                return this.ArmletThresholdMenu.GetValue<Slider>().Value;
            }
        }

        public int LBThreshold
        {
            get
            {
                return this.LifeBreakThreshold.GetValue<Slider>().Value;
            }
        }

        public int SThreshold
        {
            get
            {
                return this.SatanicThreshold.GetValue<Slider>().Value;
            }
        }


    }
}
