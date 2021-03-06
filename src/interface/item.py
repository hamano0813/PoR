#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtCore import Qt, QSize
from PySide6.QtWidgets import QGridLayout, QSpacerItem, QSizePolicy

from parameter import DataSetting, EnumData
from widget import BackgroundFrame, MapCombo, ValueSpin, BoolCheck


# noinspection PyTypeChecker
class Item(BackgroundFrame):
    def __init__(self, parent):
        BackgroundFrame.__init__(self, parent)

        main_layout = QGridLayout()
        for idx in range(1, 9):
            self[f'道具{idx}'] = MapCombo(self, EnumData().IID_MAPPING, 'IID', value=DataSetting()[f'道具{idx}'])
            self[f'道具{idx}'].setIconSize(QSize(24, 24))
            self[f'道具{idx}'].setFixedWidth(280)
            self[f'耐久{idx}'] = ValueSpin(self, value=DataSetting()[f'耐久{idx}'])
            self[f'装备{idx}'] = BoolCheck(self, '装备', value=DataSetting()[f'装备{idx}'])
            self[f'掉落{idx}'] = BoolCheck(self, '掉落', value=DataSetting()[f'掉落{idx}'])
            main_layout.addWidget(self[f'道具{idx}'], idx - 1, 0, 1, 1)
            main_layout.addWidget(self[f'耐久{idx}'], idx - 1, 1, 1, 1)
            main_layout.addWidget(self[f'装备{idx}'], idx - 1, 2, 1, 1, Qt.AlignCenter)
            main_layout.addWidget(self[f'掉落{idx}'], idx - 1, 3, 1, 1, Qt.AlignCenter)

        main_layout.addItem(QSpacerItem(60, 1, QSizePolicy.Fixed, QSizePolicy.Fixed), 8, 2, 1, 1)
        main_layout.addItem(QSpacerItem(60, 1, QSizePolicy.Fixed, QSizePolicy.Fixed), 8, 3, 1, 1)
        main_layout.addItem(QSpacerItem(1, 1, QSizePolicy.Expanding, QSizePolicy.Expanding), 8, 4, 1, 1)
        main_layout.setSpacing(3)
        self.setLayout(main_layout)
