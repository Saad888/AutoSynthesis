using AutoSynthesis.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace AutoSynthesis
{
    // TODO:
    // Update Readme (especially with images post changes)

    // GLITCHES:
    // 1. Saving a blank profile name actually saves it
    // 2. Try doing only two inputs after food or syrup are consumed? Or hit another cancel? Find out a way around the event cancel
    // 3. Apparently food not going through on another system, invstigate

    // HOW TO UPDATE:
    // Update version info under AssemblyInfo.cs
    // Publish app
    // Upload the exe and any additional files to Azure under the correct folder
    // Update AutoSynthesisUpdate.json with the new version and file locations
    // Verify changes through Dev as well

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Hotkey Properties
        private Dictionary<TextBox, HotkeyContainer> HKTContainers { get; set; }
        private enum HotkeyStates
        {
            UNFOCUSED,
            FOCUSED,
            ACTIVE
        }
        private Dictionary<HotkeyStates, System.Windows.Media.SolidColorBrush> HKTBrushes { get; set; }
        #endregion

        #region Time Properties
        private Dictionary<TextBox, TimeInputContainer> TimerContainers { get; set; }
        private enum TimeStates
        {
            UNFOCUSED,
            FOCUSED
        }
        private Dictionary<TimeStates, SolidColorBrush> TimeBrushes { get; set; }
        private const int DEFAULT_CONSUMABLE_TIMER = 4000;
        private int FoodTimer { get; set; } = 30;
        #endregion

        #region General System Properties
        private enum SystemStates
        {
            IDLE,
            PREPARINGCCRAFT,
            ACTIVECRAFTING,
            COMPLETINGFINALFOOD,
            COMPLETINGFINALCRAFT,
            CANCELLINGCRAFT
        }
        private SystemStates SystemState { get; set; }
        private Dictionary<SystemStates, Style> MainButtonStyles { get; set; }
        private Dictionary<SystemStates, string> MainButtonText { get; set; }
        public Action<Exception> ErrorMessageHandler { get; set; }
        public Action<int, int> GetFoodAndSyrupTimings { get; set; }
        public Action<int> GetCraftCount { get; set; }
        private System.Windows.Forms.NotifyIcon Notify { get; set; }
        private bool NotifyFlagged { get; set; } = true;
        private int StartCraftingDelay { get; set; } = 0;
        private int EndCraftingDelay { get; set; } = 0;
        #endregion

        #region Brush Colors
        private SolidColorBrush DefaultLight { get; set; } = (SolidColorBrush)(new BrushConverter().ConvertFrom("#DCEDFF"));
        private SolidColorBrush DefaultFocused { get; set; } = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFF82"));
        private SolidColorBrush DefaultActive { get; set; } = (SolidColorBrush)(new BrushConverter().ConvertFrom("#DD7373"));
        #endregion

        #region Initiating Methods
        public MainWindow()
        {
            // Verify Updates
            //CheckForUpdates();

            InitializeComponent();
            SetContainerValues();
            SetUIDictionaries();
            SetErrorAction();
            SetupSystemTray();
            SetupFoodAndSyrupTimings();
            SetupCraftCount();
            ReadSettingsFile();

            // Set up UICommunicator
            UICommunicator.ConnectUI(LBLTimerTotal, LBLUpdate, LBLUpdateFooter, LBLTimerCraft, LBLTimerMacro, LBLFoodSyrupTimer, LBLTimerFood,
                 PGBTotal, PGBCraft, PGBMacro, PGBFood);

            // Set system state
            SystemState = SystemStates.IDLE;

            // Load Profiles
            GetProfiles();
            LoadDefaultProfile();
        }

        private void SetupSystemTray()
        {
            Notify = new System.Windows.Forms.NotifyIcon();
            Notify.Icon = new System.Drawing.Icon("Icon.ico");
            Notify.Visible = true;
            Notify.Click +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
        }

        private void SetContainerValues()
        {
            // Attach all hotkey textboxes
            HKTContainers = new Dictionary<TextBox, HotkeyContainer>();
            HKTContainers.Add(TXBMacro1Key, new HotkeyContainer());
            HKTContainers.Add(TXBMacro2Key, new HotkeyContainer());
            HKTContainers.Add(TXBMacro3Key, new HotkeyContainer());
            HKTContainers.Add(TXBFoodKey, new HotkeyContainer());
            HKTContainers.Add(TXBSyrupKey, new HotkeyContainer());
            HKTContainers.Add(TXBConfirmKey, new HotkeyContainer());
            HKTContainers.Add(TXBCancelKey, new HotkeyContainer());

            // Attach all time textboxes
            TimerContainers = new Dictionary<TextBox, TimeInputContainer>();
            TimerContainers.Add(TXBMacro1Timer, new TimeInputContainer());
            TimerContainers.Add(TXBMacro2Timer, new TimeInputContainer());
            TimerContainers.Add(TXBMacro3Timer, new TimeInputContainer());
            TimerContainers.Add(TXBFoodTimer, new TimeInputContainer());
            TimerContainers.Add(TXBSyrupTimer, new TimeInputContainer());
            TimerContainers.Add(TXBCraftCount, new TimeInputContainer(false));
            TimerContainers.Add(TXBFoodCount, new TimeInputContainer(false));
        }

        private void SetUIDictionaries()
        {
            // Set all brush dictionaries
            HKTBrushes = new Dictionary<HotkeyStates, SolidColorBrush>
            {
                { HotkeyStates.UNFOCUSED, DefaultLight },
                { HotkeyStates.FOCUSED, DefaultFocused },
                { HotkeyStates.ACTIVE, DefaultActive }
            };

            TimeBrushes = new Dictionary<TimeStates, SolidColorBrush>
            {
                { TimeStates.UNFOCUSED, DefaultLight },
                { TimeStates.FOCUSED, DefaultFocused }
            };

            MainButtonStyles = new Dictionary<SystemStates, Style>
            {
                { SystemStates.IDLE, Resources["ButtonStyleIdle"] as Style },
                { SystemStates.PREPARINGCCRAFT,Resources["ButtonStyleProcessing"] as Style },
                { SystemStates.ACTIVECRAFTING, Resources["ButtonStyleCrafting"] as Style },
                { SystemStates.COMPLETINGFINALFOOD, Resources["ButtonStyleCrafting"] as Style },
                { SystemStates.COMPLETINGFINALCRAFT, Resources["ButtonStyleCrafting"] as Style },
                { SystemStates.CANCELLINGCRAFT, Resources["ButtonStyleProcessing"] as Style }
            };


            MainButtonText = new Dictionary<SystemStates, string>
            {
                { SystemStates.IDLE, "Start" },
                { SystemStates.PREPARINGCCRAFT, "Preparing..." },
                { SystemStates.ACTIVECRAFTING, "Crafting" },
                { SystemStates.COMPLETINGFINALFOOD, "Last food..." },
                { SystemStates.COMPLETINGFINALCRAFT, "Last craft..." },
                { SystemStates.CANCELLINGCRAFT, "Ending..." }
            };
        }

        private void SetDefaultValues()
        {
            var macro1 = new HotkeyContainer(Key.D1, new HashSet<Key>());
            var macro2 = new HotkeyContainer(Key.D2, new HashSet<Key>());
            var macro3 = new HotkeyContainer(Key.D3, new HashSet<Key>());
            var food = new HotkeyContainer(Key.D4, new HashSet<Key>());
            var syrup = new HotkeyContainer(Key.D5, new HashSet<Key>());
            var confirm = new HotkeyContainer(Key.NumPad0, new HashSet<Key>());
            var cancel = new HotkeyContainer(Key.NumPad1, new HashSet<Key>());

            int macro1timer = 5;
            int macro2timer = 5;
            int macro3timer = 5;

            bool macro2check = false;
            bool macro3check = false;
            bool foodcheck = false;
            bool syrupcheck = false;
            bool collectableCraft = false;
            int foodDuration = 30;

            var profile = new Profile(macro1, macro1timer, macro2, macro2timer, macro2check, macro3, macro3timer,
                          macro3check, food, foodcheck, syrup, syrupcheck, confirm, cancel,
                          collectableCraft, foodDuration);
            SetAllHoykeys(profile);
        }

        private void SetErrorAction()
        {
            ErrorMessageHandler = (Exception e) =>
            {
                MessageBox.Show("An error occurerd! Please see ERROR.txt for more details");
                string profile;
                try
                {
                    Func<string> GetProfileString = () => { return BuildProfileFromCurrent().ToString(); };
                    profile = MainWindowGrid.Dispatcher.Invoke(GetProfileString);
                }
                catch (Exception f)
                {
                    profile = "ERROR WHEN BUILDING PROFILE\n";
                    profile = f.Message;
                }
                Logger.ErrorHandler(e, profile);
            };
        }

        private void SetupFoodAndSyrupTimings()
        {
            GetFoodAndSyrupTimings = (int foodTime, int syrupTime) =>
            {
                foodTime = Math.Max(0, foodTime);
                var FoodTimeContainer = TimerContainers[TXBFoodTimer];
                FoodTimeContainer.Timer = foodTime;
                TXBFoodTimer.Dispatcher.Invoke(() => { TXBFoodTimer.Text = foodTime.ToString(); });

                syrupTime = Math.Max(0, syrupTime);
                var SyrupTimeContainer = TimerContainers[TXBSyrupTimer];
                SyrupTimeContainer.Timer = syrupTime;
                TXBSyrupTimer.Dispatcher.Invoke(() => { TXBSyrupTimer.Text = syrupTime.ToString(); });
            };
        }

        private void SetupCraftCount()
        {
            GetCraftCount = (int craftCount) =>
            {
                craftCount = Math.Max(0, craftCount);
                var CraftCountContainer = TimerContainers[TXBCraftCount];
                CraftCountContainer.Timer = craftCount;
                TXBCraftCount.Dispatcher.Invoke(() => { TXBCraftCount.Text = craftCount.ToString(); });
            };
        }
        #endregion

        #region Crafting Methods
        /// <summary>
        /// Initiates or cancels the crafting system
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BTNCraftInitiate(object sender, RoutedEventArgs e)
        {
            if (SystemState == SystemStates.IDLE)
            {
                SetCraftingStatus(SystemStates.PREPARINGCCRAFT);

                var hotkeys = new Dictionary<HKType, Hotkey>();
                SettingsContainer settings = null;
                HotkeyContainer hkContainer;
                TimeInputContainer timerContainer;

                try
                {
                    // Macro 1
                    hkContainer = HKTContainers[TXBMacro1Key];
                    timerContainer = TimerContainers[TXBMacro1Timer];
                    ValidateHotkeyInputs(hkContainer, timerContainer, "Macro 1");
                    hotkeys.Add(HKType.Macro1, new Hotkey(hkContainer.Keys(), hkContainer.ModKeys(),
                                TXBMacro1Key.Text, timerContainer.Timer * 1000));

                    // Macro 2
                    if ((bool)CHBMacro2.IsChecked)
                    {
                        hkContainer = HKTContainers[TXBMacro2Key];
                        timerContainer = TimerContainers[TXBMacro2Timer];
                        ValidateHotkeyInputs(hkContainer, timerContainer, "Macro 2");
                        hotkeys.Add(HKType.Macro2, new Hotkey(hkContainer.Keys(), hkContainer.ModKeys(),
                                    TXBMacro2Key.Text, timerContainer.Timer * 1000));
                    }
                    else
                        hotkeys.Add(HKType.Macro2, null);

                    // Macro 3
                    if ((bool)CHBMacro3.IsChecked)
                    {
                        hkContainer = HKTContainers[TXBMacro3Key];
                        timerContainer = TimerContainers[TXBMacro3Timer];
                        ValidateHotkeyInputs(hkContainer, timerContainer, "Macro 3");
                        hotkeys.Add(HKType.Macro3, new Hotkey(hkContainer.Keys(), hkContainer.ModKeys(),
                                    TXBMacro3Key.Text, timerContainer.Timer * 1000));
                    }
                    else
                        hotkeys.Add(HKType.Macro3, null);

                    // Food
                    if ((bool)CHBFood.IsChecked)
                    {
                        hkContainer = HKTContainers[TXBFoodKey];
                        timerContainer = TimerContainers[TXBFoodTimer];
                        ValidateHotkeyInputs(hkContainer, timerContainer, "Food");
                        hotkeys.Add(HKType.Food, new Hotkey(hkContainer.Keys(), hkContainer.ModKeys(),
                                    TXBFoodKey.Text, DEFAULT_CONSUMABLE_TIMER));
                    }
                    else
                        hotkeys.Add(HKType.Food, null);

                    // Syrup
                    if ((bool)CHBSyrup.IsChecked)
                    {
                        hkContainer = HKTContainers[TXBSyrupKey];
                        timerContainer = TimerContainers[TXBSyrupTimer];
                        ValidateHotkeyInputs(hkContainer, timerContainer, "Syrup");
                        hotkeys.Add(HKType.Syrup, new Hotkey(hkContainer.Keys(), hkContainer.ModKeys(),
                                    TXBSyrupKey.Text, DEFAULT_CONSUMABLE_TIMER));
                    }
                    else
                        hotkeys.Add(HKType.Syrup, null);

                    // Select/Confirm
                    hkContainer = HKTContainers[TXBConfirmKey];
                    timerContainer = null;
                    ValidateHotkeyInputs(hkContainer, timerContainer, "Confirm");
                    hotkeys.Add(HKType.Confirm, new Hotkey(hkContainer.Keys(), hkContainer.ModKeys(), TXBConfirmKey.Text));

                    // Cancel
                    if ((bool)CHBFood.IsChecked || (bool)CHBSyrup.IsChecked)
                    {
                        hkContainer = HKTContainers[TXBCancelKey];
                        timerContainer = null;
                        ValidateHotkeyInputs(hkContainer, timerContainer, "Cancel");
                        hotkeys.Add(HKType.Cancel, new Hotkey(hkContainer.Keys(), hkContainer.ModKeys(), TXBCancelKey.Text));
                    }
                    else
                        hotkeys.Add(HKType.Cancel, null);

                    // Settings
                    if ((bool)CHBCraftCount.IsChecked && TimerContainers[TXBCraftCount].Timer == 0)
                        throw new InvalidUserParametersException("Craft Count must be greater than 0");
                    var craftCount = (bool)CHBCraftCount.IsChecked ? TimerContainers[TXBCraftCount].Timer : 0;
                    var foodCount = (bool)CHBFoodCount.IsChecked ? TimerContainers[TXBFoodCount].Timer : 0;
                    settings = new SettingsContainer(
                        craftCount,
                        foodCount,
                        (bool)CHBCollectableCraft.IsChecked,
                        FoodTimer,
                        TimerContainers[TXBFoodTimer].Timer,
                        TimerContainers[TXBSyrupTimer].Timer,
                        StartCraftingDelay * 1000,
                        EndCraftingDelay * 1000
                    );
                }
                catch (InvalidUserParametersException error)
                {
                    MessageBox.Show(error.Message);
                    SetCraftingStatus(SystemStates.IDLE);
                    return;
                }

                // Create ending action
                Action action = () =>
                {
                    BTNCraft.Dispatcher.Invoke(() => { SetCraftingStatus(SystemStates.IDLE); });
                };
                try
                {
                    CraftingEngine.InitiateCraftingEngine(hotkeys, settings, action, ErrorMessageHandler, GetFoodAndSyrupTimings, GetCraftCount);
                    SetCraftingStatus(SystemStates.ACTIVECRAFTING);
                }
                catch (ProcessMissingException)
                {
                    MessageBox.Show("FFXIV Was Not Detected. Please ensure the game is running.");
                    SetCraftingStatus(SystemStates.IDLE);
                    return;
                }
                catch (Exception error)
                {
                    ErrorMessageHandler(error);
                    SetCraftingStatus(SystemStates.IDLE);
                    return;
                }

            }
            else if (SystemState == SystemStates.ACTIVECRAFTING)
            {
                // Cancel the craft
                SetCraftingStatus(SystemStates.CANCELLINGCRAFT);
                if ((bool)CHBFood.IsChecked)
                {
                    CraftingEngine.CancelAfterFood();
                    SetCraftingStatus(SystemStates.COMPLETINGFINALFOOD);
                }
                else
                {
                    CraftingEngine.CancelCrafting();
                    SetCraftingStatus(SystemStates.COMPLETINGFINALCRAFT);
                }
            }
            else if (SystemState == SystemStates.COMPLETINGFINALFOOD)
            {
                CraftingEngine.CancelCrafting();
                SetCraftingStatus(SystemStates.COMPLETINGFINALCRAFT);
            }
            else if (SystemState == SystemStates.COMPLETINGFINALCRAFT)
            {
                // Cancel the craft
                SetCraftingStatus(SystemStates.CANCELLINGCRAFT);
                CraftingEngine.CancelCrafting();
            }
        }

        /// <summary>
        /// Verifies if the hotkeys are inputted correctly
        /// </summary>
        /// <param name="hotkeyContainer"></param>
        /// <returns>True if no issues</returns>
        private void ValidateHotkeyInputs(HotkeyContainer hotkeyContainer, TimeInputContainer timeInput, string source)
        {
            if (hotkeyContainer.LastPressedKey == Key.None)
            {
                throw new InvalidUserParametersException(source + " Hotkey not set correctly.");
            }
            if (timeInput != null && timeInput.Timer == 0)
            {
                throw new InvalidUserParametersException(source + " Timer must be higher than 0.");
            }
        }
        #endregion

        #region Hotkey Methods
        /// <summary>
        /// Activates the hotkey for user inputs, accepts user inputs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotkeyKeyDown(object sender, KeyEventArgs e)
        {
            // If key state is not accepting new inputs, do nothing UNLESS its a SPACE or ENTER
            //      Then set the key state to accept inputs 
            // If key state is accepting inputs, process the inputs

            try
            {
                // get textbox and state information
                var textBox = (TextBox)sender;
                var hotkeyContainer = HKTContainers[textBox];
                var key = e.Key == Key.System ? e.SystemKey : e.Key;

                // check state
                if (hotkeyContainer.AcceptingInputs)
                {
                    // add if modifier, send if not
                    switch (key)
                    {
                        case Key.LeftShift:
                        case Key.RightShift:
                        case Key.LeftCtrl:
                        case Key.RightCtrl:
                        case Key.LeftAlt:
                        case Key.RightAlt:
                            hotkeyContainer.ActiveModKeys.Add(key);
                            if (!HotkeyProcessor.NumpadKey(hotkeyContainer.LastPressedKey))  //Numpads do not behave well with modifiers
                                textBox.Text = HotkeyProcessor.GetKeyInputText(hotkeyContainer.LastPressedKey, hotkeyContainer.ActiveModKeys);
                            break;
                        default:
                            hotkeyContainer.ActiveNonModKeys.Add(key);
                            hotkeyContainer.LastPressedKey = key;
                            textBox.Text = HotkeyProcessor.GetKeyInputText(key, hotkeyContainer.ActiveModKeys);
                            break;
                    }
                }
                else
                {
                    // if ENTER or SPACE, then start accepting inputs
                    if (key == Key.Enter || key == Key.Space)
                        ActivateHotkeyTextbox(textBox);
                }
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }


        private void HotkeyInputDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ActivateHotkeyTextbox((TextBox)sender);
        }

        /// <summary>
        /// Processes key up events for hotkeys
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotkeyKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                // if not accepting inputs, do nothing
                // if accepting inputs, check if the last non-mod key has been lifted. If that happens, stop accepting inputs
                var textBox = (TextBox)sender;
                var hotkeyContainer = HKTContainers[textBox];
                var key = e.Key == Key.System ? e.SystemKey : e.Key;

                if (!hotkeyContainer.AcceptingInputs)
                    return;

                if (hotkeyContainer.ActiveNonModKeys.Contains(key))
                {
                    hotkeyContainer.ActiveNonModKeys.Remove(key);
                    if (key == hotkeyContainer.LastPressedKey)
                    {
                        DeactivateHotkeyTextbox(textBox);
                    }
                }
                if (hotkeyContainer.ActiveModKeys.Contains(key))
                {
                    // glitch: shift release event doesnt fire if both shifts are being pressed and only one is released
                    hotkeyContainer.ActiveModKeys.Remove(key);
                    textBox.Text = HotkeyProcessor.GetKeyInputText(hotkeyContainer.LastPressedKey, hotkeyContainer.ActiveModKeys);
                }
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }

        /// <summary>
        /// Activates hotkey textbox, sets parameters and display
        /// </summary>
        /// <param name="txb"></param>
        private void ActivateHotkeyTextbox(TextBox txb)
        {
            try
            {
                var key = HKTContainers[txb].LastPressedKey;
                var modKeys = HKTContainers[txb].ActiveModKeys;
                HKTContainers[txb] = new HotkeyContainer();
                HKTContainers[txb].AcceptingInputs = true;
                HKTContainers[txb].LastSetKey = key;
                HKTContainers[txb].LastSetModKeys = modKeys;
                txb.Background = HKTBrushes[HotkeyStates.ACTIVE];
                txb.Text = "Enter Keybind...";

                DisableTabbing();
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }

        /// <summary>
        /// Deactivate hotkey 
        /// </summary>
        /// <param name="txb"></param>
        private void DeactivateHotkeyTextbox(TextBox txb)
        {
            try
            {
                if (HKTContainers[txb].AcceptingInputs)
                {
                    HKTContainers[txb].AcceptingInputs = false;
                    txb.Background = HKTBrushes[HotkeyStates.FOCUSED];
                    if (HKTContainers[txb].LastPressedKey == Key.None)
                    {
                        HKTContainers[txb].LastPressedKey = HKTContainers[txb].LastSetKey;
                        HKTContainers[txb].ActiveModKeys = HKTContainers[txb].LastSetModKeys;
                    }
                    txb.Text = HotkeyProcessor.GetKeyInputText(HKTContainers[txb].LastPressedKey, HKTContainers[txb].ActiveModKeys);
                    EnableTabbing();
                }
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }

        /// <summary>
        /// Processes hotkey taking focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotkeyTextboxGainFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var txb = (TextBox)sender;
                txb.Background = HKTBrushes[HotkeyStates.FOCUSED];
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }

        /// <summary>
        /// Processes hotkey losing focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotkeyTextboxLoseFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var txb = (TextBox)sender;
                DeactivateHotkeyTextbox(txb);
                txb.Background = HKTBrushes[HotkeyStates.UNFOCUSED];
                EnableTabbing();
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }
        #endregion

        #region Timeline Methods
        // TAKE FOCUS: Change to focused color
        // LOSE FOCUS: Change to normal color
        // KEY IN: Only accept numbers, up to 2 digits. When taking focus, get ready to reset the number

        /// <summary>
        /// Processes time textbox inputs for numbers
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetTimeValue(object sender, KeyEventArgs e)
        {
            try
            {
                var tbx = (TextBox)sender;
                var container = TimerContainers[tbx];
                int keyValue = 0;
                if (TimeInputProcessor.TryGetNumber(e.Key, ref keyValue))
                {
                    var currentTime = container.Timer;
                    // if freshly focused, reset the timer on input
                    if (container.FreshFocus)
                        currentTime = 0;

                    int newTime;
                    if (container.Limit)
                    {
                        newTime = (currentTime % 10) * 10 + keyValue;
                    }
                    else
                    {
                        newTime = (currentTime) * 10 + keyValue;
                        newTime %= 1000;
                    }
                    container.Timer = newTime;
                    tbx.Text = newTime.ToString();
                    container.FreshFocus = false;
                }
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }

        /// <summary>
        /// Handles gaining focus for time textboxes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeGetFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var tbx = (TextBox)sender;
                var container = TimerContainers[tbx];
                // Set fresh focus to true
                container.FreshFocus = true;
                // Modify display
                tbx.Background = TimeBrushes[TimeStates.FOCUSED];
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }

        /// <summary>
        /// Handles losing focus 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var tbx = (TextBox)sender;
                // Modify display
                tbx.Background = TimeBrushes[TimeStates.UNFOCUSED];
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }
        #endregion

        #region Settings Methods
        private void RDFood30_Checked(object sender, RoutedEventArgs e)
        {
            FoodTimer = 30;
        }

        private void RDFood40_Checked(object sender, RoutedEventArgs e)
        {
            FoodTimer = 40;
        }

        private void RDFood45_Checked(object sender, RoutedEventArgs e)
        {
            FoodTimer = 45;
        }


        #endregion

        #region Set Presets Methods
        private void SetAllHoykeys(Profile profile)
        {
            try
            {
                // Set Macro 1
                SetHotkeyState(TXBMacro1Key, profile.Macro1);
                SetTimeState(TXBMacro1Timer, profile.Macro1Time);
                // Set Macro 2
                SetHotkeyState(TXBMacro2Key, profile.Macro2);
                SetTimeState(TXBMacro2Timer, profile.Macro2Time);
                CHBMacro2.IsChecked = profile.Macro2Check;
                // Set Macro 3
                SetHotkeyState(TXBMacro3Key, profile.Macro3);
                SetTimeState(TXBMacro3Timer, profile.Macro3Time);
                CHBMacro3.IsChecked = profile.Macro3Check;
                // Set Food
                SetHotkeyState(TXBFoodKey, profile.Food);
                CHBFood.IsChecked = profile.FoodCheck;
                // Set Syrup
                SetHotkeyState(TXBSyrupKey, profile.Syrup);
                CHBSyrup.IsChecked = profile.SyrupCheck;
                // Set Select
                SetHotkeyState(TXBConfirmKey, profile.Select);
                // Set Cancel
                SetHotkeyState(TXBCancelKey, profile.Cancel);
                // Settings
                CHBCollectableCraft.IsChecked = profile.Collectable;
                if (profile.FoodDuration == 30)
                    RDFood30.IsChecked = true;
                else if (profile.FoodDuration == 40)
                    RDFood40.IsChecked = true;
                else
                    RDFood45.IsChecked = true;
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }

        /// <summary>
        /// Sets the hotkey textbox and internal state for a single hotkey
        /// </summary>
        /// <param name="hotkeyBox"></param>
        /// <param name="pressedKey"></param>
        /// <param name="pressedModKeys"></param>
        private void SetHotkeyState(TextBox hotkeyBox, HotkeyContainer container)
        {
            try
            {
                HKTContainers[hotkeyBox] = container;
                if (container != null)
                    hotkeyBox.Text = HotkeyProcessor.GetKeyInputText(container.LastPressedKey, container.ActiveModKeys);
                else
                    hotkeyBox.Text = "";
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }

        /// <summary>
        /// Sets the time value and internal state for a single timebox
        /// </summary>
        /// <param name="timeBox"></param>
        /// <param name="timer"></param>
        private void SetTimeState(TextBox timeBox, int timer)
        {
            try
            {
                var container = TimerContainers[timeBox];
                timer = Math.Max(0, Math.Min(timer, 99));
                container.Timer = timer;
                timeBox.Text = timer.ToString();
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }
        #endregion

        #region Setting Crafting Button States
        private void SetCraftingStatus(SystemStates state)
        {
            try
            {
                SystemState = state;
                BTNCraft.Style = MainButtonStyles[SystemState];
                BTNCraft.Content = MainButtonText[SystemState];
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }
        #endregion

        #region Full Winodws Methods
        private void DisableTabbing()
        {
            try
            {
                KeyboardNavigation.SetTabNavigation(MainWindowGrid, KeyboardNavigationMode.None);
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }
        private void EnableTabbing()
        {
            try
            {
                KeyboardNavigation.SetTabNavigation(MainWindowGrid, KeyboardNavigationMode.Continue);
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }
        #endregion

        #region Save and Load Profile System
        private void GetProfiles()
        {
            try
            {
                var profileNameList = ProfileManager.GetProfilesList();
                CMBProfileList.ItemsSource = profileNameList;
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }

        private void LoadProfile(string name)
        {
            try
            {
                if (String.IsNullOrEmpty(name))
                    return;

                var profileToLoad = ProfileManager.LoadProfile(name);
                if (profileToLoad == null)
                    return;

                SetAllHoykeys(profileToLoad);
                CMBProfileList.SelectedItem = name;
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }

        private void LoadDefaultProfile()
        {
            try
            {
                if (ProfileManager.VerifyDefaultProfile())
                    LoadProfile(ProfileManager.DefaultProfile);
                else
                    SetDefaultValues();
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }

        private void BTNSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var profileName = CMBProfileList.SelectedItem == null ? "" : CMBProfileList.SelectedItem.ToString();
                var saveDialog = new SaveDialog(profileName);
                saveDialog.Owner = Application.Current.MainWindow;
                if (saveDialog.ShowDialog() == true)
                {
                    var name = saveDialog.SaveName;
                    var newProfile = BuildProfileFromCurrent();
                    ProfileManager.SaveProfile(name, newProfile);
                    GetProfiles();
                    CMBProfileList.SelectedItem = name;
                }
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }

        private void BTNLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CMBProfileList.SelectedItem == null)
                    return;
                var profileToLoad = CMBProfileList.SelectedItem.ToString();
                LoadProfile(profileToLoad);
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }

        private void BTNDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentIndex = CMBProfileList.SelectedIndex;
                if (currentIndex == -1)
                    return;
                var profileToDelete = CMBProfileList.SelectedItem.ToString();
                ProfileManager.DeleteProfile(profileToDelete);
                GetProfiles();
                CMBProfileList.SelectedIndex = Math.Min(currentIndex, CMBProfileList.Items.Count - 1);
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }

        private Profile BuildProfileFromCurrent()
        {
            try
            {
                var profile = new Profile(
                    HKTContainers[TXBMacro1Key], TimerContainers[TXBMacro1Timer].Timer,
                    HKTContainers[TXBMacro2Key], TimerContainers[TXBMacro2Timer].Timer, (bool)CHBMacro2.IsChecked,
                    HKTContainers[TXBMacro3Key], TimerContainers[TXBMacro3Timer].Timer, (bool)CHBMacro3.IsChecked,
                    HKTContainers[TXBFoodKey], (bool)CHBFood.IsChecked,
                    HKTContainers[TXBSyrupKey], (bool)CHBSyrup.IsChecked,
                    HKTContainers[TXBConfirmKey], HKTContainers[TXBCancelKey],
                    (bool)CHBCollectableCraft.IsChecked, FoodTimer
                );
                return profile;
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
            return null;
        }
        #endregion

        #region Checkbox Enable/Disables
        private void CHBMacro2_Checked(object sender, RoutedEventArgs e)
        {
            bool check = (bool)CHBMacro2.IsChecked;
            TXBMacro2Key.IsEnabled = check;
            TXBMacro2Timer.IsEnabled = check;
        }

        private void CHBMacro3_Checked(object sender, RoutedEventArgs e)
        {
            bool check = (bool)CHBMacro3.IsChecked;
            TXBMacro3Key.IsEnabled = check;
            TXBMacro3Timer.IsEnabled = check;
        }

        private void CHBFood_Checked(object sender, RoutedEventArgs e)
        {
            bool foodcheck = (bool)CHBFood.IsChecked;
            bool syrupcheck = (bool)CHBSyrup.IsChecked;
            TXBFoodKey.IsEnabled = foodcheck;
            TXBFoodTimer.IsEnabled = foodcheck;
            TXBCancelKey.IsEnabled = foodcheck || syrupcheck;
        }

        private void CHBSyrup_Checked(object sender, RoutedEventArgs e)
        {
            bool foodcheck = (bool)CHBFood.IsChecked;
            bool syrupcheck = (bool)CHBSyrup.IsChecked;
            TXBSyrupKey.IsEnabled = syrupcheck;
            TXBSyrupTimer.IsEnabled = syrupcheck;
            TXBCancelKey.IsEnabled = foodcheck || syrupcheck;
        }

        private void CHBCraftCount_Checked(object sender, RoutedEventArgs e)
        {
            bool check = (bool)CHBCraftCount.IsChecked;
            TXBCraftCount.IsEnabled = check;
        }

        private void CHBFoodCount_Checked(object sender, RoutedEventArgs e)
        {
            bool check = (bool)CHBFoodCount.IsChecked;
            TXBFoodCount.IsEnabled = check;
        }
        #endregion

        #region Misc Functions
        private void InputTextChange(object sender, TextChangedEventArgs e)
        {
            var txtbox = (TextBox)sender;
            if (txtbox.Text.Length >= 20)
                txtbox.FontSize = 12;
            else
                txtbox.FontSize = 16;
        }

        private void CheckFocusGain(object sender, RoutedEventArgs e)
        {
            var ckbbox = (CheckBox)sender;
            ckbbox.Background = DefaultFocused;
        }

        private void CheckFocusLoss(object sender, RoutedEventArgs e)
        {
            var ckbbox = (CheckBox)sender;
            ckbbox.Background = DefaultLight;
        }

        private void CheckRadioGain(object sender, RoutedEventArgs e)
        {
            var ckbbox = (RadioButton)sender;
            ckbbox.Background = DefaultFocused;
        }

        private void CheckRadioLoss(object sender, RoutedEventArgs e)
        {
            var ckbbox = (RadioButton)sender;
            ckbbox.Background = DefaultLight;
        }
        #endregion

        #region Help Window
        private string URL = "https://github.com/Saad888/AutoSynthesis/blob/master/README.md";
        private void LBLHelp_MouseEnter(object sender, MouseEventArgs e)
        {
            var image = (Image)sender;
            image.Source = new BitmapImage(new Uri("Resources/Images/Background/InfoHighlighted.png", UriKind.Relative));
        }

        private void LBLHelp_MouseLeave(object sender, MouseEventArgs e)
        {
            var image = (Image)sender;
            image.Source = new BitmapImage(new Uri("Resources/Images/Background/InfoUnhighlighted.png", UriKind.Relative));
        }

        private void LBLHelp_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var image = (Image)sender;
            image.Source = new BitmapImage(new Uri("Resources/Images/Background/InfoPressed.png", UriKind.Relative));
        }

        private void LBLHelp_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var image = (Image)sender;
            image.Source = new BitmapImage(new Uri("Resources/Images/Background/InfoHighlighted.png", UriKind.Relative));
            System.Diagnostics.Process.Start(URL);
        }
        #endregion

        #region Minimize To Tray
        private void MinimizeLabelMouseEnter(object sender, MouseEventArgs e)
        {
            var image = (Image)sender;
            image.Source = new BitmapImage(new Uri("Resources/Images/Buttons/Minimize to Tray Hover.png", UriKind.Relative));
        }

        private void MinimizeLabelMouseExit(object sender, MouseEventArgs e)
        {
            var image = (Image)sender;
            image.Source = new BitmapImage(new Uri("Resources/Images/Buttons/Minimize to Tray.png", UriKind.Relative));
        }

        private void MinimizeLabelMouseDown(object sender, MouseButtonEventArgs e)
        {
            var image = (Image)sender;
            image.Source = new BitmapImage(new Uri("Resources/Images/Buttons/Minimize to Tray Pressed.png", UriKind.Relative));
        }

        private void MinimizeLabelMouseUp(object sender, MouseButtonEventArgs e)
        {
            var image = (Image)sender;
            image.Source = new BitmapImage(new Uri("Resources/Images/Buttons/Minimize to Tray Hover.png", UriKind.Relative));
            Hide();
            if (NotifyFlagged)
            {
                NotifyFlagged = false;
                Notify.ShowBalloonTip(3000, "", "AutoSynthesis is still running in the system tray.", System.Windows.Forms.ToolTipIcon.None);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Notify.Icon.Dispose();
            Notify.Dispose();
        }
        #endregion

        #region Always On Top
        private static bool AlwaysOnTopEnabled { get; set; } = false;
        private static string SettingsFileDirectory { get; set; }
        private void Window_Deactivated(object sender, EventArgs e)
        {
            Window window = (Window)sender;

            if (AlwaysOnTopEnabled)
            {
                window.Topmost = true;
            }
            else
            {
                window.Topmost = false;
            }
        }
        private void AlwaysOnTopLabelMouseDown(object sender, MouseButtonEventArgs e)
        {
            var image = (Image)sender;
            var resourceUrl = AlwaysOnTopEnabled ? "Resources/Images/Buttons/AlwaysOnTopPressedOn.png" : "Resources/Images/Buttons/AlwaysOnTopPressedOff.png";
            image.Source = new BitmapImage(new Uri(resourceUrl, UriKind.Relative));
        }

        private void AlwaysOnTopLabelMouseEnter(object sender, MouseEventArgs e)
        {
            var image = (Image)sender;
            var resourceUrl = AlwaysOnTopEnabled ? "Resources/Images/Buttons/AlwaysOnTopHoverOn.png" : "Resources/Images/Buttons/AlwaysOnTopHoverOff.png";
            image.Source = new BitmapImage(new Uri(resourceUrl, UriKind.Relative));
        }

        private void AlwaysOnTopLabelMouseExit(object sender, MouseEventArgs e)
        {
            var image = (Image)sender;
            var resourceUrl = AlwaysOnTopEnabled ? "Resources/Images/Buttons/AlwaysOnTopOn.png" : "Resources/Images/Buttons/AlwaysOnTopOff.png";
            image.Source = new BitmapImage(new Uri(resourceUrl, UriKind.Relative));
        }

        private void AlwaysOnTopLabelMouseUp(object sender, MouseButtonEventArgs e)
        {
            FlipAlwaysOnTop();
            var image = (Image)sender;
            var resourceUrl = AlwaysOnTopEnabled ? "Resources/Images/Buttons/AlwaysOnTopHoverOn.png" : "Resources/Images/Buttons/AlwaysOnTopHoverOff.png";
            image.Source = new BitmapImage(new Uri(resourceUrl, UriKind.Relative));
        }

        private void FlipAlwaysOnTop()
        {
            AlwaysOnTopEnabled = !AlwaysOnTopEnabled;
            WriteSaveSettings();
        }
        #endregion

        #region Update Verify
        private string UpdateMetadataURL = "https://autosynthesis.blob.core.windows.net/autosynthesis/Releases/AutoSynthesisUpdate.json";
        private void CheckForUpdates()
        {
            try
            {
                // If debugger is attached, ignore
                if (Debugger.IsAttached)
                    return;

                // If environmental variable exists, ignore
                var args = Environment.GetCommandLineArgs();
                if (args.Contains("-isupdated"))
                    return;

                // Download update metadata json file
                string contents;
                using (var webClient = new WebClient())
                    contents = webClient.DownloadString(UpdateMetadataURL);
                var updateMetadata = JsonConvert.DeserializeObject<UpdateMetadata>(contents);

                // Check system version
                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                string version = fileVersionInfo.ProductVersion;

                // If version mismatch, launch updater
                if (version == updateMetadata.UpdateVersion)
                    return;

                // Check user input 
                MessageBoxResult result = MessageBox.Show("A new version is available for AutoSynthesis, would you like to update?",
                                                          "Update Available", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No || result == MessageBoxResult.Cancel)
                    return;

                string path = Assembly.GetExecutingAssembly().CodeBase;
                var directory = System.IO.Path.GetDirectoryName(path).Replace(@"file:\", "") + @"\AutoSynthesis Updater.exe";
                Process.Start(directory);
                Application.Current.Shutdown();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }


        }

        #endregion

        #region Settings
        private void SettingsEnter(object sender, MouseEventArgs e)
        {
            var image = (Image)sender;
            var resourceUrl = "Resources/Images/Buttons/settings-hover.png";
            image.Source = new BitmapImage(new Uri(resourceUrl, UriKind.Relative));
        }

        private void SettingsExit(object sender, MouseEventArgs e)
        {
            var image = (Image)sender;
            var resourceUrl = "Resources/Images/Buttons/settings.png";
            image.Source = new BitmapImage(new Uri(resourceUrl, UriKind.Relative));
        }

        private void OpenSettings(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var settingsDialogue = new Settings(StartCraftingDelay.ToString(), EndCraftingDelay.ToString());
                settingsDialogue.Owner = Application.Current.MainWindow;
                if (settingsDialogue.ShowDialog() == true)
                {
                    StartCraftingDelay = Convert.ToInt32(settingsDialogue.StartDelay);
                    EndCraftingDelay = Convert.ToInt32(settingsDialogue.EndDelay);
                }
                WriteSaveSettings();
            }
            catch (Exception error)
            {
                ErrorMessageHandler(error);
            }
        }
        #endregion

        #region Settings Saving
        private void WriteSaveSettings()
        {
            try
            {
                var text = AlwaysOnTopEnabled.ToString() + "|" + StartCraftingDelay.ToString() + "|" + EndCraftingDelay.ToString();
                File.WriteAllText(SettingsFileDirectory, text);
            }
            catch
            {

            }
        }


        private void ReadSettingsFile()
        {
            string path = Assembly.GetExecutingAssembly().CodeBase;
            SettingsFileDirectory = Path.GetDirectoryName(path).Replace(@"file:\", "") + @"\Settings.txt";
            try
            {
                var fileResult = File.ReadAllText(SettingsFileDirectory).Split('|');
                AlwaysOnTopEnabled = Convert.ToBoolean(fileResult[0]);
                StartCraftingDelay = Convert.ToInt32(fileResult[1]);
                EndCraftingDelay = Convert.ToInt32(fileResult[2]);
            }
            catch
            {
                AlwaysOnTopEnabled = false;
                StartCraftingDelay = 0;
                EndCraftingDelay = 0;
            }
            var resourceUrl = AlwaysOnTopEnabled ? "Resources/Images/Buttons/AlwaysOnTopOn.png" : "Resources/Images/Buttons/AlwaysOnTopOff.png";
            AlwaysOnTop_Label.Source = new BitmapImage(new Uri(resourceUrl, UriKind.Relative));
        }
        #endregion
    }
}
