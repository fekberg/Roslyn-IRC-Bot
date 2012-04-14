using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;


namespace Rossbot.Windows.Service
{
    [RunInstaller(true)]
    public partial class RoslynServiceInstaller : System.Configuration.Install.Installer
    {
        public RoslynServiceInstaller()
        {
            InitializeComponent();
        }
    }
}
