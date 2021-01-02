using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AmongUsReader
{
    public partial class GameReader
    {
        const int PROCESS_WM_READ = 0x0010;

        [DllImport("kernel32.dll")]
        internal static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        internal T ReadMemory<T>(IntPtr Address, List<int> Offsets)
        {
            if (!this.IsAttached)
                return default;

            if (Address == IntPtr.Zero)
            {
                //Logger.Write($"\'ReadMemory<{nameof(T)}>\' attempted to read a <DarkRed>NullPtr</DarkRed>, Skipping the read.", Logger.Severity.Warning);
                return default;
            }

            var addr = this.OffsetAddress(Address, Offsets);

            return ReadRawMem<T>(addr.address + addr.last);
        }

        internal string ReadString(IntPtr Address)
        {
            if (!this.IsAttached)
                return null;

            if (Address == IntPtr.Zero)
            {
                //Logger.Write("\'ReadString\' attempted to read a <DarkRed>NullPtr</DarkRed>, Skipping the read.", Logger.Severity.Warning);
                return null;
            }

            // Read the length
            int length = ReadRawMem<int>(Address + 0x8) * 2;

            byte[] buffer = new byte[length];

            ReadProcessMemory(this.amongUsHandle, Address + 0xc, buffer, length, out var read);

            return Encoding.Latin1.GetString(buffer).Replace("\0", "");
        }


        internal T ReadRawMem<T>(IntPtr Address)
        {
            if (!this.IsAttached)
                return default;

            if (Address == IntPtr.Zero)
            {
                //Logger.Write($"\'ReadRawMem<{nameof(T)}>\' attempted to read a <DarkRed>NullPtr</DarkRed>, Skipping the read.", Logger.Severity.Warning);
                return default;
            }

            byte[] buffer = new byte[Marshal.SizeOf<T>()];

            ReadProcessMemory(this.amongUsHandle, Address, buffer, buffer.Length, out var ammoungRead);

            return ParseBuffer<T>(buffer);
        }

        internal (IntPtr address, int last) OffsetAddress(IntPtr address, List<int> offsets)
        {
            if (!this.IsAttached)
                return default;

            if (offsets.Count == 0)
                return (address, 0);

            address = (IntPtr)((uint)address & 0xffffffff);

            for (int i = 0; i < offsets.Count - 1; i++)
            {
                address = (IntPtr)ReadRawMem<uint>(IntPtr.Add(address, offsets[i]));

                if (address == IntPtr.Zero) break;
            }

            var last = offsets.Count > 0 ? offsets[offsets.Count - 1] : 0;
            return (address, last);
        }

        internal Dictionary<Type, Func<byte[], object>> parseList = new Dictionary<Type, Func<byte[], object>>()
        {
            { typeof(byte[]), (byte[] b) => b },
            { typeof(bool),   (byte[] b) => BitConverter.ToBoolean(b, 0) },
            { typeof(char),   (byte[] b) => BitConverter.ToChar(b, 0)    },
            { typeof(double), (byte[] b) => BitConverter.ToDouble(b, 0)  },
            { typeof(short),  (byte[] b) => BitConverter.ToInt16(b, 0)   },
            { typeof(int),    (byte[] b) => BitConverter.ToInt32(b, 0)   },
            { typeof(long),   (byte[] b) => BitConverter.ToInt64(b, 0)   },
            { typeof(float),  (byte[] b) => BitConverter.ToSingle(b, 0)  },
            { typeof(ushort), (byte[] b) => BitConverter.ToUInt16(b, 0)  },
            { typeof(uint),   (byte[] b) => BitConverter.ToUInt32(b, 0)  },
            { typeof(ulong),  (byte[] b) => BitConverter.ToUInt16(b, 0)  },
            { typeof(IntPtr), (byte[] b) => (IntPtr)BitConverter.ToInt32(b, 0)  }
        };

        internal T ParseBuffer<T>(byte[] buffer)
        {
            if (parseList.TryGetValue(typeof(T), out var parser))
            {
                return (T)parser(buffer);
            }
            else
            {
                return default;
            }
        }
    }
}
