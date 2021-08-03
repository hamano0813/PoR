#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtWidgets import QLabel, QSpacerItem, QSizePolicy, QGroupBox, QGridLayout, QFormLayout
from PySide6.QtCore import Qt, QSize
from widget import BackgroundFrame, MapCombo, ValueSpin, BoolCheck
from parameter import DataSetting, EnumData


# noinspection PyTypeChecker
class Status(BackgroundFrame):
    def __init__(self, parent):
        BackgroundFrame.__init__(self, parent)
        unit_group = self.init_unit_group()
        condition_group = self.init_condition_group()
        with_group = self.init_with_group()
        position_group = self.init_position_group()
        debuff_group = self.init_debuff_group()
        combat_group = self.init_combat_group()

        main_layout = QGridLayout()
        main_layout.addWidget(unit_group, 0, 0, 1, 4)
        main_layout.addWidget(condition_group, 1, 0, 2, 1)
        main_layout.addWidget(debuff_group, 1, 3, 2, 1)
        main_layout.addWidget(with_group, 1, 1, 1, 2)
        main_layout.addWidget(position_group, 2, 1, 1, 1)
        main_layout.addWidget(combat_group, 2, 2, 1, 1)
        main_layout.addItem(QSpacerItem(1, 1, QSizePolicy.Expanding, QSizePolicy.Expanding), 4, 5, 1, 1)
        self.setLayout(main_layout)

    def init_unit_group(self):
        group = QGroupBox('地图单位')
        layout = QFormLayout()
        self['人物'] = MapCombo(self, EnumData().PID_MAPPING, 'PID', value=DataSetting()['人物'])
        self['人物'].setIconSize(QSize(100, 100))
        self['人物'].setMaxVisibleItems(5)
        self['人物'].setMinimumWidth(260)
        self['职业'] = MapCombo(self, EnumData().JID_MAPPING, value=DataSetting()['职业'])
        self['模型'] = MapCombo(self, EnumData().AID_MAPPING, value=DataSetting()['模型'])
        self['阵营'] = MapCombo(self, EnumData().GROUP_MAPPING, value=DataSetting()['阵营'])
        self['部队'] = MapCombo(self, EnumData().ARMY_MAPPING, value=DataSetting()['部队'])
        self['首领'] = BoolCheck(self, '部队首领', value=DataSetting()['首领'])
        layout.addRow('人物', self['人物'])
        layout.addRow('职业', self['职业'])
        layout.addRow('模型', self['模型'])
        layout.addRow('阵营', self['阵营'])
        layout.addRow('部队', self['部队'])
        layout.addWidget(self['首领'])
        group.setLayout(layout)
        return group

    def init_condition_group(self):
        group = QGroupBox('角色状况')
        self['序号'] = ValueSpin(self, value=DataSetting()['序号'])
        self['序号'].setEnabled(False)
        self['等级'] = ValueSpin(self, value=DataSetting()['等级'])
        self['经验'] = ValueSpin(self, value=DataSetting()['经验'])
        self['ＨＰ'] = ValueSpin(self, value=DataSetting()['ＨＰ'])
        self['化身'] = ValueSpin(self, value=DataSetting()['化身'])
        layout = QFormLayout()
        layout.addRow('序号', self['序号'])
        layout.addRow('等级', self['等级'])
        layout.addRow('经验', self['经验'])
        layout.addRow('ＨＰ', self['ＨＰ'])
        layout.addRow('化身', self['化身'])
        group.setLayout(layout)
        return group

    def init_with_group(self):
        group = QGroupBox('行动状态')
        self['同行'] = MapCombo(self, self.parent().SLOT_MAPPING, value=DataSetting()['同行'])
        self['行动'] = BoolCheck(self, '结束', value=DataSetting()['行动'])
        self['救出'] = BoolCheck(self, '救出', value=DataSetting()['救出'])
        self['被救'] = BoolCheck(self, '被救', value=DataSetting()['被救'])
        layout = QGridLayout()
        layout.addWidget(QLabel('同行'), 0, 0, 1, 1)
        layout.addWidget(self['同行'], 0, 1, 1, 6)
        layout.addWidget(self['行动'], 1, 1, 1, 2)
        layout.addWidget(self['救出'], 1, 3, 1, 2)
        layout.addWidget(self['被救'], 1, 5, 1, 2)
        group.setLayout(layout)
        return group

    def init_position_group(self):
        group = QGroupBox('位置状态')
        self['坐标Ｘ'] = ValueSpin(self, value=DataSetting()['坐标Ｘ'])
        self['坐标Ｙ'] = ValueSpin(self, value=DataSetting()['坐标Ｙ'])
        layout = QFormLayout()
        layout.addRow('坐标Ｘ', self['坐标Ｘ'])
        layout.addRow('坐标Ｙ', self['坐标Ｙ'])
        group.setLayout(layout)
        return group

    def init_debuff_group(self):
        group = QGroupBox('异常状态')
        self['麻痹'] = ValueSpin(self, value=DataSetting()['麻痹'])
        self['沉默'] = ValueSpin(self, value=DataSetting()['沉默'])
        self['睡眠'] = ValueSpin(self, value=DataSetting()['睡眠'])
        self['狂暴'] = ValueSpin(self, value=DataSetting()['狂暴'])
        self['中毒'] = ValueSpin(self, value=DataSetting()['中毒'])
        layout = QFormLayout()
        layout.addRow('中毒', self['中毒'])
        layout.addRow('睡眠', self['睡眠'])
        layout.addRow('沉默', self['沉默'])
        layout.addRow('狂暴', self['狂暴'])
        layout.addRow('麻痹', self['麻痹'])
        group.setLayout(layout)
        return group

    def init_combat_group(self):
        group = QGroupBox('交战计数')
        self['战斗'] = ValueSpin(self, value=DataSetting()['战斗'])
        self['胜利'] = ValueSpin(self, value=DataSetting()['胜利'])
        layout = QFormLayout()
        layout.addRow('战斗', self['战斗'])
        layout.addRow('胜利', self['胜利'])
        group.setLayout(layout)
        return group
