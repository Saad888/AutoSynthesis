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
        private static int TotalCount = 0;
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

            if(!CraftPrimedToCancel)
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
                UICommunicator.UpdateCompletedUIInfo(0, Settings.CraftCount);


                CraftCount = 1;
                TotalCount = 0;
                if (HotkeySet[HKType.Food] != null)
                {
                    NextFoodUse = CalculateNextConsumableUse(Settings.StartingFoodTime);
                    UICommunicator.UpdateFood(NextFoodUse);
                }
                if (HotkeySet[HKType.Syrup] != null)
                {
                    NextSyrupUse = CalculateNextConsumableUse(Settings.StartingSyrupTime);
                    UICommunicator.UpdateSyrup(NextSyrupUse);
                }

                Logger.Write("Initiating Crafting");

                // If crafts remaining was 0, loop infinitley
                // If not, craft until quota is met
                while (!CraftingComplete())
                {
                    // UI MESSAGE: Set timer for overall craft
                    UICommunicator.UpdateCraftUIInfo(CraftCount, Settings.CraftCount);

                    // Begin Craft Timer:
                    RunCraftProgressBar();

                    // Initiate Macro 1
                    SendMacroInput(HotkeySet[HKType.Macro1], 1);

                    // Initiate Macro 2
                    SendMacroInput(HotkeySet[HKType.Macro2], 2);

                    // Initiate Macro 3
                    SendMacroInput(HotkeySet[HKType.Macro3], 3);

                    // Update UI Message
                    TotalCount = CraftCount;
                    UICommunicator.UpdateCompletedUIInfo(TotalCount, Settings.CraftCount);

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
            craftCompleted += Settings.CraftCount > 0 ? $"{TotalCount}/{Settings.CraftCount}" : $"{TotalCount}";
            UICommunicator.UpdateStatus2(craftCompleted);
            CraftingActive = false;

            var foodRemaining = (int)(NextFoodUse.AddMinutes(CalculateFoodSyrupMargin()) - DateTime.Now).TotalMinutes;
            var syrupRemaining = (int)(NextSyrupUse.AddMinutes(CalculateFoodSyrupMargin()) - DateTime.Now).TotalMinutes;
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
            UICommunicator.UpdateMacroUIInfo(macroNumber, hotkey.TimerInMiliseconds, VerifyFinalMacro(macroNumber));
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
            SendInput(HotkeySet[HKType.Confirm]);
            SendInput(HotkeySet[HKType.Confirm]);
        }

        private static bool CraftingComplete()
        {
            return CraftPrimedToCancel || ((Settings.CraftCount != 0) && (TotalCount >= Settings.CraftCount));
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
            Break(500);

            try
            {
                ProcessManager.DisableInputs();
                Break(50);
                SendInput(HotkeySet[HKType.Confirm], 3);
                Break(50);
            }
            finally
            {
                ProcessManager.EnableInputs();
            }

            Break(1500);

            try
            {
                ProcessManager.DisableInputs();
                Break(50);
                SendInput(HotkeySet[HKType.Confirm]);
                SendInput(HotkeySet[HKType.Cancel]);
                SendInput(HotkeySet[HKType.Confirm]);
                Break(50);
            }
            finally
            {
                ProcessManager.EnableInputs();
            }

            Break(1500);

            // use food and syrup as needed
            if (useFood)
            {
                UICommunicator.UpdateStatus("Using Food...");
                Logger.Write("Using Food");
                SendInput(HotkeySet[HKType.Food]);
                NextFoodUse = CalculateNextConsumableUse(Settings.FoodDuration);
                UICommunicator.UpdateFood(NextFoodUse);
            }
            if (useSyrup)
            {
                UICommunicator.UpdateStatus("Using Syrup...");
                Logger.Write("Using Syrup");
                SendInput(HotkeySet[HKType.Syrup]);
                NextSyrupUse = CalculateNextConsumableUse(STANDARD_SYRUP_TIME);
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
                Break(50);
                SendInput(HotkeySet[HKType.Confirm], 3);
                Break(50);
            }
            finally
            {
                ProcessManager.EnableInputs();
            }


        }

        private static DateTime CalculateNextConsumableUse(int timeRemainingInMinutes)
        {
            var timeMargin = CalculateFoodSyrupMargin();
            return DateTime.Now.AddMinutes(timeRemainingInMinutes - timeMargin);
        }

        private static double CalculateFoodSyrupMargin()
        {
            double totalTime = HotkeySet[HKType.Macro1].TimerInMiliseconds;
            if (HotkeySet[HKType.Macro2] != null)
                totalTime += HotkeySet[HKType.Macro2].TimerInMiliseconds;
            if (HotkeySet[HKType.Macro3] != null)
                totalTime += HotkeySet[HKType.Macro3].TimerInMiliseconds;
            return totalTime / (1000 * 60) + 0.25;
        }

        #endregion

        #region UI Methods
        private static void RunCraftProgressBar()
        {
            int totalTime = HotkeySet[HKType.Macro1].TimerInMiliseconds;
            if (HotkeySet[HKType.Macro2] != null)
                totalTime += HotkeySet[HKType.Macro2].TimerInMiliseconds;
            if (HotkeySet[HKType.Macro3] != null)
                totalTime += HotkeySet[HKType.Macro3].TimerInMiliseconds;

            UICommunicator.BeginCraftTimer(totalTime);
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
                SleepThread(hotkey.TimerInMiliseconds);

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
