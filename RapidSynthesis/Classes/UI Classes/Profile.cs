using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidSynthesis
{
    class Profile
    {
        #region Properties and Consts
        public HotkeyContainer Macro1 { get; set; }
        public int Macro1Time { get; set; }

        public HotkeyContainer Macro2 { get; set; }
        public int Macro2Time { get; set; }
        public bool Macro2Check { get; set; }

        public HotkeyContainer Macro3 { get; set; }
        public int Macro3Time { get; set; }
        public bool Macro3Check { get; set; }

        public HotkeyContainer Food { get; set; }
        public bool FoodCheck { get; set; }

        public HotkeyContainer Syrup { get; set; }
        public bool SyrupCheck { get; set; }

        public HotkeyContainer Select { get; set; }
        public HotkeyContainer Cancel { get; set; }
        public bool Collectable { get; set; }
        public int FoodDuration { get; set; }

        private enum KeyType
        {
            Macro1, Macro2, Macro3,
            Macro1Time, Macro2Time, Macro3Time, 
            Macro2Check, Macro3Check,
            Food, FoodCheck,
            Syrup, SyrupCheck,
            Select, Cancel, Collectable, FoodDuration
        }

        private Dictionary<KeyType, string> Defaults;



        private const int PropertyCount = 16;
        #endregion

        #region Constructors
        public Profile() { }

        public Profile(HotkeyContainer macro1, int macro1Time, 
                       HotkeyContainer macro2, int macro2Time, bool macro2Check, 
                       HotkeyContainer macro3, int macro3Time, bool macro3Check, 
                       HotkeyContainer food, bool foodCheck, 
                       HotkeyContainer syrup, bool syrupCheck,
                       HotkeyContainer select, HotkeyContainer cancel, 
                       bool collectable, int foodDuration)
        {
            Macro1 = macro1;
            Macro2 = macro2;
            Macro3 = macro3;
            Macro1Time = macro1Time;
            Macro2Time = macro2Time;
            Macro3Time = macro3Time;
            Macro2Check = macro2Check;
            Macro3Check = macro3Check;
            Food = food;
            FoodCheck = foodCheck;
            Syrup = syrup;
            SyrupCheck = syrupCheck;
            Select = select;
            Cancel = cancel;
            Collectable = collectable;
            FoodDuration = foodDuration;

            SetDefaults();

        }

        public Profile(string profileInput)
        {
            // Create from input string
            List<string> inputs = profileInput.Split('|').ToList();
            if (inputs.Count < PropertyCount)
            {
                for (int i = inputs.Count; i < PropertyCount; i++)
                    inputs.Add("");
            }

            SetDefaults();

            Macro1 = GetHotkeyString(inputs[0], KeyType.Macro1);
            Macro1Time = GetInteger(inputs[1], KeyType.Macro1Time);

            Macro2 = GetHotkeyString(inputs[2], KeyType.Macro2);
            Macro2Time = GetInteger(inputs[3], KeyType.Macro2Time);
            Macro2Check = GetBoolean(inputs[4], KeyType.Macro2Check);

            Macro3 = GetHotkeyString(inputs[5], KeyType.Macro3);
            Macro3Time = GetInteger(inputs[6], KeyType.Macro3Time);
            Macro3Check = GetBoolean(inputs[7], KeyType.Macro3Check);

            Food = GetHotkeyString(inputs[8], KeyType.Food);
            FoodCheck = GetBoolean(inputs[9], KeyType.FoodCheck);

            Syrup = GetHotkeyString(inputs[10], KeyType.Syrup);
            SyrupCheck = GetBoolean(inputs[11], KeyType.SyrupCheck);

            Select = GetHotkeyString(inputs[12], KeyType.Select);
            Cancel = GetHotkeyString(inputs[13], KeyType.Cancel);
            Collectable = GetBoolean(inputs[14], KeyType.Collectable);
            FoodDuration = GetInteger(inputs[15], KeyType.FoodDuration);
        }

        private void SetDefaults()
        {
            Defaults = new Dictionary<KeyType, string>
            {
                { KeyType.Macro1, "D1"},
                { KeyType.Macro2, "D2"},
                { KeyType.Macro3, "D3"},
                { KeyType.Macro1Time, "5"},
                { KeyType.Macro2Time, "5"},
                { KeyType.Macro3Time, "5"},
                { KeyType.Macro2Check, "false"},
                { KeyType.Macro3Check, "false"},
                { KeyType.Food, "D4"},
                { KeyType.FoodCheck, "false"},
                { KeyType.Syrup, "D5"},
                { KeyType.SyrupCheck, "false"},
                { KeyType.Select, "NumPad0"},
                { KeyType.Cancel, "NumPad1"},
                { KeyType.Collectable, "false"},
                { KeyType.FoodDuration, "30"}
            };
        }
        #endregion

        private HotkeyContainer GetHotkeyString(string input, KeyType keyType)
        {
            try
            {
                return HotkeyContainer.FromString(input);
            }
            catch (ArgumentException)
            {
                return HotkeyContainer.FromString(Defaults[keyType]);
            }
        }

        private int GetInteger(string input, KeyType keyType)
        {
            try
            {
                return Convert.ToInt32(input);
            }
            catch (FormatException)
            {
                return Convert.ToInt32(Defaults[keyType]);
            }
        }

        private bool GetBoolean(string input, KeyType keyType)
        {
            try
            {
                return Convert.ToBoolean(input);
            }
            catch (FormatException)
            {
                return Convert.ToBoolean(Defaults[keyType]);
            }
        }

        public override string ToString()
        {
            string output = String.Join("|", new string[]
            {
                Macro1.ToString(),          // 0
                Macro1Time.ToString(),      // 1

                Macro2.ToString(),          // 2
                Macro2Time.ToString(),      // 3
                Macro2Check.ToString(),     // 4

                Macro3.ToString(),          // 5
                Macro3Time.ToString(),      // 6
                Macro3Check.ToString(),     // 7

                Food.ToString(),            // 8
                FoodCheck.ToString(),       // 9

                Syrup.ToString(),           // 10
                SyrupCheck.ToString(),      // 11

                Select.ToString(),          // 12
                Cancel.ToString(),          // 13
                Collectable.ToString(),     // 14
                FoodDuration.ToString()       // 15
            });
            return output;
        }

    }
}
