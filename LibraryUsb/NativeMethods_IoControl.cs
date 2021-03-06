﻿using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace LibraryUsb
{
    public class NativeMethods_IoControl
    {
        public enum IoControlCodes : uint
        {
            IOCTL_STORAGE_EJECT_MEDIA = 0x2D4808,
            IOCTL_STORAGE_MEDIA_REMOVAL = 0x002D4804,
            IOCTL_BTH_DISCONNECT_DEVICE = 0x41000C,
            IOCTL_HID_ACTIVATE_DEVICE = 0xB001F,
            IOCTL_HID_DEACTIVATE_DEVICE = 0xB0023
        }

        [DllImport("kernel32.dll")]
        public static extern bool DeviceIoControl(IntPtr hDevice, IoControlCodes dwIoControlCode, byte[] lpInBuffer, int nInBufferSize, byte[] lpOutBuffer, int nOutBufferSize, out int lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll")]
        public static extern bool DeviceIoControl(SafeFileHandle hDevice, uint dwIoControlCode, IntPtr lpInBuffer, int nInBufferSize, IntPtr lpOutBuffer, int nOutBufferSize, out int lpBytesReturned, NativeOverlapped lpOverlapped);

        [DllImport("kernel32.dll")]
        public static extern bool DeviceIoControl(SafeFileHandle hDevice, uint dwIoControlCode, IntPtr lpInBuffer, int nInBufferSize, IntPtr lpOutBuffer, int nOutBufferSize, out int lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll")]
        public static extern bool GetOverlappedResult(SafeFileHandle hFile, ref NativeOverlapped lpOverlapped, out int lpBytesTransferred, bool bWait);
    }
}