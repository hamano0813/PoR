#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtWidgets import QSpacerItem, QSizePolicy, QGroupBox, QGridLayout, QFormLayout
from PySide6.QtCore import QSize
from widget import BackgroundFrame, MapCombo, ValueSpin, BoolCheck
from parameter import DataSetting, EnumData


# noinspection PyTypeChecker
class Status(BackgroundFrame):
    def __init__(self, parent):
        BackgroundFrame.__init__(self, parent)
        unit_group = self.init_unit_group()
        with_group = self.init_with_group()
        position_group = self.init_position_group()
        debuff_group = self.init_debuff_group()

        main_layout = QGridLayout()
        main_layout.addWidget(unit_group, 0, 0, 1, 2)
        main_layout.addWidget(with_group, 1, 0, 1, 1)
        main_layout.addWidget(position_group, 2, 0, 1, 1)
        main_layout.addWidget(debuff_group, 1, 1, 2, 1)
        main_layout.addItem(QSpacerItem(1, 1, QSizePolicy.Expanding, QSizePolicy.Expanding), 10, 10, 1, 1)
        self.setLayout(main_layout)

    def init_unit_group(self):
        group = QGroupBox(self.tr('Unit'))
        self['人物'] = MapCombo(self, EnumData().PID_MAPPING, 'PID', value=DataSetting()['人物'])
        self['人物'].setIconSize(QSize(100, 100))
        self['人物'].setMaxVisibleItems(5)
        self['职业'] = MapCombo(self, EnumData().JID_MAPPING, value=DataSetting()['职业'])
        self['模型'] = MapCombo(self, EnumData().AID_MAPPING, value=DataSetting()['模型'])
        self['阵营'] = MapCombo(self, EnumData().GROUP_MAPPING, value=DataSetting()['阵营'])
        self['部队'] = MapCombo(self, EnumData().ARMY_MAPPING, value=DataSetting()['部队'])
        layout = QFormLayout()
        layout.addRow(self.tr('Person'), self['人物'])
        layout.addRow(self.tr('Job'), self['职业'])
        layout.addRow(self.tr('Action'), self['模型'])
        layout.addRow(self.tr('Group'), self['阵营'])
        layout.addRow(self.tr('Army'), self['部队'])
        group.setLayout(layout)
        return group

    def init_with_group(self):
        group = QGroupBox(self.tr('With'))
        self['同行'] = MapCombo(self, self.parent().SLOT_MAPPING, value=DataSetting()['同行'])
        self['救出'] = BoolCheck(self, self.tr('Saving'), value=DataSetting()['救出'])
        self['被救'] = BoolCheck(self, self.tr('Saved'), value=DataSetting()['被救'])
        layout = QGridLayout()
        layout.addWidget(self['同行'], 0, 0, 1, 2)
        layout.addWidget(self['救出'], 1, 0, 1, 1)
        layout.addWidget(self['被救'], 1, 1, 1, 1)
        group.setLayout(layout)
        return group

    def init_position_group(self):
        group = QGroupBox(self.tr('Position'))
        self['坐标Ｘ'] = ValueSpin(self, value=DataSetting()['坐标Ｘ'])
        self['坐标Ｙ'] = ValueSpin(self, value=DataSetting()['坐标Ｙ'])
        layout = QFormLayout()
        layout.addRow(self.tr('Position X'), self['坐标Ｘ'])
        layout.addRow(self.tr('Position Y'), self['坐标Ｙ'])
        group.setLayout(layout)
        return group

    def init_debuff_group(self):
        group = QGroupBox(self.tr('Debuff'))
        self['麻痹'] = ValueSpin(self, value=DataSetting()['麻痹'])
        self['沉默'] = ValueSpin(self, value=DataSetting()['沉默'])
        self['睡眠'] = ValueSpin(self, value=DataSetting()['睡眠'])
        self['狂暴'] = ValueSpin(self, value=DataSetting()['狂暴'])
        self['中毒'] = ValueSpin(self, value=DataSetting()['中毒'])
        layout = QFormLayout()
        layout.addRow(self.tr('Poison'), self['中毒'])
        layout.addRow(self.tr('Sleep'), self['睡眠'])
        layout.addRow(self.tr('Silence'), self['沉默'])
        layout.addRow(self.tr('Berserk'), self['狂暴'])
        layout.addRow(self.tr('Stun'), self['麻痹'])
        group.setLayout(layout)
        return group
