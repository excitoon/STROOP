﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SM64_Diagnostic.Utilities;
using System.Windows.Forms;
using SM64_Diagnostic.Structs;
using SM64_Diagnostic.Structs.Configurations;
using SM64_Diagnostic.Controls;

namespace SM64_Diagnostic.Managers
{
    public class DebugManager : DataManager
    {
        RadioButton _advancedModeOffRadioButton;
        RadioButton[] _advancedModeSettingRadioButtons;

        RadioButton _resourceMeterOffRadioButton;
        RadioButton _resourceMeter1RadioButton;
        RadioButton _resourceMeter2RadioButton;

        CheckBox _classicModeCheckbox;
        CheckBox _spawnModeCheckbox;
        CheckBox _stageSelectCheckbox;
        CheckBox _freeMovementCheckbox;

        public DebugManager(List<WatchVariable> variableData, Control tabControl, NoTearFlowLayoutPanel variableTable)
            : base(variableData, variableTable)
        {
            SplitContainer splitContainerDebug = tabControl.Controls["splitContainerDebug"] as SplitContainer;

            // Advanced mode
            GroupBox advancedModeGroupbox = splitContainerDebug.Panel1.Controls["groupBoxAdvancedMode"] as GroupBox;

            _advancedModeOffRadioButton = advancedModeGroupbox.Controls["radioButtonAdvancedModeOff"] as RadioButton;
            _advancedModeOffRadioButton.Click += (sender, e) =>
            {
                Config.Stream.SetValue((byte)0, Config.Debug.AdvancedModeAddress);
                Config.Stream.SetValue((byte)0, Config.Debug.AdvancedModeSettingAddress);
            };

            _advancedModeSettingRadioButtons = new RadioButton[6];
            _advancedModeSettingRadioButtons[0] = advancedModeGroupbox.Controls["radioButtonAdvancedModeObjectCounter"] as RadioButton;
            _advancedModeSettingRadioButtons[1] = advancedModeGroupbox.Controls["radioButtonAdvancedModeCheckInfo"] as RadioButton;
            _advancedModeSettingRadioButtons[2] = advancedModeGroupbox.Controls["radioButtonAdvancedModeMapInfo"] as RadioButton;
            _advancedModeSettingRadioButtons[3] = advancedModeGroupbox.Controls["radioButtonAdvancedModeStageInfo"] as RadioButton;
            _advancedModeSettingRadioButtons[4] = advancedModeGroupbox.Controls["radioButtonAdvancedModeEffectInfo"] as RadioButton;
            _advancedModeSettingRadioButtons[5] = advancedModeGroupbox.Controls["radioButtonAdvancedModeEnemyInfo"] as RadioButton;
            for (int i = 0; i < _advancedModeSettingRadioButtons.Length; i++)
            {
                byte localIndex = (byte)i;
                _advancedModeSettingRadioButtons[i].Click += (sender, e) =>
                {
                    Config.Stream.SetValue((byte)1, Config.Debug.AdvancedModeAddress);
                    Config.Stream.SetValue(localIndex, Config.Debug.AdvancedModeSettingAddress);
                };
            }

            // Resource meter
            GroupBox resourceMeterGroupbox = splitContainerDebug.Panel1.Controls["groupBoxResourceMeter"] as GroupBox;

            _resourceMeterOffRadioButton = resourceMeterGroupbox.Controls["radioButtonResourceMeterOff"] as RadioButton;
            _resourceMeterOffRadioButton.Click += (sender, e) =>
            {
                Config.Stream.SetValue((byte)0, Config.Debug.ResourceMeterAddress);
                Config.Stream.SetValue((ushort)0, Config.Debug.ResourceMeterSettingAddress);
            };

            _resourceMeter1RadioButton = resourceMeterGroupbox.Controls["radioButtonResourceMeter1"] as RadioButton;
            _resourceMeter1RadioButton.Click += (sender, e) =>
            {
                Config.Stream.SetValue((byte)1, Config.Debug.ResourceMeterAddress);
                Config.Stream.SetValue((ushort)0, Config.Debug.ResourceMeterSettingAddress);
            };

            _resourceMeter2RadioButton = resourceMeterGroupbox.Controls["radioButtonResourceMeter2"] as RadioButton;
            _resourceMeter2RadioButton.Click += (sender, e) =>
            {
                Config.Stream.SetValue((byte)1, Config.Debug.ResourceMeterAddress);
                Config.Stream.SetValue((ushort)1, Config.Debug.ResourceMeterSettingAddress);
            };

            // Misc debug
            GroupBox miscDebugGroupbox = splitContainerDebug.Panel1.Controls["groupBoxMiscDebug"] as GroupBox;

            _classicModeCheckbox = miscDebugGroupbox.Controls["checkBoxClassicMode"] as CheckBox;
            _classicModeCheckbox.Click += (sender, e) =>
            {
                Config.Stream.SetValue(_classicModeCheckbox.Checked ? (byte)0x01 : (byte)0x00, Config.Debug.ClassicModeAddress);
            };

            _spawnModeCheckbox = miscDebugGroupbox.Controls["checkBoxSpawnMode"] as CheckBox;
            _spawnModeCheckbox.Click += (sender, e) =>
            {
                Config.Stream.SetValue(_spawnModeCheckbox.Checked ? (byte)0x03 : (byte)0x00, Config.Debug.AdvancedModeSettingAddress);
                Config.Stream.SetValue(_spawnModeCheckbox.Checked ? (byte)0x01 : (byte)0x00, Config.Debug.SpawnModeAddress);
            };

            _stageSelectCheckbox = miscDebugGroupbox.Controls["checkBoxStageSelect"] as CheckBox;
            _stageSelectCheckbox.Click += (sender, e) =>
            {
                Config.Stream.SetValue(_stageSelectCheckbox.Checked ? (byte)0x01 : (byte)0x00, Config.Debug.StageSelectAddress);
            };

            _freeMovementCheckbox = miscDebugGroupbox.Controls["checkBoxFreeMovement"] as CheckBox;
            _freeMovementCheckbox.Click += (sender, e) => 
            {
                Config.Stream.SetValue(
                    _freeMovementCheckbox.Checked ? Config.Debug.FreeMovementOnValue : Config.Debug.FreeMovementOffValue,
                    Config.Debug.FreeMovementAddress);
            };
        }

        public override void Update(bool updateView)
        {
            if (!updateView)
                return;

            // Advanced mode
            byte advancedModeOn = Config.Stream.GetByte(Config.Debug.AdvancedModeAddress);
            byte advancedModeSetting = Config.Stream.GetByte(Config.Debug.AdvancedModeSettingAddress);
            if (advancedModeOn % 2 != 0)
            {
                if (advancedModeSetting > 0 && advancedModeSetting < _advancedModeSettingRadioButtons.Length)
                    _advancedModeSettingRadioButtons[advancedModeSetting].Checked = true;
                else
                    _advancedModeSettingRadioButtons[0].Checked = true;
            }
            else
            {
                _advancedModeOffRadioButton.Checked = true;
            }

            // Resource meter
            byte resourceMeterOn = Config.Stream.GetByte(Config.Debug.ResourceMeterAddress);
            ushort resourceMeterSetting = Config.Stream.GetUInt16(Config.Debug.ResourceMeterSettingAddress);
            if (resourceMeterOn != 0)
            {
                if (resourceMeterSetting != 0)
                    _resourceMeter2RadioButton.Checked = true;
                else
                    _resourceMeter1RadioButton.Checked = true;
            }
            else
            {
                _resourceMeterOffRadioButton.Checked = true;
            }

            // Misc debug
            _classicModeCheckbox.Checked = Config.Stream.GetByte(Config.Debug.ClassicModeAddress) == 0x01;
            _spawnModeCheckbox.Checked = Config.Stream.GetByte(Config.Debug.AdvancedModeSettingAddress) == 0x03
                 && Config.Stream.GetByte(Config.Debug.SpawnModeAddress) == 0x01;
            _stageSelectCheckbox.Checked = Config.Stream.GetByte(Config.Debug.StageSelectAddress) == 0x01;
            _freeMovementCheckbox.Checked = Config.Stream.GetUInt16(Config.Debug.FreeMovementAddress) == Config.Debug.FreeMovementOnValue;

            base.Update();
        }
    }
}
