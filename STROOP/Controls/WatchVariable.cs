﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using STROOP.Utilities;
using STROOP.Structs;
using STROOP.Extensions;
using System.Reflection;
using STROOP.Managers;
using STROOP.Structs.Configurations;

namespace STROOP.Controls
{
    public class WatchVariable
    {
        public readonly string MemoryTypeName;
        public readonly Type MemoryType;
        public readonly int? ByteCount;
        public readonly int? NibbleCount;
        public readonly bool? SignedType;

        public readonly string SpecialType;

        public readonly BaseAddressTypeEnum BaseAddressType;

        public readonly uint? OffsetUS;
        public readonly uint? OffsetJP;
        public readonly uint? OffsetPAL;
        public readonly uint? OffsetDefault;

        public readonly uint? Mask;

        private readonly Func<uint, string> _getterFunction;
        private readonly Func<string, uint, bool> _setterFunction;

        public bool IsSpecial { get { return SpecialType != null; } }

        private bool UseAbsoluteAddressing { get { return BaseAddressType == BaseAddressTypeEnum.Absolute; } }

        public uint Offset
        {
            get
            {
                switch (Config.Version)
                {
                    case RomVersion.US:
                        if (OffsetUS.HasValue) return OffsetUS.Value;
                        break;
                    case RomVersion.JP:
                        if (OffsetJP.HasValue) return OffsetJP.Value;
                        break;
                    case RomVersion.PAL:
                        if (OffsetPAL.HasValue) return OffsetPAL.Value;
                        break;
                }
                if (OffsetDefault.HasValue) return OffsetDefault.Value;
                return 0;
            }
        }

        public List<uint> BaseAddressList
        {
            get
            {
                return WatchVariableUtilities.GetBaseAddressListFromBaseAddressType(BaseAddressType);
            }
        }

        public List<uint> AddressList
        {
            get
            {
                return BaseAddressList.ConvertAll(baseAddress => baseAddress + Offset);
            }
        }

        public WatchVariable(string memoryTypeName, string specialType, BaseAddressTypeEnum baseAddress,
            uint? offsetUS, uint? offsetJP, uint? offsetPAL, uint? offsetDefault, uint? mask)
        {
            BaseAddressType = baseAddress;

            OffsetUS = offsetUS;
            OffsetJP = offsetJP;
            OffsetPAL = offsetPAL;
            OffsetDefault = offsetDefault;

            SpecialType = specialType;

            MemoryTypeName = memoryTypeName;
            MemoryType = memoryTypeName == null ? null : WatchVariableUtilities.StringToType[MemoryTypeName];
            ByteCount = memoryTypeName == null ? (int?)null : WatchVariableUtilities.TypeSize[MemoryType];
            NibbleCount = memoryTypeName == null ? (int?)null : WatchVariableUtilities.TypeSize[MemoryType] * 2;
            SignedType = memoryTypeName == null ? (bool?)null : WatchVariableUtilities.TypeSign[MemoryType];

            Mask = mask;
            
            // Created getter/setter functions
            if (IsSpecial)
            {
                (_getterFunction, _setterFunction) = WatchVariableSpecialUtilities.CreateGetterSetterFunctions(SpecialType);
            }
            else
            {
                _getterFunction = (uint address) =>
                {
                    return Config.Stream.GetValue(MemoryType, address, UseAbsoluteAddressing, Mask).ToString();
                };
                _setterFunction = (string value, uint address) =>
                {
                    return Config.Stream.SetValueRoundingWrapping(
                        MemoryType, value, address, UseAbsoluteAddressing, Mask);
                };

            }
        }

        public List<string> GetValues(List<uint> addresses = null)
        {
            List<uint> addressList = addresses ?? AddressList;
            return addressList.ConvertAll(
                address => _getterFunction(address));
        }

        public bool SetValue(string value, List<uint> addresses = null)
        {
            List<uint> addressList = addresses ?? AddressList;
            if (addressList.Count == 0) return false;

            bool streamAlreadySuspended = Config.Stream.IsSuspended;
            if (!streamAlreadySuspended) Config.Stream.Suspend();
            bool success = addressList.ConvertAll(
                address => _setterFunction(value, address))
                    .Aggregate(true, (b1, b2) => b1 && b2);
            if (!streamAlreadySuspended) Config.Stream.Resume();
            return success;
        }

        public List<WatchVariableLock> GetLocks(List<uint> addresses = null)
        {
            List<string> values = GetValues(addresses);
            List<uint> addressList = addresses ?? AddressList;
            if (values.Count != addressList.Count) return new List<WatchVariableLock>();

            List<WatchVariableLock> locks = new List<WatchVariableLock>();
            for (int i = 0; i < values.Count; i++)
            {
                locks.Add(new WatchVariableLock(
                    IsSpecial, MemoryType, ByteCount, Mask, addressList[i], SpecialType, _setterFunction, values[i]));
            }
            return locks;
        }

        public string GetTypeDescription()
        {
            if (IsSpecial)
            {
                string memoryTypeString = MemoryTypeName != null ? " (" + MemoryTypeName + ")" : "";
                return "special" + memoryTypeString;
            }
            else
            {
                string maskString = "";
                if (Mask != null)
                {
                    maskString = " with mask " + String.Format("0x{0:X" + NibbleCount.Value + "}", Mask.Value);
                }
                return MemoryTypeName + maskString;
            }
        }

        public string GetBaseOffsetDescription()
        {
            string offsetString = IsSpecial ? SpecialType : String.Format("0x{0:X}", Offset);
            return BaseAddressType + " + " + offsetString;
        }

        public string GetRamAddressString(bool addressArea = true, List<uint> addresses = null)
        {
            List<uint> addressList = addresses ?? AddressList;
            if (IsSpecial) return "(none)";
            if (addressList.Count == 0) return "(none)";
            return String.Format("0x{0:X8}", GetRamAddress(addressArea, addresses));
        }

        public uint GetRamAddress(bool addressArea = true, List<uint> addresses = null)
        {
            List<uint> addressList = addresses ?? AddressList;
            if (addressList.Count == 0) return 0;

            UIntPtr addressPtr = new UIntPtr(addressList[0]);
            uint address;

            if (UseAbsoluteAddressing)
                address = (uint)Config.Stream.ConvertAddressEndianess(
                    new UIntPtr(addressPtr.ToUInt64() - (ulong)Config.Stream.ProcessMemoryOffset.ToInt64()),
                    ByteCount.Value);
            else
                address = addressPtr.ToUInt32();

            return addressArea ? address | 0x80000000 : address & 0x0FFFFFFF;
        }

        public string GetProcessAddressString(List<uint> addresses = null)
        {
            List<uint> addressList = addresses ?? AddressList;
            if (IsSpecial) return "(none)";
            if (addressList.Count == 0) return "(none)";
            return String.Format("0x{0:X8}", GetProcessAddress(addresses).ToUInt64());
        }

        public UIntPtr GetProcessAddress(List<uint> addresses = null)
        {
            uint address = GetRamAddress(false, addresses);
            return Config.Stream.ConvertAddressEndianess(
                new UIntPtr(address + (ulong)Config.Stream.ProcessMemoryOffset.ToInt64()), ByteCount.Value);
        }
    }
}
