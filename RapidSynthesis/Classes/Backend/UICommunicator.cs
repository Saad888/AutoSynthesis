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
        public static Label HeaderLabel { get; set; }
        public static ProgressBar ProgressOverall { get; set; }
        public static ProgressBar ProgressCraft { get; set; }
        public static ProgressBar ProgressMacro { get; set; }

        private const int MIN_PROG = 0;
        private const int MAX_PROG = 1000;
        private const int TICK_TIME = 50;

        private static Dictionary<ProgressBar, CancellationTokenSource> CancelTokenSources { get; set; }
        #endregion

        #region Setup Methods
        public static void ConnectUI(Label headerLabel, Label updateLabel, ProgressBar progressOverall,
                                     ProgressBar progressCraft, ProgressBar progressMacro)
        {
            HeaderLabel = headerLabel;
            UpdateLabel = updateLabel;
            ProgressOverall = progressOverall;
            ProgressCraft = progressCraft;
            ProgressMacro = progressMacro;

            CancelTokenSources = new Dictionary<ProgressBar, CancellationTokenSource>();
        }
        #endregion

        #region Update Methods (Called by External Functions)
        public static void UpdateCraftUIInfo(int craftCount, int max)
        {
            // Updates visual display on craft status
            // Label
            var uiText = $"Craft Number {craftCount}";
            if (max != 0)
            {
                uiText += $" out of {max}";
            }
            UpdateCraftNumberLabel(uiText);

            // Progress Bar
            if (max > 0)
            {
                double p = (double)craftCount / max;
                //UpdateProgressBar(ProgressOverall, p);
                SmoothProgressUpdate(ProgressOverall, p);
            }
        }

        public static void UpdateMacroUIInfo(int macroNumber, int macroTimer)
        {
            UpdateStatus($"Using Macro {macroNumber}...");
            TimedProgressUpdate(ProgressMacro, macroTimer);
        }

        public static void BeginCraftTimer(int totalTime)
        {
            TimedProgressUpdate(ProgressCraft, totalTime);
        }

        public static void EndAllProgress()
        {
            foreach(var tokenSources in CancelTokenSources.Values)
                tokenSources.Cancel();
            UpdateProgressBar(ProgressOverall, 0);
            UpdateProgressBar(ProgressCraft, 0);
            UpdateProgressBar(ProgressMacro, 0);
        }
        #endregion

        #region Label Updates
        private static void UpdateCraftNumberLabel(string uiText)
        {
            // Update label
            Action action = () => { HeaderLabel.Content = uiText; };
            DispatchActionLabel(HeaderLabel, action);
            // Update progress bar
        }

        public static void UpdateStatus(string text)
        {
            Action action = () => { UpdateLabel.Content = text; };
            DispatchActionLabel(UpdateLabel, action);
        }
        #endregion

        #region Progress Bar Updates
        private static void TimedProgressUpdate(ProgressBar prog, int timeInMilliseconds)
        {
            if (CancelTokenSources.ContainsKey(prog))
            {
                CancelTokenSources[prog].Cancel();
                CancelTokenSources.Remove(prog);
            }

            // Create cancellation token
            CancelTokenSources.Add(prog, new CancellationTokenSource());
            var token = CancelTokenSources[prog].Token;

            Action action = () =>
            {
                var progVal = 0;
                // Set bar to 0
                UpdateProgressBar(prog, progVal);

                // Update progress bar to max value at 20HZ (aka 50ms ticks)
                var tickCount = timeInMilliseconds / TICK_TIME;
                for (int i = 0; i <= tickCount; i++)
                {
                    if (!token.IsCancellationRequested)
                    {
                        double p = (double)i / tickCount;
                        UpdateProgressBar(prog, p);
                        Thread.Sleep(50);
                    } 
                    else
                        break;
                }
                // Clear token from dictionary
                CancelTokenSources.Remove(prog);
            };
            Task.Run(action, token);
        }

        private static void SmoothProgressUpdate(ProgressBar prog, double targetValue)
        {
            if (CancelTokenSources.ContainsKey(prog))
            {
                return;
            }

            // Create cancellation token
            CancelTokenSources.Add(prog, new CancellationTokenSource());
            var token = CancelTokenSources[prog].Token;

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

                // Clear token from dictionary
                CancelTokenSources.Remove(prog);
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

        #region Dispatcher Functions
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
