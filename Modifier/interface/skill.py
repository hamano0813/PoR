#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtWidgets import QLabel, QGridLayout, QScrollArea
from PySide6.QtGui import QPixmap
from widget import BackgroundFrame, BoolCheck
from parameter import DataSetting, EnumData


# noinspection PyTypeChecker
class Skill(BackgroundFrame):
    def __init__(self, parent):
        BackgroundFrame.__init__(self, parent)
        skill_frame = BackgroundFrame()
        mastery_frame = BackgroundFrame()
        feature_frame = BackgroundFrame()
        skill_layout = QGridLayout()
        mastery_layout = QGridLayout()
        feature_layout = QGridLayout()

        for idx, skill in enumerate(EnumData.SKILL_ENUM):
            self[skill] = BoolCheck(self, EnumData().SID_ENUM[skill].removeprefix('SID_'),
                                    value=DataSetting()[skill])
            label = QLabel()
            label.setPixmap(QPixmap(f':SID/{skill}.gif'))
            label.setFixedSize(32, 32)
            skill_layout.addWidget(label, idx, 0, 1, 1)
            skill_layout.addWidget(self[skill], idx, 1, 1, 1)
        skill_frame.setLayout(skill_layout)
        skill_scroll = QScrollArea()
        skill_scroll.setWidget(skill_frame)

        for idx, mastery in enumerate(EnumData.MASTERY_ENUM):
            self[mastery] = BoolCheck(self, EnumData().SID_ENUM[mastery].removeprefix('SID_'),
                                      value=DataSetting()[mastery])
            label = QLabel()
            label.setPixmap(QPixmap(f':SID/{mastery}.gif'))
            label.setFixedSize(32, 32)
            mastery_layout.addWidget(label, idx, 0, 1, 1)
            mastery_layout.addWidget(self[mastery], idx, 1, 1, 1)
        mastery_frame.setLayout(mastery_layout)
        mastery_scorll = QScrollArea()
        mastery_scorll.setWidget(mastery_frame)

        for idx, feature in enumerate(EnumData.FEATURE_ENUM):
            self[feature] = BoolCheck(self, EnumData().SID_ENUM[feature].removeprefix('SID_'),
                                      value=DataSetting()[feature])
            feature_layout.addWidget(self[feature], idx, 0, 1, 1)
        feature_frame.setLayout(feature_layout)
        feature_scorll = QScrollArea()
        feature_scorll.setWidget(feature_frame)

        main_layout = QGridLayout()
        main_layout.addWidget(QLabel('技能'), 0, 0, 1, 1)
        main_layout.addWidget(QLabel('奥义'), 0, 1, 1, 1)
        main_layout.addWidget(QLabel('特性'), 0, 2, 1, 1)
        main_layout.addWidget(skill_scroll, 1, 0, 1, 1)
        main_layout.addWidget(mastery_scorll, 1, 1, 1, 1)
        main_layout.addWidget(feature_scorll, 1, 2, 1, 1)
        self.setLayout(main_layout)
