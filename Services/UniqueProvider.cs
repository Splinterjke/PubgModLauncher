using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PubgMod.Services
{
    public static class UniqueProvider
    {
        private static string GetHash(string s)
        {
            //Создаем экземпляр MD5 Crypto Service Provider для генерации хэша
            using (MD5 sec = new MD5CryptoServiceProvider())
            {
                //Grab the bytes of the variable 's'
                byte[] bt = Encoding.ASCII.GetBytes(s);
                //Берем хекс значение MD5 хэша
                return GetHexString(sec.ComputeHash(bt));
            }                
        }

        public static string GetHexString(byte[] bt)
        {
            string s = string.Empty;
            for (int i = 0; i < bt.Length; i++)
            {
                byte b = bt[i];
                int n = b;
                int n1 = n & 15;
                int n2 = (n >> 4) & 15;
                if (n2 > 9)
                    s += ((char)(n2 - 10 + 'A')).ToString(CultureInfo.InvariantCulture);
                else
                    s += n2.ToString(CultureInfo.InvariantCulture);
                if (n1 > 9)
                    s += ((char)(n1 - 10 + 'A')).ToString(CultureInfo.InvariantCulture);
                else
                    s += n1.ToString(CultureInfo.InvariantCulture);
                if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
            }
            return s;
        }

        private static string _fingerPrint = string.Empty;
        public static string Value()
        {
            //Скипаем генерацию, если хвид уже был сгенерен в процессе, не нагружая поток лишней работой
            //К тому же хоть ХВИД обычно не меняется во время работы компа, но может.
            if (string.IsNullOrEmpty(_fingerPrint))
            {
                //Можно убрать по необходимости не нужные генерации, если будут траблы у юзеров
                _fingerPrint = GetHash("CPU >> " + CpuId() + "BIOS >> " + BiosId() + "BASE >> " + BaseId() + "DISK >> " + DiskId());
            }
            return _fingerPrint;
        }

        //Метод идентефикации устройства
        private static string Identifier(string wmiClass, string wmiProperty)
        {
            string result = "";
            System.Management.ManagementClass mc = new System.Management.ManagementClass(wmiClass);
            System.Management.ManagementObjectCollection moc = mc.GetInstances();
            foreach (System.Management.ManagementBaseObject mo in moc)
            {
                if (result != "") continue;
                try
                {
                    result = mo[wmiProperty].ToString();
                    break;
                }
                catch
                {
                }
            }
            return result;
        }

        //CPU
        private static string CpuId()
        {
            //Берем айдишник первого по индексу проца
            //Берем один, потому что метод вэри медленный для старых компов
            string retVal = Identifier("Win32_Processor", "UniqueId");
            if (retVal != "") return retVal;
            retVal = Identifier("Win32_Processor", "ProcessorId");
            if (retVal != "") return retVal;
            retVal = Identifier("Win32_Processor", "Name");
            if (retVal == "") //If no Name, use Manufacturer
            {
                retVal = Identifier("Win32_Processor", "Manufacturer");
            }
            //Для суперуникальности грабим макс. частоту проца
            retVal += Identifier("Win32_Processor", "MaxClockSpeed");
            return retVal;
        }

        //BIOS
        private static string BiosId()
        {
            return Identifier("Win32_BIOS", "Manufacturer") + Identifier("Win32_BIOS", "SMBIOSBIOSVersion") + Identifier("Win32_BIOS", "IdentificationCode") + Identifier("Win32_BIOS", "SerialNumber") + Identifier("Win32_BIOS", "ReleaseDate") + Identifier("Win32_BIOS", "Version");
        }

        //Основной носитель (ssd/hdd)
        private static string DiskId()
        {
            return Identifier("Win32_DiskDrive", "Model") + Identifier("Win32_DiskDrive", "Manufacturer") + Identifier("Win32_DiskDrive", "Signature") + Identifier("Win32_DiskDrive", "TotalHeads");
        }

        //Мать
        private static string BaseId()
        {
            return Identifier("Win32_BaseBoard", "Model") + Identifier("Win32_BaseBoard", "Manufacturer") + Identifier("Win32_BaseBoard", "Name") + Identifier("Win32_BaseBoard", "SerialNumber");
        }
    }
}
