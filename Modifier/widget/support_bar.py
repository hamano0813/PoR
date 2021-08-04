#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtWidgets import QProgressBar


class SupportBar(QProgressBar):
    def __init__(self, text: str):
        QProgressBar.__init__(self, parent=None)
        self.setTextVisible(True)
        self.setFormat(text)
        self.setMaximumSize(100, 28)
        self.setStyleSheet('''
        QProgressBar {text-align: center; font: bold; border: 1px solid Grey; border-radius: 5px;}
        QProgressBar::chunk {background-color: #05B8CC; width: 1px;}
        ''')

    def refresh(self):
        value = self.value()
        self.setValue(self.maximum())
        self.setValue(self.minimum())
        self.setValue(value)

    def set_value(self, value: int):
        if value < self.minimum():
            self.setValue(self.minimum())
        elif value > self.maximum():
            self.setValue(self.maximum())
        else:
            self.setValue(value)
