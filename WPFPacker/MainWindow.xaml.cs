using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Text;
using Common;
using System.Linq;

namespace WPFPacker
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Invoke_Click(object sender, RoutedEventArgs e)
        {
            //
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "exe file|*.dat;*.exe";
            fileDialog.ShowDialog();
            string file = fileDialog.FileName;
            string oriFile = @"E:\MySchoolProject\WPFPacker\WPFPacker\bin\Debug\Wind\WindHover.AADL.Designer.exe";
            byte[] oriFileData = File.ReadAllBytes(oriFile);
            byte[] packData = File.ReadAllBytes(file);
            ShellInfo shellInfo = new ShellInfo();
            byte[] encData = Separate(out shellInfo,packData);
            //
            byte[] oriKey = Tool.RemovePadding(Algorithm.AsymmetricEncrypt.Decrypt(shellInfo.encKey, shellInfo));
            byte[] compressedData = Algorithm.SymmetricEncrypt.Decrypt(encData,shellInfo,oriKey);
            byte[] oriData = Algorithm.AllCompress.DeCompress(shellInfo.compType, compressedData, shellInfo.oriSize);
            bool eq = oriData.SequenceEqual(oriFileData);
            string ss = Tool.BytesToStr(oriKey);
            Launcher.Start(oriFile);
        }
        private byte[]  Separate(out Common.ShellInfo shellInfo, in byte[] data)
        {
            using (MemoryStream readMemoryStream = new MemoryStream(data))
            {
                // 读取 shellInfo.Size
                byte[] readSizeBytes = new byte[sizeof(int)];
                readMemoryStream.Read(readSizeBytes, 0, readSizeBytes.Length);
                int shellInfoSize = System.BitConverter.ToInt32(readSizeBytes, 0);
                // 读取 shellInfo 的字节数组
                byte[] shellInfoBytes = new byte[shellInfoSize];
                readMemoryStream.Read(shellInfoBytes, 0, shellInfoBytes.Length);
                //反序列化
                shellInfo = ShellInfo.Deserialize(shellInfoBytes);

                //读取 data
                byte[] readData = new byte[data.Length - sizeof(int) - shellInfoSize];

                readMemoryStream.Read(readData, 0, readData.Length);
                return readData;

            }
        }
    }
}
