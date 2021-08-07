#!/usr/bin/env python
# -*- coding: utf-8 -*-

import sys
from PySide6.QtWidgets import QApplication, QMessageBox
from PySide6.QtGui import QIcon
from dolphin_memory_engine import hook, is_hooked
from interface.window import Window
# noinspection PyUnresolvedReferences
from interface.resource import *


# noinspection PyUnresolvedReferences
def main():
    app = QApplication(sys.argv)

    box = QMessageBox()
    box.setWindowIcon(QIcon(':/ICON/icon.png'))
    box.setWindowTitle('选择语言')
    box.setText('界面名词使用语言')
    accept = box.addButton('日文（原版）', QMessageBox.AcceptRole)
    reject = box.addButton('汉化（默认）', QMessageBox.RejectRole)
    accept.setFixedSize(120, 30)
    reject.setFixedSize(120, 30)
    box.exec()
    language = 'jp' if box.clickedButton() == accept else 'zh'

    trans = QtCore.QTranslator()
    trans.load(f':QM/{language}.qm')
    app.installTranslator(trans)
    app.setStyleSheet('* {font-family: "Iosevka Semibold", "Inziu Roboto SC"; font-size: 12pt;}')

    window = Window()
    window.show()
    sys.exit(app.exec())


if __name__ == '__main__':
    hook()
    if is_hooked():
        main()
