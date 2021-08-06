#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtWidgets import QLabel, QGridLayout, QScrollArea
from PySide6.QtGui import QPixmap
from widget import BackgroundFrame, BoolCheck
from parameter import DataSetting, EnumData


# noinspection PyTypeChecker
class Skill(BackgroundFrame):
    ICON_SIZE = 36

    def __init__(self, parent):
        BackgroundFrame.__init__(self, parent)
        skill_frame = BackgroundFrame()
        feature_frame = BackgroundFrame()
        skill_layout = QGridLayout()
        feature_layout = QGridLayout()

        for idx, skill in enumerate(EnumData.SKILL_ENUM):
            self[skill] = BoolCheck(self, EnumData().SID_ENUM[skill].removeprefix('SID_'),
                                    value=DataSetting()[skill])
            self[skill].setFixedHeight(self.ICON_SIZE)
            label = QLabel()
            label.setPixmap(QPixmap(f':SID/{skill}.gif'))
            label.setFixedSize(self.ICON_SIZE, self.ICON_SIZE)
            skill_layout.addWidget(label, idx, 0, 1, 1)
            skill_layout.addWidget(self[skill], idx, 1, 1, 1)

        for idx, mastery in enumerate(EnumData.MASTERY_ENUM):
            self[mastery] = BoolCheck(self, EnumData().SID_ENUM[mastery].removeprefix('SID_'),
                                      value=DataSetting()[mastery])
            self[mastery].setFixedHeight(self.ICON_SIZE)
            label = QLabel()
            label.setPixmap(QPixmap(f':SID/{mastery}.gif'))
            label.setFixedSize(self.ICON_SIZE, self.ICON_SIZE)
            skill_layout.addWidget(label, idx + 32, 0, 1, 1)
            skill_layout.addWidget(self[mastery], idx + 32, 1, 1, 1)

        skill_layout.setContentsMargins(10, 5, 5, 5)
        skill_frame.setLayout(skill_layout)
        skill_scroll = QScrollArea()
        skill_scroll.setFixedHeight(400)
        skill_scroll.setWidget(skill_frame)

        for idx, feature in enumerate(EnumData.FEATURE_ENUM):
            self[feature] = BoolCheck(self, EnumData().SID_ENUM[feature].removeprefix('SID_'),
                                      value=DataSetting()[feature])
            feature_layout.addWidget(self[feature], idx, 0, 1, 1)
        feature_layout.setContentsMargins(10, 5, 5, 5)
        feature_frame.setLayout(feature_layout)
        feature_scorll = QScrollArea()
        feature_scorll.setFixedHeight(400)
        feature_scorll.setWidget(feature_frame)

        main_layout = QGridLayout()
        main_layout.addWidget(skill_scroll, 0, 0, 1, 1)
        main_layout.addWidget(feature_scorll, 0, 1, 1, 1)
        self.setLayout(main_layout)
