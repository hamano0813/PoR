#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtGui import QPixmap
from PySide6.QtWidgets import QLabel, QGridLayout, QScrollArea

from parameter import DataSetting, EnumData
from widget import BackgroundFrame, BoolCheck


# noinspection PyTypeChecker,SpellCheckingInspection
class Skill(BackgroundFrame):
    ICON_SIZE = 36
    SKILL = (
        'SID_ELITE',
        'SID_TURNREGENE',
        'SID_SWIFT',
        'SID_BLAVE',
        'SID_IMPREGNABLE',
        'SID_TEMPER',
        'SID_CALM',
        'SID_RESCUEP',
        'SID_HANDI',
        'SID_CHANT',
        'SID_EQUIPKNIFE',
        'SID_EQUIPLIGHT',
        'SID_AMBUSH',
        'SID_GRASP',
        'SID_ANGER',
        'SID_DEFENCE',
        'SID_PRAY',
        'SID_CONTINUATION',
        'SID_WEAPONDESTROY',
        'SID_COUNTER',
        'SID_HORROR',
        'SID_STEAL',
        'SID_PROVOKE',
        'SID_SHADE',
        'SID_GAMBLE',
        'SID_FAIRNESS',
        'SID_TACKLE2',
        'SID_REINFORCEMENTS',
        'SID_FRAC90',
        'SID_TELEGNOSIS',
        'SID_BIGEAR',
        'SID_GODDESSBLESS',
    )
    MASTERY = (
        'SID_SUNMOON',
        'SID_IMPACT',
        'SID_BRIGHTNESS',
        'SID_SUNTRICK',
        'SID_STARTRICK',
        'SID_MOONTRICK',
        'SID_RUMBLE',
        'SID_SNIPE',
        'SID_ASSASSINATE',
        'SID_SNARL',
        'SID_WINGSHIELD',
        'SID_FLUTTER',
        'SID_SKYBLESSING',
        'SID_EARTHBLESSING',
    )
    FEATURE = (
        'SID_HERO',
        'SID_FEMALE',
        'SID_HIGHER',
        'SID_TWICE',
        'SID_FLY',
        'SID_VILLAGEDESTROY',
        'SID_BOSS',
        'SID_FINAL',
        'SID_ANIMALIZE',
        'SID_LYCANTHROPE',
        'SID_FIXEDBEAST',
        'SID_BARGAIN',
        'SID_WLUPTOS',
        'SID_FLYTHRU',
        'SID_WALKTHRU',
        'SID_CHANTHP',
        'SID_CHANTSTR',
        'SID_CHANTMPOW',
        'SID_CHANTTECH',
        'SID_CHANTQUICK',
        'SID_CHANTLUCK',
        'SID_CHANTDEF',
        'SID_CHANTMDEF',
        'SID_EQUIPFANG',
        'SID_EQSW',
        'SID_EQLA',
        'SID_EQAX',
        'SID_EQBW',
        'SID_EQRD',
        'SID_CHAOS',
        'SID_CHARISMA',
        'SID_ABSMOVE',
        'SID_CONFRONT',
        'SID_TACKLE',
        'SID_EVILEYE',
        'SID_EVENTCC',
        'SID_SUMMONED',
        'SID_SHOOT',
        'SID_KEY50',
        'SID_TRI_A',
        'SID_TRI_B',
        'SID_EQ_A',
        'SID_EQ_B',
        'SID_EQ_C',
        'SID_TEMP_ON_DIE',
        'SID_KEY0',
        'SID_IMMORTAL',
        'SID_EQ_D',
        'SID_EQREV_A',
        'SID_WEAK_A',
        'SID_AHIMSA',
    )

    def __init__(self, parent):
        BackgroundFrame.__init__(self, parent)
        skill_frame = BackgroundFrame()
        feature_frame = BackgroundFrame()
        skill_layout = QGridLayout()
        feature_layout = QGridLayout()

        for idx, skill in enumerate(self.SKILL):
            self[skill] = BoolCheck(self, EnumData().SID_ENUM[skill].removeprefix('SID_'),
                                    value=DataSetting()[skill])
            self[skill].setFixedHeight(self.ICON_SIZE)
            label = QLabel()
            label.setPixmap(QPixmap(f':SID/{skill}.gif'))
            label.setFixedSize(self.ICON_SIZE, self.ICON_SIZE)
            skill_layout.addWidget(label, idx, 0, 1, 1)
            skill_layout.addWidget(self[skill], idx, 1, 1, 1)

        for idx, mastery in enumerate(self.MASTERY):
            self[mastery] = BoolCheck(self, EnumData().SID_ENUM[mastery].removeprefix('SID_'),
                                      value=DataSetting()[mastery])
            self[mastery].setFixedHeight(self.ICON_SIZE)
            label = QLabel()
            label.setPixmap(QPixmap(f':SID/{mastery}.gif'))
            label.setFixedSize(self.ICON_SIZE, self.ICON_SIZE)
            skill_layout.addWidget(label, idx + 32, 0, 1, 1)
            skill_layout.addWidget(self[mastery], idx + 32, 1, 1, 1)

        skill_layout.setContentsMargins(10, 5, 5, 5)
        skill_layout.setSpacing(3)
        skill_frame.setLayout(skill_layout)
        skill_scroll = QScrollArea()
        skill_scroll.setWidget(skill_frame)

        for idx, feature in enumerate(self.FEATURE):
            self[feature] = BoolCheck(self, EnumData().SID_ENUM[feature].removeprefix('SID_'),
                                      value=DataSetting()[feature])
            feature_layout.addWidget(self[feature], idx, 0, 1, 1)
        feature_layout.setContentsMargins(10, 5, 5, 5)
        feature_layout.setSpacing(3)
        feature_frame.setLayout(feature_layout)
        feature_scorll = QScrollArea()
        feature_scorll.setWidget(feature_frame)

        main_layout = QGridLayout()
        main_layout.setSpacing(3)
        main_layout.addWidget(skill_scroll, 0, 0, 1, 1)
        main_layout.addWidget(feature_scorll, 0, 1, 1, 1)
        self.setLayout(main_layout)
