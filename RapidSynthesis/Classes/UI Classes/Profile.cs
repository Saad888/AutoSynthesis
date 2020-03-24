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
        public bool ThirtyFood { get; set; }

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
                       bool collectable, bool thirtyFood)
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
            ThirtyFood = thirtyFood;
        }

        public Profile(string profileInput)
        {
            // Create from input string
            List<string> inputs = profileInput.Split('|').ToList();
            if (inputs.Count != PropertyCount)
            {
                throw new ProfileImproperStringFormatException();
            }

            Macro1 = HotkeyContainer.FromString(inputs[0]);
            Macro1Time = Convert.ToInt32(inputs[1]);

            Macro2 = HotkeyContainer.FromString(inputs[2]);
            Macro2Time = Convert.ToInt32(inputs[3]);
            Macro2Check = Convert.ToBoolean(inputs[4]);

            Macro3 = HotkeyContainer.FromString(inputs[5]);
            Macro3Time = Convert.ToInt32(inputs[6]);
            Macro3Check = Convert.ToBoolean(inputs[7]);

            Food = HotkeyContainer.FromString(inputs[8]);
            FoodCheck = Convert.ToBoolean(inputs[9]);

            Syrup = HotkeyContainer.FromString(inputs[10]);
            SyrupCheck = Convert.ToBoolean(inputs[11]);

            Select = HotkeyContainer.FromString(inputs[12]);
            Cancel = HotkeyContainer.FromString(inputs[13]);
            Collectable = Convert.ToBoolean(inputs[14]);
            ThirtyFood = Convert.ToBoolean(inputs[15]);
        }
        #endregion

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
                ThirtyFood.ToString()       // 15
            });
            return output;
        }

    }
}
