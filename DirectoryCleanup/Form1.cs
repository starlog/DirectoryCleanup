using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DirectoryCleanup
{
    ///////////////////////////////////////////////////////////////////////////////////////////////
    public partial class Form1 : Form
    {
        private string[] _dirName;
        public Form1()
        {
            InitializeComponent();
            textBox1.AllowDrop = true;
            Text += @" V1.3";
        }

        public override sealed string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        private void ProcessFile(FileSystemInfo myF, string targetDirectory)
        {
            //목적지 폴더 설정
            var targetFilename = string.Format("{0}\\{1}", targetDirectory, myF.Name);

            // 같은 이름의 파일은 지운다
            if (File.Exists(targetFilename))
            {
                try
                {
                    var count = 0;
                    string newName;

                    // 중복파일 이름 정하기
                    while (true)
                    {
                        newName =
                           string.Format("{0}\\{1}{2}({4}){3}",
                           targetDirectory,
                           myF.Name.Remove(myF.Name.Length - myF.Extension.Length, myF.Extension.Length),
                           "_dup",
                           myF.Extension, count);

                        if (File.Exists(newName))
                        {
                            count++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    textBox1.Text += 
                        string.Format(@"같은 이름의 파일처리:{0} => {2}_dup({3}){1}", 
                        myF.Name, 
                        Environment.NewLine, 
                        myF.Name, 
                        count);

                    File.Move(myF.FullName, newName);
                }
                catch (Exception ex)
                {
                    Report(@"중복파일 오류:"+ex);
                }
            }
            else
            {
                if ((myF.Extension == ".txt" || myF.Extension == ".nfo") && checkBox1.Checked)
                {
                    // .txt, .nfo 파일은 지운다
                    textBox1.Text += string.Format(@"파일삭제:{0}{1}", myF.Name, Environment.NewLine);
                    File.Delete(myF.FullName);
                }
                else
                {
                    // 나머지 파일은 이동한다
                    textBox1.Text += string.Format(@"파일이동:{0}{1}", myF.Name, Environment.NewLine);
                    try
                    {
                        File.Move(myF.FullName, targetFilename);
                    }
                    catch (Exception ex)
                    {
                        Report(@"파일이동 오류:" + ex);
                    }
                }
            }
        }
        ///////////////////////////////////////////////////////////////////////////////////////////
        private void ProcessFolder(string folder)
        {
            var dInfo = new DirectoryInfo(folder);

            var sInfo = dInfo.GetDirectories();

            if (sInfo.Any()) //폴더가 존재하면 폴더 Travel, recursive 이동
            {
                this.ProcessFolder(sInfo[0].FullName);
            }
            else
            {
                if (dInfo.FullName != _dirName[0]) //이동해야하는 경우에
                {
                    var fInfo = dInfo.GetFiles();

                    if (fInfo.Any())
                    {
                        this.Report(@"폴더작업시작: " + dInfo.FullName);
                        foreach (var file1 in fInfo)
                        {
                            this.ProcessFile(file1, _dirName[0]);
                        }
                    }
                    Directory.Delete(path: dInfo.FullName, recursive: true);
                    this.Report("");
                    this.ProcessFolder(_dirName[0]); //이동이 끝나면 다시 recursive 호출
                }
            }
        }
        ///////////////////////////////////////////////////////////////////////////////////////////
        private void Drag_Drop(object sender, DragEventArgs e)
        {
            try
            {
                //드랙 & 드롭된 폴더 이름 설정
                _dirName = (string[])e.Data.GetData(DataFormats.FileDrop);

                this.ProcessFolder(_dirName[0]); //처리 시작

                this.Report("---- 작업 완료 ----");
            }
            catch (Exception ex)
            {
                textBox1.Text = ex.ToString();
            }
        }
        ///////////////////////////////////////////////////////////////////////////////////////////
        private void Drag_Enter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }
        ///////////////////////////////////////////////////////////////////////////////////////////
        private void Report(string msg)
        {
            this.textBox1.Text += msg + Environment.NewLine;

            this.textBox1.Refresh();
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.ScrollToCaret();
        }
    }
}
