#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtCore import QSize, Qt
from PySide6.QtWidgets import QSpacerItem, QSizePolicy, QGridLayout

from parameter import DataSetting, EnumData
from widget import BackgroundFrame, MapCombo, ValueSpin, BoolCheck, NameLabel


# noinspection PyTypeChecker
class Status(BackgroundFrame):
    # noinspection SpellCheckingInspection
    def __init__(self, parent):
        BackgroundFrame.__init__(self, parent)

        self['人物'] = MapCombo(self, EnumData().PID_MAPPING, 'PID', value=DataSetting()['人物'])
        self['人物'].setIconSize(QSize(100, 100))
        self['人物'].setMaxVisibleItems(5)
        self['职业'] = MapCombo(self, EnumData().JID_MAPPING, value=DataSetting()['职业'])
        self['模型'] = MapCombo(self, EnumData().AID_MAPPING, value=DataSetting()['模型'])
        self['阵营'] = MapCombo(self, EnumData().GROUP_MAPPING, value=DataSetting()['阵营'])
        self['部队'] = MapCombo(self, EnumData().ARMY_MAPPING, value=DataSetting()['部队'])
        self['首领'] = BoolCheck(self, '部队首领', value=DataSetting()['首领'])
        self['同行'] = MapCombo(self, self.parent().SLOT_MAPPING, value=DataSetting()['同行'])
        self['行动'] = BoolCheck(self, {'未行动': 0, '锁定未行动': 0, '行动完毕': 1}, value=DataSetting()['行动'])
        self['行动'].setProperty('class', 'action')
        self['救出'] = BoolCheck(self, '救出', value=DataSetting()['救出'])
        self['被救'] = BoolCheck(self, '被救', value=DataSetting()['被救'])
        self['序号'] = ValueSpin(self, value=DataSetting()['序号'])
        self['序号'].setEnabled(False)
        self['等级'] = ValueSpin(self, value=DataSetting()['等级'])
        self['经验'] = ValueSpin(self, value=DataSetting()['经验'])
        self['ＨＰ'] = ValueSpin(self, value=DataSetting()['ＨＰ'])
        self['化身'] = ValueSpin(self, value=DataSetting()['化身'])
        self['麻痹'] = ValueSpin(self, value=DataSetting()['麻痹'])
        self['沉默'] = ValueSpin(self, value=DataSetting()['沉默'])
        self['睡眠'] = ValueSpin(self, value=DataSetting()['睡眠'])
        self['狂暴'] = ValueSpin(self, value=DataSetting()['狂暴'])
        self['中毒'] = ValueSpin(self, value=DataSetting()['中毒'])
        self['坐标Ｘ'] = ValueSpin(self, value=DataSetting()['坐标Ｘ'])
        self['坐标Ｙ'] = ValueSpin(self, value=DataSetting()['坐标Ｙ'])
        self['战斗'] = ValueSpin(self, value=DataSetting()['战斗'])
        self['胜利'] = ValueSpin(self, value=DataSetting()['胜利'])

        self['MTYPE'] = MapCombo(self, EnumData().MTYPE_MAPPING, value=DataSetting()['MTYPE'])
        self['SEQ1'] = MapCombo(self, EnumData().SEQ_MAPPING, value=DataSetting()['SEQ1'])
        self['SEQ2'] = MapCombo(self, EnumData().SEQ_MAPPING, value=DataSetting()['SEQ2'])
        self['SEQ3'] = MapCombo(self, EnumData().SEQ_MAPPING, value=DataSetting()['SEQ3'])

        main_layout = QGridLayout()
        main_layout.addWidget(NameLabel('人物'), 0, 0, 1, 1)
        main_layout.addWidget(NameLabel('职业'), 1, 0, 1, 1)
        main_layout.addWidget(NameLabel('模型'), 2, 0, 1, 1)
        main_layout.addWidget(NameLabel('阵营'), 3, 0, 1, 1)
        main_layout.addWidget(NameLabel('部队'), 3, 2, 1, 1)
        main_layout.addWidget(NameLabel('同行'), 4, 0, 1, 1)

        main_layout.addWidget(NameLabel('模式'), 5, 0, 1, 1)
        main_layout.addWidget(NameLabel('攻击'), 6, 0, 1, 1)
        main_layout.addWidget(NameLabel('目标'), 7, 0, 1, 1)
        main_layout.addWidget(NameLabel('回复'), 8, 0, 1, 1)

        main_layout.addWidget(NameLabel('序号'), 9, 0, 1, 1)
        main_layout.addWidget(NameLabel('等级'), 10, 0, 1, 1)
        main_layout.addWidget(NameLabel('经验'), 11, 0, 1, 1)
        main_layout.addWidget(NameLabel('ＨＰ'), 12, 0, 1, 1)
        main_layout.addWidget(NameLabel('化身'), 13, 0, 1, 1)
        main_layout.addWidget(NameLabel('中毒'), 9, 2, 1, 1)
        main_layout.addWidget(NameLabel('睡眠'), 10, 2, 1, 1)
        main_layout.addWidget(NameLabel('沉默'), 11, 2, 1, 1)
        main_layout.addWidget(NameLabel('狂暴'), 12, 2, 1, 1)
        main_layout.addWidget(NameLabel('麻痹'), 13, 2, 1, 1)
        main_layout.addWidget(NameLabel('坐标Ｘ'), 9, 4, 1, 1)
        main_layout.addWidget(NameLabel('坐标Ｙ'), 10, 4, 1, 1)
        main_layout.addWidget(NameLabel('战斗'), 12, 4, 1, 1)
        main_layout.addWidget(NameLabel('胜利'), 13, 4, 1, 1)

        main_layout.addWidget(self['人物'], 0, 1, 1, 5)
        main_layout.addWidget(self['职业'], 1, 1, 1, 5)
        main_layout.addWidget(self['模型'], 2, 1, 1, 5)
        main_layout.addWidget(self['阵营'], 3, 1, 1, 1)
        main_layout.addWidget(self['部队'], 3, 3, 1, 2)
        main_layout.addWidget(self['首领'], 3, 5, 1, 1)
        main_layout.addWidget(self['同行'], 4, 1, 1, 2)
        main_layout.addWidget(self['救出'], 4, 3, 1, 1, Qt.AlignCenter)
        main_layout.addWidget(self['被救'], 4, 4, 1, 1, Qt.AlignCenter)

        main_layout.addWidget(self['MTYPE'], 5, 1, 1, 5)
        main_layout.addWidget(self['SEQ1'], 6, 1, 1, 5)
        main_layout.addWidget(self['SEQ2'], 7, 1, 1, 5)
        main_layout.addWidget(self['SEQ3'], 8, 1, 1, 5)

        main_layout.addWidget(self['序号'], 9, 1, 1, 1)
        main_layout.addWidget(self['等级'], 10, 1, 1, 1)
        main_layout.addWidget(self['经验'], 11, 1, 1, 1)
        main_layout.addWidget(self['ＨＰ'], 12, 1, 1, 1)
        main_layout.addWidget(self['化身'], 13, 1, 1, 1)
        main_layout.addWidget(self['中毒'], 9, 3, 1, 1)
        main_layout.addWidget(self['睡眠'], 10, 3, 1, 1)
        main_layout.addWidget(self['沉默'], 11, 3, 1, 1)
        main_layout.addWidget(self['狂暴'], 12, 3, 1, 1)
        main_layout.addWidget(self['麻痹'], 13, 3, 1, 1)
        main_layout.addWidget(self['坐标Ｘ'], 9, 5, 1, 1)
        main_layout.addWidget(self['坐标Ｙ'], 10, 5, 1, 1)
        main_layout.addWidget(self['行动'], 11, 4, 1, 2, Qt.AlignLeft)
        main_layout.addWidget(self['战斗'], 12, 5, 1, 1)
        main_layout.addWidget(self['胜利'], 13, 5, 1, 1)

        main_layout.addItem(QSpacerItem(1, 1, QSizePolicy.Expanding, QSizePolicy.Expanding), 14, 6, 1, 1)
        main_layout.setSpacing(3)
        self.setLayout(main_layout)
