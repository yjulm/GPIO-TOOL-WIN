using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace GPIO_TOOL_WIN
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Gpio _gpio = null;
        private bool _close = false;
        Thread _thread;

        public MainWindow()
        {
            InitializeComponent();

            _thread = new Thread(() =>
            {
                while (!_close)
                {
                    byte gp_value = _gpio.ReadGpioVal();
                    bool trigger = FormatGPIOOutVal(gp_value);
                    if (trigger)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            gp66_out.Text = "1";
                            Out_Click(null, null);
                        }));
                    }
                    else
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            gp66_out.Text = "0";
                            Out_Click(null, null);
                        }));
                    }
                    SpinWait.SpinUntil(() => _close, 10);
                }
            });

            Loaded += MainWindow_Loaded;
            this.Closing += Gpio_Closing;
            Init();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _thread.Start();
        }

        private void Gpio_Closing(object sender, CancelEventArgs e)
        {
            _close = true;
            _thread.Join();

            _gpio.ExitSuperIo();
            e.Cancel = false;
            Console.WriteLine("gpio tool exit...");
        }

        private void Init()
        {
            _gpio = new Gpio();
            bool initResult = _gpio.Initialize();
            if (!initResult)
            {
                error.Visibility = Visibility.Visible;
                mode_btn.IsEnabled = false;
                out_btn.IsEnabled = false;
            }
            else
            {
                _gpio.InitSuperIO();

                chip_type.Content = _gpio.GetChipName();

                _gpio.InitGpioReg();
                InitGpioModeAndVal();

                _gpio.ExitSuperIo();

                AddTextChangeEvent();

                chip_type.Visibility = Visibility.Visible;
                chip_name.Visibility = Visibility.Visible;
            }
        }

        private void InitGpioModeAndVal()
        {
            _gpio.GetEcBaseAddress();

            byte gp_mode = _gpio.ReadGpioMode();
            FormatGPIOModeVal(gp_mode);
            string str_mode = Utils.BinaryStrToHexStr(Utils.ByteToBinaryStr(gp_mode));
            mode_val.Text = str_mode;

            byte gp_value = _gpio.ReadGpioVal();
            FormatGPIOOutVal(gp_value);
            string str_val = Utils.BinaryStrToHexStr(Utils.ByteToBinaryStr(gp_value));
            out_val.Text = str_val;
        }

        private void OpenHelp(object sender, RoutedEventArgs e)
        {
            HelpWindow help = new HelpWindow();
            help.ShowDialog();
        }

        private void OpenAbout(object sender, RoutedEventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.ShowDialog();
        }

        private void ModeTextChanged(object sender, TextChangedEventArgs e)
        {
            var t = sender as TextBox;
            TextBox tb = this.gpio_mode_grid.FindName(t.Name) as TextBox;
            Console.WriteLine("mode textbox:" + tb.Name + "----" + tb.Text);
            if (Utils.VerificationNumber(Utils.StrToNum(tb.Text)))
            {
                string mode = GetAllModeStr();
                Console.WriteLine("ModeTextChanged............" + mode);
                string str = Utils.BinaryStrToHexStr(mode);
                mode_val.Text = str;
            }
            else
            {
                MessageBox.Show("0 or 1");
                tb.TextChanged -= new TextChangedEventHandler(ModeTextChanged);
                tb.Text = "";
                tb.TextChanged += new TextChangedEventHandler(ModeTextChanged);
            }
        }

        private void OutTextChanged(object sender, TextChangedEventArgs e)
        {
            var t = sender as TextBox;
            TextBox tb = this.gpio_mode_grid.FindName(t.Name) as TextBox;
            Console.WriteLine("out textbox:" + tb.Name + "----" + tb.Text);
            if (Utils.VerificationNumber(Utils.StrToNum(tb.Text)))
            {
                string value = GetAllValueStr();
                Console.WriteLine("OutTextChanged............" + value);
                string str = Utils.BinaryStrToHexStr(value);
                out_val.Text = str;
            }
            else
            {
                MessageBox.Show("Please enter 0 or 1");
                tb.TextChanged -= new TextChangedEventHandler(OutTextChanged);
                tb.Text = "";
                tb.TextChanged += new TextChangedEventHandler(OutTextChanged);
            }
        }

        private void Mode_Click(object sender, RoutedEventArgs e)
        {
            String value = mode_val.Text;
            InputGpioVal(value, "mode");
        }

        private void Out_Click(object sender, RoutedEventArgs e)
        {
            String value = out_val.Text;
            InputGpioVal(value, "");
        }

        private void InputGpioVal(String val, string type)
        {
            String value = val;
            try
            {
                byte gp_value = Convert.ToByte(value, 16);

                if (type == "mode")
                {
                    FormatGPIOModeVal(gp_value);
                    _gpio.InitSuperIO();
                    _gpio.SetGpioMode(gp_value);
                    _gpio.ExitSuperIo();
                }
                else
                {
                    FormatGPIOOutVal(gp_value);
                    _gpio.InitSuperIO();
                    _gpio.SetGpioOutValue(gp_value);
                    _gpio.ExitSuperIo();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Please enter 0-255 hexadecimal data!");
                return;
            }
        }

        private void FormatGPIOModeVal(byte val)
        {
            String str = Utils.ByteToBinaryStr(val);

            char[] gpio_vals = str.ToCharArray().Reverse().ToArray();

            gp60.Text = gpio_vals[0] + "";
            gp61.Text = gpio_vals[1] + "";
            gp62.Text = gpio_vals[2] + "";
            gp63.Text = gpio_vals[3] + "";
            gp64.Text = gpio_vals[4] + "";
            gp65.Text = gpio_vals[5] + "";
            gp66.Text = gpio_vals[6] + "";
            gp67.Text = gpio_vals[7] + "";
        }

        private bool FormatGPIOOutVal(byte val)
        {
            String str = Utils.ByteToBinaryStr(val);

            char[] gpio_vals = str.ToCharArray().Reverse().ToArray();

            Dispatcher.Invoke(new Action(() =>
            {
                gp60_out.Text = gpio_vals[0] + "";
                gp61_out.Text = gpio_vals[1] + "";
                gp62_out.Text = gpio_vals[2] + "";
                gp63_out.Text = gpio_vals[3] + "";
                gp64_out.Text = gpio_vals[4] + "";
                gp65_out.Text = gpio_vals[5] + "";
                gp66_out.Text = gpio_vals[6] + "";
                gp67_out.Text = gpio_vals[7] + "";
            }));

            return gpio_vals[4] == '0';
        }

        private void AddTextChangeEvent()
        {
            foreach (UIElement control in gpio_mode_grid.Children)
            {
                if (control is TextBox)
                {
                    TextBox textBox = control as TextBox;
                    textBox.TextChanged += new TextChangedEventHandler(ModeTextChanged);
                }
            }
            foreach (UIElement control in gpio_value_grid.Children)
            {
                if (control is TextBox)
                {
                    TextBox textBox = control as TextBox;
                    textBox.TextChanged += new TextChangedEventHandler(OutTextChanged);
                }
            }
        }

        private String GetAllModeStr()
        {
            StringBuilder sb = new StringBuilder(8);
            foreach (UIElement control in gpio_mode_grid.Children)
            {
                if (control is TextBox)
                {
                    TextBox textBox = control as TextBox;
                    sb.Append(textBox.Text);
                }
            }
            return sb.ToString()
                .ToArray()
                .Reverse()
                .Select(i => i.ToString())
                .Aggregate((c, i) => c = c + i.ToString());
        }

        private String GetAllValueStr()
        {
            StringBuilder sb = new StringBuilder(8);
            foreach (UIElement control in gpio_value_grid.Children)
            {
                if (control is TextBox)
                {
                    TextBox textBox = control as TextBox;
                    sb.Append(textBox.Text);
                }
            }
            return sb.ToString()
                .ToArray()
                .Reverse()
                .Select(i => i.ToString())
                .Aggregate((c, i) => c = c + i.ToString());
        }

        private void ChangeToCn(object sender, RoutedEventArgs e)
        {
            menu_cn.IsChecked = true;
            menu_en.IsChecked = false;

            Utils.ChangeLaungage("cn");
        }

        private void ChangeToEn(object sender, RoutedEventArgs e)
        {
            menu_en.IsChecked = true;
            menu_cn.IsChecked = false;

            Utils.ChangeLaungage("en");
        }
    }
}