#!/usr/bin/env python
# -*- coding: utf-8 -*-

from PySide6.QtGui import QIcon
from PySide6.QtWidgets import QComboBox

from .customize import Customize


class MapCombo(Customize, QComboBox):
    def __init__(self, parent, mapping: callable, icon: str = None, **kwargs):
        QComboBox.__init__(self, parent)
        Customize.__init__(self, parent, **kwargs)
        self.mapping = mapping
        self.icon = icon
        self.init_editor()

    def init_editor(self):
        self.clear()
        self.addItem(QIcon(':/ICON/blank.png'), '——', 0x0) if self.icon else self.addItem('——', 0x0)
        if mapping := self.mapping():
            for value, item in mapping.items():
                if isinstance(item, tuple):
                    icon, name = item
                    self.addItem(QIcon(f":/{self.icon}/{icon}.gif"), name, value) if self.icon else self.addItem(name,
                                                                                                                 value)
                else:
                    self.addItem(item, value)

    def refresh(self):
        self.disconnect(self)
        self.init_editor()
        value = self.structure['value'].get(self.offset)
        if mapping := self.mapping():
            if isinstance(item := mapping.get(value, '——'), str):
                self.setCurrentText(item)
            else:
                self.setCurrentText(item[-1])
            # noinspection PyUnresolvedReferences
            self.currentIndexChanged.connect(self.rewrite)

    def rewrite(self):
        value: int = self.currentData()
        self.structure['value'].set(value, self.offset)
