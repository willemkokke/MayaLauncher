using System;
using System.Collections.Generic;
using System.Text;

namespace MayaLauncher
{    
    public class EntryPoint
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var app = new App();
            app.Run();
        }
    }
}
