using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Common;
using System.Diagnostics;


namespace Algorithm
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //处理输入
            Console.WriteLine("请输入要读取的文件路径：");
            //string inputFilePath = Console.ReadLine();
            string inputFilePath = @"E:\Debug2\WindHover.AADL.Designer.exe";
            if (!CheckFile(inputFilePath))
            {
                Console.WriteLine("该文件不是.net程序，请检查输入是否正确");
                return;
            }
            string outputFolderPath = Path.GetDirectoryName(inputFilePath);
            Console.WriteLine("请输入加密密钥（32个字符）：");
            //string key = Console.ReadLine();
            //测试不同密码，随机
            string key = "qwertyuiopasdfghjklzxcvbnmqwerty";
            if (key.Length < 32)
            {
                key = key.PadRight(32, '0');
            }

            //混淆
            //string temp = Confuse.ObfuscasteCode(inputFilePath);

            byte[] fileData = File.ReadAllBytes(inputFilePath);
            //初始化shellInfo
            ShellInfo shellInfo = new ShellInfo();
            shellInfo.sysEncType = SymmetricEncryptType.Aes;
            shellInfo.asysEncType = AsymmetricEncryptType.Rsa;
            shellInfo.compType = CompressType.Lzma;
            shellInfo.oriSize = fileData.Length;
            {
                //RSA加解密测试
                byte[] testData = Encoding.UTF8.GetBytes("qwert");
                var oriType = shellInfo.asysEncType;
                shellInfo.asysEncType = AsymmetricEncryptType.Rsa;
                byte[] enc = AsymmetricEncrypt.Encrypt(testData, ref shellInfo);
                byte[] dec = AsymmetricEncrypt.Decrypt(enc, shellInfo);
                shellInfo.asysEncType = oriType;

            }
            //压缩
            byte[] compressedData = AllCompress.Compress(shellInfo.compType, fileData);
            //非对称加密算法对密钥进行加密
            Byte[] encKey = AsymmetricEncrypt.Encrypt(Tool.StrToBytes(key), ref shellInfo);
            shellInfo.encKey = encKey;
            //对称加密
            var encryptedData = SymmetricEncrypt.Encrypt(compressedData,ref shellInfo,Encoding.ASCII.GetBytes(key));
            {
                //解密测试
                //byte[] d_sysKey = Tool.RemovePadding( Ecc.decrypt(encKey, Ecc.str2PrivateKey(shellInfo.asPriKey)));
                //byte[] decryptedData = Algorithm.SymmetricEncrypt.Decrypt(encryptedData, shellInfo,d_sysKey);
                //bool iseq = decryptedData.SequenceEqual(compressedData);
            }

            {
                //序列化shellInfo测试
                byte[] b = shellInfo.Serialize();
                var she = ShellInfo.Deserialize(b);
                    
            }
            //
            JointAndSave(outputFolderPath, shellInfo, encryptedData);

        }


        public static void JointAndSave( string outputFolderPath, ShellInfo shellInfo, byte[] encryptedData)
        {
                     
            string outputFile = Path.Combine(outputFolderPath, "encrypted.dat");
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
        public static void CompressByProcess(string compressFile, int type)
        {
            // 创建一个进程启动信息对象
            ProcessStartInfo startInfo = new ProcessStartInfo();

            // 设置要启动的可执行文件的路径
            startInfo.FileName = "你的可执行文件路径";

            // 设置要传递给进程的命令行参数
            startInfo.Arguments = $"{compressFile} {type}";

            // 创建一个进程对象
            Process process = new Process();

            // 将启动信息对象指定给进程对象
            process.StartInfo = startInfo;

            // 启动进程
            process.Start();

            // 等待进程执行完成
            process.WaitForExit();

            // 获取进程的退出代码
            int exitCode = process.ExitCode;

        }

        public static bool CheckFile(string filePath)
        {

            if (File.Exists(filePath))
            {
                if (IsDotNetProgram(filePath))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool IsDotNetProgram(string filePath)
        {
            using (FileStream fileStream = File.OpenRead(filePath))
            using (PEReader peReader = new PEReader(fileStream))
            {
                if (peReader.HasMetadata)
                {
                    MetadataReader metadataReader = peReader.GetMetadataReader();
                    if (metadataReader.IsAssembly)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

}
