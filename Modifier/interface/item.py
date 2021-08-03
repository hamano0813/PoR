#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtWidgets import QGridLayout, QSpacerItem, QSizePolicy
from PySide6.QtCore import QSize
from widget import BackgroundFrame, MapCombo, ValueSpin, BoolCheck
from parameter import DataSetting, EnumData


# noinspection PyTypeChecker
class Item(BackgroundFrame):
    def __init__(self, parent):
        BackgroundFrame.__init__(self, parent)
        main_layout = QGridLayout()
        for idx in range(1, 9):
            self[f'道具{idx}'] = MapCombo(self, EnumData().IID_MAPPING, 'IID', value=DataSetting()[f'道具{idx}'])
            self[f'道具{idx}'].setIconSize(QSize(24, 24))
            self[f'道具{idx}'].setFixedWidth(200)
            self[f'耐久{idx}'] = ValueSpin(self, value=DataSetting()[f'耐久{idx}'])
            self[f'装备{idx}'] = BoolCheck(self, '装备', value=DataSetting()[f'装备{idx}'])
            self[f'掉落{idx}'] = BoolCheck(self, '掉落', value=DataSetting()[f'掉落{idx}'])
            main_layout.addWidget(self[f'道具{idx}'], idx - 1, 0, 1, 1)
            main_layout.addWidget(self[f'耐久{idx}'], idx - 1, 1, 1, 1)
            main_layout.addWidget(self[f'装备{idx}'], idx - 1, 2, 1, 1)
            main_layout.addWidget(self[f'掉落{idx}'], idx - 1, 3, 1, 1)
        main_layout.addItem(QSpacerItem(1, 1, QSizePolicy.Expanding, QSizePolicy.Expanding), 8, 4, 1, 1)
        self.setLayout(main_layout)
