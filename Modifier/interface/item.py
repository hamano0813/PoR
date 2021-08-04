#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtWidgets import QGroupBox, QGridLayout, QSpacerItem, QSizePolicy
from PySide6.QtCore import QSize
from widget import BackgroundFrame, MapCombo, ValueSpin, BoolCheck
from parameter import DataSetting, EnumData


# noinspection PyTypeChecker
class Item(BackgroundFrame):
    def __init__(self, parent):
        BackgroundFrame.__init__(self, parent)
        main_layout = QGridLayout()

        weapon_group = QGroupBox('武器')
        weapon_layout = QGridLayout()
        for idx in range(1, 5):
            self[f'道具{idx}'] = MapCombo(self, EnumData().IID_MAPPING, 'IID', value=DataSetting()[f'道具{idx}'])
            self[f'道具{idx}'].setIconSize(QSize(24, 24))
            self[f'道具{idx}'].setFixedWidth(300)
            self[f'耐久{idx}'] = ValueSpin(self, value=DataSetting()[f'耐久{idx}'])
            self[f'装备{idx}'] = BoolCheck(self, '装备', value=DataSetting()[f'装备{idx}'])
            self[f'掉落{idx}'] = BoolCheck(self, '掉落', value=DataSetting()[f'掉落{idx}'])
            weapon_layout.addWidget(self[f'道具{idx}'], idx - 1, 0, 1, 1)
            weapon_layout.addWidget(self[f'耐久{idx}'], idx - 1, 1, 1, 1)
            weapon_layout.addWidget(self[f'装备{idx}'], idx - 1, 2, 1, 1)
            weapon_layout.addWidget(self[f'掉落{idx}'], idx - 1, 3, 1, 1)
        weapon_group.setLayout(weapon_layout)

        item_group = QGroupBox('道具')
        item_layout = QGridLayout()
        for idx in range(5, 9):
            self[f'道具{idx}'] = MapCombo(self, EnumData().IID_MAPPING, 'IID', value=DataSetting()[f'道具{idx}'])
            self[f'道具{idx}'].setIconSize(QSize(24, 24))
            self[f'道具{idx}'].setFixedWidth(300)
            self[f'耐久{idx}'] = ValueSpin(self, value=DataSetting()[f'耐久{idx}'])
            self[f'装备{idx}'] = BoolCheck(self, '装备', value=DataSetting()[f'装备{idx}'])
            self[f'掉落{idx}'] = BoolCheck(self, '掉落', value=DataSetting()[f'掉落{idx}'])
            item_layout.addWidget(self[f'道具{idx}'], idx - 5, 0, 1, 1)
            item_layout.addWidget(self[f'耐久{idx}'], idx - 5, 1, 1, 1)
            item_layout.addWidget(self[f'装备{idx}'], idx - 5, 2, 1, 1)
            item_layout.addWidget(self[f'掉落{idx}'], idx - 5, 3, 1, 1)
        item_group.setLayout(item_layout)

        main_layout.addWidget(weapon_group, 0, 0, 1, 1)
        main_layout.addWidget(item_group, 1, 0, 1, 1)
        main_layout.addItem(QSpacerItem(1, 1, QSizePolicy.Expanding, QSizePolicy.Expanding), 2, 1, 1, 1)
        self.setLayout(main_layout)
