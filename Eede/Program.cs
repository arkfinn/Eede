using Avalonia;
using System;
using System.Windows.Forms;

namespace Eede
{
    internal static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        private static void Main()
        {
            //System.Windows.Forms.Application.EnableVisualStyles();
            //System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            //System.Windows.Forms.Application.Run(new Form1());


            ApplicationConfiguration.Initialize();

            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseWin32()
                .With(new Win32PlatformOptions
                {

                    //                    UseWindowsUIComposition = false
                })
                .LogToTrace()
                .SetupWithoutStarting();
            System.Windows.Forms.Application.Run(new Form1());
        }
    }
}