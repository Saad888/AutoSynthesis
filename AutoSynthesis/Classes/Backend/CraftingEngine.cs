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
        public static DateTime FoodExpiration { get; set; }
        public static DateTime NextSyrupUse { get; set; }
        public static CancellationTokenSource Cts { get; set; }
        private static Action EndCraftCallback { get; set; }
        public static Action<Exception> ErrorMessageHandler { get; set; }
        private static Action<int, int> SetFoodAndSyrupTimings { get; set; }

        // Action to update the "Craft Count" text box
        private static Action<int> SetCraftCount { get; set; }

        private static bool CraftPrimedToCancel { get; set; }
        private static bool FoodPrimedToCancel { get; set; }

        private const int CONSUMABLE_MARGIN_IN_MINUTES = 2;
        private const int STANDARD_SYRUP_TIME = 15;
        private const int STANDARD_MENU_DELAY = 2000;
        private const int STANDARD_ANIMATION_DELAY = 2000;
        private const int STANDARD_TICK_TIME = 50;

        // The current craft number
        private static int CraftNumber = 0;

        // The craft number target
        private static int CraftCountTarget = 0;

        // The number of crafts completed
        private static int CompletedCount = 0;

        private static int FoodConsumed = 0;
        #endregion

        #region System Methods
        public static void InitiateCraftingEngine(Dictionary<HKType, Hotkey> hotKeyDictionary,
            SettingsContainer userSettings, Action endCraftCallback, Action<Exception> errorMessageHandler,
            Action<int, int> setFoodAndSyrupTimings, Action<int> setCraftCount)
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
                SetCraftCount = setCraftCount;

                // Load process or throw error if process does not exist
                ProcessManager.LoadProcess();

                // Set values 
                CraftPrimedToCancel = false;
                FoodPrimedToCancel = false;

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

        public static void CancelAfterFood()
        {
            if (!CraftingActive)
                return;

            if (!FoodPrimedToCancel)
            {
                FoodPrimedToCancel = true;
                UICommunicator.UpdateStatus("Finishing this food...", true);
                return;
            }

            UICommunicator.UpdateStatus("Ending Food...");
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

                CraftNumber = 1;
                CraftCountTarget = Settings.CraftCount;
                CompletedCount = 0;
                FoodConsumed = 0;
                if (HotkeySet[HKType.Food] != null)
                {
                    UICommunicator.UpdateFoodUIInfo(FoodConsumed, Settings.FoodCount);
                    NextFoodUse = CalculateNextConsumableUse(Settings.StartingFoodTime, DateTime.Now);
                    UICommunicator.UpdateFood(NextFoodUse);

                    FoodExpiration = DateTime.Now.AddMinutes(Settings.StartingFoodTime);
                    RunFoodProgressBar(Settings.StartingFoodTime * 60 * 1000);
                }
                if (HotkeySet[HKType.Syrup] != null)
                {
                    NextSyrupUse = CalculateNextConsumableUse(Settings.StartingSyrupTime, DateTime.Now);
                    UICommunicator.UpdateSyrup(NextSyrupUse);
                }

                RunTotalProgressBar();
                DateTime craftStartTime = DateTime.Now;

                // If crafts remaining was 0, loop infinitely
                // If not, craft until quota is met
                while (!CraftingComplete())
                {
                    // UI MESSAGE: Set timer for overall craft
                    UICommunicator.UpdateCraftUIInfo(CraftNumber, CraftCountTarget);

                    // Begin Craft Timer:
                    RunCraftProgressBar();

                    // Add requested delay
                    Break(Settings.StartingDelay);

                    // Initiate Macro 1
                    SendMacroInput(HotkeySet[HKType.Macro1], 1);

                    // Initiate Macro 2
                    SendMacroInput(HotkeySet[HKType.Macro2], 2);

                    // Initiate Macro 3
                    SendMacroInput(HotkeySet[HKType.Macro3], 3);

                    // Finish and add requested end delay
                    Break(STANDARD_MENU_DELAY + Settings.EndingDelay);

                    // Collectable Menu Option
                    SendCollectableConfirmationInput();

                    CompletedCount = CraftNumber;

                    // Use Food and Syrup
                    SendFoodAndSyrupInput();

                    // Prepare next craft if crafting is not finished
                    PrepareNextCraftInput();

                    CraftNumber += 1;
                }

                var totalCraftTime = (int)(DateTime.Now - craftStartTime).TotalSeconds;
                Logger.Write($"Actual total craft time of {totalCraftTime}");
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
            craftCompleted += CraftCountTarget > 0 ? $"{CompletedCount}/{CraftCountTarget}" : $"{CompletedCount}";
            UICommunicator.UpdateStatus2(craftCompleted);
            CraftingActive = false;

            var foodRemaining = (int)(FoodExpiration - DateTime.Now).TotalMinutes;
            var syrupRemaining = (int)(NextSyrupUse - DateTime.Now).TotalMinutes;
            SetFoodAndSyrupTimings.Invoke(foodRemaining, syrupRemaining);

            if (Settings.CraftCount > 0)
            {
                // Update the "Craft Count" text box
                var craftsRemaining = CraftCountTarget - CompletedCount;
                SetCraftCount.Invoke(craftsRemaining);
            }

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
            UICommunicator.UpdateMacroUIInfo(macroNumber, hotkey.TimerInMilliseconds);
            SendInput(hotkey);
        }

        private static void SendCollectableConfirmationInput()
        {
            if (Settings.CollectableCraft == false)
                return;

            Logger.Write("Accepting Collectable Craft");

            UICommunicator.UpdateStatus("Accepting Collectable Craft...");
            SendInput(HotkeySet[HKType.Confirm], 2);
            Break(STANDARD_MENU_DELAY);
        }

        private static bool CraftingComplete()
        {
            return CraftPrimedToCancel || ((CraftCountTarget != 0) && (CompletedCount >= CraftCountTarget));
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

            if (useFood && FoodPrimedToCancel || (Settings.FoodCount > 0 && FoodConsumed >= Settings.FoodCount))
            {
                // Only cancel crafting if food is actually about to expire
                if (DateTime.Compare(FoodExpiration, DateTime.Now) <= 0)
                {
                    throw new CraftCancelRequest();
                }
                // Don't use food
                return;
            }

            UICommunicator.UpdateStatus("Refreshing Consumables...");
            Logger.Write("Refreshing Consumables");

            // enter a craft and leave it out
            Break(Settings.StartingDelay);

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

            Break(STANDARD_ANIMATION_DELAY);

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

                FoodConsumed++;
                UICommunicator.UpdateFoodUIInfo(FoodConsumed, Settings.FoodCount);

                NextFoodUse = CalculateNextConsumableUse(Settings.FoodDuration, FoodExpiration);
                UICommunicator.UpdateFood(NextFoodUse);

                FoodExpiration = FoodExpiration.AddMinutes(Settings.FoodDuration);
                RunFoodProgressBar((int)(FoodExpiration - DateTime.Now).TotalMilliseconds);
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

            Break(STANDARD_ANIMATION_DELAY);
        }

        #endregion

        #region Calculation Methods

        private static DateTime CalculateNextConsumableUse(int timeRemainingInMinutes, DateTime start)
        {
            var timeMargin = CalculateFoodSyrupMargin();
            return start.AddMinutes(timeRemainingInMinutes - timeMargin);
        }

        private static double CalculateFoodSyrupMargin()
        {
            return (double)CalculateMacroTime() / (1000 * 60) + 0.25;
        }

        private static int CalculateInputTime(HKType type)
        {
            if (HotkeySet[type] == null)
            {
                return 0;
            }

            int totalTime = HotkeySet[type].TimerInMilliseconds;
            if (HotkeySet[type].ModKeyCodes != null)
            {
                totalTime += 50;
            }
            return totalTime;
        }

        private static int CalculateMacroTime()
        {
            int totalTime = CalculateInputTime(HKType.Macro1);
            totalTime += CalculateInputTime(HKType.Macro2);
            totalTime += CalculateInputTime(HKType.Macro3);
            return totalTime;
        }

        private static int CalculateCraftTime()
        {
            int totalTime = Settings.StartingDelay;
            totalTime += CalculateMacroTime();
            totalTime += STANDARD_MENU_DELAY;
            totalTime += Settings.EndingDelay;
            if (Settings.CollectableCraft)
            {
                totalTime += CalculateInputTime(HKType.Confirm) * 2;
                totalTime += STANDARD_MENU_DELAY;
            }
            return totalTime;
        }

        #endregion

        #region UI Methods
        private static void RunTotalProgressBar()
        {
            // Don't set up the total progress bar if we don't have targets
            if (CraftCountTarget == 0 && Settings.FoodCount == 0)
            {
                return;
            }

            DateTime start = DateTime.Now;
            DateTime simulated = start;
            DateTime simulatedNextFood = NextFoodUse;
            DateTime simulatedNextSyrup = NextSyrupUse;
            DateTime simulatedFoodExpiration = FoodExpiration;
            bool done = false;
            int craftsCompleted = 0;
            int foodConsumed = 0;
            while (!done)
            {
                simulated = simulated.AddMilliseconds(CalculateCraftTime());
                craftsCompleted++;

                if (CraftCountTarget > 0 && craftsCompleted >= CraftCountTarget)
                {
                    break;
                }

                bool useFood = (HotkeySet[HKType.Food] != null && DateTime.Compare(simulatedNextFood, simulated) <= 0);
                bool useSyrup = (HotkeySet[HKType.Syrup] != null && DateTime.Compare(simulatedNextSyrup, simulated) <= 0);
                if (Settings.FoodCount > 0 &&
                    HotkeySet[HKType.Food] != null &&
                    foodConsumed >= Settings.FoodCount
                    && DateTime.Compare(simulatedFoodExpiration, simulated) <= 0)
                {
                    break;
                }
                if (useFood || useSyrup)
                {
                    simulated = simulated.AddMilliseconds(
                        Settings.StartingDelay +
                        100 +
                        CalculateInputTime(HKType.Confirm) * 3 +
                        STANDARD_ANIMATION_DELAY +
                        100 +
                        CalculateInputTime(HKType.Confirm) +
                        CalculateInputTime(HKType.Cancel) +
                        CalculateInputTime(HKType.Confirm) +
                        STANDARD_MENU_DELAY);
                    if (useFood)
                    {
                        simulated = simulated.AddMilliseconds(CalculateInputTime(HKType.Food));
                        simulatedFoodExpiration = simulatedFoodExpiration.AddMinutes(Settings.FoodDuration);
                        simulatedNextFood = CalculateNextConsumableUse(Settings.FoodDuration, simulatedFoodExpiration);
                        foodConsumed++;
                    }
                    if (useSyrup)
                    {
                        simulated = simulated.AddMilliseconds(CalculateInputTime(HKType.Syrup));
                        simulatedNextSyrup = CalculateNextConsumableUse(STANDARD_SYRUP_TIME, simulated);
                    }
                }

                // Prepare next craft input
                simulated = simulated.AddMilliseconds(
                    100 +
                    CalculateInputTime(HKType.Confirm) * 3 +
                    STANDARD_ANIMATION_DELAY);

                // For each craft, add an artificial execution delay
                simulated = simulated.AddMilliseconds(400);
            }

            CraftCountTarget = craftsCompleted;
            var totalDuration = (int)(simulated - start).TotalMilliseconds;
            Logger.Write($"Estimating total craft time of {totalDuration}");
            UICommunicator.BeginTotalTimer(totalDuration);
        }

        private static void RunCraftProgressBar()
        {
            int totalTime = CalculateCraftTime();
            UICommunicator.BeginCraftTimer(totalTime);
        }

        private static void RunFoodProgressBar(int milliseconds)
        {
            // int totalTime = minutes * 60 * 1000;
            UICommunicator.BeginFoodTimer(milliseconds);

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
