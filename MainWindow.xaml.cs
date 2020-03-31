using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace v2ray_traffic_info
{
    public static class Info
    {
        public static string v2return;
        public static string apiaddress;
        public static int isOK;
        public static int setFlash = 0;
        
        public static int userNum;
        public static string[,] allUser;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public void Button_Click(object sender, RoutedEventArgs e)
        {
            Info.apiaddress = textBox_apiadd.Text;
            Thread getInfoThread = new Thread(new ThreadStart(MainProgram.GetInfo))
            {
                IsBackground = true
            };
            getInfoThread.Start();
        }
        public void Button_process_Click(object sender, RoutedEventArgs e)
        {
            MainProgram.GetAllUser();
            SetDataGrid();
        }
        ObservableCollection<Users> userList;
        public void SetDataGrid()
        {
            userList = new ObservableCollection<Users>();
            if (Info.allUser.Length == 0)
            { return; }
            for (int i = 0; i < Info.allUser.Length / 4; i++)
            {
                userList.Add(new Users()
                {
                    User = Info.allUser[i, 0],
                    Upload = Info.allUser[i, 1],
                    Download = Info.allUser[i, 2],
                    Byte = System.Convert.ToInt64(Info.allUser[i, 3]),
                });
            }
            //Action action1 = () =>
            //{
            //    dataGrid.ItemsSource = userList;
            //};
            //dataGrid.Dispatcher.Invoke(action1);
            DataGridSort("Byte", ListSortDirection.Descending);
        }
        public void DataGridSort(string ColumnName, ListSortDirection DescOrAsce)
        {
            Action action1 = () =>
            {
                dataGrid.ItemsSource = userList;
                ICollectionView v = CollectionViewSource.GetDefaultView(userList);
                v.SortDescriptions.Clear();
                v.SortDescriptions.Add(new SortDescription(ColumnName, DescOrAsce));
                v.Refresh();
                dataGrid.ColumnFromDisplayIndex(3).SortDirection = DescOrAsce;
            };
            dataGrid.Dispatcher.Invoke(action1);
        }
        public System.Timers.Timer t;
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBox_StartFlash.IsChecked == true)
            {
                Info.setFlash = 1;
                Info.apiaddress = textBox_apiadd.Text;
                Thread getInfoThread = new Thread(new ThreadStart(MainProgram.GetInfo))
                {
                    IsBackground = true
                };
                getInfoThread.Start();

                t = new System.Timers.Timer(1000);
                t.Elapsed += new ElapsedEventHandler(StartSetDataGrid);
                t.AutoReset = true;
                t.Enabled = true;
                t.Start();
            }
            else
            {
                Info.setFlash = 0;
            }
        }
        public void StartSetDataGrid(object source, System.Timers.ElapsedEventArgs e)
        {
            t.Stop();
            SetDataGrid();
            if (Info.setFlash == 1)
            {
                t.Start();
            }
            else
            {
                t.AutoReset = false;
                t.Enabled = false;
            }
        }
    }
    public class Users
    {
        public string User { get; set; }
        public string Upload { get; set; }
        public string Download { get; set; }
        public long Byte { get; set; }
    }
    public class MainProgram
    {
        public static void GetInfo()
        {
            Info.isOK = 0;
            while (Info.isOK == 0)
            {
                Process process = new Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.StandardInput.WriteLine("v2ctl.exe api --server=\"" + Info.apiaddress + "\" StatsService.QueryStats \"pattern: \"\"\"\"\"\" reset: false\"" + "&exit");
                string output = process.StandardOutput.ReadToEnd();
                Info.v2return = output;
                if (Info.setFlash == 0)
                {
                    Info.isOK = 1;
                    break;
                }
                GetAllUser();
                Thread.Sleep(1000);
            }
            
        }
        public static string[] SplitOutputInfo(string str)
        {
            int numOfInfo = 0;
            int ifIndex = 0;
            while ((ifIndex = str.IndexOf("stat:", ifIndex)) != -1)
            {
                numOfInfo++;
                ifIndex += 5;
            }
            if (numOfInfo == 0)
            {
                string[] none = { };
                return none;
            }
            string[] baseInfo = new string[numOfInfo];
            string[] splitInfo = str.Split("stat:");
            for (int i = 0; i < numOfInfo; i++)
            {
                baseInfo[i] = splitInfo[i + 1];
            }
            return baseInfo;
        }
        public static string HumanReadableFilesize(double size)
        {
            String[] units = new String[] { " B", " KB", " MB", " GB", " TB", " PB" };
            double mod = 1024.0;
            int i = 0;
            while (size >= mod)
            {
                size /= mod;
                i++;
            }
            return Math.Round(size) + units[i];
        }
        public static void GetAllUser()
        {
            string[] baseInfo = MainProgram.SplitOutputInfo(Info.v2return);
            if (baseInfo.Length == 0)
            { return; }
            string cache;
            string[] cache2;
            string[,] result = new string[baseInfo.Length, 5];
            Info.userNum = 0;
            int inboundNum = 0;
            for (int i = 0; i < baseInfo.Length; i++)
            {
                cache = (baseInfo[i].Split('"'))[1];
                cache2 = cache.Split(">>>");
                result[i, 0] = cache2[0];//数据类型
                if (cache2[0] == "user")
                { Info.userNum++; }
                if (cache2[0] == "inbound")
                { inboundNum++; }
                result[i, 1] = cache2[1];//名称
                result[i, 2] = cache2[3];//上行下行

                cache = (baseInfo[i].Split('"'))[2];
                cache = cache.Replace(" ", "");
                cache = cache.Replace("value:", "");
                cache = cache.Replace(">", "");
                cache = cache.Replace("\n", "");
                result[i, 3] = cache;//流量字节
                result[i, 4] = MainProgram.HumanReadableFilesize(System.Convert.ToDouble(cache));//转为可阅读流量
            }

            Info.allUser = new string[Info.userNum / 2, 4];
            int yesNo;
            int allUserIndex = 0;
            for (int i = 0; i < result.Length / 5; i++)
            {
                if (result[i, 0] != "user")
                { continue; }
                yesNo = 0;

                for (int ii = 0; ii < Info.allUser.Length / 4; ii++)
                {
                    if (Info.allUser[ii, 0] == result[i, 1])
                    {
                        yesNo = 1;
                        if (result[i, 2] == "uplink")
                        {
                            Info.allUser[ii, 1] = result[i, 4];
                            long a = System.Convert.ToInt64(Info.allUser[ii, 3]) + System.Convert.ToInt64(result[i, 3]);
                            Info.allUser[ii, 3] = a.ToString();
                        }
                        else
                        {
                            Info.allUser[ii, 2] = result[i, 4];
                            long a = System.Convert.ToInt64(Info.allUser[ii, 3]) + System.Convert.ToInt64(result[i, 3]);
                            Info.allUser[ii, 3] = a.ToString();
                        }
                    }
                }

                if (yesNo == 0)
                {
                    Info.allUser[allUserIndex, 0] = result[i, 1];
                    if (result[i, 2] == "uplink")
                    {
                        Info.allUser[allUserIndex, 1] = result[i, 4];
                        Info.allUser[allUserIndex, 3] = result[i, 3];
                    }
                    else
                    {
                        Info.allUser[allUserIndex, 2] = result[i, 4];
                        Info.allUser[allUserIndex, 3] = result[i, 3];
                    }
                    allUserIndex++;
                }

            }

        }
    }

    
}
