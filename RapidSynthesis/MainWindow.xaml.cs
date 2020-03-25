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
using RapidSynthesis.Windows;

namespace RapidSynthesis
{
    // keycodes being eaten
    // https://stackoverflow.com/questions/1458748/wpf-onkeydown-not-being-called-for-space-key-in-control-derived-from-wpf-text
    // brush colors:
    // https://stackoverflow.com/questions/979876/set-background-color-of-wpf-textbox-in-c-sharp-code

    // GLITCHES:
    // Macro progress bar keeps running after a craft is ended if you end a craft as the next one starts


    // TO DO: 
    // Check if the tab sorting is correct
    // See ProcessManager for ToDo's
    // Implement a proper logger


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
            CANCELLINGCRAFT
        }
        private SystemStates SystemState { get; set; }
        private Dictionary<SystemStates, SolidColorBrush> MainButtonBrushes { get; set; }
        #endregion

        #region Initiating Methods
        public MainWindow()
        {
            InitializeComponent();
            SetContainerValues();
            SetBrushValues();

            // Set up UICommunicator
            UICommunicator.ConnectUI(LBLCraftNumber, LBLUpdate, PGBOverall, PGBCraft, PGBMacro);

            // Set system state
            SystemState = SystemStates.IDLE;

            // Load Profiles
            GetProfiles();
            LoadDefaultProfile();
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
            TimerContainers.Add(TXBCraftCount, new TimeInputContainer());
        }

        private void SetBrushValues()
        {
            // Set all brush dictionaries
            HKTBrushes = new Dictionary<HotkeyStates, SolidColorBrush>();
            HKTBrushes.Add(HotkeyStates.UNFOCUSED, Brushes.White);
            HKTBrushes.Add(HotkeyStates.FOCUSED, Brushes.Yellow);
            HKTBrushes.Add(HotkeyStates.ACTIVE, Brushes.Red);

            TimeBrushes = new Dictionary<TimeStates, SolidColorBrush>();
            TimeBrushes.Add(TimeStates.UNFOCUSED, Brushes.White);
            TimeBrushes.Add(TimeStates.FOCUSED, Brushes.Yellow);

            MainButtonBrushes = new Dictionary<SystemStates, SolidColorBrush>();
            MainButtonBrushes.Add(SystemStates.IDLE, Brushes.White);
            MainButtonBrushes.Add(SystemStates.PREPARINGCCRAFT, Brushes.White);
            MainButtonBrushes.Add(SystemStates.ACTIVECRAFTING, Brushes.Green);
            MainButtonBrushes.Add(SystemStates.CANCELLINGCRAFT, Brushes.Red);
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
            bool thirtyMinCraft = true;

            var profile = new Profile(macro1, macro1timer, macro2, macro2timer, macro2check, macro3, macro3timer, 
                          macro3check, food, foodcheck, syrup, syrupcheck, confirm, cancel, 
                          collectableCraft, thirtyMinCraft);
            SetAllHoykeys(profile);
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
                    hotkeys.Add(HKType.Macro1, new Hotkey(hkContainer.Keys(), hkContainer.ModKeys(), timerContainer.Timer * 1000));

                    // Macro 2
                    if ((bool)CHBMacro2.IsChecked)
                    {
                        hkContainer = HKTContainers[TXBMacro2Key];
                        timerContainer = TimerContainers[TXBMacro2Timer];
                        ValidateHotkeyInputs(hkContainer, timerContainer, "Macro 2");
                        hotkeys.Add(HKType.Macro2, new Hotkey(hkContainer.Keys(), hkContainer.ModKeys(), timerContainer.Timer * 1000));
                    }
                    else
                        hotkeys.Add(HKType.Macro2, null);

                    // Macro 3
                    if ((bool)CHBMacro3.IsChecked)
                    {
                        hkContainer = HKTContainers[TXBMacro3Key];
                        timerContainer = TimerContainers[TXBMacro3Timer];
                        ValidateHotkeyInputs(hkContainer, timerContainer, "Macro 3");
                        hotkeys.Add(HKType.Macro3, new Hotkey(hkContainer.Keys(), hkContainer.ModKeys(), timerContainer.Timer * 1000));
                    }
                    else
                        hotkeys.Add(HKType.Macro3, null);

                    // Food
                    if ((bool)CHBFood.IsChecked)
                    {
                        hkContainer = HKTContainers[TXBFoodKey];
                        timerContainer = TimerContainers[TXBFoodTimer];
                        ValidateHotkeyInputs(hkContainer, timerContainer, "Food");
                        hotkeys.Add(HKType.Food, new Hotkey(hkContainer.Keys(), hkContainer.ModKeys(), DEFAULT_CONSUMABLE_TIMER));
                    }
                    else
                        hotkeys.Add(HKType.Food, null);

                    // Syrup
                    if ((bool)CHBSyrup.IsChecked)
                    {
                        hkContainer = HKTContainers[TXBSyrupKey];
                        timerContainer = TimerContainers[TXBSyrupTimer];
                        ValidateHotkeyInputs(hkContainer, timerContainer, "Syrup");
                        hotkeys.Add(HKType.Syrup, new Hotkey(hkContainer.Keys(), hkContainer.ModKeys(), DEFAULT_CONSUMABLE_TIMER));
                    }
                    else
                        hotkeys.Add(HKType.Syrup, null);

                    // Select/Confirm
                    hkContainer = HKTContainers[TXBConfirmKey];
                    timerContainer = null;
                    ValidateHotkeyInputs(hkContainer, timerContainer, "Confirm");
                    hotkeys.Add(HKType.Confirm, new Hotkey(hkContainer.Keys(), hkContainer.ModKeys()));

                    // Cancel
                    if ((bool)CHBFood.IsChecked || (bool)CHBSyrup.IsChecked)
                    {
                        hkContainer = HKTContainers[TXBCancelKey];
                        timerContainer = null;
                        ValidateHotkeyInputs(hkContainer, timerContainer, "Cancel");
                        hotkeys.Add(HKType.Cancel, new Hotkey(hkContainer.Keys(), hkContainer.ModKeys()));
                    }
                    else
                        hotkeys.Add(HKType.Cancel, null);

                    // Settings
                    var craftCount = (bool)CHBCraftCount.IsChecked ? TimerContainers[TXBCraftCount].Timer : 0;
                    settings = new SettingsContainer(
                        craftCount,
                        (bool)CHBCollectableCraft.IsChecked,
                        FoodTimer == 30,
                        TimerContainers[TXBFoodTimer].Timer,
                        TimerContainers[TXBSyrupTimer].Timer
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
                    ButtonTest.Dispatcher.Invoke(() => { SetCraftingStatus(SystemStates.IDLE); });
                };
                try
                {
                    CraftingEngine.InitiateCraftingEngine(hotkeys, settings, action);
                    SetCraftingStatus(SystemStates.ACTIVECRAFTING);
                }
                catch (ProcessMissingException)
                {
                    MessageBox.Show("FFXIV Was Not Detected! Please ensure the game is running.");
                    SetCraftingStatus(SystemStates.IDLE);
                    return;
                }

            }
            else if (SystemState == SystemStates.ACTIVECRAFTING)
            {
                // Cancel the craft
                SetCraftingStatus(SystemStates.CANCELLINGCRAFT);
                CraftingEngine.CancelCrafting();
                SetCraftingStatus(SystemStates.IDLE);
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
                throw new InvalidUserParametersException(source + " Hotkey not set correctly");
            }
            if (timeInput != null && timeInput.Timer == 0)
            {
                throw new InvalidUserParametersException(source + " Timer is set to 0");
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

            // get textbox and state information
            var textBox = (TextBox)sender;
            var hotkeyContainer = HKTContainers[textBox];
            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            // check state
            if (hotkeyContainer.AcceptingInputs)
            {
                // add if modifier, send if not
                switch(key)
                {
                    case Key.LeftShift:
                    case Key.RightShift:
                    case Key.LeftCtrl:
                    case Key.RightCtrl:
                    case Key.LeftAlt:
                    case Key.RightAlt:
                        if (!HotkeyProcessor.NumpadKey(hotkeyContainer.LastPressedKey))  //Numpads do not behave well with modifiers
                            if (hotkeyContainer.LastPressedKey != Key.None)  // Update visuals if key is currently stored
                                textBox.Text = HotkeyProcessor.GetKeyInputText(hotkeyContainer.LastPressedKey, hotkeyContainer.ActiveModKeys);
                        hotkeyContainer.ActiveModKeys.Add(key);
                        break;
                    default:
                        hotkeyContainer.ActiveNonModKeys.Add(key);
                        hotkeyContainer.LastPressedKey = key;
                        textBox.Text = HotkeyProcessor.GetKeyInputText(key, hotkeyContainer.ActiveModKeys);
                        break;
                }
            } else
            {
                // if ENTER or SPACE, then start accepting inputs
                if (key == Key.Enter || key == Key.Space)
                    ActivateHotkeyTextbox(textBox);
            }
        }

        /// <summary>
        /// Processes key up events for hotkeys
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotkeyKeyUp(object sender, KeyEventArgs e)
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
                if (hotkeyContainer.LastPressedKey != Key.None)
                {
                    textBox.Text = HotkeyProcessor.GetKeyInputText(hotkeyContainer.LastPressedKey, hotkeyContainer.ActiveModKeys);
                }
            }
        }

        /// <summary>
        /// Activates hotkey textbox, sets parameters and display
        /// </summary>
        /// <param name="txb"></param>
        private void ActivateHotkeyTextbox(TextBox txb)
        {
            HKTContainers[txb] = new HotkeyContainer();
            HKTContainers[txb].AcceptingInputs = true;
            txb.Background = HKTBrushes[HotkeyStates.ACTIVE];
            txb.Text = "Enter Keybind...";

            DisableTabbing();
        }

        /// <summary>
        /// Deactivate hotkey 
        /// </summary>
        /// <param name="txb"></param>
        private void DeactivateHotkeyTextbox(TextBox txb)
        {
            HKTContainers[txb].AcceptingInputs = false;
            txb.Background = HKTBrushes[HotkeyStates.FOCUSED];
            txb.Text = HotkeyProcessor.GetKeyInputText(HKTContainers[txb].LastPressedKey, HKTContainers[txb].ActiveModKeys);
            EnableTabbing();
        }

        /// <summary>
        /// Processes hotkey taking focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotkeyTextboxGainFocus(object sender, RoutedEventArgs e)
        {
            var txb = (TextBox)sender;
            txb.Background = HKTBrushes[HotkeyStates.FOCUSED];
        }

        /// <summary>
        /// Processes hotkey losing focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HotkeyTextboxLoseFocus(object sender, RoutedEventArgs e)
        {
            var txb = (TextBox)sender;
            DeactivateHotkeyTextbox(txb);
            txb.Background = HKTBrushes[HotkeyStates.UNFOCUSED];
            EnableTabbing();
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
            var tbx = (TextBox)sender;
            var container = TimerContainers[tbx];
            int keyValue = 0;
            if (TimeInputProcessor.TryGetNumber(e.Key, ref keyValue))
            {
                var currentTime = container.Timer;
                // if freshly focused, reset the timer on input
                if (container.FreshFocus)
                    currentTime = 0;

                var newTime = (currentTime % 10) * 10 + keyValue;
                container.Timer = newTime;
                tbx.Text = newTime.ToString();
                container.FreshFocus = false;
            }
        }

        /// <summary>
        /// Handles gaining focus for time textboxes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeGetFocus(object sender, RoutedEventArgs e)
        {
            var tbx = (TextBox)sender;
            var container = TimerContainers[tbx];
            // Set fresh focus to true
            container.FreshFocus = true;
            // Modify display
            tbx.Background = TimeBrushes[TimeStates.FOCUSED];
        }

        /// <summary>
        /// Handles losing focus 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeLostFocus(object sender, RoutedEventArgs e)
        {
            var tbx = (TextBox)sender;
            // Modify display
            tbx.Background = TimeBrushes[TimeStates.UNFOCUSED];
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


        #endregion

        #region Set Presets Methods
        private void SetAllHoykeys(Profile profile)
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
            if (profile.ThirtyFood)
                RDFood30.IsChecked = true;
            else
                RDFood40.IsChecked = true;
        }

        /// <summary>
        /// Sets the hotkey textbox and internal state for a single hotkey
        /// </summary>
        /// <param name="hotkeyBox"></param>
        /// <param name="pressedKey"></param>
        /// <param name="pressedModKeys"></param>
        private void SetHotkeyState(TextBox hotkeyBox, HotkeyContainer container)
        {
            HKTContainers[hotkeyBox] = container;
            if (container != null)
                hotkeyBox.Text = HotkeyProcessor.GetKeyInputText(container.LastPressedKey, container.ActiveModKeys);
            else
                hotkeyBox.Text = "";
        }

        /// <summary>
        /// Sets the time value and internal state for a single timebox
        /// </summary>
        /// <param name="timeBox"></param>
        /// <param name="timer"></param>
        private void SetTimeState(TextBox timeBox, int timer)
        {
            var container = TimerContainers[timeBox];
            timer = Math.Max(0, Math.Min(timer, 99));
            container.Timer = timer;
            timeBox.Text = timer.ToString();
            
        }
        #endregion

        #region UI Updates During Crafting
        public void UpdateCraftingLabel(string text)
        {
            LBLUpdate.Content = text;
        }
        #endregion

        #region Setting Crafting Button States
        private void SetCraftingStatus(SystemStates state)
        {
            SystemState = state;
            ButtonTest.Background = MainButtonBrushes[SystemState];
        }
        #endregion

        #region Full Winodws Methods
        private void DisableTabbing()
        {
            KeyboardNavigation.SetTabNavigation(MainWindowGrid, KeyboardNavigationMode.None);
        }
        private void EnableTabbing()
        {
            KeyboardNavigation.SetTabNavigation(MainWindowGrid, KeyboardNavigationMode.Continue);
        }

        private void TEST_Click(object sender, RoutedEventArgs e)
        {
            KeyInputEngine.SendKeysToGame(HKTContainers[TXBMacro1Key].Keys(), HKTContainers[TXBMacro1Key].ModKeys());
        }
        #endregion

        #region Save and Load Profile System
        private void GetProfiles()
        {
            var profileNameList = ProfileManager.GetProfilesList();
            CMBProfileList.ItemsSource = profileNameList;
        }

        private void LoadProfile(string name)
        {
            if (String.IsNullOrEmpty(name))
                return;
            var profileToLoad = ProfileManager.LoadProfile(name);
            SetAllHoykeys(profileToLoad);
            CMBProfileList.SelectedItem = name;
        }

        private void LoadDefaultProfile()
        {
            if (ProfileManager.VerifyDefaultProfile())
                LoadProfile(ProfileManager.DefaultProfile);
            else
                SetDefaultValues();
        }

        private void BTNSave_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveDialog();
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

        private void BTNLoad_Click(object sender, RoutedEventArgs e)
        {
            var profileToLoad = CMBProfileList.SelectedItem.ToString();
            LoadProfile(profileToLoad);
        }

        private void BTNDelete_Click(object sender, RoutedEventArgs e)
        {
            var currentIndex = CMBProfileList.SelectedIndex;
            if (currentIndex == -1)
                return;
            var profileToDelete = CMBProfileList.SelectedItem.ToString();
            ProfileManager.DeleteProfile(profileToDelete);
            GetProfiles();
            CMBProfileList.SelectedIndex = Math.Min(currentIndex, CMBProfileList.Items.Count - 1);
        }

        private Profile BuildProfileFromCurrent()
        {
            return new Profile(
                HKTContainers[TXBMacro1Key], TimerContainers[TXBMacro1Timer].Timer,
                HKTContainers[TXBMacro2Key], TimerContainers[TXBMacro2Timer].Timer, (bool)CHBMacro2.IsChecked,
                HKTContainers[TXBMacro3Key], TimerContainers[TXBMacro3Timer].Timer, (bool)CHBMacro3.IsChecked,
                HKTContainers[TXBFoodKey], (bool)CHBFood.IsChecked,
                HKTContainers[TXBSyrupKey], (bool)CHBSyrup.IsChecked,
                HKTContainers[TXBConfirmKey], HKTContainers[TXBCancelKey],
                (bool)CHBCollectableCraft.IsChecked, (bool)RDFood30.IsChecked
            );
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
        #endregion

    }
}
