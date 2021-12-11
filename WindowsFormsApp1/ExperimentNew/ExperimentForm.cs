using EyeGaze;
using EyeGaze.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Experiment;

namespace EyeGaze
{
    public delegate void EndExperiment();
    public partial class ExperimentForm : Form
    {
        private EndExperiment _end;
        private Form _popUp;
        private int _expNum;
        private string _systemName;
        Controller controller;

        public ExperimentForm(Controller c)
        {
            InitializeComponent();
            List<Button> btns = new List<Button>(new Button[] { this.exp1Btn, this.exp2Btn, this.pilotBtn, this.CloseBtn, this.finishBtn,
                                                                this.videoBtn, this.SUSbtn, this.hepButton});
            foreach (Button b in btns)
            {
                b.MouseEnter += ChangeBackColorEnter;
                b.MouseLeave += ChangeBackColorLeave;
            }

            this.controller = c;
            controller.engineMain.mainExperiment = new MainClass();
            this.SetEndExpFunc((EndExperiment)(this.controller.engineMain.End));
            controller.key = "69a12462814f4df1a7b1d38c67963adf";
            controller.region = "westeurope";
            controller.speechToText = "EyeGaze.SpeechToText.MicrosoftCloudSpeechToText";
        }
        public void SetEndExpFunc(EndExperiment ee)
        {
            _end = ee;
        }
        private void ChangeBackColorEnter(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = Color.CadetBlue;
        }
        private void ChangeBackColorLeave(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = Color.Transparent;
        }

        private void finishBtn_Click(object sender, EventArgs e)
        {
            this._end();
            this.Dispose();
        }

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            this._end();
            this.Dispose();
            Environment.Exit(0);
        }

        private void IdBtn_Click(object sender, EventArgs e)
        {
            //Check Valid ID
            string id = this.IdTxtBox.Text;
            string username = this.UserameTxb.Text;
            bool valid = CheckInput(id, 9);

            if (valid)
            {
                this.IdTxtBox.Visible = false;
                this.ID_LBL.Visible = false;

                this.UserameTxb.Visible = false;
                this.usernameLbl.Visible = false;

                this.IdBtn.Visible = false;

                //Init App System

                //pilot visible
                this.EnableAll.Visible = true;
                this.pilotBtn.Visible = true;
                this.Pilot_LBL.Visible = true;
            }
            else
            {
                MessageBox.Show("Invalid ID Numebr", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private bool CheckInput(string inp, int numbers)
        {
            if(numbers == -1)
            {
                return inp.All(char.IsLetter);
            }
            else if(inp.Length == numbers)
            {
                return inp.All(char.IsDigit);
            }
            return false;
        }

        private void pilotBtn_Click(object sender, EventArgs e)
        {
            this.pilotBtn.Enabled = false;


            //Start Pilot
            _expNum = 0;
            RunAppSystem(0);
        }

        private void exp1Btn_Click(object sender, EventArgs e)
        {
            this.exp1Btn.Enabled = false;
            //Start Exp1
            _expNum = 1;
            RunAppSystem(1);
            
        }

        private void exp2Btn_Click(object sender, EventArgs e)
        {
            this.exp2Btn.Enabled = false;
            //Start Exp1
            _expNum = 2;
            RunAppSystem(2);
            // Add Link BTN releas
            
        }

        private void RunAppSystem(int expNumber)
        {
            End();

            if (expNumber != 0) { this.controller.engineMain.End(); }

            string tempSystemName = _systemName;


            MainClass mainExpreriment = controller.engineMain.mainExperiment;
            String path = mainExpreriment.GetPath(this.IdTxtBox.Text, this.UserameTxb.Text, tempSystemName, expNumber);

            controller.path = path;
            controller.StartProgram("EyeGaze.SpellChecker.WordSpell", controller.speechToText, _systemName,expNumber);
            Thread.Sleep(5000);

            mainExpreriment.StartExperiment(DateTime.Now);

        }

        private void End()
        {
            Invoke((MethodInvoker)delegate
            {
                if (_expNum == 0)
                {
                    this.FinishPilot();
                }
                else if (_expNum == 1)
                {
                    this.FinishExp1();
                }
                else if (_expNum == 2)
                {
                    this.FinishExp2();
                }
                else if (_expNum == 3)
                {
                    this.FinishExp3();
                }
            });
        }

        //public void GetMessageFromApp(UIMessageEvent uime)
        //{
        //    Console.WriteLine(uime.ToString());
        //    string content = "";
        //    if (uime._type == UIMessageType.CommandDetectDispaly)
        //    {
        //        Console.WriteLine("Command UI");
        //        content = $"{uime._command} {string.Join(" ", uime._content)}";
        //    }
        //    else if (uime._type == UIMessageType.ContinueCommandDispaly)
        //    {
        //        Console.WriteLine("Continue Command UI");
        //        content = $"{string.Join(" ", uime._content)}";
        //    }
        //    else
        //    {
        //        Console.WriteLine("Option UI");
        //        if (uime._content.Count > 0)
        //        {
        //            content += $"1. {uime._content[0]}\r\n\n";
        //            if (uime._content.Count > 1)
        //            {
        //                content += $"2. {uime._content[1]}\r\n\n";
        //                if (uime._content.Count > 2)
        //                {
        //                    content += $"3. {uime._content[2]}\r\n\n";
        //                }
        //            }
        //        }
        //    }
        //    Invoke((MethodInvoker)delegate
        //    {
        //        this.ShowPopUp(content, uime._type);
        //    });
        //    return;
        //}
        //private void ShowPopUp(string content, UIMessageType type)
        //{
        //    try
        //    {
        //        if (_popUp != null)
        //        {
        //            _popUp.Close();
        //        }
        //        if (type == UIMessageType.OptionDisplay)
        //        {
        //            _popUp = new PopUpOptionForm();
        //            ((PopUpOptionForm)_popUp).SetValues(content);
        //            Console.WriteLine("PopUpOptionForm");
        //        }
        //        else
        //        {
        //            _popUp = new PopUpForm();
        //            ((PopUpForm)_popUp).SetValues(content);
        //            Console.WriteLine("PopUpForm");
        //        }
        //        _popUp.TopMost = true;
        //        _popUp.Show();
        //
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine($"Exception in Show POPUP : {e.Message}");
        //    }
        //}

        private void StartExperiments()
        {
            this.mouse_key.Visible = false;
            this.voice_only.Visible = false;

            this.IdBtn.Visible = true;
            this.ID_LBL.Visible = true;
            this.IdTxtBox.Visible = true;

            this.UserameTxb.Visible = true;
            this.usernameLbl.Visible = true;
        }

        private void FinishPilot()
        {
            this.pilotBtn.Visible = false;
            this.Pilot_LBL.Visible = false;

            //Experimentsss
            this.Exp1_LBL.Visible = true;
            this.Exp2_LBL.Visible = true;
            this.Exp3_LBL.Visible = true;
            this.exp1Btn.Visible = true;
            this.exp2Btn.Visible = true;
            this.exp3Btn.Visible = true;

            this.exp2Btn.Enabled = false;
            this.exp3Btn.Enabled = false;
        }
        private void FinishExp1()
        {
            this.exp2Btn.Enabled = true;
        }
        private void FinishExp2()
        {
            this.exp3Btn.Enabled = true;
        }

        private void hepButton_Click(object sender, EventArgs e)
        {
            ExperimentHelpForm help = new ExperimentHelpForm();
            help.Show();
        }

        private void SUSbtn_Click(object sender, EventArgs e)
        {
            this.SUSbtn.Visible = false;
            this.SUSlbl.Visible = false;
            System.Diagnostics.Process.Start("https://qfreeaccountssjc1.az1.qualtrics.com/jfe/form/SV_cAe1TEXWig4aH3M");
            this.finishBtn.Visible = true;
        }

        private void videoBtn_Click(object sender, EventArgs e)
        {
            if(_systemName== "VoiceGaze")
                System.Diagnostics.Process.Start("https://drive.google.com/file/d/1G16J5cNrpBv9QmDkYJPYxsAdBhTJQwtw/view?usp=sharing");
            else if (_systemName == "VoiceMouse")
                System.Diagnostics.Process.Start("https://drive.google.com/file/d/1Ep8s-pknOIA_Pmouv-W0hPc3IeEm5aju/view?usp=sharing");
        }

        private void voice_only_Click(object sender, EventArgs e)
        {
            _systemName = "VoiceGaze";
            this.StartExperiments();
        }

        private void mouse_key_Click(object sender, EventArgs e)
        {
            _systemName = "VoiceMouse";
            this.StartExperiments();
        }

        private void EnableAll_Click(object sender, EventArgs e)
        {
            this.pilotBtn.Visible = true;
            this.Pilot_LBL.Visible = true;
            this.Exp1_LBL.Visible = true;
            this.Exp2_LBL.Visible = true;
            this.Exp3_LBL.Visible = true;
            this.exp1Btn.Visible = true;
            this.exp2Btn.Visible = true;
            this.exp3Btn.Visible = true;
            this.SUSbtn.Visible = true;
            this.SUSlbl.Visible = true;
        }

        private void exp3Btn_Click(object sender, EventArgs e)
        {
            this.exp3Btn.Enabled = false;
            //Start Exp1
            _expNum = 3;
            RunAppSystem(3);
            // Add Link BTN releas
        }
        private void FinishExp3()
        {
            this.Exp1_LBL.Visible = false;
            this.Exp2_LBL.Visible = false;
            this.Exp3_LBL.Visible = false;
            this.exp1Btn.Visible = false;
            this.exp2Btn.Visible = false;
            this.exp3Btn.Visible = false;

            this.SUSbtn.Visible = true;
            this.SUSlbl.Visible = true;

        }

        public void handlerMessageFromEngine(object sender, MessageEvent e)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {                        //change back to main thread
                    if (e.type == MessageEvent.messageType.WrongAuthentication)
                    {
                        this.TopMost = true;
                        //showPopUp(e.message);
                        //if (this.thread != null)
                        //    this.thread.Abort();
                    }
                    if (e.type == MessageEvent.messageType.ConnectionFail)
                    {
                        PopTimer pt = new PopTimer(e.message);
                    }
                    if (e.type == MessageEvent.messageType.TriggerWord)
                    {
                        string[] actions = new string[] { "fix", "fix to", "change", "add", "move", "replace", "options", "delete", "copy from", "copy to", "paste before", "paste after", "cancel", "1", "2", "3", "4", "5" };
                        Dictionary<String, int> distances = new Dictionary<string, int>();
                        String contains = "";
                        foreach (String word in actions)
                        {
                            distances.Add(word, controller.engineMain.LevenshteinDistance(e.message.ToLower(), word));
                            if (e.message.ToLower().Contains(word)) contains = word;
                        }

                        PopTimer pt = null;
                        var chosen = distances.OrderBy(kvp => kvp.Value).First();
                        if (contains != "")
                            pt = new PopTimer(contains);
                        else if (chosen.Value < 3)
                            pt = new PopTimer(distances.OrderBy(kvp => kvp.Value).First().Key);

                    }
                    if (e.type == MessageEvent.messageType.closeFile)
                    {

                    }
                });
                return;
            }
        }
    }
}
