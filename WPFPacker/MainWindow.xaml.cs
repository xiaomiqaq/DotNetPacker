using System.Windows;
using Microsoft.Win32;
using System.IO;
using System;
using Common;
using System.Linq;
using Algorithm;

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
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Filter = "exe file|*.dat;*.exe"
            };
            fileDialog.ShowDialog();
            string file = fileDialog.FileName;
            string oriFile = @"E:\MySchoolProject\WPFPacker\WPFPacker\bin\Debug\Wind\WindHover.AADL.Designer.exe";
            byte[] oriFileData = File.ReadAllBytes(oriFile);
            byte[] packData = File.ReadAllBytes(file);
            ShellInfo shellInfo = new ShellInfo();
            byte[] encData = Separate(out shellInfo,packData);
            shellInfo.usePuf = true;
            //
            byte[] oriKey = Algorithm.AsymmetricEncrypt.Decrypt(shellInfo.encKey, shellInfo);
            byte[] compressedData = Algorithm.SymmetricEncrypt.Decrypt(encData,shellInfo,oriKey);
            byte[] oriData = Algorithm.AllCompress.DeCompress(shellInfo.compType, compressedData, shellInfo.oriSize);
            bool eq = oriData.SequenceEqual(oriFileData);
            string ss = Tool.BytesToStr(oriKey);
            Launcher.Start(oriFile);
            if (shellInfo.usePuf)
                ReEncrypt(ref shellInfo, oriData, file);
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
        private void ReEncrypt(ref ShellInfo shellInfo, byte[] oridata, string filePath)
        {
            byte[] key = Tool.GenerateRandomKey(32);
            //压缩
            byte[] compressedData = AllCompress.Compress(shellInfo.compType, oridata);
            //非对称加密算法对密钥进行加密
            Byte[] encKey = AsymmetricEncrypt.Encrypt(key, ref shellInfo);
            shellInfo.encKey = encKey;
            //对称加密
            var encryptedData = SymmetricEncrypt.Encrypt(compressedData, ref shellInfo, key);
            FileTool.DeleteFile(filePath);
            JointAndSave(filePath,shellInfo, encryptedData);
            Console.WriteLine("重新加密完成");
        }
        public static void JointAndSave(string outputFile, ShellInfo shellInfo, byte[] encryptedData)
        {

            byte[] shellBytes = shellInfo.Serialize();

            //var testde = Tool.DeserializeFromXml(Encoding.UTF8.GetString(shellBytes));
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // 写入 shellInfo.Size
                byte[] sizeBytes = BitConverter.GetBytes(shellBytes.Length);
                memoryStream.Write(sizeBytes, 0, sizeof(int));
                //写入shellInfo
                memoryStream.Write(shellBytes, 0, shellBytes.Length);
                // 写入 data
                memoryStream.Write(encryptedData, 0, encryptedData.Length);
                // 将写入的数据保存到文件
                File.WriteAllBytes(outputFile, memoryStream.ToArray());
            }
        }
    }
}
