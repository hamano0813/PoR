#!/usr/bin/env python
# -*- coding: utf-8 -*-

import sys

from PySide6.QtGui import QIcon
from PySide6.QtWidgets import QApplication, QMessageBox
from dolphin_memory_engine import hook, is_hooked

from interface.language import get_language
# noinspection PyUnresolvedReferences
from interface.resource import *
from interface.window import Window


# noinspection PyUnresolvedReferences
def main():
    hook()
    app = QApplication(sys.argv)

    if is_hooked():
        qss = QtCore.QFile(':QSS/custom.qss')
        qss.open(QtCore.QFile.ReadOnly)
        stylesheet = bytearray(qss.readAll()).decode('UTF-8')
        app.setStyleSheet(stylesheet)

        language = get_language()
        trans = QtCore.QTranslator()
        trans.load(f':QM/{language}.qm')
        app.installTranslator(trans)

        window = Window()
        window.show()
        sys.exit(app.exec())
    else:
        box = QMessageBox(QMessageBox.Critical, '出错啦', '请先开始模拟游戏')
        box.setWindowIcon(QIcon(':/ICON/icon.ico'))
        box.addButton('关闭', QMessageBox.NoRole)
        sys.exit(box.exec())


if __name__ == '__main__':
    main()
