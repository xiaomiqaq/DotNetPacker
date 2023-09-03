using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.IO;

namespace Algorithm
{
    class Confuse
    {
        static bool isAntiDe4Dot = false, 
                    isAddFakeTypes = false, 
                    isJunk = false, 
                    isEncyptString = false, 
                    isIntConfusion = false, 
                    isAntiDebug = false, 
                    isControlFlow = true;
        public static string ObfuscasteCode(string ToProtect)
        {
            byte[] AssemblyToProtect = File.ReadAllBytes(ToProtect);
            ModuleContext ModuleCont = ModuleDefMD.CreateModuleContext();
            ModuleDefMD FileModule = ModuleDefMD.Load(AssemblyToProtect, ModuleCont);
            AssemblyDef Assembly1 = FileModule.Assembly;

            if(isAntiDe4Dot)
            {
                for (int i = 200; i < 300; i++)
                {
                    InterfaceImpl Interface = new InterfaceImplUser(FileModule.GlobalType);
                    TypeDef typedef = new TypeDefUser("", $"Form{i.ToString()}", FileModule.CorLibTypes.GetTypeRef("System", "Attribute"));
                    InterfaceImpl interface1 = new InterfaceImplUser(typedef);
                    FileModule.Types.Add(typedef);
                    typedef.Interfaces.Add(interface1);
                    typedef.Interfaces.Add(Interface);
                }
            }
            if(isAddFakeTypes)
            {
                string[] FakeObfuscastionsAttributes = { "ConfusedByAttribute", "YanoAttribute", "NetGuard", "DotfuscatorAttribute", "BabelAttribute", "ObfuscatedByGoliath", "dotNetProtector" };
                for (int i = 0; i < FakeObfuscastionsAttributes.Length; i++)
                {
                    //FileModule.CorLibTypes.Object.TypeDefOrRef 表示创建的类继承自System.Object
                    var FakeObfuscastionsAttribute = new TypeDefUser(FakeObfuscastionsAttributes[i], FileModule.CorLibTypes.Object.TypeDefOrRef);
                    FileModule.Types.Add(FakeObfuscastionsAttribute);
                }
            }
            if (isJunk)
            {
                for (int i = 0; i < 200; i++)
                {
                    var Junk = new TypeDefUser("A" + RandomChineseCharacters(10) + RandomChineseCharacters(10) + RandomChineseCharacters(10) + i, FileModule.CorLibTypes.Object.Namespace);
                    FileModule.Types.Add(Junk);
                }

                for (int i = 0; i < 200; i++)
                {
                    var Junk = new TypeDefUser(RandomChineseCharacters(10) + RandomChineseCharacters(10) + RandomChineseCharacters(10) + i, FileModule.CorLibTypes.Object.TypeDefOrRef);
                    FileModule.Types.Add(Junk);
                }

                for (int i = 0; i < 200; i++)
                {
                    var Junk = new TypeDefUser(RandomChineseCharacters(10) + RandomChineseCharacters(2) + RandomChineseCharacters(10) + RandomChineseCharacters(10) + i, FileModule.CorLibTypes.Object.TypeDefOrRef);
                    var Junk2 = new TypeDefUser(RandomChineseCharacters(11) + RandomChineseCharacters(3) + RandomChineseCharacters(11) + RandomChineseCharacters(11) + i, FileModule.CorLibTypes.Object.TypeDefOrRef);
                    FileModule.Types.Add(Junk);
                    FileModule.Types.Add(Junk2);
                }

                for (int i = 0; i < 200; i++)
                {
                    var Junk = new TypeDefUser(RandomChineseCharacters(10) + RandomChineseCharacters(4) + RandomChineseCharacters(10) + RandomChineseCharacters(10) + i, FileModule.CorLibTypes.Object.Namespace);
                    var Junk2 = new TypeDefUser(RandomChineseCharacters(11) + RandomChineseCharacters(5) + RandomChineseCharacters(11) + RandomChineseCharacters(11) + i, FileModule.CorLibTypes.Object.Namespace);
                    FileModule.Types.Add(Junk);
                    FileModule.Types.Add(Junk2);
                }
            }
            if (isEncyptString)
            {
                foreach (TypeDef type in FileModule.Types)
                {
                    foreach (MethodDef method in type.Methods)
                    {
                        if (method.Body == null) continue;
                        method.Body.SimplifyBranches();
                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr)
                            {
                                string EncodedString = method.Body.Instructions[i].Operand.ToString();
                                string InsertEncodedString = Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(EncodedString));
                                method.Body.Instructions[i].OpCode = OpCodes.Nop;
                                method.Body.Instructions.Insert(i + 1, new Instruction(OpCodes.Call, FileModule.Import(typeof(Encoding).GetMethod("get_UTF8", new Type[] { }))));
                                method.Body.Instructions.Insert(i + 2, new Instruction(OpCodes.Ldstr, InsertEncodedString));
                                method.Body.Instructions.Insert(i + 3, new Instruction(OpCodes.Call, FileModule.Import(typeof(Convert).GetMethod("FromBase64String", new Type[] { typeof(string) }))));
                                method.Body.Instructions.Insert(i + 4, new Instruction(OpCodes.Callvirt, FileModule.Import(typeof(Encoding).GetMethod("GetString", new Type[] { typeof(byte[]) }))));
                                i += 4;
                            }
                        }
                    }
                }
            }
            if(isIntConfusion)
            {
                foreach (var type in FileModule.GetTypes())
                {
                    if (type.IsGlobalModuleType) continue;
                    foreach (var method in type.Methods)
                    {
                        if (!method.HasBody) continue;
                        {
                            for (var i = 0; i < method.Body.Instructions.Count; i++)
                            {
                                if (!method.Body.Instructions[i].IsLdcI4()) continue;
                                var numorig = new Random(Guid.NewGuid().GetHashCode()).Next();
                                var div = new Random(Guid.NewGuid().GetHashCode()).Next();
                                var num = numorig ^ div;
                                var nop = OpCodes.Nop.ToInstruction();
                                var local = new Local(method.Module.ImportAsTypeSig(typeof(int)));
                                method.Body.Variables.Add(local);
                                method.Body.Instructions.Insert(i + 1, OpCodes.Stloc.ToInstruction(local));
                                method.Body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Ldc_I4, method.Body.Instructions[i].GetLdcI4Value() - sizeof(float)));
                                method.Body.Instructions.Insert(i + 3, Instruction.Create(OpCodes.Ldc_I4, num));
                                method.Body.Instructions.Insert(i + 4, Instruction.Create(OpCodes.Ldc_I4, div));
                                method.Body.Instructions.Insert(i + 5, Instruction.Create(OpCodes.Xor));
                                method.Body.Instructions.Insert(i + 6, Instruction.Create(OpCodes.Ldc_I4, numorig));
                                method.Body.Instructions.Insert(i + 7, Instruction.Create(OpCodes.Bne_Un, nop));
                                method.Body.Instructions.Insert(i + 8, Instruction.Create(OpCodes.Ldc_I4, 2));
                                method.Body.Instructions.Insert(i + 9, OpCodes.Stloc.ToInstruction(local));
                                method.Body.Instructions.Insert(i + 10, Instruction.Create(OpCodes.Sizeof, method.Module.Import(typeof(float))));
                                method.Body.Instructions.Insert(i + 11, Instruction.Create(OpCodes.Add));
                                method.Body.Instructions.Insert(i + 12, nop);
                                i += 12;
                            }
                            method.Body.SimplifyBranches();
                        }
                    }
                }
            }
            if (isControlFlow)
            {
                foreach (var type in FileModule.GetTypes())
                {
                    if (type.IsGlobalModuleType) continue;
                    foreach (var method in type.Methods)
                    {
                        if (!method.HasBody) continue;
                        {
                            for (var i = 0; i < method.Body.Instructions.Count; i++)
                            {
                                if (!method.Body.Instructions[i].IsLdcI4()) continue;
                                var numorig = new Random(Guid.NewGuid().GetHashCode()).Next();
                                var div = new Random(Guid.NewGuid().GetHashCode()).Next();
                                var num = numorig ^ div;
                                var nop = OpCodes.Nop.ToInstruction();
                                var local = new Local(method.Module.ImportAsTypeSig(typeof(int)));
                                method.Body.Variables.Add(local);
                                method.Body.Instructions.Insert(i + 1, OpCodes.Stloc.ToInstruction(local));
                                method.Body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Ldc_I4, method.Body.Instructions[i].GetLdcI4Value() - sizeof(float)));
                                method.Body.Instructions.Insert(i + 3, Instruction.Create(OpCodes.Ldc_I4, num));
                                method.Body.Instructions.Insert(i + 4, Instruction.Create(OpCodes.Ldc_I4, div));
                                method.Body.Instructions.Insert(i + 5, Instruction.Create(OpCodes.Xor));
                                method.Body.Instructions.Insert(i + 6, Instruction.Create(OpCodes.Ldc_I4, numorig));
                                method.Body.Instructions.Insert(i + 7, Instruction.Create(OpCodes.Bne_Un, nop));
                                method.Body.Instructions.Insert(i + 8, Instruction.Create(OpCodes.Ldc_I4, 2));
                                method.Body.Instructions.Insert(i + 9, OpCodes.Stloc.ToInstruction(local));
                                method.Body.Instructions.Insert(i + 10, Instruction.Create(OpCodes.Sizeof, method.Module.Import(typeof(float))));
                                method.Body.Instructions.Insert(i + 11, Instruction.Create(OpCodes.Add));
                                method.Body.Instructions.Insert(i + 12, nop);
                                i += 12;
                            }
                            method.Body.SimplifyBranches();
                        }
                    }
                }
            }
            if (isAntiDebug)
            {
                //将AntiDebugCheck插入到源程序的静态构造函数的开头
                var typeModule = ModuleDefMD.Load(typeof(Classes.AntiDebug).Module);
                var cctor = FileModule.GlobalType.FindOrCreateStaticConstructor();  //获取源程序的静态构造函数
                var typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(Classes.AntiDebug).MetadataToken));
                var members = InjectHelper.Inject(typeDef, FileModule.GlobalType, FileModule);
                //在cctor的方法体的开头插入一条调用AntiDebugCheck方法的指令。
                var init = (MethodDef)members.Single(method => method.Name == "AntiDebugCheck");
                cctor.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, init));
                foreach (var md in FileModule.GlobalType.Methods)
                {
                    if (md.Name != ".ctor") continue;
                    FileModule.GlobalType.Remove(md);
                    break;
                }
            }

            string tempFile = Path.GetDirectoryName(ToProtect) + @"\Obfuscasted.exe";
            if (File.Exists(tempFile) == false)
            {
                FileModule.Write(tempFile);
            }
            else
            {
                File.Delete(tempFile);
                FileModule.Write(tempFile);
            }
            return tempFile;
        }
        private string RandomPassword(int PasswordLength)
        {
            StringBuilder MakePassword = new StringBuilder();
            Random MakeRandom = new Random();
            while (0 < PasswordLength--)
            {
                string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ*!@=&?&/abcdefghijklmnopqrstuvwxyz1234567890";
                MakePassword.Append(characters[MakeRandom.Next(characters.Length)]);
            }
            return MakePassword.ToString();
        }

        private string RandomName(int NameLength)
        {
            StringBuilder MakePassword = new StringBuilder();
            Random MakeRandom = new Random();
            while (0 < NameLength--)
            {
                string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
                MakePassword.Append(characters[MakeRandom.Next(characters.Length)]);
            }
            return MakePassword.ToString();
        }

        public static string RandomChineseCharacters(int NameLength)
        {
            const string chars = "的是有为也而要你可生家发如成起经";
            return new string(Enumerable.Repeat(chars, NameLength)
                .Select(s => s[new Random(Guid.NewGuid().GetHashCode()).Next(s.Length)]).ToArray());
        }
    }
}
