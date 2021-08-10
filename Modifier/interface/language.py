#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtWidgets import QMessageBox
from PySide6.QtGui import QIcon


def get_language():
    box = QMessageBox(QMessageBox.Question, '选择语言', '界面名词使用语言')
    box.setWindowIcon(QIcon(':/ICON/icon.png'))
    accept = box.addButton('日文（原版）', QMessageBox.AcceptRole)
    reject = box.addButton('汉化（默认）', QMessageBox.RejectRole)
    accept.setFixedSize(120, 30)
    reject.setFixedSize(120, 30)
    box.exec()
    language = 'jp' if box.clickedButton() == accept else 'zh'
    return language
