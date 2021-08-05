#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtWidgets import QLabel, QSizePolicy
from PySide6.QtCore import Qt


class NameLabel(QLabel):
    def __init__(self, text):
        super(NameLabel, self).__init__(text, parent=None)
        self.setAlignment(Qt.AlignCenter)
        self.setFixedWidth(80)
        self.setMinimumHeight(30)
        self.setStyleSheet("* {border:1px solid #808080;}")
        self.setSizePolicy(QSizePolicy.Fixed, QSizePolicy.Preferred)
