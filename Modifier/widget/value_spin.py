#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtWidgets import QSpinBox
from PySide6.QtCore import Qt
from PySide6.QtGui import QIntValidator
from .customize import Customize


class ValueSpin(Customize, QSpinBox):
    def __init__(self, parent, **kwargs):
        QSpinBox.__init__(self, parent)
        Customize.__init__(self, parent, **kwargs)
        self.init_editor()
        self.setFixedWidth(80)
        self.setAlignment(Qt.AlignRight | Qt.AlignVCenter)

    def init_editor(self):
        if (length := self.structure['value'].length) > 0:
            maximum = 0x100 ** length
        else:
            maximum = 0x100 ** abs(length) // 2
        minimum = maximum - 0x100 ** abs(length)
        if (bit := self.structure['value'].bit) is not None:
            minimum = 0
            if isinstance(bit, int):
                maximum = 2
            else:
                maximum = (1 << (bit[1] - bit[0]))
        self.setRange(minimum, maximum - 1)
        self.lineEdit().setValidator(QIntValidator(minimum, maximum - 1))

    def refresh(self):
        self.disconnect(self)
        value = self.structure['value'].get(self.offset)
        self.setValue(value)
        # noinspection PyUnresolvedReferences
        self.valueChanged.connect(self.rewrite)

    def rewrite(self):
        value = self.value()
        self.structure['value'].set(value, self.offset)
