#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtWidgets import QCheckBox
from .customize import Customize


class BoolCheck(Customize, QCheckBox):
    def __init__(self, parent, text, **kwargs):
        QCheckBox.__init__(self, parent)
        Customize.__init__(self, parent, ** kwargs)
        self.setText(text)

    # noinspection PyUnresolvedReferences,PyTypeChecker
    def refresh(self):
        self.disconnect(self)
        self.setChecked(self.structure['value'].get(self.offset))
        self.stateChanged.connect(self.rewrite)

    def rewrite(self):
        self.structure['value'].set(self.isChecked())
