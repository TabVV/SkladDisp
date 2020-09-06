using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

using ScannerAll;

namespace SkladGP
{
    public partial class MainF : Form
    {

        public MainF(BarcodeScanner xSc)
        {
            InitializeComponent();
            xSc.BCInvoker = this;

            Point p;
            Size s;
            switch (xSc.nTermType)
            {
                case TERM_TYPE.HWELL6100:
                case TERM_TYPE.DL_SCORP:
                    p = new Point(136, 140);
                    s = new Size(72, 20);
                    break;
                default:
                    p = new Point(137, 141);
                    s = new Size(70, 18);
                    break;
            }
            InitializeDop(xSc, s, p);
        }

    }
}