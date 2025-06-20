﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using MaterialSkin.Controls;
using System.Threading;
using System.Collections;
using System.Collections.ObjectModel;
using OpenQA.Selenium.Interactions;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using OfficeOpenXml;
using WASender.enums;
using WASender.Models;
using WASender.Model;

namespace WASender
{
    public partial class OldClassic : UserControl
    {

        InitStatusEnum initStatusEnum;
        System.Windows.Forms.Timer timerInitChecker;
        public IWebDriver driver;
        BackgroundWorker worker;
        CampaignStatusEnum campaignStatusEnum;
        Logger logger;
        List<GMapModel> gMapModelList;
        public bool grabEmailId = false;
        private List<GMapGlobal> inputs;
        GeneralSettingsModel generalSettingsModel;
        GmExtractorForm gmExtractorForm;
        public OldClassic(GmExtractorForm _gmExtractorForm)
        {
            InitializeComponent();
            gmExtractorForm = _gmExtractorForm;
        }

        private void OldClassic_Load(object sender, EventArgs e)
        {
            initLanguage();
            logger = new Logger("GMAPExtractor");
            init();
            gMapModelList = new List<GMapModel>();
            //CHeckForActivation();
        }

        private void init()
        {
            ChangeInitStatus(InitStatusEnum.NotInitialised);
            ChangeCampStatus(CampaignStatusEnum.NotStarted);
        }

        private void ChangeCampStatus(CampaignStatusEnum _campaignStatus)
        {
            AutomationCommon.ChangeCampStatus(_campaignStatus, lblRunStatus);
        }

        private void ChangeInitStatus(InitStatusEnum _initStatus)
        {
            logger.WriteLog("ChangeInitStatus = " + _initStatus.ToString());
            this.initStatusEnum = _initStatus;
            AutomationCommon.ChangeInitStatus(_initStatus, lblInitStatus);
        }

        private void initLanguage()
        {


            getSelectedLanguage();
            //SetLanguagesDropdown();




            this.materialLabel2.Text = Strings.Clickbellowbuttontoopenbrowser;
            this.label5.Text = Strings.Status;
            this.materialLabel1.Text = Strings.Usethatwindowtosearchforbusinessesandwhensearchresultsareshown;
            this.materialButton1.Text = Strings.StartGrabbing;
            this.btnInitWA.Text = Strings.Start;
            this.materialButton2.Text = Strings.Stop;
            materialButton3.Text = Strings.Export;

            label2.Text = Strings.Count;
            dataGridView1.Columns[0].HeaderText = Strings.Name;
            dataGridView1.Columns[1].HeaderText = Strings.MobileNumber;
            dataGridView1.Columns[2].HeaderText = Strings.ReviewCount;
            dataGridView1.Columns[3].HeaderText = Strings.RatingCount;
            dataGridView1.Columns[4].HeaderText = Strings.Catagory;
            dataGridView1.Columns[5].HeaderText = Strings.Address;
            dataGridView1.Columns[6].HeaderText = Strings.Website;
            dataGridView1.Columns[7].HeaderText = Strings.EmailId;
            dataGridView1.Columns[9].HeaderText = Strings.PlusCode;

            dataGridView1.Columns[9].HeaderText = Strings.clossinghour;
            dataGridView1.Columns[10].HeaderText = Strings.latitude;
            dataGridView1.Columns[11].HeaderText = Strings.longitude;
            dataGridView1.Columns[12].HeaderText = Strings.instagramprofile;
            dataGridView1.Columns[13].HeaderText = Strings.facebookprofile;
            dataGridView1.Columns[14].HeaderText = Strings.linkedinprofile;
            dataGridView1.Columns[15].HeaderText = Strings.twitterprofile;

            dataGridView1.Columns[16].HeaderText = Strings.ImagesFolder;

            materialCheckbox1.Text = Strings.GrabEmailId;
            materialCheckbox2.Text = Strings.GrabImages;


            materialButton4.Text = Strings.ImportNumbers;
            materialButton5.Text = Strings.ExportNumbersOnly;
        }


        private void getSelectedLanguage()
        {
            string settingPath = Config.GetGeneralSettingsFilePath();

            if (!File.Exists(settingPath))
            {
                File.Create(settingPath).Close();
            }
            generalSettingsModel = new GeneralSettingsModel();
            generalSettingsModel.selectedLanguage = "English";
            try
            {
                string GeneralSettingJson = "";
                using (StreamReader r = new StreamReader(settingPath))
                {
                    GeneralSettingJson = r.ReadToEnd();
                }
                var dict = JsonConvert.DeserializeObject<GeneralSettingsModel>(GeneralSettingJson);
                if (dict != null)
                {
                    generalSettingsModel = dict;
                }
                if (generalSettingsModel.selectedLanguage == null || generalSettingsModel.selectedLanguage == "")
                {
                    generalSettingsModel.selectedLanguage = "English";
                }
                Strings.selectedLanguage = generalSettingsModel.selectedLanguage;
            }
            catch (Exception ex)
            {

            }

        }

        private void btnInitWA_Click(object sender, EventArgs e)
        {

            InputDialog id = new InputDialog(this);
            id.ShowDialog();
        }

        public void InputReturnList(List<GMapGlobal> _inputs)
        {
            inputs = _inputs;
            InputReturn(inputs[0].searchQuery);
        }
        public void InputReturn(string searchTurm)
        {
            logger.WriteLog("btnInitWA_Click");
            ChangeInitStatus(InitStatusEnum.Initialising);
            grabEmailId = materialCheckbox1.Checked;
            try
            {
                //Config.KillChromeDriverProcess();
                var chromeDriverService = ChromeDriverService.CreateDefaultService(Config.GetChromeDriverFolder());
                chromeDriverService.HideCommandPromptWindow = true;



                driver = new ChromeDriver(chromeDriverService, Config.GetChromeOptions());
                try
                {
                    //driver.Url = "https://www.google.com/search?q=" + searchTurm + "&biw=1418&bih=679&tbm=lcl";
                    driver.Url = "https://www.google.com/search?q=" + searchTurm + "&tbm=lcl&hl=en";

                }
                catch (Exception ex)
                {

                }
                ChangeInitStatus(InitStatusEnum.Initialised);

            }
            catch (Exception ex)
            {
                ChangeInitStatus(InitStatusEnum.Unable);
                logger.WriteLog(ex.Message);
                logger.WriteLog(ex.StackTrace);

                if (ex.Message.Contains("session not created"))
                {
                    DialogResult dr = MessageBox.Show("Your Chrome Driver and Google Chrome Version Is not same, You can upgrade your ChromeDriver from Setting  Click 'Yes botton' to Open settings ", "Error ", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error);
                    //if (dr == DialogResult.Yes)
                    //{
                    //    System.Diagnostics.Process.Start("https://medium.com/fusionqa/selenium-webdriver-error-sessionnotcreatederror-session-not-created-this-version-of-7b3a8acd7072");
                    //}
                    //GeneralSettings generalSettings = new GeneralSettings(this.gmExtractorForm);
                    //generalSettings.ShowDialog();

                }
                else if (ex.Message.Contains("invalid argument: user data directory is already in use"))
                {
                    //Config.KillChromeDriverProcess();
                    MaterialSnackBar SnackBarMessage = new MaterialSnackBar("Please Close All Previous Sessions and Browsers if open, Then try again", Strings.OK, true);
                    SnackBarMessage.Show(this);
                }
                else
                {
                    MessageBox.Show("Error", ex.Message);
                }
            }
        }

        private void materialButton1_Click(object sender, EventArgs e)
        {
            if (initStatusEnum != InitStatusEnum.Initialised)
            {
                Utils.showAlert(Strings.PleasefollowStepNo1FirstInitialiseWhatsapp, Alerts.Alert.enmType.Error);
                return;
            }
            //else if (!IsResultShown())
            //{
            //    Utils.showAlert(Strings.PleaseSearchsomething, Alerts.Alert.enmType.Info);
            //}
            else if (campaignStatusEnum != CampaignStatusEnum.Running)
            {
                doPerform();


            }
            else
            {
                Utils.showAlert(Strings.Processisalreadyrunning, Alerts.Alert.enmType.Info);
            }
        }

        bool isStop = false;
        private void doPerform()
        {
            if (isStop == false)
            {
                if (inputs.Where(x => x.isDone == true).Count() == inputs.Count())
                {
                    ChangeCampStatus(CampaignStatusEnum.Finish);
                }
                for (int i = 0; i < inputs.Count(); i++)
                {
                    if (inputs[i].isDone == false)
                    {
                        logger.WriteLog("btnInitWA_Click");
                        ChangeInitStatus(InitStatusEnum.Initialising);
                        grabEmailId = materialCheckbox1.Checked;
                        try
                        {
                            try
                            {
                                //driver.Url = "https://www.google.com/search?q=" + searchTurm + "&biw=1418&bih=679&tbm=lcl";
                                driver.Url = "https://www.google.com/search?q=" + inputs[i].searchQuery + "&tbm=lcl&hl=en";
                                inputs[i].isDone = true;
                                Thread.Sleep(2000);
                                i = inputs.Count;
                                isStop = false;
                                grabEmailId = materialCheckbox1.Checked;
                                initBackgroundWorker();
                                worker.RunWorkerAsync();
                                ChangeCampStatus(CampaignStatusEnum.Running);
                            }
                            catch (Exception ex)
                            {

                            }
                            ChangeInitStatus(InitStatusEnum.Initialised);

                        }
                        catch (Exception ex)
                        {
                            ChangeInitStatus(InitStatusEnum.Unable);
                            logger.WriteLog(ex.Message);
                            logger.WriteLog(ex.StackTrace);

                            string ss = "";
                            if (ex.Message.Contains("session not created"))
                            {
                                DialogResult dr = MessageBox.Show("Your Chrome Driver and Google Chrome Version Is not same, You can upgrade your ChromeDriver from Setting  Click 'Yes botton' to Open settings ", "Error ", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error);
                                //if (dr == DialogResult.Yes)
                                //{
                                //    System.Diagnostics.Process.Start("https://medium.com/fusionqa/selenium-webdriver-error-sessionnotcreatederror-session-not-created-this-version-of-7b3a8acd7072");
                                //}
                                //GeneralSettings generalSettings = new GeneralSettings(this.gmExtractorForm);
                                //generalSettings.ShowDialog();
                            }
                            else if (ex.Message.Contains("invalid argument: user data directory is already in use"))
                            {
                                //Config.KillChromeDriverProcess();
                                MaterialSnackBar SnackBarMessage = new MaterialSnackBar("Please Close All Previous Sessions and Browsers if open, Then try again", Strings.OK, true);
                                SnackBarMessage.Show(this);
                            }
                            else
                            {
                                MessageBox.Show("Error", ex.Message);
                            }
                        }
                    }
                }

            }

        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {

            }
            doPerform();
        }


        private void initBackgroundWorker()
        {
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;

            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

            initTimer();
        }

        System.Windows.Forms.Timer timerRunner;
        private void initTimer()
        {
            timerRunner = new System.Windows.Forms.Timer();
            timerRunner.Interval = 1000;
            timerRunner.Tick += timerRunnerChecker_Tick;
            timerRunner.Start();
        }

        public void timerRunnerChecker_Tick(object sender, EventArgs e)
        {
            try
            {
                int i = 1;
                label1.Text = gMapModelList.Count().ToString();
                foreach (var item in gMapModelList)
                {
                    if (item.Logged == false)
                    {
                        var globalCounter = dataGridView1.Rows.Count - 1;
                        dataGridView1.Rows.Add();
                        dataGridView1.Rows[globalCounter].Cells[0].Value = item.Name;


                        dataGridView1.Rows[globalCounter].Cells[1].Value = item.mobilenumber;
                        dataGridView1.Rows[globalCounter].Cells[2].Value = item.reviewCount;
                        dataGridView1.Rows[globalCounter].Cells[3].Value = item.rating;
                        dataGridView1.Rows[globalCounter].Cells[4].Value = item.category;
                        dataGridView1.Rows[globalCounter].Cells[5].Value = item.address;
                        dataGridView1.Rows[globalCounter].Cells[6].Value = item.website;
                        dataGridView1.Rows[globalCounter].Cells[7].Value = item.email;
                        dataGridView1.Rows[globalCounter].Cells[8].Value = item.PlusCode;

                        dataGridView1.Rows[globalCounter].Cells[9].Value = item.closingHour;
                        dataGridView1.Rows[globalCounter].Cells[10].Value = item.latitude;
                        dataGridView1.Rows[globalCounter].Cells[11].Value = item.longitude;
                        dataGridView1.Rows[globalCounter].Cells[12].Value = item.instagramprofile;
                        dataGridView1.Rows[globalCounter].Cells[13].Value = item.facebookprofile;
                        dataGridView1.Rows[globalCounter].Cells[14].Value = item.linkedinprofile;
                        dataGridView1.Rows[globalCounter].Cells[15].Value = item.twitterprofile;
                        dataGridView1.Rows[globalCounter].Cells[16].Value = item.imagesFolder;

                        dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.RowCount - 1;
                        item.Logged = true;
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }


        private string GetAttributeMulti(By by, string attr)
        {
            if (AutomationCommon.IsElementPresent(by, driver))
            {
                var els = driver.FindElements(by);
                string value = "";
                foreach (var el in els)
                {
                    String val = el.GetAttribute(attr);
                    if (val != null && val != null && (!val.Contains("google")))
                    {
                        value = val;
                    }

                }
                return value;
            }
            return "";
        }

        private string GetString(By by)
        {
            if (AutomationCommon.IsElementPresent(by, driver))
            {
                IWebElement el = driver.FindElement(by);
                return el.Text;
            }
            return "";
        }



        private bool ispopupopen()
        {
            bool displayed;
            try
            {
                displayed = driver.FindElement(By.CssSelector("div[jsname = 'qUvFee']")).Displayed;
            }
            catch (Exception exception)
            {
                //ProjectData.SetProjectError(exception);
                displayed = false;
                // ProjectData.ClearProjectError();
            }
            return displayed;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {

            //logger.WriteLog("Started Grabbing chat list");

            IJavaScriptExecutor jsFunction = (IJavaScriptExecutor)driver;
            jsFunction.ExecuteScript("function getElementByXpath(path) { return document.evaluate(path, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue; }");

            int needtoSkip = 0;
            int gloibalCounter = 0;
            while (!isStop)
            {
                try
                {
                    ArrayList laglist = new ArrayList();
                    GMapModel gMapModel;
                    ArrayList latlist = new ArrayList();

                    ReadOnlyCollection<IWebElement> results = driver.FindElements(By.XPath(XPathStore.GMap_Result));
                    ReadOnlyCollection<IWebElement> latlong = driver.FindElements(By.CssSelector("div.rllt__mi"));

                    try
                    {
                        for (int gr = 0; gr < latlong.Count; gr++)
                        {
                            try
                            {
                                string lat = latlong[gr].GetAttribute("data-lat");
                                string log = latlong[gr].GetAttribute("data-lng");
                                latlist.Add(lat);
                                laglist.Add(log);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                    catch (Exception ex5)
                    {
                        // Log("latlog -" + ex5.Message.ToString() + ex5.StackTrace.ToString());
                    }

                    int i = 0;
                    bool isLoopBreaked = false;
                    foreach (var item in results)
                    {
                        if (isStop)
                        {
                            break;
                        }
                        if (!isStop)
                        {
                            var titiles = item.FindElements(By.ClassName("OSrXXb"));
                            bool canBreak = false;
                            foreach (var title in titiles)
                            {
                                string stringtitle = title.Text;
                                int Existcount = gMapModelList.Where(x => x.Name == stringtitle).Count();
                                if (Existcount > 0)
                                {
                                    canBreak = true;
                                }
                            }
                            if (canBreak)
                            {
                                continue;
                            }


                            gMapModel = new GMapModel();
                            try
                            {
                                logger.WriteLog("item Click");

                                try
                                {
                                    Actions actions = new Actions(driver);
                                    IWebElement selectedlink = item.FindElement(By.CssSelector("a.rllt__link"));
                                    actions.MoveToElement(selectedlink).Click().Perform();
                                }
                                catch (Exception ex)
                                {
                                    //SvrollDown(driver);
                                    try
                                    {
                                        try
                                        {
                                            Actions actions = new Actions(driver);
                                            IWebElement selectedlink = item.FindElement(By.CssSelector("div.QU77pf"));
                                            actions.MoveToElement(selectedlink).Click().Perform();
                                        }
                                        catch (Exception)
                                        {

                                        }

                                        Actions actionss = new Actions(driver);
                                        IWebElement selectedlinks = item.FindElement(By.CssSelector("div.uMdZh"));
                                        actionss.MoveToElement(selectedlinks).Click().Perform();

                                        Thread.Sleep(1000);
                                        item.Click();
                                    }
                                    catch (Exception exd)
                                    {
                                        // break;
                                        isLoopBreaked = true;
                                        break;
                                    }
                                }

                                Thread.Sleep(3000);
                                int retryAttempt = 0;
                                //isLoopBreaked
                                while (driver.FindElements(By.CssSelector("div.rllt__local-item-selected")).Count <= 0)
                                {
                                    if (retryAttempt >= 4)
                                    {
                                        isLoopBreaked = true;
                                        break;
                                    }
                                    try
                                    {
                                        retryAttempt++;
                                        item.Click();

                                    }
                                    catch (Exception)
                                    {

                                    }
                                    Thread.Sleep(3000);
                                }
                                if (latlist.Count > i)
                                {
                                    gMapModel.latitude = (string)latlist[i];
                                    gMapModel.longitude = (string)laglist[i];
                                }
                                if (!ispopupopen())
                                {
                                    Thread.Sleep(3000);
                                    if (!ispopupopen())
                                        continue;
                                }
                                ReadOnlyCollection<IWebElement> popupdiv = driver.FindElements(By.CssSelector("div.xpdopen"));

                                if (popupdiv.Count > 0)
                                {

                                    if (AutomationCommon.IsElementPresent(By.XPath(XPathStore.GMap_Heading), driver))
                                    {
                                        logger.WriteLog("Heading is present");
                                    }
                                    else
                                    {
                                        logger.WriteLog("Heading is not present");
                                    }

                                    By GMap_HeadingBy = By.XPath(XPathStore.GMap_Heading);
                                    By GMap_MobileNumberBy = By.XPath(XPathStore.GMap_MobileNumber);

                                    By GMap_AddressBy = By.XPath(XPathStore.GMap_Address);
                                    By GMap_WebSiteBy = By.XPath("//a[contains(@class,'mI8Pwc')] | //a[contains(@class,'n1obkb')]");
                                    By GMap_PlusCodeBy = By.XPath(XPathStore.GMap_PlusCode);
                                    By GMap_RatingBy = By.XPath(XPathStore.GMap_Rating);

                                    By GMap_ReviewCountBy = By.XPath(XPathStore.GMap_ReviewCount);
                                    By GMap_CatagoryBy = By.XPath(XPathStore.GMap_Catagory);
                                    By GMap_ClosingHoursBy = By.XPath(XPathStore.GMap_ClosingHours);

                                    try
                                    {
                                        bool icChecked = materialCheckbox2.Checked;

                                        if (icChecked)
                                        {

                                            string _tempFolderpath = Config.GetTempFolderPath();

                                            string NewGuidFolderPath = _tempFolderpath + "\\" + Guid.NewGuid().ToString();
                                            Directory.CreateDirectory(NewGuidFolderPath);


                                            By GMap_ImageMain = By.XPath("//div[contains(@class,'jls5X')] | //div[contains(@class,'Rbx14')]");

                                            if (AutomationCommon.IsElementPresent(GMap_ImageMain, driver))
                                            {
                                                IWebElement ImageMain = item.FindElement(GMap_ImageMain);
                                                ImageMain.Click();

                                                By GMap_Images = By.XPath("//img[contains(@class,'m7eMIc')] ");

                                                AutomationCommon.WaitUntilElementVisible(driver, GMap_Images, 5);

                                                ReadOnlyCollection<IWebElement> Images = driver.FindElements(GMap_Images);

                                                int ImageCount = 1;
                                                foreach (var image in Images)
                                                {
                                                    try
                                                    {
                                                        //image.Click();
                                                        //Thread.Sleep(500);

                                                        string _src = image.GetAttribute("src");
                                                        string[] splitter = _src.Split('=');

                                                        string finalIMageUrl = splitter[0] + "=s680-w680-h510";
                                                        using (WebClient client = new WebClient())
                                                        {
                                                            client.DownloadFile(new Uri(finalIMageUrl), NewGuidFolderPath + "\\" + ImageCount.ToString() + ".png");
                                                            ImageCount++;
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {

                                                        try
                                                        {
                                                            string _src = image.GetAttribute("src");
                                                            string[] splitter = _src.Split(new string[] { "&w=" }, StringSplitOptions.None); ;

                                                            string finalIMageUrl = splitter[0] + "&w=512&h=512";
                                                            using (WebClient client = new WebClient())
                                                            {
                                                                client.DownloadFile(new Uri(finalIMageUrl), NewGuidFolderPath + "\\" + ImageCount.ToString() + ".png");
                                                                ImageCount++;
                                                            }
                                                        }
                                                        catch (Exception exdd)
                                                        {

                                                        }
                                                    }
                                                }
                                                Actions action = new Actions(driver);
                                                action.SendKeys(OpenQA.Selenium.Keys.Escape);


                                                By GMap_ImagesHeader = By.XPath("//div[contains(@class,'hCEX6e')]");
                                                if (AutomationCommon.IsElementPresent(GMap_ImagesHeader, driver))
                                                {
                                                    IWebElement _imagesHeader = driver.FindElement(GMap_ImagesHeader);

                                                    By GMap_ImagesHeaderCloseButton = By.XPath("//span[contains(@class,'jA3abb')]");

                                                    IWebElement ImagesHeaderCloseButton = _imagesHeader.FindElement(GMap_ImagesHeaderCloseButton);
                                                    ImagesHeaderCloseButton.Click();
                                                }
                                                gMapModel.imagesFolder = NewGuidFolderPath;

                                            }
                                        }



                                    }
                                    catch (Exception ex)
                                    {
                                        string ssss = "";
                                    }
                                    AutomationCommon.WaitUntilElementVisible(driver, GMap_HeadingBy, 5);


                                    gMapModel.Name = GetString(GMap_HeadingBy);

                                    string MobileNumber = GetString(GMap_MobileNumberBy);
                                    if (MobileNumber.StartsWith("0"))
                                    {
                                        MobileNumber = MobileNumber.Substring(1);
                                    }

                                    MobileNumber = MobileNumber.Replace(@" ", "");
                                    MobileNumber = MobileNumber.Replace(@"(", "");
                                    MobileNumber = MobileNumber.Replace(@")", "");
                                    MobileNumber = MobileNumber.Replace(@"+", "");
                                    MobileNumber = MobileNumber.Replace(@"-", "");
                                    gMapModel.mobilenumber = MobileNumber;
                                    gMapModel.address = GetString(GMap_AddressBy);
                                    gMapModel.website = GetAttributeMulti(GMap_WebSiteBy, "href");
                                    gMapModel.PlusCode = GetString(GMap_PlusCodeBy);
                                    gMapModel.rating = GetString(GMap_RatingBy);
                                    gMapModel.reviewCount = GetString(GMap_ReviewCountBy);
                                    gMapModel.category = GetString(GMap_CatagoryBy);


                                    try
                                    {
                                        if (popupdiv[0].FindElements(By.CssSelector("span.BTP3Ac")).Count > 0)
                                        {
                                            popupdiv[0].FindElement(By.CssSelector("span.BTP3Ac")).Click();
                                            string clossinghour = popupdiv[0].FindElement(By.XPath("//table[contains(@class,'WgFkxc')]")).Text;
                                            gMapModel.closingHour = clossinghour.Replace("\r\n", " ");
                                        }
                                    }
                                    catch (Exception)
                                    {

                                    }

                                    if (gMapModel.website != "" && gMapModel.website != null && grabEmailId == true)
                                    {
                                        try
                                        {
                                            gMapModel.email = EmailExtractor.GetEmail(gMapModel.website, new string[8] { "contact", "contactus", "contacty", "kontakt", "conta", "contacts", "cont", "contact_us" });
                                        }
                                        catch (Exception)
                                        {

                                        }
                                    }



                                    try
                                    {
                                        ReadOnlyCollection<IWebElement> profilelink = driver.FindElements(By.CssSelector("g-link"));

                                        if (profilelink.Count > 0)
                                        {
                                            for (int ij = 0; ij < profilelink.Count; ij++)
                                            {
                                                string profilename = profilelink[ij].Text;
                                                if (profilename.Equals("Instagram"))
                                                {
                                                    gMapModel.instagramprofile = profilelink[ij].FindElement(By.CssSelector("a")).GetAttribute("href");
                                                }
                                                if (profilename.Equals("Facebook"))
                                                {
                                                    gMapModel.facebookprofile = profilelink[ij].FindElement(By.CssSelector("a")).GetAttribute("href");
                                                }
                                                if (profilename.Equals("LinkedIn"))
                                                {
                                                    gMapModel.linkedinprofile = profilelink[ij].FindElement(By.CssSelector("a")).GetAttribute("href");
                                                }
                                                if (profilename.Equals("Twitter"))
                                                {
                                                    gMapModel.twitterprofile = profilelink[ij].FindElement(By.CssSelector("a")).GetAttribute("href");
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {


                                    }

                                    gMapModel.Logged = false;
                                    int Existcount = gMapModelList.Where(x => x.Name == gMapModel.Name && x.mobilenumber == gMapModel.mobilenumber).Count();
                                    if (Existcount == 0)
                                    {
                                        gMapModelList.Add(gMapModel);
                                    }
                                    else
                                    {
                                        logger.WriteLog("Same Name Exist");
                                    }

                                }



                            }
                            catch (Exception ex)
                            {
                                string ss = "";
                                logger.WriteLog("ex= " + ex.Message);
                                i = i - 1;
                            }

                        }

                        i = i + 1;
                    }

                    try
                    {
                        if (isLoopBreaked == false)
                        {
                            logger.WriteLog("checking Next button ");


                            if (driver.FindElements(By.CssSelector("a#pnnext")).Count <= 0)
                            {
                                //break;
                                //
                                string ss = "";
                                e.Cancel = true;
                                worker.CancelAsync();
                                return;
                            }
                            string str = driver.Url.ToString();
                            driver.FindElement(By.CssSelector("a#pnnext")).Click();
                            Thread.Sleep(3000);
                            while (driver.Url.ToString() == str)
                            {
                                Thread.Sleep(3000);
                                Application.DoEvents();
                            }
                        }
                        else
                        {
                            isLoopBreaked = false;
                            //driver.Navigate().Refresh();

                            results = driver.FindElements(By.XPath(XPathStore.GMap_Result));
                            latlong = driver.FindElements(By.CssSelector("div.rllt__mi"));
                            Thread.Sleep(3000);
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.WriteLog("ex= " + ex.Message);
                    }



                }
                catch (Exception ex)
                {
                    string ssss = "";
                    logger.WriteLog("ex= " + ex.Message);
                }

            }

        }

        private void materialButton2_Click(object sender, EventArgs e)
        {
            timerRunner.Stop();
            worker.CancelAsync();
            isStop = true;
            ChangeCampStatus(CampaignStatusEnum.Stopped);
        }

        private void materialButton4_Click(object sender, EventArgs e)
        {
            String FolderPath = Config.GetTempFolderPath();
            String file = Path.Combine(FolderPath, "GMapData" + Guid.NewGuid().ToString() + ".json");
            string NewFileName = file.ToString();
            string json = JsonConvert.SerializeObject(gMapModelList.ToArray(), Formatting.Indented);
            File.WriteAllText(NewFileName, json);

            savesampleExceldialog.FileName = "GMapData.json";
            savesampleExceldialog.Filter = "JSON file (*.json)|*.json";
            if (savesampleExceldialog.ShowDialog() == DialogResult.OK)
            {
                File.Copy(NewFileName, savesampleExceldialog.FileName.EndsWith(".json") ? savesampleExceldialog.FileName : savesampleExceldialog.FileName + ".json", true);
                Utils.showAlert(Strings.Filedownloadedsuccessfully, Alerts.Alert.enmType.Success);
            }
        }

        private void materialButton3_Click(object sender, EventArgs e)
        {
            String FolderPath = Config.GetTempFolderPath();
            String file = Path.Combine(FolderPath, "GMapData" + Guid.NewGuid().ToString() + ".xlsx");
            string NewFileName = file.ToString();
            File.Copy("ChatListTemplate.xlsx", NewFileName, true);
            var newFile = new FileInfo(NewFileName);
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (ExcelPackage xlPackage = new ExcelPackage(newFile))
            {
                var ws = xlPackage.Workbook.Worksheets[0];

                ws.Cells[1, 1].Value = Strings.Name;
                ws.Cells[1, 2].Value = Strings.MobileNumber;
                ws.Cells[1, 3].Value = Strings.ReviewCount;
                ws.Cells[1, 4].Value = Strings.RatingCount;
                ws.Cells[1, 5].Value = Strings.Catagory;
                ws.Cells[1, 6].Value = Strings.Address;
                ws.Cells[1, 7].Value = Strings.Website;
                ws.Cells[1, 8].Value = Strings.EmailId;
                ws.Cells[1, 9].Value = Strings.PlusCode;

                ws.Cells[1, 10].Value = Strings.clossinghour;
                ws.Cells[1, 11].Value = Strings.latitude;
                ws.Cells[1, 12].Value = Strings.latitude;
                ws.Cells[1, 13].Value = Strings.instagramprofile;
                ws.Cells[1, 14].Value = Strings.facebookprofile;
                ws.Cells[1, 15].Value = Strings.linkedinprofile;
                ws.Cells[1, 16].Value = Strings.twitterprofile;
                ws.Cells[1, 17].Value = Strings.ImagesFolder;



                for (int i = 0; i < gMapModelList.Count(); i++)
                {
                    ws.Cells[i + 2, 1].Value = gMapModelList[i].Name;
                    ws.Cells[i + 2, 2].Value = gMapModelList[i].mobilenumber;
                    ws.Cells[i + 2, 3].Value = gMapModelList[i].reviewCount;
                    ws.Cells[i + 2, 4].Value = gMapModelList[i].rating;
                    ws.Cells[i + 2, 5].Value = gMapModelList[i].category;
                    ws.Cells[i + 2, 6].Value = gMapModelList[i].address;
                    ws.Cells[i + 2, 7].Value = gMapModelList[i].website;
                    ws.Cells[i + 2, 8].Value = gMapModelList[i].email;
                    ws.Cells[i + 2, 9].Value = gMapModelList[i].PlusCode;


                    ws.Cells[i + 2, 10].Value = gMapModelList[i].closingHour;
                    ws.Cells[i + 2, 11].Value = gMapModelList[i].latitude;
                    ws.Cells[i + 2, 12].Value = gMapModelList[i].longitude;
                    ws.Cells[i + 2, 13].Value = gMapModelList[i].instagramprofile;
                    ws.Cells[i + 2, 14].Value = gMapModelList[i].facebookprofile;
                    ws.Cells[i + 2, 15].Value = gMapModelList[i].linkedinprofile;
                    ws.Cells[i + 2, 16].Value = gMapModelList[i].twitterprofile;


                    if (gMapModelList[i].imagesFolder != null)
                    {
                        ws.Cells[i + 2, 17].Hyperlink = new Uri(gMapModelList[i].imagesFolder);
                        ws.Cells[i + 2, 17].Style.Font.Color.SetColor(Color.Blue);
                        ws.Cells[i + 2, 17].Style.Font.UnderLine = true;
                        ws.Cells[i + 2, 17].Value = gMapModelList[i].imagesFolder;
                    }

                }
                xlPackage.Save();
            }


            savesampleExceldialog.FileName = "GMapData.xlsx";
            savesampleExceldialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
            if (savesampleExceldialog.ShowDialog() == DialogResult.OK)
            {
                File.Copy(NewFileName, savesampleExceldialog.FileName.EndsWith(".xlsx") ? savesampleExceldialog.FileName : savesampleExceldialog.FileName + ".xlsx", true);
                Utils.showAlert(Strings.Filedownloadedsuccessfully, Alerts.Alert.enmType.Success);
            }
        }

        private void materialButton4_Click_1(object sender, EventArgs e)
        {
            this.gmExtractorForm.importNumbers(gMapModelList);
        }

        private void materialButton5_Click(object sender, EventArgs e)
        {
            String FolderPath = Config.GetTempFolderPath();
            String file = Path.Combine(FolderPath, "GMapData_Numbers" + Guid.NewGuid().ToString() + ".xlsx");
            string NewFileName = file.ToString();
            File.Copy("ChatListTemplate.xlsx", NewFileName, true);
            var newFile = new FileInfo(NewFileName);
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (ExcelPackage xlPackage = new ExcelPackage(newFile))
            {
                var ws = xlPackage.Workbook.Worksheets[0];

                ws.Cells[1, 1].Value = Strings.MobileNumber;
                int ActualCounter = 0;
                for (int i = 0; i < gMapModelList.Count(); i++)
                {
                    if (gMapModelList[i].mobilenumber != "")
                    {
                        ws.Cells[ActualCounter + 2, 1].Value = gMapModelList[i].mobilenumber;
                        ActualCounter++;
                    }

                }
                xlPackage.Save();
            }


            savesampleExceldialog.FileName = "GMapData.xlsx";
            savesampleExceldialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
            if (savesampleExceldialog.ShowDialog() == DialogResult.OK)
            {
                File.Copy(NewFileName, savesampleExceldialog.FileName.EndsWith(".xlsx") ? savesampleExceldialog.FileName : savesampleExceldialog.FileName + ".xlsx", true);
                Utils.showAlert(Strings.Filedownloadedsuccessfully, Alerts.Alert.enmType.Success);
            }
        }


        
    }
}
