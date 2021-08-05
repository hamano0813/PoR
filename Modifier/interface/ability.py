#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtWidgets import QGridLayout, QSpacerItem, QSizePolicy
from PySide6.QtCore import Qt, QSize
from PySide6.QtGui import QPixmap
from widget import BackgroundFrame, NameLabel, ValueSpin
from parameter import DataSetting, EnumData


# noinspection PyTypeChecker
class Ability(BackgroundFrame):
    ABILITY = {
        'HP': 'ＨＰ',
        'STR': '力量',
        'MAG': '魔力',
        'SKL': '技术',
        'SPD': '速度',
        'LUK': '幸运',
        'DEF': '守备',
        'MDF': '魔防',
        'CON': '体格',
        'MOV': '移动',
    }
    WEAPON = {
        'SWORD': '剑',
        'LANCE': '枪',
        'AXE': '斧',
        'BOW': '弓',
        'FIRE': '炎',
        'THUNDER': '雷',
        'WIND': '风',
        'STAFF': '杖',
    }

    def __init__(self, parent):
        BackgroundFrame.__init__(self, parent)

        self['S_HP'] = ValueSpin(self, value=DataSetting()['S_HP'])
        self['S_STR'] = ValueSpin(self, value=DataSetting()['S_STR'])
        self['S_MAG'] = ValueSpin(self, value=DataSetting()['S_MAG'])
        self['S_SKL'] = ValueSpin(self, value=DataSetting()['S_SKL'])
        self['S_SPD'] = ValueSpin(self, value=DataSetting()['S_SPD'])
        self['S_LUK'] = ValueSpin(self, value=DataSetting()['S_LUK'])
        self['S_DEF'] = ValueSpin(self, value=DataSetting()['S_DEF'])
        self['S_MDF'] = ValueSpin(self, value=DataSetting()['S_MDF'])
        self['S_CON'] = ValueSpin(self, value=DataSetting()['S_CON'])
        self['S_MOV'] = ValueSpin(self, value=DataSetting()['S_MOV'])

        self['E_HP'] = ValueSpin(self, value=DataSetting()['E_HP'])
        self['E_STR'] = ValueSpin(self, value=DataSetting()['E_STR'])
        self['E_MAG'] = ValueSpin(self, value=DataSetting()['E_MAG'])
        self['E_SKL'] = ValueSpin(self, value=DataSetting()['E_SKL'])
        self['E_SPD'] = ValueSpin(self, value=DataSetting()['E_SPD'])
        self['E_LUK'] = ValueSpin(self, value=DataSetting()['E_LUK'])
        self['E_DEF'] = ValueSpin(self, value=DataSetting()['E_DEF'])
        self['E_MDF'] = ValueSpin(self, value=DataSetting()['E_MDF'])

        self['剑'] = ValueSpin(self, value=DataSetting()['剑'])
        self['枪'] = ValueSpin(self, value=DataSetting()['枪'])
        self['斧'] = ValueSpin(self, value=DataSetting()['斧'])
        self['弓'] = ValueSpin(self, value=DataSetting()['弓'])
        self['炎'] = ValueSpin(self, value=DataSetting()['炎'])
        self['雷'] = ValueSpin(self, value=DataSetting()['雷'])
        self['风'] = ValueSpin(self, value=DataSetting()['风'])
        self['杖'] = ValueSpin(self, value=DataSetting()['杖'])

        main_layout = QGridLayout()
        main_layout.addWidget(NameLabel('能力'), 0, 1, 1, 1)
        main_layout.addWidget(NameLabel('经验'), 0, 2, 1, 1)
        main_layout.addWidget(NameLabel('熟练'), 0, 4, 1, 1)
        for idx, (ability, name) in enumerate(self.ABILITY.items()):
            main_layout.addWidget(NameLabel(name), idx + 1, 0)
            main_layout.addWidget(self[f'S_{ability}'], idx + 1, 1)
            if idx < 8:
                main_layout.addWidget(self[f'E_{ability}'], idx + 1, 2)
        for idx, (name, weapon) in enumerate(self.WEAPON.items()):
            label = NameLabel(name)
            label.setPixmap(QPixmap(f':WP/WP_{name}.png'))
            label.setStyleSheet('')
            label.setFixedWidth(50)
            label.setAlignment(Qt.AlignRight | Qt.AlignVCenter)
            main_layout.addWidget(label, idx + 1, 3)
            main_layout.addWidget(self[f'{weapon}'], idx + 1, 4)

        main_layout.addItem(QSpacerItem(1, 1, QSizePolicy.Expanding, QSizePolicy.Expanding), 11, 6, 1, 1)
        main_layout.setSpacing(3)
        self.setLayout(main_layout)
