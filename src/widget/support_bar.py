#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtWidgets import QProgressBar


class SupportBar(QProgressBar):
    def __init__(self, text: str):
        QProgressBar.__init__(self, parent=None)
        self.setTextVisible(True)
        self.setFormat(text)
        self.setMaximumSize(90, 30)

    def set_value(self, value: int):
        if value < self.minimum():
            self.setValue(self.minimum())
        elif value > self.maximum():
            self.setValue(self.maximum())
        else:
            self.setValue(value)
