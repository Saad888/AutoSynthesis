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
        public static Label CraftsCompletedLabel { get; set; }
        public static Label FoodSyrupLabel { get; set; }
        public static Label CraftTimerLabel { get; set; }
        public static Label MacroTimerLabel { get; set; }
        public static ProgressBar ProgressOverall { get; set; }
        public static ProgressBar ProgressCraft { get; set; }
        public static ProgressBar ProgressMacro { get; set; }

        private static double ProgressCraftTimeDuration { get; set; }
        private static DateTime ProgressCraftTime { get; set; }
        private static double ProgressMacroTimeDuration { get; set; }
        private static DateTime ProgressMacroTime { get; set; }
        private static DateTime NullDateTime { get; set; }
        private static int MacroNumber { get; set; }
        private static int CraftNumber { get; set; }
        private static int MaxNumber { get; set; }

        private const int MIN_PROG = 0;
        private const int MAX_PROG = 1000;
        private const int TICK_TIME = 25;

        private static CancellationTokenSource OverallCancellationToken { get; set; }
        private static CancellationTokenSource TimedCancellationToken { get; set; }
        #endregion

        #region Setup Methods
        public static void ConnectUI(Label headerLabel, Label updateLabel, Label foodSyrupLabel, Label craftLabel, 
                                     Label macroLabel, ProgressBar progressOverall,
                                     ProgressBar progressCraft, ProgressBar progressMacro)
        {
            CraftsCompletedLabel = headerLabel;
            UpdateLabel = updateLabel;
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
        
        public static void EndAllProgress()
        {
            TimedCancellationToken.Cancel();
            if (OverallCancellationToken != null)
                OverallCancellationToken.Cancel();
            DropProgressToZero();
            UpdateProgressBar(ProgressOverall, 0);
            UpdateProgressBar(ProgressCraft, 0);
            UpdateProgressBar(ProgressMacro, 0);
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

        public static void UpdateStatus(string text)
        {
            Action action = () => { UpdateLabel.Content = text; };
            DispatchActionLabel(UpdateLabel, action);
        }
        #endregion

        #region Progress Bar Updates
        public static void StartTimedProgressBarUpdates()
        {
            TimedCancellationToken = new CancellationTokenSource();
            var token = TimedCancellationToken.Token;

            Action action = () =>
            {
                SetLabelVisibility(Visibility.Visible);

                while (!token.IsCancellationRequested)
                {
                    // Update Craft Timer
                    UpdateTimerProgressBar(ProgressCraft, ProgressCraftTime, ProgressCraftTimeDuration);
                    UpdateCraftTimerText();

                    // Update Macro Timer
                    UpdateTimerProgressBar(ProgressMacro, ProgressMacroTime, ProgressMacroTimeDuration);
                    UpdateMacroTimerText();

                    Thread.Sleep(TICK_TIME);
                }

                SetLabelVisibility(Visibility.Hidden);
            };
            Task.Run(action, token);
        }

        private static void UpdateCraftTimerText()
        {
            var output = "Craft " + CraftNumber + ": ";
            var difference = ProgressCraftTime - DateTime.Now + new TimeSpan(0, 0, 1);
            var timer = "";

            if (difference.TotalMilliseconds < 0)
                timer = "0:00";
            else
                timer = difference.ToString(@"m\:ss");

            output += timer;

            CraftTimerLabel.Dispatcher.Invoke(() => { CraftTimerLabel.Content = output; });
        }

        private static void UpdateMacroTimerText()
        {
            var output = "Macro " + MacroNumber + ": ";
            var difference = ProgressMacroTime - DateTime.Now + new TimeSpan(0, 0, 1);
            var timer = "";

            if (difference.TotalMilliseconds < 0)
                timer = "0:00";
            else
                timer = difference.ToString(@"m\:ss");

            output += timer;
            MacroTimerLabel.Dispatcher.Invoke(() => { MacroTimerLabel.Content = output; });
        }

        private static void SetLabelVisibility(Visibility setting)
        {
            Action action = () =>
            {
                CraftsCompletedLabel.Visibility = setting;
                MacroTimerLabel.Visibility = setting;
                CraftTimerLabel.Visibility = setting;
            };
            MacroTimerLabel.Dispatcher.Invoke(action);
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
            var overallProgress = GetProgressBarValue(ProgressOverall);
            var overallProgressStep = overallProgress / 10;
            var craftProgress = GetProgressBarValue(ProgressCraft);
            var craftProgressStep = craftProgress / 10;
            var macroProgress = GetProgressBarValue(ProgressMacro);
            var macroProgressStep = macroProgress / 10;
            for (int i = 0; i < 20; i++)
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
