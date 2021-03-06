#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtWidgets import QGridLayout, QTextEdit

from parameter import DataSetting
from widget import BackgroundFrame, ValueSpin, NameLabel


# noinspection PyTypeChecker
class Other(BackgroundFrame):
    def __init__(self, parent):
        BackgroundFrame.__init__(self, parent)

        self['所持金'] = ValueSpin(None, value=DataSetting()['所持金'])
        self['奖励EX'] = ValueSpin(None, value=DataSetting()['奖励EX'])
        self['所持金'].set_parent(None)
        self['奖励EX'].set_parent(None)
        text_edit = QTextEdit()
        text_edit.setHtml('''
        <b>修改可能出现严重错误，及时存档是好习惯</b><br>
        1.支持日文原版和汉化版，同时支持Dolphin5.0以上至今的任意开发版本<br>
        2.修改人物职业为上级职业时，记得在技能的隐藏特性里勾选“上级职业”<br>
        3.模型原则上可以修改人物是否持有武器等等，可能会出现奇妙的3D贴图错误，慎用<br>
        4.将敌方修改为本方可能在过关时加入，但容易造成后续关卡的阵容和强制出击出现问题，慎用<br>
        5.武器熟练只有在职业可以使用该类武器时生效，圣骑士等职业可以勾选隐藏特性装备各类武器<br>
        6.人物的生理节奏与战斗次数有关，每战斗10次生理曲线前进1格，30格为一个周期<br>
        <br>
        <i>created by Hamano0813</i>
        ''')
        text_edit.setReadOnly(True)

        main_layout = QGridLayout()
        main_layout.addWidget(NameLabel('所持金'), 0, 0, 1, 1)
        main_layout.addWidget(self['所持金'], 0, 1, 1, 1)
        main_layout.addWidget(NameLabel('奖励EX'), 1, 0, 1, 1)
        main_layout.addWidget(self['奖励EX'], 1, 1, 1, 1)
        main_layout.addWidget(text_edit, 2, 0, 1, 3)

        main_layout.setSpacing(3)
        self.setLayout(main_layout)
