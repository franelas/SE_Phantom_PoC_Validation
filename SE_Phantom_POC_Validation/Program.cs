using SE_Phantom_PoC_Validation.Structs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SE_Phantom_PoC_Validation
{
    class Program
    {
        static void Main(string[] args)
        {
            //Controller.InsertCompanies();
            //Controller.InsertEmployees();
            Controller.CheckCompanies();
        }
    }
}
