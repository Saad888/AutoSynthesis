using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace AutoSynthesis
{
    static class CraftingEngine
    {
        #region Class Properties and Consts
        private static Dictionary<HKType, Hotkey> HotkeySet { get; set; }
        private static SettingsContainer Settings { get; set; }
        public static bool CraftingActive { get; set; } = false;
        public static DateTime NextFoodUse { get; set; }
        public static DateTime NextSyrupUse { get; set; }
        public static CancellationTokenSource Cts { get; set; }
        private static Action EndCraftCallback { get; set; }
        public static Action<Exception> ErrorMessageHandler { get; set; }
        private static Action<int, int> SetFoodAndSyrupTimings { get; set; }
        private static bool CraftPrimedToCancel { get; set; }

        private const int CONSUMABLE_MARGIN_IN_MINUTES = 2;
        private const int STANDARD_SYRUP_TIME = 15;
        private const int STANDARD_MENU_DELAY = 2000;
        private const int STANDARD_ANIMATION_DELAY = 2000;
        private const int STANDARD_TICK_TIME = 50;
        private static int CraftCount = 0;
        private static int CompletedCount = 0;
        #endregion

        #region System Methods
        public static void InitiateCraftingEngine(Dictionary<HKType, Hotkey> hotKeyDictionary,
            SettingsContainer userSettings, Action endCraftCallback, Action<Exception> errorMessageHandler, Action<int, int> setFoodAndSyrupTimings)
        {
            // Ensure craft is not already happening
            if (CraftingActive)
            {
                throw new DuplicateCraftingAttemptedException();
            }
            try
            {

                Logger.Write("Preparing Crafting");
                CraftingActive = true;

                EndCraftCallback = endCraftCallback;
                ErrorMessageHandler = errorMessageHandler;
                SetFoodAndSyrupTimings = setFoodAndSyrupTimings;

                // Load process or throw error if process does not exist
                ProcessManager.LoadProcess();

                // Set values 
                CraftPrimedToCancel = false;

                // Store values
                HotkeySet = hotKeyDictionary;
                Settings = userSettings;

                // Run crafting system on a new thread
                Cts = new CancellationTokenSource();
                var token = Cts.Token;
                Task.Run(() => RunCraftingEngine(token), token);
            }
            catch (Exception e)
            {
                CraftingActive = false;
                throw e;
            }
        }

        public static void CancelCrafting()
        {
            if (!CraftingActive)
                return;

            if (!CraftPrimedToCancel)
            {
                CraftPrimedToCancel = true;
                UICommunicator.UpdateStatus("Finishing this craft...", true);
                return;
            }

            UICommunicator.UpdateStatus("Ending Craft...");
            Cts.Cancel();
        }

        private static void RunCraftingEngine(CancellationToken token)
        {
            // PROCESS:
            // set queue parameters
            // calcualte overall time for full process
            // macro 1
            // macro 2
            // macro 3
            // collectable if needed
            // food if needed
            // pot if needed
            // begin next craft

            try
            {
                // Set crafting parameters
                UICommunicator.ResetValues();
                UICommunicator.ErrorMessageHandler = ErrorMessageHandler;
                UICommunicator.UpdateStatus("Setting up for Crafting...");
                UICommunicator.StartTimedProgressBarUpdates();

                RunTotalProgressBar();


                CraftCount = 1;
                CompletedCount = 0;
                if (HotkeySet[HKType.Food] != null)
                {
                    NextFoodUse = CalculateNextConsumableUse(Settings.StartingFoodTime, DateTime.Now);
                    UICommunicator.UpdateFood(NextFoodUse);

                    RunFoodProgressBar(Settings.StartingFoodTime);
                }
                if (HotkeySet[HKType.Syrup] != null)
                {
                    NextSyrupUse = CalculateNextConsumableUse(Settings.StartingSyrupTime, DateTime.Now);
                    UICommunicator.UpdateSyrup(NextSyrupUse);
                }


                // If crafts remaining was 0, loop infinitley
                // If not, craft until quota is met
                while (!CraftingComplete())
                {
                    // UI MESSAGE: Set timer for overall craft
                    UICommunicator.UpdateCraftUIInfo(CraftCount, Settings.CraftCount);

                    // Add requested delay
                    Break(Settings.StartingDelay);

                    // Begin Craft Timer:
                    RunCraftProgressBar();

                    // Initiate Macro 1
                    SendMacroInput(HotkeySet[HKType.Macro1], 1);

                    // Initiate Macro 2
                    SendMacroInput(HotkeySet[HKType.Macro2], 2);

                    // Initiate Macro 3
                    SendMacroInput(HotkeySet[HKType.Macro3], 3);
                    CompletedCount = CraftCount;

                    // Collectable Menu Option
                    SendCollectableConfirmationInput();

                    // Standard delay for menus
                    Break(STANDARD_MENU_DELAY);

                    // Use Food and Syrup
                    SendFoodAndSyrupInput();

                    // Prepare next craft if crafting is not finished
                    PrepareNextCraftInput();
                    Break(STANDARD_ANIMATION_DELAY);

                    CraftCount += 1;
                }
            }
            catch (Exception e) when (!(e is CraftCancelRequest))
            {
                ErrorMessageHandler(e);
            }
            finally
            {
                EndCraftingProcess();
            }
        }

        private static void EndCraftingProcess()
        {
            // ALL CLEANUP
            UICommunicator.EndAllProgress();
            UICommunicator.UpdateStatus("Crafting Finished!");
            var craftCompleted = "Completed ";
            craftCompleted += Settings.CraftCount > 0 ? $"{CompletedCount}/{Settings.CraftCount}" : $"{CompletedCount}";
            UICommunicator.UpdateStatus2(craftCompleted);
            CraftingActive = false;

            var foodRemaining = (int)(NextFoodUse - DateTime.Now).TotalMinutes;
            var syrupRemaining = (int)(NextSyrupUse - DateTime.Now).TotalMinutes;
            SetFoodAndSyrupTimings.Invoke(foodRemaining, syrupRemaining);

            EndCraftCallback.Invoke();
        }

        private static void CheckCancelRequest()
        {
            if (Cts.IsCancellationRequested)
            {
                Logger.Write("Crafting Cancelled");
                throw new CraftCancelRequest();
            }

        }
        #endregion

        #region Crafting Methods
        private static void SendMacroInput(Hotkey hotkey, int macroNumber)
        {
            if (hotkey == null)
                return;
            Logger.Write("Sending Macro " + macroNumber);
            // UI message: MACRO NUMBER macroNumber
            UICommunicator.UpdateMacroUIInfo(macroNumber, hotkey.TimerInMilliseconds, VerifyFinalMacro(macroNumber));
            SendInput(hotkey);
        }

        private static bool VerifyFinalMacro(int macroNumber)
        {
            switch (macroNumber)
            {
                case 1:
                    return (HotkeySet[HKType.Macro2] == null && HotkeySet[HKType.Macro3] == null);
                case 2:
                    return (HotkeySet[HKType.Macro3] == null);
            }
            return true;
        }

        private static void SendCollectableConfirmationInput()
        {
            if (Settings.CollectableCraft == false)
                return;

            Logger.Write("Accepting Collectable Craft");

            UICommunicator.UpdateStatus("Accepting Collectable Craft...");
            Break(STANDARD_MENU_DELAY);
            SendInput(HotkeySet[HKType.Confirm], 2);
        }

        private static bool CraftingComplete()
        {
            return CraftPrimedToCancel || ((Settings.CraftCount != 0) && (CompletedCount >= Settings.CraftCount));
        }

        private static void SendFoodAndSyrupInput()
        {
            if (CraftingComplete())
                return;

            // check if timers is passed
            bool useFood = (HotkeySet[HKType.Food] != null && DateTime.Compare(NextFoodUse, DateTime.Now) <= 0);
            bool useSyrup = (HotkeySet[HKType.Syrup] != null && DateTime.Compare(NextSyrupUse, DateTime.Now) <= 0);

            // leave if neither are to be used
            if (!useFood && !useSyrup)
                return;
            UICommunicator.UpdateStatus("Refreshing Consumables...");
            Logger.Write("Refreshing Consumables");

            // enter a craft and leave it out
            Break(Settings.StartingDelay);

            try
            {
                ProcessManager.DisableInputs();
                Break(100);
                SendInput(HotkeySet[HKType.Confirm], 3);
                SendInput(HotkeySet[HKType.Cancel]);
            }
            finally
            {
                ProcessManager.EnableInputs();
            }

            Break(1500);

            try
            {
                ProcessManager.DisableInputs();
                Break(100);
                SendInput(HotkeySet[HKType.Confirm]);
                SendInput(HotkeySet[HKType.Cancel]);
                SendInput(HotkeySet[HKType.Confirm]);
            }
            finally
            {
                ProcessManager.EnableInputs();
            }

            Break(STANDARD_MENU_DELAY);

            // use food and syrup as needed
            if (useFood)
            {
                UICommunicator.UpdateStatus("Using Food...");
                Logger.Write("Using Food");
                SendInput(HotkeySet[HKType.Food]);
                NextFoodUse = CalculateNextConsumableUse(Settings.FoodDuration, DateTime.Now);
                UICommunicator.UpdateFood(NextFoodUse);

                RunFoodProgressBar(Settings.FoodDuration);
            }
            if (useSyrup)
            {
                UICommunicator.UpdateStatus("Using Syrup...");
                Logger.Write("Using Syrup");
                SendInput(HotkeySet[HKType.Syrup]);
                NextSyrupUse = CalculateNextConsumableUse(STANDARD_SYRUP_TIME, DateTime.Now);
                UICommunicator.UpdateSyrup(NextSyrupUse);
            }
        }

        private static void PrepareNextCraftInput()
        {
            if (CraftingComplete())
                return;

            UICommunicator.UpdateStatus("Preparing Next Craft...");
            Logger.Write("Resetting Craft Cycle");
            try
            {
                ProcessManager.DisableInputs();
                Break(100);
                SendInput(HotkeySet[HKType.Confirm], 3);
            }
            finally
            {
                ProcessManager.EnableInputs();
            }


        }
        private static DateTime CalculateNextConsumableUse(int timeRemainingInMinutes, DateTime start)
        {
            var timeMargin = CalculateFoodSyrupMargin();
            return start.AddMinutes(timeRemainingInMinutes - timeMargin);
        }

        private static double CalculateFoodSyrupMargin()
        {
            double totalTime = HotkeySet[HKType.Macro1].TimerInMilliseconds;
            if (HotkeySet[HKType.Macro2] != null)
                totalTime += HotkeySet[HKType.Macro2].TimerInMilliseconds;
            if (HotkeySet[HKType.Macro3] != null)
                totalTime += HotkeySet[HKType.Macro3].TimerInMilliseconds;
            return totalTime / (1000 * 60) + 0.25;
        }

        #endregion

        #region UI Methods
        private static void RunTotalProgressBar()
        {
            DateTime start = DateTime.Now;
            DateTime simulated = start;
            DateTime simulatedNextFood = NextFoodUse;
            DateTime simulatedNextSyrup = NextSyrupUse;
            for (int i = 0; i < Settings.CraftCount; i++)
            {
                simulated = simulated.AddMilliseconds(Settings.StartingDelay);

                simulated = simulated.AddMilliseconds(HotkeySet[HKType.Macro1].TimerInMilliseconds);
                if (HotkeySet[HKType.Macro2] != null)
                    simulated = simulated.AddMilliseconds(HotkeySet[HKType.Macro2].TimerInMilliseconds);
                if (HotkeySet[HKType.Macro3] != null)
                    simulated = simulated.AddMilliseconds(HotkeySet[HKType.Macro3].TimerInMilliseconds);

                if (Settings.CollectableCraft)
                {
                    simulated = simulated.AddMilliseconds(STANDARD_MENU_DELAY);
                    simulated = simulated.AddMilliseconds(HotkeySet[HKType.Confirm].TimerInMilliseconds * 2);
                }

                simulated = simulated.AddMilliseconds(STANDARD_MENU_DELAY);

                bool useFood = (HotkeySet[HKType.Food] != null && DateTime.Compare(simulatedNextFood, start) <= 0);
                bool useSyrup = (HotkeySet[HKType.Syrup] != null && DateTime.Compare(simulatedNextSyrup, start) <= 0);
                if (useFood || useSyrup)
                {
                    simulated = simulated.AddMilliseconds(Settings.StartingDelay);
                    simulated = simulated.AddMilliseconds(100);
                    simulated = simulated.AddMilliseconds(HotkeySet[HKType.Confirm].TimerInMilliseconds * 3);
                    simulated = simulated.AddMilliseconds(HotkeySet[HKType.Cancel].TimerInMilliseconds);
                    simulated = simulated.AddMilliseconds(1500);
                    simulated = simulated.AddMilliseconds(100);
                    simulated = simulated.AddMilliseconds(HotkeySet[HKType.Confirm].TimerInMilliseconds);
                    simulated = simulated.AddMilliseconds(HotkeySet[HKType.Cancel].TimerInMilliseconds);
                    simulated = simulated.AddMilliseconds(HotkeySet[HKType.Confirm].TimerInMilliseconds);
                    simulated = simulated.AddMilliseconds(STANDARD_MENU_DELAY);
                    if (useFood)
                    {
                        simulated = simulated.AddMilliseconds(HotkeySet[HKType.Food].TimerInMilliseconds);
                        simulatedNextFood = CalculateNextConsumableUse(Settings.FoodDuration, simulated);
                    }
                    if (useSyrup)
                    {
                        simulated = simulated.AddMilliseconds(HotkeySet[HKType.Syrup].TimerInMilliseconds);
                        simulatedNextSyrup = CalculateNextConsumableUse(STANDARD_SYRUP_TIME, simulated);
                    }
                }

                // Prepare next craft input
                if (!CraftingComplete())
                {
                    simulated = simulated.AddMilliseconds(100);
                    simulated = simulated.AddMilliseconds(HotkeySet[HKType.Confirm].TimerInMilliseconds * 3);
                }

                simulated = simulated.AddMilliseconds(STANDARD_ANIMATION_DELAY);
            }

            var totalDuration = (int)(simulated - start).TotalMilliseconds;
            UICommunicator.BeginTotalTimer(totalDuration);
        }

        private static void RunCraftProgressBar()
        {
            int totalTime = HotkeySet[HKType.Macro1].TimerInMilliseconds;
            if (HotkeySet[HKType.Macro2] != null)
                totalTime += HotkeySet[HKType.Macro2].TimerInMilliseconds;
            if (HotkeySet[HKType.Macro3] != null)
                totalTime += HotkeySet[HKType.Macro3].TimerInMilliseconds;

            UICommunicator.BeginCraftTimer(totalTime);
        }

        private static void RunFoodProgressBar(int time)
        {
            int totalTime = time * 60 * 1000;

            totalTime -= HotkeySet[HKType.Macro1].TimerInMilliseconds;
            if (HotkeySet[HKType.Macro2] != null)
                totalTime -= HotkeySet[HKType.Macro2].TimerInMilliseconds;
            if (HotkeySet[HKType.Macro3] != null)
                totalTime -= HotkeySet[HKType.Macro3].TimerInMilliseconds;
            totalTime -= 15000;

            UICommunicator.BeginFoodTimer(totalTime);

        }
        #endregion

        #region Timing Methods
        private static void Break(int time)
        {
            SleepThread(time);
        }

        private static void SendInput(Hotkey hotkey, int repeat = 1)
        {
            for (int i = repeat; i > 0; i--)
            {
                UICommunicator.UpdateStatus2("Sending \"" + hotkey.ToString() + "\"");
                KeyInputEngine.SendKeysToGame(hotkey.KeyCode, hotkey.ModKeyCodes);
                SleepThread(hotkey.TimerInMilliseconds);
            }
        }

        private static void SleepThread(int timeInMilliseconds)
        {
            var targetTimeEnd = DateTime.Now.AddMilliseconds(timeInMilliseconds);
            Logger.Write("Sleeping Thread for " + timeInMilliseconds);
            while (DateTime.Now <= targetTimeEnd)
            {
                CheckCancelRequest();
                Thread.Sleep(STANDARD_TICK_TIME);
            }
        }
        #endregion
    }
}
