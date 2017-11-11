using System;
using System.Net;
using MunApp.Common;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace MunApp.Win32
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if !DEBUG
            try
            {
#endif
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
#if !DEBUG
                Application.ThreadException += (sender, e) =>
                {
                    Debug.Log(e.Exception);
                    MessageBox.Show("An error has occured! If the error was fatal, the application will automatically exit to prevent further damage. An error log file (called ErrorLog.dat) has been created containing information about the error. Please submit this file to the developer to better help diagnose the problem", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                };
                AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                {
                    Debug.Log(e.ExceptionObject as Exception);
                    MessageBox.Show("An error has occured! If the error was fatal, the application will automatically exit to prevent further damage. An error log file (called ErrorLog.dat) has been created containing information about the error. Please submit this file to the developer to better help diagnose the problem", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                };
#endif
                Data.Load();
                if(Data.FirstRun)
                {
                    Application.Run(new Intro());
                }
                else
                {
                    if (Data.GetMacAddress() == Data.CurrentMacAddress)
                    {
                        Application.Run(new MainWindow());
                    }
                    else
                    {
                        //Launch just activator
                        Application.Run(new MainWindow());
                    }
                }
#if !DEBUG
            }
            catch (Exception e)
            {
                Debug.Log(e);
                MessageBox.Show("An error has occured! If the error was fatal, the application will automatically exit to prevent further damage. An error log file (called ErrorLog.dat) has been created containing information about the error. Please submit this file to the developer to better help diagnose the problem", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#endif
        }
    }
}
