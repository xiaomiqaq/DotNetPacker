using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public enum EncryptType
    {
        AES,
        DES,
        TDEA,
        RSA
    }
    struct ShellInfo
    {
        EncryptType sysEncType;
        EncryptType asysEncType;
        int partLen;

    }
}
