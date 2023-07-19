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

namespace AutoSynthesis
{
    static class UICommunicator
    {
        #region Properties and Consts
        public static Label UpdateLabel { get; set; }
        public static Label UpdateLabel2 { get; set; }
        public static Label TotalTimerLabel { get; set; }
        public static Label FoodSyrupLabel { get; set; }
        public static Label CraftTimerLabel { get; set; }
        public static Label MacroTimerLabel { get; set; }
        public static Label FoodTimerLabel { get; set; }
        public static ProgressBar ProgressTotal { get; set; }
        public static ProgressBar ProgressCraft { get; set; }
        public static ProgressBar ProgressMacro { get; set; }

        public static ProgressBar ProgressFood { get; set; }

        public static Action<Exception> ErrorMessageHandler { get; set; }

        private static double ProgressTotalTimeDuration { get; set; }
        private static double ProgressCraftTimeDuration { get; set; }
        private static DateTime ProgressTotalTime { get; set; }
        private static DateTime ProgressCraftTime { get; set; }
        private static double ProgressMacroTimeDuration { get; set; }
        private static DateTime ProgressMacroTime { get; set; }
        private static double ProgressFoodTimeDuration { get; set; }
        private static DateTime ProgressFoodTime { get; set; }
        private static DateTime NullDateTime { get; set; }
        private static int MacroNumber { get; set; }
        private static int CraftNumber { get; set; }
        private static int MaxCraft { get; set; }
        private static int FoodNumber { get; set; }
        private static int MaxFood { get; set; }

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


        private static string PreviousTotalTimerText { get; set; }
        private static string PreviousCraftTimerText { get; set; }
        private static string PreviousMacroTimerText { get; set; }
        private static string PreviousFoodTimerText { get; set; }
        private static string PreviousFoodSyrupTimerText { get; set; }

        #endregion

        #region Setup Methods
        public static void ConnectUI(Label totalLabel, Label updateLabel, Label updateLabel2, Label craftTimerLabel,
                                     Label macroTimerLabel, Label foodSyrupLabel, Label foodTimerLabel, ProgressBar progressTotal,
                                     ProgressBar progressCraft, ProgressBar progressMacro, ProgressBar progressFood)
        {
            TotalTimerLabel = totalLabel;
            UpdateLabel = updateLabel;
            UpdateLabel2 = updateLabel2;
            FoodSyrupLabel = foodSyrupLabel;
            CraftTimerLabel = craftTimerLabel;
            MacroTimerLabel = macroTimerLabel;
            FoodTimerLabel = foodTimerLabel;
            ProgressTotal = progressTotal;
            ProgressCraft = progressCraft;
            ProgressMacro = progressMacro;
            ProgressFood = progressFood;

            TotalTimerLabel.Visibility = Visibility.Hidden;
            CraftTimerLabel.Visibility = Visibility.Hidden;
            MacroTimerLabel.Visibility = Visibility.Hidden;
            FoodTimerLabel.Visibility = Visibility.Hidden;

            updateLabel.Content = "";
            updateLabel2.Content = "";
            foodSyrupLabel.Content = "";
        }

        public static void ResetValues()
        {
            CraftNumber = 0;
            MaxCraft = 0;
            MacroNumber = 0;
            FoodEnabled = false;
            SyrupEnabled = false;
            PreviousUpdate2Message = "";
            ProgressTotalTime = new DateTime();
            ProgressTotalTimeDuration = 0;
            ProgressCraftTime = new DateTime();
            ProgressCraftTimeDuration = 0;
            ProgressMacroTime = new DateTime();
            ProgressMacroTimeDuration = 0;
            ProgressFoodTime = new DateTime();
            ProgressFoodTimeDuration = 0;
            UpdateOverride = false;
            PreviousCraftTimerText = "";
            PreviousMacroTimerText = "";
            PreviousFoodSyrupTimerText = "";

        }
        #endregion

        #region Update Methods (Called by External Functions)
        // Updates visual display on craft status
        public static void UpdateCraftUIInfo(int craftCount, int max)
        {
            CraftNumber = craftCount;
            MaxCraft = max;
        }

        // Updates visual display on food status
        public static void UpdateFoodUIInfo(int foodCount, int max)
        {
            FoodNumber = foodCount;
            MaxFood = max;
        }

        public static void UpdateMacroUIInfo(int macroNumber, int macroTimer)
        {
            MacroNumber = macroNumber;
            UpdateStatus($"Using Macro {MacroNumber}...");

            ProgressMacroTimeDuration = macroTimer;
            ProgressMacroTime = DateTime.Now.AddMilliseconds(macroTimer);
        }

        public static void BeginTotalTimer(int totalTime)
        {
            ProgressTotalTimeDuration = totalTime;
            ProgressTotalTime = DateTime.Now.AddMilliseconds(totalTime);
        }

        public static void BeginCraftTimer(int totalTime)
        {
            ProgressCraftTimeDuration = totalTime;
            ProgressCraftTime = DateTime.Now.AddMilliseconds(totalTime);
        }

        public static void BeginFoodTimer(int totalTime)
        {
            ProgressFoodTimeDuration = totalTime;
            if (IsDateNull(ProgressFoodTime))
            {
                ProgressFoodTime = DateTime.Now.AddMilliseconds(totalTime);
            }
            else
            {
                ProgressFoodTime = ProgressFoodTime.AddMilliseconds(totalTime);
            }
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
            }
            else
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
            UpdateProgressBar(ProgressTotal, 0);
            UpdateProgressBar(ProgressCraft, 0);
            UpdateProgressBar(ProgressMacro, 0);
            FoodSyrupLabel.Dispatcher.Invoke(() => { FoodSyrupLabel.Content = ""; });
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
                    SetProgressBarLabelVisible();

                    while (!token.IsCancellationRequested)
                    {
                        // Update Total Craft Timer
                        UpdateTimerProgressBar(ProgressTotal, ProgressTotalTime, ProgressTotalTimeDuration);
                        UpdateTotalTimerText();

                        // Update Craft Timer
                        UpdateTimerProgressBar(ProgressCraft, ProgressCraftTime, ProgressCraftTimeDuration);
                        UpdateCraftTimerText();

                        // Update Macro Timer
                        UpdateTimerProgressBar(ProgressMacro, ProgressMacroTime, ProgressMacroTimeDuration);
                        UpdateMacroTimerText();

                        // Update Food Label
                        UpdateTimerProgressBar(ProgressFood, ProgressFoodTime, ProgressFoodTimeDuration);
                        UpdateFoodTimerText();

                        UpdateFoodSyrupLabel();

                        Thread.Sleep(TICK_TIME);
                    }

                    SetProgressBarLabelVisible(false);
                }
                catch (Exception e)
                {
                    ErrorMessageHandler(e);
                }
            };
            Task.Run(action, token);
        }

        private static void UpdateTotalTimerText()
        {
            var output = $"Total: ";
            var timer = GetTimeRemainingString(ProgressTotalTime);
            output += timer;

            if (output != PreviousTotalTimerText)
            {
                TotalTimerLabel.Dispatcher.Invoke(() => { TotalTimerLabel.Content = output; });
            }

            PreviousTotalTimerText = output;
        }

        private static void UpdateCraftTimerText()
        {
            var output = $"Craft {CraftNumber}";
            if (MaxCraft > 0)
                output += $"/{MaxCraft}";
            output += ": ";
            var timer = GetTimeRemainingString(ProgressCraftTime);
            output += timer;

            if (output != PreviousCraftTimerText)
                CraftTimerLabel.Dispatcher.Invoke(() => { CraftTimerLabel.Content = output; });

            PreviousCraftTimerText = output;
        }

        private static void UpdateMacroTimerText()
        {
            var output = "Macro " + MacroNumber + ": ";
            var timer = GetTimeRemainingString(ProgressMacroTime);
            output += timer;

            if (output != PreviousMacroTimerText)
                MacroTimerLabel.Dispatcher.Invoke(() => { MacroTimerLabel.Content = output; });

            PreviousMacroTimerText = output;
        }

        private static void UpdateFoodTimerText()
        {
            var output = $"Food {FoodNumber}";
            if (MaxFood > 0)
            {
                output += $"/{MaxFood}";
            }
            output += ": ";
            var timer = GetTimeRemainingString(ProgressFoodTime);
            output += timer;

            if (output != PreviousFoodTimerText)
                FoodTimerLabel.Dispatcher.Invoke(() => { FoodTimerLabel.Content = output; });

            PreviousFoodTimerText = output;
        }

        private static void UpdateFoodSyrupLabel()
        {
            var output = GetFoodSyrupLabelString();

            if (output != PreviousFoodSyrupTimerText)
                FoodSyrupLabel.Dispatcher.Invoke(() => { FoodSyrupLabel.Content = output; });
            PreviousFoodSyrupTimerText = output;
        }

        private static string GetFoodSyrupLabelString()
        {
            if (FoodEnabled && SyrupEnabled)
            {
                var foodString = GetTimeRemainingString(NextFood, true);
                var syrupString = GetTimeRemainingString(NextSyrup, true);
                return "Next Food: " + foodString + ". Next Syrup: " + syrupString;
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
                    return minutes.ToString() + ":" + seconds.ToString("00");
                }
            }
        }

        private static void SetProgressBarLabelVisible(bool setToVisible = true)
        {
            var startingOpacity = setToVisible ? 0 : 100;
            var visibility = setToVisible ? Visibility.Visible : Visibility.Hidden;
            // Set the labels on and their opacity to 0
            TotalTimerLabel.Dispatcher.Invoke(() => TotalTimerLabel.Opacity = startingOpacity);
            CraftTimerLabel.Dispatcher.Invoke(() => CraftTimerLabel.Opacity = startingOpacity);
            MacroTimerLabel.Dispatcher.Invoke(() => MacroTimerLabel.Opacity = startingOpacity);
            FoodTimerLabel.Dispatcher.Invoke(() => FoodTimerLabel.Opacity = startingOpacity);
            if (setToVisible)
            {
                TotalTimerLabel.Dispatcher.Invoke(() => TotalTimerLabel.Visibility = visibility);
                CraftTimerLabel.Dispatcher.Invoke(() => CraftTimerLabel.Visibility = visibility);
                MacroTimerLabel.Dispatcher.Invoke(() => MacroTimerLabel.Visibility = visibility);
                if (FoodEnabled)
                    FoodTimerLabel.Dispatcher.Invoke(() => FoodTimerLabel.Visibility = visibility);
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
                    TotalTimerLabel.Dispatcher.Invoke(() => TotalTimerLabel.Opacity = prog);
                    CraftTimerLabel.Dispatcher.Invoke(() => CraftTimerLabel.Opacity = prog);
                    MacroTimerLabel.Dispatcher.Invoke(() => MacroTimerLabel.Opacity = prog);
                    if (FoodEnabled)
                        FoodTimerLabel.Dispatcher.Invoke(() => FoodTimerLabel.Opacity = prog);
                    Thread.Sleep(TICK_TIME);
                }
                TotalTimerLabel.Dispatcher.Invoke(() => TotalTimerLabel.Opacity = 1);
                CraftTimerLabel.Dispatcher.Invoke(() => CraftTimerLabel.Opacity = 1);
                MacroTimerLabel.Dispatcher.Invoke(() => MacroTimerLabel.Opacity = 1);
                if (FoodEnabled)
                    FoodTimerLabel.Dispatcher.Invoke(() => FoodTimerLabel.Opacity = 1);

                if (!setToVisible)
                {
                    TotalTimerLabel.Dispatcher.Invoke(() => TotalTimerLabel.Visibility = visibility);
                    CraftTimerLabel.Dispatcher.Invoke(() => CraftTimerLabel.Visibility = visibility);
                    MacroTimerLabel.Dispatcher.Invoke(() => MacroTimerLabel.Visibility = visibility);
                    if (FoodEnabled)
                        FoodTimerLabel.Dispatcher.Invoke(() => FoodTimerLabel.Visibility = visibility);
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
            var totalProgress = GetProgressBarValue(ProgressTotal);
            var totalProgressStep = totalProgress / tickCount;
            var craftProgress = GetProgressBarValue(ProgressCraft);
            var craftProgressStep = craftProgress / tickCount;
            var macroProgress = GetProgressBarValue(ProgressMacro);
            var macroProgressStep = macroProgress / tickCount;
            var foodProgress = GetProgressBarValue(ProgressFood);
            var foodProgressStep = foodProgress / tickCount;
            for (int i = 0; i < tickCount; i++)
            {
                totalProgress -= totalProgressStep;
                craftProgress -= craftProgressStep;
                macroProgress -= macroProgressStep;
                foodProgress -= foodProgressStep;
                UpdateProgressBar(ProgressTotal, totalProgress);
                UpdateProgressBar(ProgressCraft, craftProgress);
                UpdateProgressBar(ProgressMacro, macroProgress);
                UpdateProgressBar(ProgressFood, foodProgress);
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
            Func<double> func = () => { return (progress.Value / progress.Maximum); };
            var value = progress.Dispatcher.Invoke(func);
            return value;
        }
        #endregion
    }
}
