using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
        public static string v2return;//存储v2ray返回的原始信息
        public static string apiaddress;//存储用户输入的API地址
        public static int isOK;
        public static int setFlash = 0;//是否开启实时刷新
        
        public static int userNum;//存储用户数据总数
        public static string[,] allUser;//存储处理后的用户信息数组
        public static string[,] oldAllUser;//存储实时刷新前一次的用户信息

        public static int close = 0;
        public static int wait = 0;
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
            button.IsEnabled = false;
            Info.apiaddress = textBox_apiadd.Text;//获取用户输入API地址
            Thread getInfoThread = new Thread(new ThreadStart(MainProgram.GetInfo))
            {
                IsBackground = true
            };
            getInfoThread.Start();//启动线程获取原始信息存入Info.v2return
        }
        public void Button_process_Click(object sender, RoutedEventArgs e)
        {
            Info.wait = 0;
            MainProgram.GetAllUser();//处理Info.v2return中得到的信息，输出为Info.allUser
            SetDataGrid();//以Info.allUser刷新数据表格
        }
        ObservableCollection<Users> userList;
        public void SetDataGrid()
        {
            userList = new ObservableCollection<Users>();
            if (Info.allUser == null || Info.allUser.Length == 0)
            { return; }//Info.allUser中无数据
            for (int i = 0; i < Info.allUser.Length / 5; i++)//将Info.allUser填入userList
            {
                if (Info.setFlash == 1)//判断是否输出实时流量
                {
                    userList.Add(new Users()
                    {
                        User = Info.allUser[i, 0],
                        Upload = Info.allUser[i, 1],
                        Download = Info.allUser[i, 2],
                        Byte = Convert.ToInt64(Info.allUser[i, 3]),
                        Traffic = Info.allUser[i, 4],
                    });
                }
                else
                {
                    userList.Add(new Users()
                    {
                        User = Info.allUser[i, 0],
                        Upload = Info.allUser[i, 1],
                        Download = Info.allUser[i, 2],
                        Byte = Convert.ToInt64(Info.allUser[i, 3]),
                    });
                }
            }
            DataGridSort("Byte", ListSortDirection.Descending);//以userList刷新排序数据表格
        }
        public void DataGridSort(string ColumnName, ListSortDirection DescOrAsce)
        {
            void action1()
            {
                dataGrid.ItemsSource = userList;//刷新表格
                ICollectionView v = CollectionViewSource.GetDefaultView(userList);
                v.SortDescriptions.Clear();
                v.SortDescriptions.Add(new SortDescription(ColumnName, DescOrAsce));
                v.Refresh();
                dataGrid.ColumnFromDisplayIndex(4).SortDirection = DescOrAsce;//排序表格
                checkBox_StartFlash.IsEnabled = true;
            }
            void action2()
            {
                checkBox_StartFlash.IsEnabled = true;
                button.IsEnabled = true;
                button_process.IsEnabled = true;
            }
            if (Info.close == 1)
            { return; }
            if (Info.wait == 1)
            { Dispatcher.InvokeAsync(action2); return; }
            dataGrid.Dispatcher.InvokeAsync(action1);
        }
        public System.Timers.Timer t;
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox_StartFlash.IsChecked == true)
            {
                Info.wait = 0;
                button.IsEnabled = false;
                button_process.IsEnabled = false;
                checkBox_StartFlash.IsEnabled = false;
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
                button.IsEnabled = false;
                button_process.IsEnabled = false;
                checkBox_StartFlash.IsEnabled = false;
                Info.setFlash = 0;
                Info.wait = 1;
            }
        }
        public void StartSetDataGrid(object source, ElapsedEventArgs e)
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
        private void Window_Closed(object sender, EventArgs e)
        {
            Info.close = 1;
        }
        private void CheckBox_Up_Click(object sender, RoutedEventArgs e)
        {
            if(checkBox_Up.IsChecked == true)
            {
                MainWindows.Topmost = true;
            }
            else
            {
                MainWindows.Topmost = false;
            }
        }
    }
    public class Users
    {
        public string User { get; set; }
        public string Upload { get; set; }
        public string Download { get; set; }
        public string Traffic { get; set; }
        public long Byte { get; set; }
    }
    public class MainProgram
    {
        public static void GetInfo()
        {
            Info.isOK = 0;
            while (Info.isOK == 0)//循环获取数据
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
                if (Info.setFlash == 0)//跳出循环
                {
                    Info.isOK = 1;
                    break;
                }
                GetAllUser();//处理Info.v2return，输出为Info.allUser
                Thread.Sleep(1000);
            }
            
        }
        public static string[] SplitOutputInfo(string str)
        {
            int numOfInfo = 0;//信息块总数
            int ifIndex = 0;
            while ((ifIndex = str.IndexOf("stat:", ifIndex)) != -1)
            {
                numOfInfo++;
                ifIndex += 5;
            }
            if (numOfInfo == 0)//取得信息为空
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
            return baseInfo;//返回分割完成的信息块数组
        }
        public static string HumanReadableSize(double size)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int o = 0;
            while (size >= 1024 && o < sizes.Length - 1)
            {
                o++;
                size /= 1024;
            }
            string result = string.Format("{0:0.##} {1}", size, sizes[o]);
            return result;
        }
        public static void GetAllUser()
        {
            string[] baseInfo = SplitOutputInfo(Info.v2return);//分割原始信息为块
            if (baseInfo == null || baseInfo.Length == 0)
            { return; }//获得数据为空
            string cache;
            string[] cache2;
            string[,] result = new string[baseInfo.Length, 5];
            Info.userNum = 0;
            int inboundNum = 0;//暂时无用
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
                result[i, 4] = HumanReadableSize(Convert.ToDouble(cache));//转为可阅读文本
            }

            Info.allUser = new string[Info.userNum / 2, 5];
            int yesNo;
            int allUserIndex = 0;
            for (int i = 0; i < result.Length / 5; i++)
            {
                if (result[i, 0] != "user")
                { continue; }
                yesNo = 0;

                for (int ii = 0; ii < Info.allUser.Length / 5; ii++)
                {
                    if (Info.allUser[ii, 0] == result[i, 1])//查找是否已有该用户
                    {
                        yesNo = 1;
                        if (result[i, 2] == "uplink")
                        {
                            Info.allUser[ii, 1] = result[i, 4];
                            long a = Convert.ToInt64(Info.allUser[ii, 3]) + Convert.ToInt64(result[i, 3]);
                            Info.allUser[ii, 3] = a.ToString();
                        }
                        else
                        {
                            Info.allUser[ii, 2] = result[i, 4];
                            long a = Convert.ToInt64(Info.allUser[ii, 3]) + Convert.ToInt64(result[i, 3]);
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
            if (Info.setFlash == 1)
            {
                if (Info.oldAllUser == null)
                { }//没有上一次数据
                else
                {
                    int get = 0;
                    for (int i = 0; i < Info.allUser.Length / 5; i++)
                    {
                        for (int ii = 0; ii < Info.oldAllUser.Length / 5; ii++)
                        {
                            get = 0;
                            if (Info.allUser[i, 0] == Info.oldAllUser[ii, 0])
                            {
                                double a = Convert.ToDouble(Convert.ToInt64(Info.allUser[i, 3]) - Convert.ToInt64(Info.oldAllUser[ii, 3]));//总流量字节变化
                                Info.allUser[i, 4] = HumanReadableSize(a) + "/s";
                                get = 1;
                                break;
                            }
                        }
                        if (get == 0)
                        {
                            Info.allUser[i, 4] = "";
                        }
                    }
                }
            }

            Info.oldAllUser = Info.allUser;
        }
    }

    
}
