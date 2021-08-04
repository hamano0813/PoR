#!/usr/bin/env python
# -*- coding: utf-8 -*-

import sys
from PySide6.QtWidgets import QApplication
from dolphin_memory_engine import hook, is_hooked
from interface.window import Window
# noinspection PyUnresolvedReferences
from interface.resource import *


# noinspection PyUnresolvedReferences
def main():
    app = QApplication(sys.argv)
    trans = QtCore.QTranslator()
    trans.load(':QM/jp.qm')
    app.installTranslator(trans)
    window = Window()
    window.show()
    sys.exit(app.exec())


if __name__ == '__main__':
    hook()
    if is_hooked():
        main()
