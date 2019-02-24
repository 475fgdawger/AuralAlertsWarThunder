using System;
using System.Windows;
using System.Windows.Threading;
using System.Globalization;
using System.Windows.Documents;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using System.IO;





namespace AuralAlertsWarThunder
{
   
    public partial class MainWindow : Window
    {
        private indicator myIndicator = new indicator();
        private state myState = new state();
        string indicatorsurl = "http://localhost:8111/indicators";
        string statesurl = "http://localhost:8111/state";
        DispatcherTimer dispatcherTimer1 = new DispatcherTimer();
        DispatcherTimer dispatcherTimer2 = new DispatcherTimer();
        CultureInfo culture = new CultureInfo("en-US");
        FlowDocument myFlowDoc = new FlowDocument();
        Paragraph par = new Paragraph();
        int JokerFuel = 7, BingoFuel = 3;
        bool FirstFlight = true,   FLTransit = false;
        decimal CritMach = 0.65M, Alt = 0, G = 1, AoA = 0, mach = 0;
        int OverGHd = 9, AoACrit = 14, GDSpd = 225, GuSpd = 270;
        int FuelKgPM = 5, FuelBingo = 0, FuelJoker = 0;
        int OilCaut = 95, H2OCaut = 125, OilWarn = 100, H2OWarn = 130;
        double Oil1Temp = 0, H2O1Temp = 0, Oil2Temp = 0, H2O2Temp = 0;
        int Throttle = 0, gear = 100, IAS = 0, flaps = 0, Fuel = 0;
        int BingoPlayed = 0, JokerPlayed = 0, OilCautSwitch = 0, H2OCautSwitch = 0, FlapSuggestPlayedLastTime = 0;
        int OilWarnCount = 0, H2OWarnCount = 0, GearWarnCount = 0;
        int Throttle1 = 0, Throttle2 = 0, elevator = 0;
        double Thrust1 = 0, Thrust2 = 0 /*, HorsePower1 = 0, HorsePower2 = 0*/;
        int ThrustFailed = 0, /*ThrustReduced = 0,*/ TRCount = 0, Vfc = 900, Vft = 900, Vfl = 900;
        int rad1 = 0, rad2 = 0, RadiatorModeSwitch = 0;
        Random FFVary = new Random();

        public MainWindow()
        {
            InitializeComponent();

            textBox_jokSlider.Text = slider_jok.Value.ToString();
            textBox_bgoSlider.Text = slider_bgo.Value.ToString();
            dispatcherTimer1.Tick += new EventHandler(dispatcherTimer1_Tick);
            dispatcherTimer1.Interval = new TimeSpan(0, 0, 0, 0, 200);
            dispatcherTimer2.Tick += new EventHandler(dispatcherTimer2_Tick);
            dispatcherTimer2.Interval = new TimeSpan(0, 0, 5);
            CheckPath();



   
        }

        private void dispatcherTimer2_Tick(object sender, EventArgs e)
        {
            ConnectToJSONIPAddress();
        }

        public void CheckPath()
        {
            var installedLocation = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            if (!Directory.Exists(installedLocation + @"\DataLogs"))
            {
                installedLocation.CreateSubdirectory("DataLogs");
            }
        }

        public void ConnectToJSONIPAddress()
        {
            try
            {
                if (UserConnectionQuery("localhost", 8111))
                {
                    myState = JsonSerializer._download_serialized_json_data<state>(statesurl);
                    if (myState.valid == "true")
                    {
                        dispatcherTimer2.Stop();
                        dispatcherTimer1.Start();
                        tbx_msgs.Text = ("Running");
                    }
                    else if (myState.valid == "false")
                    {
                        dispatcherTimer2.Start();
                        dispatcherTimer1.Stop();
                        tbx_msgs.Text = "Waiting for a flight...";
                    }
                }
                else
                {
                    //Switch to listening
                    dispatcherTimer2.Start();
                    dispatcherTimer1.Stop();
                    tbx_msgs.Text = ("War Thunder is not running...");
                }
            }
            catch (Exception ex)
            {
                tbx_msgs.Text = ex.Message;
                dispatcherTimer1.Stop();
                dispatcherTimer2.Start();
            }
        }

        private void dispatcherTimer1_Tick(object sender, EventArgs e)
        {
            getACtype();
            getData();
        }

        private bool UserConnectionQuery(string address, int port)
        {
            try
            {

                System.Net.Sockets.TcpClient connection = new System.Net.Sockets.TcpClient(address, port);
                connection.Close();

                return true;
            }
            catch
            {
                return false;
            }

        }

        private void getData()
        {
            try
            {
                myIndicator = JsonSerializer._download_serialized_json_data<indicator>(indicatorsurl);
                myState = JsonSerializer._download_serialized_json_data<state>(statesurl);

                if (myState.valid == "true")
                {
                    G = Convert.ToDecimal(myState.Ny, culture);
                    mach = Convert.ToDecimal(myState.M, culture);
                    AoA = Convert.ToDecimal(myState.AoA, culture);
                    Alt = Convert.ToDecimal(myIndicator.altitude_hour, culture);
                    Oil1Temp = Convert.ToDouble(myIndicator.oil_temperature);
                    H2O1Temp = Convert.ToDouble(myIndicator.water_temperature, culture);
                    Oil2Temp = Convert.ToDouble(myIndicator.oil_temperature1, culture);
                    H2O2Temp = Convert.ToDouble(myIndicator.water_temperature1, culture);
                    rad1 = Convert.ToInt32(myState.radiator_1);
                    rad2 = Convert.ToInt32(myState.radiator_2);
                    Fuel = Convert.ToInt32(myState.Mfuel);
                    elevator = Convert.ToInt32(myState.elevator);
                    int FuelFull = Convert.ToInt32(myState.Mfuel0);
                    Throttle = Convert.ToInt32(myState.throttle_1);
                    Throttle1 = Convert.ToInt32(myState.throttle_1);
                    Throttle2 = Convert.ToInt32(myState.throttle_2);
                    gear = Convert.ToInt32(myState.gear);
                    IAS = Convert.ToInt32(myState.IAS);
                    flaps = Convert.ToInt32(myState.flaps);
                    label.Content = myIndicator.type;
                    string ACtype = myIndicator.type;
                    if (cbx_ovsd.IsChecked == true)
                    {
                        OverSpeed();
                    }
                    if (cbx_ovrg.IsChecked == true)
                    {
                        OverG();
                    }
                    if (cbx_engfail.IsChecked == true) { }
                    {
                        if (ThrustFailed > 0)
                        {
                            IsEngineFailed();
                        }
                    }
                    if (cbx_stall.IsChecked == true)
                    {
                        StallWarning();
                    }
                    if (cbx_flaps.IsChecked == true)
                    {
                        FlapsPO();
                    }
                    if (cbx_VGM.IsChecked == true)
                    {
                        VerbalGMeter();
                    }
                    if (cbx_overht.IsChecked == true)
                    {
                        OilRadiatorStatus();
                        H2ORadiatorStatus();
                    }
                    if (cbx_gear.IsChecked == true)
                    {
                        GearWarns();
                    }
                    BettyStatus();
                    if (cbx_recorddata.IsChecked == true)
                    {
                        RecordData();
                    }
                    if (cbx_flapsuggest.IsChecked == true)
                    {
                        FlapSuggestions();
                    }
                    if (IAS > GuSpd)
                    {
                        if (cbx_jok.IsChecked == true)
                        {
                            if (JokerPlayed < 2)
                            {
                                Joker();
                            }
                        }
                        if (cbx_bgo.IsChecked == true)
                        {
                            if (BingoPlayed < 2)
                            {
                                Bingo();
                            }
                        }
                    }

                }
                else
                {
                    dispatcherTimer1.Stop();
                    dispatcherTimer2.Start();
                }
            }
            catch (Exception ex)
            {
                tbx_msgs.Text = ex.Message;
                dispatcherTimer1.Stop();
                dispatcherTimer2.Start();
            }
        }
       
        private void RecordData()
        {
            if (Throttle > 50 && IAS > 10)
            {
                var installedLocation = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
                if (!Directory.Exists(installedLocation + @"\DataLogs"))
                {
                    CheckPath();
                }
                string[] indicators = { myState.Ny, myState.AoA, myState.IAS, myState.TAS, myState.elevator, myState.aileron, myState.rudder, myState.flaps, myState.gear, myState.H, myState.M, myState.throttle_1, myState.thrust1_kgs, myState.RPM_1, myState.mixture_1, myState.radiator_1, myIndicator.oil_temperature, myIndicator.water_temperature, myState.throttle_2, myState.thrust2_kgs, myState.RPM_2, myState.mixture_2, myState.radiator_2, myIndicator.oil_temperature1, myIndicator.water_temperature1, };
                string line = String.Join(",", indicators);
                string filepath = installedLocation + @"\DataLogs\Data_"
                                + myIndicator.type + ".csv";
                
                if (!File.Exists(filepath))
                {
                    using (StreamWriter sw = File.CreateText(filepath))
                    {
                        sw.WriteLine("Data_" + myIndicator.type);
                        sw.WriteLine("G , AoA , IAS , TAS , Elevator , Aileron , Rudder , Flaps , Gear , Alt (M) , Mach , Throt1 , Thrust1 , RPM1,  Mix1 , Rad1 , Oil1Temp , H2O1Temp , Throt2 , Thrust2 , RPM2,  Mix2 , Rad2 , Oil2Temp , H2O2Temp");
                    }
                }
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(line);
                }
            }
        }
       
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.EnableBGO = cbx_bgo.IsChecked.Value;
            Properties.Settings.Default.EnableJOK = cbx_jok.IsChecked.Value;
            Properties.Settings.Default.VGM = cbx_VGM.IsChecked.Value;
            Properties.Settings.Default.Overspeed = cbx_ovsd.IsChecked.Value;
            Properties.Settings.Default.Stall = cbx_stall.IsChecked.Value;
            Properties.Settings.Default.Gear = cbx_gear.IsChecked.Value;
            Properties.Settings.Default.FlapSounds = cbx_flaps.IsChecked.Value;
            Properties.Settings.Default.FlapsSuggest = cbx_flapsuggest.IsChecked.Value;
            Properties.Settings.Default.OverG = cbx_ovrg.IsChecked.Value;
            Properties.Settings.Default.RecordData = cbx_recorddata.IsChecked.Value;
            Properties.Settings.Default.Engine = cbx_engfail.IsChecked.Value;
            Properties.Settings.Default.Overheat = cbx_overht.IsChecked.Value;
            Properties.Settings.Default.BGOSlider = BingoFuel;
            Properties.Settings.Default.JOKSlider = JokerFuel;
            Properties.Settings.Default.Save();
            dispatcherTimer1.Stop();
            dispatcherTimer2.Stop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbx_bgo.IsChecked = Properties.Settings.Default.EnableBGO;
            cbx_jok.IsChecked = Properties.Settings.Default.EnableJOK;
            cbx_VGM.IsChecked = Properties.Settings.Default.VGM;
            cbx_ovsd.IsChecked = Properties.Settings.Default.Overspeed;
            cbx_stall.IsChecked = Properties.Settings.Default.Stall;
            cbx_gear.IsChecked = Properties.Settings.Default.Gear;
            cbx_flaps.IsChecked = Properties.Settings.Default.FlapSounds;
            cbx_flapsuggest.IsChecked = Properties.Settings.Default.FlapsSuggest;
            cbx_ovrg.IsChecked = Properties.Settings.Default.OverG;
            cbx_recorddata.IsChecked = Properties.Settings.Default.RecordData;
            cbx_engfail.IsChecked = Properties.Settings.Default.Engine;
            cbx_overht.IsChecked = Properties.Settings.Default.Overheat;
            slider_bgo.Value = Properties.Settings.Default.BGOSlider;
            slider_jok.Value = Properties.Settings.Default.JOKSlider;

            ConnectToJSONIPAddress();
        }

        private void Button_save_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Cbx_jok_Checked(object sender, RoutedEventArgs e)
        {
            //EnableJOK = true;
        }

        private void Cbx_jok_Unchecked(object sender, RoutedEventArgs e)
        {
            //EnableJOK = false;
        }

        private void Cbx_bgo_Unchecked(object sender, RoutedEventArgs e)
        {
            //EnableBGO = false;
        }

        private void Slider_bgo_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BingoFuel = (Convert.ToInt32(slider_bgo.Value));
        }

        private void Slider_jok_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            JokerFuel = (Convert.ToInt32(slider_jok.Value));
        }

        private void Cbx_bgo_Checked(object sender, RoutedEventArgs e)
        {
            //EnableBGO = true;
        }
        private void Bingo()
        {
            if (Fuel <= FuelBingo && Fuel > (FuelBingo - 2))
            {
                ++BingoPlayed;
                System.Media.SoundPlayer myPlayer;
                myPlayer = new System.Media.SoundPlayer(Properties.Resources.Bingo);
                myPlayer.PlaySync();

            }
        }
        private void Joker()
        {
            if (Fuel <= FuelJoker && Fuel > (FuelJoker - 2))
            {
                ++JokerPlayed;
                System.Media.SoundPlayer myPlayer;
                myPlayer = new System.Media.SoundPlayer(Properties.Resources.jokerfuel);
                myPlayer.PlaySync();
            }
        }
        private void IsEngineFailed()
        {
            if (Throttle1 > 75 && Thrust1 < ThrustFailed)
            {
                ++TRCount;
                if (TRCount < 50)
                {
                    ;
                }
                else
                {
                    System.Media.SoundPlayer myPlayer;
                    myPlayer = new System.Media.SoundPlayer(Properties.Resources.engine1fail);
                    myPlayer.PlaySync();
                }
            }
            if (Throttle2 > 75 && Thrust2 < ThrustFailed)
            {
                ++TRCount;
                if (TRCount < 50)
                {
                    ;
                }
                else
                {
                    System.Media.SoundPlayer myPlayer;
                    myPlayer = new System.Media.SoundPlayer(Properties.Resources.engine2fail);
                    myPlayer.PlaySync();
                }
            }

            /*if (Throttle1 > 90 && Thrust1 < ThrustReduced)
            {
                ++TRCount;
                if (TRCount < 50 || TRCount > 51)
                {
                    ;
                }
                else
                {

                    System.Media.SoundPlayer myPlayer;
                    myPlayer = new System.Media.SoundPlayer(Properties.Resources.engine1degraded);
                    myPlayer.PlaySync();
                }
            }
            if (Throttle2 > 90 && Thrust2 < ThrustReduced)
            {
                ++TRCount;
                if (TRCount < 50 || TRCount > 51)
                {
                    ;
                }
                else
                {
                    System.Media.SoundPlayer myPlayer;
                    myPlayer = new System.Media.SoundPlayer(Properties.Resources.engine2degraded);
                    myPlayer.PlaySync();
                }
            }*/
            if (TRCount > 1 && Throttle1 < 50 && Throttle2 < 50)
            {
                TRCount = 0;
            }
        }
        private void StallWarning()
        {
            System.Media.SoundPlayer stall1;
            System.Media.SoundPlayer stall2;
            stall1 = new System.Media.SoundPlayer(Properties.Resources.AngleOfAttackOverLimit);
            stall2 = new System.Media.SoundPlayer(Properties.Resources.MaximumAngleOfAttack);

            if (AoA > AoACrit && IAS > 50)
            {
                if (AoA < AoACrit + 2)
                {
                    stall1.Stop();
                    stall2.PlayLooping();
                }
                else
                {
                    stall2.Stop();
                    stall1.PlayLooping();
                }
            }
            else
            {
                stall1.Stop();
                stall2.Stop();
            }
        }
        private void BettyStatus()
        {
            if (FirstFlight == true && Throttle > 50)
            {
                int FFInt = FFVary.Next(0, 5);

                switch (FFInt)
                {
                    case 1:
                        System.Media.SoundPlayer myPlayer;
                        myPlayer = new System.Media.SoundPlayer(Properties.Resources.BettyIsReady);
                        myPlayer.PlaySync();
                        FirstFlight = false;
                        break;
                    case 2:
                        //System.Media.SoundPlayer myPlayer;
                        myPlayer = new System.Media.SoundPlayer(Properties.Resources.dawgerisgod);
                        myPlayer.PlaySync();
                        FirstFlight = false;
                        break;
                    case 3:
                        //System.Media.SoundPlayer myPlayer;
                        myPlayer = new System.Media.SoundPlayer(Properties.Resources.dawgerisgod);
                        myPlayer.PlaySync();
                        FirstFlight = false;
                        break;
                    case 4:
                        //System.Media.SoundPlayer myPlayer;
                        myPlayer = new System.Media.SoundPlayer(Properties.Resources.charge);
                        myPlayer.PlaySync();
                        FirstFlight = false;
                        break;
                    default:
                        FFInt = 0;
                        break;
                }

            }
        }
        private void OverG()
        {
            System.Media.SoundPlayer G1;
            System.Media.SoundPlayer G2;
            G1 = new System.Media.SoundPlayer(Properties.Resources.OverG);
            G2 = new System.Media.SoundPlayer(Properties.Resources.criticalG);
            if (G > OverGHd)
            {
                if (G > OverGHd + 4 - OverGHd / (decimal)4)
                {
                    G1.Stop();
                    G2.PlaySync();
                }
                else
                {
                    G2.Stop();
                    G1.PlaySync();
                }
            }
        }
        private void OverSpeed()
        {
            if (mach >= CritMach)
            {
                System.Media.SoundPlayer myPlayer;
                myPlayer = new System.Media.SoundPlayer(Properties.Resources.overspeed);
                myPlayer.PlaySync();
            }
        }
        private void VerbalGMeter()
        {
            if (G < OverGHd)
            {
                if (AoA >= AoACrit)
                {
                    ;
                }
                else
                {
                    if (G >= 4 && G < 5)
                    {
                        System.Media.SoundPlayer myPlayer;
                        myPlayer = new System.Media.SoundPlayer(Properties.Resources.fourG);
                        myPlayer.PlaySync();

                    }
                    else if (G >= 5 && G < 6)
                    {
                        System.Media.SoundPlayer myPlayer;
                        myPlayer = new System.Media.SoundPlayer(Properties.Resources.fiveG);
                        myPlayer.PlaySync();
                    }
                    else if (G >= 6 && G < 7)
                    {
                        System.Media.SoundPlayer myPlayer;
                        myPlayer = new System.Media.SoundPlayer(Properties.Resources.sixG);
                        myPlayer.PlaySync();
                    }
                    else if (G >= 7 && G < 8)
                    {
                        System.Media.SoundPlayer myPlayer;
                        myPlayer = new System.Media.SoundPlayer(Properties.Resources.sevenG);
                        myPlayer.PlaySync();
                    }
                    else if (G >= 8 && G < 9)
                    {
                        System.Media.SoundPlayer myPlayer;
                        myPlayer = new System.Media.SoundPlayer(Properties.Resources.eightG);
                        myPlayer.PlaySync();
                    }
                    else if (G >= 9 && G < 10)
                    {
                        System.Media.SoundPlayer myPlayer;
                        myPlayer = new System.Media.SoundPlayer(Properties.Resources.nineG);
                        myPlayer.PlaySync();
                    }
                    else if (G >= 10 && G < 11)
                    {
                        System.Media.SoundPlayer myPlayer;
                        myPlayer = new System.Media.SoundPlayer(Properties.Resources.tenG);
                        myPlayer.PlaySync();
                    }
                }
            }
        }
        private void GearWarns()
        {
            if (gear > 0 && IAS > GuSpd && Throttle > 70)
            {
                System.Media.SoundPlayer myPlayer;
                myPlayer = new System.Media.SoundPlayer(Properties.Resources.GearDown);//GearDown is American Voice checkgear2 is British
                myPlayer.PlaySync();
            }
            else if (Throttle < 40)
            {
                if (flaps < 100)
                {
                    if (GearWarnCount > 2)
                    {
                        ;
                    }
                    else if (gear < 100 && IAS < GDSpd)
                    {
                        ++GearWarnCount;
                        System.Media.SoundPlayer myPlayer;
                        myPlayer = new System.Media.SoundPlayer(Properties.Resources.GearDown);
                        myPlayer.PlaySync();
                    }
                }
                else if (gear < 100 && IAS < GDSpd && flaps == 100 && IAS > 40)
                {
                    System.Media.SoundPlayer myPlayer;
                    myPlayer = new System.Media.SoundPlayer(Properties.Resources.GearDown);
                    myPlayer.PlaySync();
                }
            }
            if (Throttle > 50)
            {
                {
                    GearWarnCount = 0;
                }
                
            }
        }
        private void OilRadiatorStatus()
        {
            if ((int)Oil1Temp < OilWarn && (int)Oil2Temp < OilWarn)
            {
                OilWarnCount = 0;
                if ((int)Oil1Temp < OilCaut && (int)Oil2Temp < OilCaut)
                {
                    OilCautSwitch = 0;
                }
                if (OilCautSwitch == 0)
                {
                    if (Oil1Temp > OilCaut | Oil2Temp > OilCaut && Throttle > 100)
                    {
                        OilCautSwitch = 967;
                        System.Media.SoundPlayer myPlayer;
                        myPlayer = new System.Media.SoundPlayer(Properties.Resources.cautionoiltemp);
                        myPlayer.PlaySync();
                    }
                }
            }
            else if (OilWarnCount > 2)
            {
                ;
            }
            else if (Oil1Temp > OilWarn | Oil2Temp > OilWarn && Throttle > 100)
            {
                ++OilWarnCount;
                System.Media.SoundPlayer myPlayer;
                myPlayer = new System.Media.SoundPlayer(Properties.Resources.warnoiltemp);
                myPlayer.PlaySync();
            }
        }
        private void H2ORadiatorStatus()
        {
            if (H2O1Temp < H2OWarn && H2O2Temp < H2OWarn)
            {
                H2OWarnCount = 0;
                if (H2O1Temp < H2OCaut && H2O2Temp < H2OCaut)
                {
                    H2OCautSwitch = 0;
                    if (H2O1Temp < (H2OCaut - 3) && H2OCaut < (H2OCaut - 3))
                    {
                        RadiatorModeSwitch = 0;
                    }
                }
                if (H2OCautSwitch == 0)
                {
                    if (H2O1Temp > H2OCaut | H2O2Temp > H2OCaut && Throttle > 100)
                    {
                        H2OCautSwitch = 967;
                        System.Media.SoundPlayer myPlayer;
                        myPlayer = new System.Media.SoundPlayer(Properties.Resources.cautionwatertemp);
                        myPlayer.PlaySync();
                    }
                }
            }
            else if (H2OWarnCount > 2)
            {
                ;
            }
            else if (H2O1Temp > H2OWarn | H2O2Temp > H2OWarn && Throttle > 100)
            {
                ++H2OWarnCount;
                System.Media.SoundPlayer myPlayer;
                myPlayer = new System.Media.SoundPlayer(Properties.Resources.warnwatertemp);
                myPlayer.PlaySync();
            }
            if (RadiatorModeSwitch < 3)
            {
                if (H2O1Temp > (H2OCaut - 10) || H2OCaut > (H2OCaut - 10))

                {
                    if (rad1 < 10 || rad2 < 10)
                    {
                        if (Throttle1 > 100 || Throttle2 > 100)
                        {
                            ++RadiatorModeSwitch;
                            System.Media.SoundPlayer myPlayer;
                            myPlayer = new System.Media.SoundPlayer(Properties.Resources.checkradiatormode);
                            myPlayer.PlaySync();
                        }
                    }
                }
            }
        }
        private void FlapSuggestions()
        {
            if (IAS > 100)
            {
                if (AoA > (AoACrit - 4) && elevator > 25)
                {
                    if (IAS < Vfc && IAS > Vft && G < 6 && flaps >= 0 && flaps < 20 && FlapSuggestPlayedLastTime != 1)
                    {
                        FlapSuggestPlayedLastTime = 1;
                        System.Media.SoundPlayer myPlayer;
                        myPlayer = new System.Media.SoundPlayer(Properties.Resources.Isuggestflapscombat);
                        myPlayer.PlaySync();
                    }

                    else if (IAS < Vft && IAS > Vfl && G < 5 && flaps >20 && flaps < 33 && FlapSuggestPlayedLastTime != 2)
                    {
                        FlapSuggestPlayedLastTime = 2;
                        System.Media.SoundPlayer myPlayer;
                        myPlayer = new System.Media.SoundPlayer(Properties.Resources.Isuggestflapstakeoff);
                        myPlayer.PlaySync();
                    }
                    else if (IAS < Vfl && G < 4 && flaps >33 && flaps < 100 && FlapSuggestPlayedLastTime != 3)
                    {
                        FlapSuggestPlayedLastTime = 3;
                        System.Media.SoundPlayer myPlayer;
                        myPlayer = new System.Media.SoundPlayer(Properties.Resources.Isuggestflapsland);
                        myPlayer.PlaySync();
                    }
                }

                else
                {
                    FlapSuggestPlayedLastTime = 0;
                }
            }
            else
            {
                FlapSuggestPlayedLastTime = 99;
            }
        }
        private void FlapsPO()
        {
            if (flaps > 0 && flaps <= 20)
            {
                if (IAS > (Vfc - 20))
                {
                    System.Media.SoundPlayer myPlayer;
                    myPlayer = new System.Media.SoundPlayer(Properties.Resources.checkflaps);
                    myPlayer.PlaySync();
                }
            }
            else if (flaps > 20 && flaps <= 33)
            {
                if (IAS > (Vft - 20))
                {
                    System.Media.SoundPlayer myPlayer;
                    myPlayer = new System.Media.SoundPlayer(Properties.Resources.checkflaps);
                    myPlayer.PlaySync();
                }
            }
            else if (flaps > 33 && flaps <= 100)
            {
              
                if (IAS > (Vfl-20))
                {
                    System.Media.SoundPlayer myPlayer;
                    myPlayer = new System.Media.SoundPlayer(Properties.Resources.checkflaps);
                    myPlayer.PlaySync();
                }
            }
            if (flaps != 0 && flaps != 20 && flaps != 33 && flaps != 100)
            {
                FLTransit = true;
            }
            if (FLTransit == true)
            {
                if (flaps == 0)
                {
                    FLTransit = false;
                    //call flaps 0
                    System.Media.SoundPlayer myPlayer;
                    myPlayer = new System.Media.SoundPlayer(Properties.Resources.flapsupamericanfemale);
                    myPlayer.PlaySync();
                    
                    
                }
                if (flaps == 20)
                {
                    //call flaps 20
                    System.Media.SoundPlayer myPlayer;
                    myPlayer = new System.Media.SoundPlayer(Properties.Resources.FlapsCombat);
                    myPlayer.PlaySync();
                    FLTransit = false;
                }
                if (flaps == 33)
                {
                    //call flaps 33
                    System.Media.SoundPlayer myPlayer;
                    myPlayer = new System.Media.SoundPlayer(Properties.Resources.FlapsTakeoff);
                    myPlayer.PlaySync();
                    FLTransit = false;
                }
                if (flaps == 100)
                {
                    FLTransit = false;
                    //call flaps 100
                    System.Media.SoundPlayer myPlayer;
                    myPlayer = new System.Media.SoundPlayer(Properties.Resources.FlapsLand);
                    myPlayer.PlaySync();
                }
            }
        }
        public void getACtype()
        {
            try
            {


                myIndicator = JsonSerializer._download_serialized_json_data<indicator>(indicatorsurl);
                myState = JsonSerializer._download_serialized_json_data<state>(statesurl);
                string ACtype = myIndicator.type;
                string ACtypeStripped = new string(ACtype.Where(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-').ToArray());
                int FuelFull = Convert.ToInt32(myState.Mfuel0);

                AoACrit = 14;
                CritMach = 0.65M;
                OverGHd = 9;
                GDSpd = 225;
                GuSpd = 270;
                FuelKgPM = 15;
                OilCaut = 95;
                OilWarn = 100;
                H2OCaut = 125;
                H2OWarn = 130;

               

                if (ACtype == "p-38k" || ACtype == "p-38l" || ACtype == "p-38j")

                {
                    AoACrit = 14;
                    CritMach = 0.65M;
                    OverGHd = 9;
                    GDSpd = 225;
                    GuSpd = 270;
                    FuelKgPM = 15;
                    ThrustFailed = 200;
                   // ThrustReduced = 450;
                    OilCaut = 95;
                    OilWarn = 100;
                    H2OCaut = 125;
                    H2OWarn = 130;
                    Vfc = 620; Vft = 403; Vfl = 250;


                }
                else if (ACtype == "p-38g" || ACtype == "p-38g_metal" || ACtype == "p-38e")
                {
                    AoACrit = 14;
                    CritMach = 0.65M;
                    OverGHd = 9;
                    GDSpd = 225;
                    GuSpd = 270;
                    FuelKgPM = 14;
                    ThrustFailed = 200;
                    //ThrustReduced = 400;
                    OilCaut = 95;
                    OilWarn = 100;
                    H2OCaut = 125;
                    H2OWarn = 130;
                    Vfc = 620; Vft = 403; Vfl = 250;
                }
                else if (ACtype == "bf-109f-4" || ACtype == "bf-109f-4_trop" || ACtype == "bf-109f-2")
                {
                    AoACrit = 14;
                    CritMach = 0.65M;
                    OverGHd = 10;
                    GDSpd = 225;
                    GuSpd = 270;
                    FuelKgPM = 4;
                    OilCaut = 93;
                    OilWarn = 100;
                    H2OCaut = 103;
                    H2OWarn = 110;
                }
                else if (ACtype == "bf-109g-6")
                {
                    AoACrit = 14;
                    CritMach = 0.65M;
                    OverGHd = 10;
                    GDSpd = 225;
                    GuSpd = 270;
                    FuelKgPM = 5;
                    OilCaut = 95;
                    OilWarn = 105;
                    H2OCaut = 105;
                    H2OWarn = 115;
                }
                else if (ACtype == "bf-109k-4")
                {
                    AoACrit = 14;
                    CritMach = 0.65M;
                    OverGHd = 10;
                    GDSpd = 225;
                    GuSpd = 270;
                    FuelKgPM = 6;
                    OilCaut = 101;
                    OilWarn = 109;
                    H2OCaut = 111;
                    H2OWarn = 118;
                }
                else
                {
                    AoACrit = 14;
                    CritMach = 0.65M;
                    OverGHd = 9;
                    GDSpd = 225;
                    GuSpd = 270;
                    FuelKgPM = (FuelFull / 60);
                    ThrustFailed = 0;
                    //ThrustReduced = 0;
                    OilCaut = 115;
                    OilWarn = 125;
                    H2OCaut = 135;
                    H2OWarn = 145;
                }
                FuelBingo = FuelKgPM * BingoFuel;
                FuelJoker = FuelKgPM * JokerFuel;
            }
            catch (Exception ex)
            {
                tbx_msgs.Text = ex.Message;
            }
        }
    }

}
