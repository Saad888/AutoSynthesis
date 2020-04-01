using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace RapidSynthesis
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

        private const int CONSUMABLE_MARGIN_IN_MINUTES = 2;
        private const int STANDARD_SYRUP_TIME = 15;
        private const int STANDARD_MENU_DELAY = 1500;
        private const int STANDARD_ANIMATION_DELAY = 2000;
        private const int STANDARD_TICK_TIME = 50;
        private static int CraftCount = 0;
        private static int TotalCount = 0;
        #endregion

        #region System Methods
        public static void InitiateCraftingEngine(Dictionary<HKType, Hotkey> hotKeyDictionary,
            SettingsContainer userSettings, Action endCraftCallback, Action<Exception> errorMessageHandler)
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

                // Load process or throw error if process does not exist
                ProcessManager.LoadProcess();

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
                while ((Settings.CraftCount == 0) || (CraftCount <= Settings.CraftCount))
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
            UICommunicator.UpdateMacroUIInfo(macroNumber, hotkey.TimerInMiliseconds);
            SendInput(hotkey);
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

        private static void SendFoodAndSyrupInput()
        {
            if ((Settings.CraftCount != 0) && (CraftCount < Settings.CraftCount))
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
            SendInput(HotkeySet[HKType.Confirm], 3);
            Break(1000);
            SendInput(HotkeySet[HKType.Cancel]);
            SendInput(HotkeySet[HKType.Confirm]);
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
            if ((Settings.CraftCount == 0) || (CraftCount < Settings.CraftCount))
            {
                UICommunicator.UpdateStatus("Preparing Next Craft...");
                Logger.Write("Resetting Craft Cycle");
                SendInput(HotkeySet[HKType.Confirm], 3);
            }
        }

        private static DateTime CalculateNextConsumableUse(int timeRemainingInMinutes)
        {
            return DateTime.Now.AddMinutes(timeRemainingInMinutes - CONSUMABLE_MARGIN_IN_MINUTES);
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
            UICommunicator.UpdateStatus2("Waiting...");
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
