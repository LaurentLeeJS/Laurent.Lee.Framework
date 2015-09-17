/*
-------------------------------------------------- -----------------------------------------
The frame content is protected by copyright law. In order to facilitate individual learning,
allows to download the program source information, but does not allow individuals or a third
party for profit, the commercial use of the source information. Without consent,
does not allow any form (even if partial, or modified) database storage,
copy the source of information. If the source content provided by third parties,
which corresponds to the third party content is also protected by copyright.

If you are found to have infringed copyright behavior, please give me a hint. THX!

Here in particular it emphasized that the third party is not allowed to contact addresses
published in this "version copyright statement" to send advertising material.
I will take legal means to resist sending spam.
-------------------------------------------------- ----------------------------------------
The framework under the GNU agreement, Detail View GNU License.
If you think about this item affection join the development team,
Please contact me: LaurentLeeJS@gmail.com
-------------------------------------------------- ----------------------------------------
Laurent.Lee.Framework Coded by Laurent Lee
*/

using System;
using System.Runtime.InteropServices;

namespace Laurent.Lee.WLB
{
    /// <summary>
    /// Mac 的摘要说明。
    /// </summary>
    public class TmphMac
    {
        public enum TmphNCBCONST
        {
            NCBNAMSZ = 16,      /* absolute length of a net name         */
            MAX_LANA = 254,      /* lana's in range 0 to MAX_LANA inclusive   */
            NCBENUM = 0x37,      /* NCB ENUMERATE LANA NUMBERS            */
            NRC_GOODRET = 0x00,      /* good return                              */
            NCBRESET = 0x32,      /* NCB RESET                        */
            NCBASTAT = 0x33,      /* NCB ADAPTER STATUS                  */
            NUM_NAMEBUF = 30,      /* Number of NAME's BUFFER               */
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TmphADAPTER_STATUS
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] adapter_address;

            public byte rev_major;
            public byte reserved0;
            public byte adapter_type;
            public byte rev_minor;
            public ushort duration;
            public ushort frmr_recv;
            public ushort frmr_xmit;
            public ushort iframe_recv_err;
            public ushort xmit_aborts;
            public uint xmit_success;
            public uint recv_success;
            public ushort iframe_xmit_err;
            public ushort recv_buff_unavail;
            public ushort t1_timeouts;
            public ushort ti_timeouts;
            public uint reserved1;
            public ushort free_ncbs;
            public ushort max_cfg_ncbs;
            public ushort max_ncbs;
            public ushort xmit_buf_unavail;
            public ushort max_dgram_size;
            public ushort pending_sess;
            public ushort max_cfg_sess;
            public ushort max_sess;
            public ushort max_sess_pkt_size;
            public ushort name_count;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TmphNAME_BUFFER
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)TmphNCBCONST.NCBNAMSZ)]
            public byte[] name;

            public byte name_num;
            public byte name_flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TmphNCB
        {
            public byte ncb_command;
            public byte ncb_retcode;
            public byte ncb_lsn;
            public byte ncb_num;
            public IntPtr ncb_buffer;
            public ushort ncb_length;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)TmphNCBCONST.NCBNAMSZ)]
            public byte[] ncb_callname;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)TmphNCBCONST.NCBNAMSZ)]
            public byte[] ncb_name;

            public byte ncb_rto;
            public byte ncb_sto;
            public IntPtr ncb_post;
            public byte ncb_lana_num;
            public byte ncb_cmd_cplt;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public byte[] ncb_reserve;

            public IntPtr ncb_event;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TmphLANA_ENUM
        {
            public byte length;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)TmphNCBCONST.MAX_LANA)]
            public byte[] lana;
        }

        [StructLayout(LayoutKind.Auto)]
        public struct TmphASTAT
        {
            public TmphADAPTER_STATUS adapt;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)TmphNCBCONST.NUM_NAMEBUF)]
            public TmphNAME_BUFFER[] NameBuff;
        }

        public class TmphWin32API
        {
            [DllImport("NETAPI32.DLL")]
            public static extern char Netbios(ref TmphNCB ncb);
        }

        public static string GetMacAddress()
        {
            string addr = "";
            int cb;
            TmphASTAT adapter;
            TmphNCB Ncb = new TmphNCB();
            char uRetCode;
            TmphLANA_ENUM lenum;

            Ncb.ncb_command = (byte)TmphNCBCONST.NCBENUM;
            cb = Marshal.SizeOf(typeof(TmphLANA_ENUM));
            Ncb.ncb_buffer = Marshal.AllocHGlobal(cb);
            Ncb.ncb_length = (ushort)cb;
            uRetCode = TmphWin32API.Netbios(ref Ncb);
            lenum = (TmphLANA_ENUM)Marshal.PtrToStructure(Ncb.ncb_buffer, typeof(TmphLANA_ENUM));
            Marshal.FreeHGlobal(Ncb.ncb_buffer);
            if (uRetCode != (short)TmphNCBCONST.NRC_GOODRET)
                return "";

            for (int i = 0; i < lenum.length; i++)
            {
                Ncb.ncb_command = (byte)TmphNCBCONST.NCBRESET;
                Ncb.ncb_lana_num = lenum.lana[i];
                uRetCode = TmphWin32API.Netbios(ref Ncb);
                if (uRetCode != (short)TmphNCBCONST.NRC_GOODRET)
                    return "";

                Ncb.ncb_command = (byte)TmphNCBCONST.NCBASTAT;
                Ncb.ncb_lana_num = lenum.lana[i];
                Ncb.ncb_callname[0] = (byte)'*';
                cb = Marshal.SizeOf(typeof(TmphADAPTER_STATUS)) + Marshal.SizeOf(typeof(TmphNAME_BUFFER)) * (int)TmphNCBCONST.NUM_NAMEBUF;
                Ncb.ncb_buffer = Marshal.AllocHGlobal(cb);
                Ncb.ncb_length = (ushort)cb;
                uRetCode = TmphWin32API.Netbios(ref Ncb);
                adapter.adapt = (TmphADAPTER_STATUS)Marshal.PtrToStructure(Ncb.ncb_buffer, typeof(TmphADAPTER_STATUS));
                Marshal.FreeHGlobal(Ncb.ncb_buffer);

                if (uRetCode == (short)TmphNCBCONST.NRC_GOODRET)
                {
                    if (i > 0)
                        addr += "-";
                    addr = string.Format("{0,2:X}-{1,2:X}-{2,2:X}-{3,2:X}-{4,2:X}-{5,2:X}",
                    adapter.adapt.adapter_address[0],
                    adapter.adapt.adapter_address[1],
                    adapter.adapt.adapter_address[2],
                    adapter.adapt.adapter_address[3],
                    adapter.adapt.adapter_address[4],
                    adapter.adapt.adapter_address[5]);
                }
            }
            return addr;
        }
    }
}