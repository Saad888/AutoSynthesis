using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsInput.Native;
using System.Threading;

namespace RapidSynthesis
{
    static class UICommunicator
    {
        #region Properties and Consts
        public static Label UpdateLabel { get; set; }
        public static Label UpdateLabel2 { get; set; }
        public static Label CraftsCompletedLabel { get; set; }
        public static Label FoodSyrupLabel { get; set; }
        public static Label CraftTimerLabel { get; set; }
        public static Label MacroTimerLabel { get; set; }
        public static ProgressBar ProgressOverall { get; set; }
        public static ProgressBar ProgressCraft { get; set; }
        public static ProgressBar ProgressMacro { get; set; }

        public static Action<Exception> ErrorMessageHandler { get; set; }

        private static double ProgressCraftTimeDuration { get; set; }
        private static DateTime ProgressCraftTime { get; set; }
        private static double ProgressMacroTimeDuration { get; set; }
        private static DateTime ProgressMacroTime { get; set; }
        private static DateTime NullDateTime { get; set; }
        private static int MacroNumber { get; set; }
        private static int CraftNumber { get; set; }
        private static int MaxNumber { get; set; }

        private static DateTime NextFood { get; set; }
        private static DateTime NextSyrup { get; set; }
        private static bool FoodEnabled { get; set; }
        private static bool SyrupEnabled { get; set; }

        private const int MIN_PROG = 0;
        private const int MAX_PROG = 1000;
        private const int TICK_TIME = 25;
        private const int FADE_TIME = 350;

        private static CancellationTokenSource OverallCancellationToken { get; set; }
        private static CancellationTokenSource TimedCancellationToken { get; set; }

        private static string PreviousUpdate2Message { get; set; }
        private static int PreviousUpdate2Count { get; set; }

        private static bool UpdateOverride { get; set; }
        #endregion

        #region Setup Methods
        public static void ConnectUI(Label headerLabel, Label updateLabel, Label updateLabel2, Label craftLabel, 
                                     Label macroLabel, Label foodSyrupLabel, ProgressBar progressOverall,
                                     ProgressBar progressCraft, ProgressBar progressMacro)
        {
            CraftsCompletedLabel = headerLabel;
            UpdateLabel = updateLabel;
            UpdateLabel2 = updateLabel2;
            FoodSyrupLabel = foodSyrupLabel;
            CraftTimerLabel = craftLabel;
            MacroTimerLabel = macroLabel;
            ProgressOverall = progressOverall;
            ProgressCraft = progressCraft;
            ProgressMacro = progressMacro;
        }

        public static void ResetValues()
        {
            CraftNumber = 0;
            MaxNumber = 0;
            MacroNumber = 0;
            FoodEnabled = false;
            SyrupEnabled = false;
            PreviousUpdate2Message = "";
            ProgressCraftTime = new DateTime();
            ProgressCraftTimeDuration = 0;
            ProgressMacroTime = new DateTime();
            ProgressMacroTimeDuration = 0;
            UpdateOverride = false;
        }
        #endregion

        #region Update Methods (Called by External Functions)
        public static void UpdateCraftUIInfo(int craftCount, int max)
        {
            // Updates visual display on craft status
            // Label
            CraftNumber = craftCount;
            MaxNumber = max;
        }

        public static void UpdateCompletedUIInfo(int craftCount, int max)
        {
            CraftNumber = craftCount;
            MaxNumber = max;
            string uiText = $"Crafted: ";
            string craftCounter = CraftNumber.ToString();
            if (max != 0)
                craftCounter += $"/{MaxNumber}";

            if (craftCounter.Length > 5)
            {
                UpdateCraftNumberLabel(craftCounter);
            } else
            {
                UpdateCraftNumberLabel(uiText + craftCounter);
            }

            // Progress Bar
            if (max > 0)
            {
                double p = (double)CraftNumber / MaxNumber;
                SmoothProgressUpdate(ProgressOverall, p);
            }
        }

        public static void UpdateMacroUIInfo(int macroNumber, int macroTimer)
        {
            MacroNumber = macroNumber;
            UpdateStatus($"Using Macro {MacroNumber}...");
            ProgressMacroTimeDuration = macroTimer;
            ProgressMacroTime = DateTime.Now.AddMilliseconds(macroTimer);
        }

        public static void BeginCraftTimer(int totalTime)
        {
            ProgressCraftTimeDuration = totalTime;
            ProgressCraftTime = DateTime.Now.AddMilliseconds(totalTime);
        }

        public static void UpdateFood(DateTime nextFood)
        {
            FoodEnabled = true;
            NextFood = nextFood;
        }

        public static void UpdateSyrup(DateTime nextSyrup)
        {
            SyrupEnabled = true;
            NextSyrup = nextSyrup;
        }

        public static void UpdateStatus(string text, bool overrideLabel = false)
        {
            if (UpdateOverride)
                return;

            if (overrideLabel)
                UpdateOverride = true;

            Action action = () => { UpdateLabel.Content = text; };
            DispatchActionLabel(UpdateLabel, action);
        }

        public static void UpdateOverrideReset()
        {
            UpdateOverride = false;
        }

        public static void UpdateStatus2(string text)
        {
            var match = text == PreviousUpdate2Message;
            PreviousUpdate2Message = text;
            if (match)
            {
                PreviousUpdate2Count++;
                text += "(" + PreviousUpdate2Count + ")";
            } else
            {
                PreviousUpdate2Count = 1;
            }
            Action action = () => { UpdateLabel2.Content = text; };
            DispatchActionLabel(UpdateLabel2, action);
        }

        public static void EndAllProgress()
        {
            TimedCancellationToken.Cancel();
            if (OverallCancellationToken != null)
                OverallCancellationToken.Cancel();
            UpdateOverrideReset();
            UpdateStatus2("");
            DropProgressToZero();
            UpdateProgressBar(ProgressOverall, 0);
            UpdateProgressBar(ProgressCraft, 0);
            UpdateProgressBar(ProgressMacro, 0);
            FoodSyrupLabel.Dispatcher.Invoke(() => { FoodSyrupLabel.Content = ""; });
        }
        #endregion

        #region Label Updates
        private static void UpdateCraftNumberLabel(string uiText)
        {
            // Update label
            Action action = () => { CraftsCompletedLabel.Content = uiText; };
            DispatchActionLabel(CraftsCompletedLabel, action);
            // Update progress bar
        }

        #endregion

        #region Progress Bar Updates
        public static void StartTimedProgressBarUpdates()
        {
            TimedCancellationToken = new CancellationTokenSource();
            var token = TimedCancellationToken.Token;

            Action action = () =>
            {
                try
                {
                    SetProgressbarLabelVisible();

                    while (!token.IsCancellationRequested)
                    {
                        // Update Craft Timer
                        UpdateTimerProgressBar(ProgressCraft, ProgressCraftTime, ProgressCraftTimeDuration);
                        UpdateCraftTimerText();

                        // Update Macro Timer
                        UpdateTimerProgressBar(ProgressMacro, ProgressMacroTime, ProgressMacroTimeDuration);
                        UpdateMacroTimerText();

                        // Update Food Label
                        UpdateFoodSyrupLabel();

                        Thread.Sleep(TICK_TIME);
                    }

                    SetProgressbarLabelVisible(false);
                }
                catch (Exception e)
                {
                    ErrorMessageHandler(e);
                }
            };
            Task.Run(action, token);
        }

        private static void UpdateCraftTimerText()
        {
            var output = "Craft " + CraftNumber + ": ";
            var timer = GetTimeRemainingString(ProgressCraftTime);
            output += timer;

            CraftTimerLabel.Dispatcher.Invoke(() => { CraftTimerLabel.Content = output; });
        }

        private static void UpdateMacroTimerText()
        {
            var output = "Macro " + MacroNumber + ": ";
            var timer = GetTimeRemainingString(ProgressMacroTime);
            output += timer;
            MacroTimerLabel.Dispatcher.Invoke(() => { MacroTimerLabel.Content = output; });
        }

        private static void UpdateFoodSyrupLabel()
        {
            var output = GetFoodSyrupLabelString();
            FoodSyrupLabel.Dispatcher.Invoke(() => { FoodSyrupLabel.Content = output; });
        }

        private static string GetFoodSyrupLabelString()
        {
            if (FoodEnabled && SyrupEnabled)
            {
                var foodString = GetTimeRemainingString(NextFood, true);
                var syrupString = GetTimeRemainingString(NextSyrup, true);
                return "Next Food: " + foodString + "             Next Syrup: " + syrupString;
            }
            if (FoodEnabled)
            {
                var foodString = GetTimeRemainingString(NextFood, true);
                return "Next Food In: " + foodString;
            }
            if (SyrupEnabled)
            {
                var syrupString = GetTimeRemainingString(NextSyrup, true);
                return "Next Syrup In: " + syrupString;
            }
            return "";
        }

        private static string GetTimeRemainingString(DateTime target, bool longFormat = false)
        {
            var difference = target - DateTime.Now + new TimeSpan(0, 0, 1);
            if (difference.TotalMilliseconds < 0)
                return longFormat ? "00:00" : "0:00";
            else
            {
                if (difference.Hours == 0)
                {
                    return longFormat ? difference.ToString(@"mm\:ss") : difference.ToString(@"m\:ss");
                }
                else
                {
                    var minutes = difference.Hours * 60 + difference.Minutes;
                    var seconds = difference.Seconds;
                    return longFormat ? minutes.ToString("00") + ":" + seconds.ToString("00") : minutes + ":" + seconds;
                }
            }
        }

        private static void SetProgressbarLabelVisible(bool setToVisible = true)
        {
            var startingOpacity = setToVisible ? 0 : 100;
            var visibility = setToVisible ? Visibility.Visible : Visibility.Hidden;
            // Set the labels on and their opacity to 0
            CraftsCompletedLabel.Dispatcher.Invoke(() => CraftsCompletedLabel.Opacity = startingOpacity);
            MacroTimerLabel.Dispatcher.Invoke(() => MacroTimerLabel.Opacity = startingOpacity);
            CraftTimerLabel.Dispatcher.Invoke(() => CraftTimerLabel.Opacity = startingOpacity);
            if (setToVisible)
            {
                CraftsCompletedLabel.Dispatcher.Invoke(() => CraftsCompletedLabel.Visibility = visibility);
                MacroTimerLabel.Dispatcher.Invoke(() => MacroTimerLabel.Visibility = visibility);
                CraftTimerLabel.Dispatcher.Invoke(() => CraftTimerLabel.Visibility = visibility);
            }

            // Fade in Bars on Separate Thread
            int tickCount = FADE_TIME / TICK_TIME;
            double jump = 1 / (double)tickCount;
            Action action = () =>
            {
                for (int i = 0; i < tickCount; i++)
                {
                    var prog = i * jump;
                    if (!setToVisible)
                        prog = 1 - prog;
                    CraftsCompletedLabel.Dispatcher.Invoke(() => CraftsCompletedLabel.Opacity = prog);
                    MacroTimerLabel.Dispatcher.Invoke(() => MacroTimerLabel.Opacity = prog);
                    CraftTimerLabel.Dispatcher.Invoke(() => CraftTimerLabel.Opacity = prog);
                    Thread.Sleep(TICK_TIME);
                }
                CraftsCompletedLabel.Dispatcher.Invoke(() => CraftsCompletedLabel.Opacity = 1);
                MacroTimerLabel.Dispatcher.Invoke(() => MacroTimerLabel.Opacity = 1);
                CraftTimerLabel.Dispatcher.Invoke(() => CraftTimerLabel.Opacity = 1);

                if (!setToVisible)
                {
                    CraftsCompletedLabel.Dispatcher.Invoke(() => CraftsCompletedLabel.Visibility = visibility);
                    MacroTimerLabel.Dispatcher.Invoke(() => MacroTimerLabel.Visibility = visibility);
                    CraftTimerLabel.Dispatcher.Invoke(() => CraftTimerLabel.Visibility = visibility);
                }
            };
            Task.Run(action);

        }

        private static void UpdateTimerProgressBar(ProgressBar prog, DateTime time, double duration)
        {
            // Update Progress Bar
            var p = GetProgressBarValue(time, duration);
            UpdateProgressBar(prog, p);
        }

        private static double GetProgressBarValue(DateTime target, double duration)
        {
            if (IsDateNull(target))
            {
                return 0;
            }
            else
            {
                var difference = (target - DateTime.Now).TotalMilliseconds;
                var percent = (duration - difference) / duration;
                return Math.Max(0, Math.Min(1, percent));
            }
        }

        private static bool IsDateNull(DateTime target)
        {
            return DateTime.Compare(target, NullDateTime) == 0;
        }

        private static void SmoothProgressUpdate(ProgressBar prog, double targetValue)
        {
            // Create cancellation token
            OverallCancellationToken = new CancellationTokenSource();
            var token = OverallCancellationToken.Token;

            Action action = () =>
            {
                try
                {
                    var currentValue = GetProgressBarValue(prog);

                    // Per tick, get value closer to target
                    while (currentValue < targetValue)
                    {
                        if (token.IsCancellationRequested)
                            break;
                        var newValue = (targetValue - currentValue) / 4 + currentValue;
                        UpdateProgressBar(prog, newValue);
                        Thread.Sleep(TICK_TIME);
                        currentValue = GetProgressBarValue(prog);
                    }
                }
                catch (Exception e)
                {
                    ErrorMessageHandler(e);
                }

            };
            Task.Run(action, token);
        }

        private static void UpdateProgressBar(ProgressBar prog, double p)
        {
            // Flat progress bar update by percent
            var value = Math.Ceiling(p * (MAX_PROG - MIN_PROG) + MIN_PROG);
            Action action = () => { prog.Value = value; };
            DispatchActionProgressBar(prog, action);
        }
        #endregion

        #region ResetUIMethods
        private static void DropProgressToZero()
        {
            var tickCount = FADE_TIME / TICK_TIME;
            var overallProgress = GetProgressBarValue(ProgressOverall);
            var overallProgressStep = overallProgress / tickCount;
            var craftProgress = GetProgressBarValue(ProgressCraft);
            var craftProgressStep = craftProgress / tickCount;
            var macroProgress = GetProgressBarValue(ProgressMacro);
            var macroProgressStep = macroProgress / tickCount;
            for (int i = 0; i < tickCount; i++)
            {
                overallProgress -= overallProgressStep;
                craftProgress -= craftProgressStep;
                macroProgress -= macroProgressStep;
                UpdateProgressBar(ProgressOverall, overallProgress);
                UpdateProgressBar(ProgressCraft, craftProgress);
                UpdateProgressBar(ProgressMacro, macroProgress);
                Thread.Sleep(TICK_TIME);
            }
        }
        #endregion

        #region Internal Functions
        private static void DispatchActionProgressBar(ProgressBar progress, Action action)
        {
            progress.Dispatcher.BeginInvoke(action);
        }

        private static void DispatchActionLabel(Label label, Action action)
        {
            label.Dispatcher.BeginInvoke(action);
        }

        private static double GetProgressBarValue(ProgressBar progress)
        {
            Func<double> func = () => { return (progress.Value/progress.Maximum); };
            var value = progress.Dispatcher.Invoke(func);
            return value;
        }
        #endregion
    }
}
