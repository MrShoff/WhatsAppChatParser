using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WhatsaAppTextChatParser
{
    public partial class Form1 : Form
    {
        BackgroundWorker parseWorker = new BackgroundWorker();

        public Form1()
        {
            InitializeComponent();

            parseWorker.DoWork += Worker_DoWork;
            parseWorker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }                    

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            UserTextFeedback.ConsoleOut("Beginning parse request on " + (string)e.Argument);
            List<WhatsAppMessage> whatsappMessageList = ParseText((string)e.Argument);

            if (chkMySQL.Checked)
            {
                UserTextFeedback.ConsoleOut("Attempting to write " + whatsappMessageList.Count + " record(s) to MySQL.");
                // send records in batches to MySQL
                int batchSize = 20000;
                int currentBatch = 0;
                UserTextFeedback.ConsoleOut("Writing records in batches of: " + batchSize);

                while (currentBatch * batchSize < whatsappMessageList.Count)
                {
                    UserTextFeedback.ConsoleOut("Writing batch #" + (currentBatch+1));
                    List<WhatsAppMessage> messagesBatch = whatsappMessageList.Skip<WhatsAppMessage>(currentBatch * batchSize).Take<WhatsAppMessage>(batchSize).ToList();
                    WhatsAppMessage.SendDataToMySql(messagesBatch, currentBatch);

                    currentBatch++;
                }
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UserTextFeedback.ConsoleOut("Parse request completed.");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string whatsappChatLogPath = txtChatLogPath.Text;
            parseWorker.RunWorkerAsync(whatsappChatLogPath);
        }

        private List<WhatsAppMessage> ParseText(string chatLogPath)
        {
            List<WhatsAppMessage> messageList = new List<WhatsAppMessage>();
            int failedLines, successLines, mediaOmitted;
            failedLines = 0;
            successLines = 0;
            mediaOmitted = 0;

            FileStream chatLogFileStream = File.OpenRead(chatLogPath);
            string groupName = Path.GetFileNameWithoutExtension(chatLogPath);
            groupName = groupName.Substring(19);

            string fileContents;
            using (StreamReader reader = new StreamReader(chatLogFileStream))
            {
                fileContents = reader.ReadToEnd();
            }

            string logSource = "Undetermined";
            if (fileContents[0] != "[".ToCharArray()[0])
            {
                logSource = "Android";
                UserTextFeedback.ConsoleOut("Log type: " + logSource);
            }
            if (fileContents[0] == "[".ToCharArray()[0])
            {
                logSource = "iPhone";
                UserTextFeedback.ConsoleOut("Log type: " + logSource);
            }

            switch(logSource)
            {
                case "Android":            
                    foreach (string potentialMessage in fileContents.Split("\n".ToCharArray()))
                    {
                        WhatsAppMessage chatMessage = new WhatsAppMessage();

                        DateTime timestamp = new DateTime();
                        string user = "";
                        string text = "";

                        int indexOfFirstDash = potentialMessage.IndexOf(" - ");
                        int indexOfFirstColon = potentialMessage.IndexOf(":");
                        int indexOfSecondColon = potentialMessage.IndexOf(":", indexOfFirstColon + 1);

                        if (indexOfFirstDash >= 0 && indexOfFirstColon >= 0 && indexOfSecondColon >= 0)
                        {
                            string datetime = potentialMessage.Substring(0, indexOfFirstDash);
                            DateTime.TryParse(datetime, out timestamp);
                            user = potentialMessage.Substring(indexOfFirstDash + 3, indexOfSecondColon - (indexOfFirstDash + 3));
                            text = potentialMessage.Substring(indexOfSecondColon + 2);
                            if (text == "<Media Omitted>")
                            {
                                successLines++;
                                mediaOmitted++;
                                continue;
                            }
                        }
                        else
                        {
                            failedLines++;
                            continue;
                        }

                        chatMessage.Timestamp = timestamp;
                        chatMessage.User = user;
                        chatMessage.Text = text;
                        chatMessage.GroupName = groupName;

                        messageList.Add(chatMessage);
                        successLines++;
                    }
                    break;
                case "iPhone":
                    foreach (string potentialMessage in fileContents.Split("\n".ToCharArray()))
                    {
                        WhatsAppMessage chatMessage = new WhatsAppMessage();

                        DateTime timestamp = new DateTime();
                        string user = "";
                        string text = "";

                        int indexOfFirstComma = potentialMessage.IndexOf(", ");
                        int indexOfFirstColon = potentialMessage.IndexOf(":");
                        int indexOfSecondColon = potentialMessage.IndexOf(":", indexOfFirstColon+1);
                        int indexOfThirdColon = potentialMessage.IndexOf(":", indexOfSecondColon+1);
                        int indexOfFirstClosingBracket = potentialMessage.IndexOf("]");

                        if (indexOfFirstComma >= 0 && indexOfFirstClosingBracket >= 0 && indexOfFirstColon >= 0 && indexOfSecondColon >= 0 && indexOfThirdColon >= 0)
                        {
                            string datetime = potentialMessage.Substring(1, indexOfFirstClosingBracket - 1).Replace(",", "");
                            DateTime.TryParse(datetime, out timestamp);
                            user = potentialMessage.Substring(indexOfFirstClosingBracket + 2, indexOfThirdColon - (indexOfFirstClosingBracket + 2));
                            text = potentialMessage.Substring(indexOfThirdColon + 2).TrimEnd("\r\r".ToCharArray());
                            if (text == "‎image omitted")
                            {
                                successLines++;
                                mediaOmitted++;
                                continue;
                            }
                        }
                        else
                        {
                            failedLines++;
                            continue;
                        }

                        chatMessage.Timestamp = timestamp;
                        chatMessage.User = user;
                        chatMessage.Text = text;
                        chatMessage.GroupName = groupName;

                        messageList.Add(chatMessage);
                        successLines++;
                    }
                    break;
                default:
                    UserTextFeedback.ConsoleOut("Unable to parse file. Log type undetermined.");
                    break;
            }
            
            UserTextFeedback.ConsoleOut("Successfully parsed line(s): " + successLines);
            UserTextFeedback.ConsoleOut("Failed parsed line(s): " + failedLines);
            UserTextFeedback.ConsoleOut("Succes rate: " + (float)successLines / (successLines + failedLines));
            UserTextFeedback.ConsoleOut("Media omitted: " + mediaOmitted);

            return messageList;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Text = ProductName + " v" + ProductVersion;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.DefaultExt = ".txt";
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                txtChatLogPath.Text = openFileDialog1.FileName;
        }

        private void parseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }
    }
}
