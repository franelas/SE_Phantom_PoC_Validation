/*************************************************************************************************************************************\
|                                                             LIBRARIES                                                               |
\*************************************************************************************************************************************/

using SE_Phantom_PoC_Validation.Structs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SE_Phantom_PoC_Validation
{
    public static class Controller
    {
        /*****************************************************************************************************************************\
        |                                                      PRIVATE VARIABLES                                                      |
        \*****************************************************************************************************************************/
        private static WebDriver mainDriver;

        private const String CompanyPath = @"C:\test\Company\";

        private const String EmployeePath = @"C:\test\Employee\";

        private const String CompanySheet = "Hoja1";

        private const String EmployeeSheet = "Hoja1";

        private static String last_sotre_number = "";

        private static String LogDirectory = @"logs\";

        private static Dictionary<string, string> states = new Dictionary<string, string>();

        private static String resultsCompanyPathFile = @"C:\test\Employee\Company_Results_LOCKED.xlsx";

        /*****************************************************************************************************************************\
        |                                                      PUBLIC VARIABLES                                                       |
        \*****************************************************************************************************************************/
        public static String Messages = "";


        /*****************************************************************************************************************************\
        |                                                      PRIVATE FUNCTIONS                                                      |
        \*****************************************************************************************************************************/
        private static string doScreenShoot(String path)
        {
            String nameImage = path + "images\\" + DateTime.Now.ToString("dd-MM-yyyy HH.mm.ss tt") + ".bmp";
            Rectangle rect = new Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Bitmap bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            bmp.Save(nameImage, ImageFormat.Bmp);
            return nameImage;
        }

        private static string getStepName(int stepNumber)
        {
            String step = null;
            switch (stepNumber)
            {
                case 0: step = "Login, click on Add company button"; break;
                case 1: step = "Add Company, Step 1"; break;
                case 2: step = "Add Company, Step 2"; break;
                case 3: step = "Add Company, Step 3"; break;
                case 4: step = "Setup company, step 1"; break;
                case 5: step = "Setup company, step 2"; break;
                case 6: step = "Setup company, step 3"; break;
                case 7: step = "Setup company, step 4 (Add Federal)"; break;
                case 8: step = "Setup company, step 4 (Add State)"; break;
                case 9: step = "Setup company, step 4 (Add Local)"; break;
                case 10: step = "Setup company, step 5"; break;
                case 11: step = "Setup company, step 6"; break;
                case 12: step = "Setup company, step 7"; break;
                case 13: step = "Setup company, step 8"; break;
                case 14: step = "Setup company, step 9"; break;
                case 15: step = "SignOut"; break;
                default: step = "Unknow step"; break;
            }

            return step;
        }

        private static void loadDriver()
        {
            if (mainDriver != null)
            {
                mainDriver.Quit();
            }
            mainDriver = new WebDriver(Constants.Overall_Id.Url);
        }

        private static String getFile(String folderPath)
        {
            //Gets the path of a file with information which is not been used by another instance.
            //Files which are been used will have "LOCKED" into their nam file.

            try
            {
                String fileSelected = Directory.GetFiles(folderPath, "*.xlsx").Where(s => !s.Contains("LOCKED")).First();

                //Remove extension and add "_LOCKED" to the name
                String destFile = fileSelected.Replace(".xlsx", "_LOCKED.xlsx");

                //Rename the file in order to lock it
                File.Move(fileSelected, destFile);
                return destFile;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static void initializeStates()
        {
            states.Clear();

            states.Add("AL", "Alabama");
            states.Add("AK", "Alaska");
            states.Add("AZ", "Arizona");
            states.Add("AR", "Arkansas");
            states.Add("CA", "California");
            states.Add("CO", "Colorado");
            states.Add("CT", "Connecticut");
            states.Add("DE", "Delaware");
            states.Add("DC", "District of Columbia");
            states.Add("FL", "Florida");
            states.Add("GA", "Georgia");
            states.Add("HI", "Hawaii");
            states.Add("ID", "Idaho");
            states.Add("IL", "Illinois");
            states.Add("IN", "Indiana");
            states.Add("IA", "Iowa");
            states.Add("KS", "Kansas");
            states.Add("KY", "Kentucky");
            states.Add("LA", "Louisiana");
            states.Add("ME", "Maine");
            states.Add("MD", "Maryland");
            states.Add("MA", "Massachusetts");
            states.Add("MI", "Michigan");
            states.Add("MN", "Minnesota");
            states.Add("MS", "Mississippi");
            states.Add("MO", "Missouri");
            states.Add("MT", "Montana");
            states.Add("NE", "Nebraska");
            states.Add("NV", "Nevada");
            states.Add("NH", "New Hampshire");
            states.Add("NJ", "New Jersey");
            states.Add("NM", "New Mexico");
            states.Add("NY", "New York");
            states.Add("NC", "North Carolina");
            states.Add("ND", "North Dakota");
            states.Add("OH", "Ohio");
            states.Add("OK", "Oklahoma");
            states.Add("OR", "Oregon");
            states.Add("PA", "Pennsylvania");
            states.Add("RI", "Rhode Island");
            states.Add("SC", "South Carolina");
            states.Add("SD", "South Dakota");
            states.Add("TN", "Tennessee");
            states.Add("TX", "Texas");
            states.Add("UT", "Utah");
            states.Add("VT", "Vermont");
            states.Add("VA", "Virginia");
            states.Add("WA", "Washington");
            states.Add("WV", "West Virginia");
            states.Add("WI", "Wisconsin");
            states.Add("WY", "Wyoming");

        }

        private static  string parseState (string abbr)
        {
            if (states.ContainsKey(normalizeString(abbr)))
                return (states[normalizeString(abbr)]);
            /* error handler is to return an empty string rather than throwing an exception */
            return abbr;
        }

        private static String normalizeString(String input)
        {
            input = input.Trim();
            StringBuilder result = new StringBuilder();
            foreach (char c in input)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '&' || c == '-' || c == ' ' || c == '@')
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        /*****************************************************************************************************************************\
        |                                                      PUBLIC FUNCTIONS                                                       |
        \*****************************************************************************************************************************/
        public static void CheckCompanies()
        {

        }

    }
}